using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DotMetrics.TestChat
{
    public class Message
    {
        public DateTime Timestamp { get; set; }

        public string Content { get; set; }

        public Message()
           : this(DateTime.Now, null)
        {
        }

        public Message(string content)
            : this(DateTime.Now, content)
        {
        }

        public Message(DateTime dateTime, string content)
        {
            this.Timestamp = dateTime;
            this.Content = content;
        }
    }

    public class ChatNode
    {
        public static ZContext Context { get; private set; }

        public bool IsServer { get; private set; }

        public string Endpoint { get; private set; }

        public bool IsStarted { get; private set; }

        private ZSocket activeSocket;

        private Thread workerThread;

        private string ErrorMessage;

        public ChatNode(bool isServer)
        {
            this.IsServer = isServer;
            if (ChatNode.Context == null)
            {
                ChatNode.Context = new ZContext();
            }
            this._receiveList = new List<Message>();
            this._sendList = new List<Message>();
            this._output = new List<Message>();
        }

        public bool Start(string endpoint)
        {
            this.Endpoint = endpoint;

            if (this.IsStarted == false)
            {
                if (this.IsServer)
                {
                    this.activeSocket = new ZSocket(ChatNode.Context, ZSocketType.REP);
                    try
                    {
                        this.activeSocket.Bind(this.Endpoint);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                    this.IsStarted = true;
                    this.workerThread = new Thread(RunExchange);
                    this.workerThread.Start();
                    return true;
                }
                else
                {
                    this.activeSocket = new ZSocket(ChatNode.Context, ZSocketType.REQ);
                    try
                    {
                        this.activeSocket.Connect(this.Endpoint);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                    this.IsStarted = true;
                    this.workerThread = new Thread(RunExchange);
                    this.workerThread.Start();
                    return true;
                }
            }
            return false;
        }

        public void Stop(string endpoint)
        {
            this.Endpoint = endpoint;

            if (this.IsStarted == true)
            {
                if (this.IsServer)
                {
                    try
                    {
                        this.activeSocket.Unbind(this.Endpoint);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }
                    this.IsStarted = false;
                }
                else
                {
                    try
                    {
                        this.activeSocket.Disconnect(this.Endpoint);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }
                    this.IsStarted = false;
                }
            }
        }

        private List<Message> _receiveList;
        private List<Message> _sendList;
        private List<Message> _output;

        private void RunExchange()
        {
            if (!this.IsStarted)
                return;//vraca kontrolu metodi koja ju je zvala

            while (this.IsStarted)
            {
                ZFrame frame;
                if (!this.IsServer)
                {
                    this.activeSocket.Send(new ZFrame("$$$$ClientREADY$$$$")); //1. st frame when client connects

                    frame = this.activeSocket.ReceiveFrame();
                    string msgJson = frame.ReadString();

                    List<Message> primljenePoruke = new List<Message>();
                    JsonConvert.PopulateObject(msgJson, primljenePoruke); //populates Message obj from frame string
                    _receiveList.AddRange(primljenePoruke);
                    _output.AddRange(_receiveList);
                    _receiveList.Clear();

                    string toSendJson = JsonConvert.SerializeObject(this._sendList);//serilize Message obj to string
                    // skini sve poruke iz liste
                    this._sendList.Clear();

                    this.activeSocket.Send(new ZFrame(toSendJson));

                    frame = this.activeSocket.ReceiveFrame(); // READY
                }
                else
                {
                    frame = this.activeSocket.ReceiveFrame(); // READY

                    string toSendJson = JsonConvert.SerializeObject(this._sendList);//serilize Message obj to string
                    // skini sve poruke iz liste
                    this._sendList.Clear();

                    this.activeSocket.Send(new ZFrame(toSendJson));

                    frame = this.activeSocket.ReceiveFrame();
                    string msgJson = frame.ReadString();

                    // skini sve poruke iz liste
                    List<Message> primljenePoruke = new List<Message>();
                    JsonConvert.PopulateObject(msgJson, primljenePoruke);//populates Message obj from frame string
                    _receiveList.AddRange(primljenePoruke);
                    _output.AddRange(_receiveList);
                    _receiveList.Clear();
                    this.activeSocket.Send(new ZFrame("$$$$ServerREADY$$$$"));
                }

                Thread.Sleep(10); //added for synchronization , but it's not best solution
            }
        }

        public void InputTextFromUi(Message inputText)
        {
            _sendList.Add(inputText);
        }

        public void OutputTextToUi(Message outputText)
        {
            _output.Add(outputText);
        }

        public string RefreshOutput()
        {
            string chatText = "";
            List<Message> sortedOutput = _output.OrderBy(item => item.Timestamp).ToList();
            foreach (Message msg in sortedOutput)
            {
                chatText += msg.Timestamp.ToString("dd.MM.yyyy HH:mm:ss - ") + msg.Content + Environment.NewLine;
            }
            return chatText;
        }
    }
}