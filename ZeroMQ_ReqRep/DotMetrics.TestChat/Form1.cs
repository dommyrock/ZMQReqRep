using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZeroMQ;

namespace DotMetrics.TestChat
{
    public partial class WindowsLiveMessenger : Form
    {
        private bool isServer = true;
        private string endpoint = "tcp://";
        private ChatNode cNode;
        private string uiTxt = "";

        public WindowsLiveMessenger()
        {
            InitializeComponent();
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (isServer)
            {
                uiTxt = "Server:     " + sendTxtbox.Text;
            }
            else
            {
                uiTxt = "Client:       " + sendTxtbox.Text;
            }

            sendTxtbox.Clear();

            Message msg = new Message(DateTime.Now, uiTxt);
            if (cNode != null)
            {
                cNode.InputTextFromUi(msg);

                cNode.OutputTextToUi(msg);
            }
        }

        private void connectionBtn_Click(object sender, EventArgs e)
        {
            isServer = checkBox1.Checked;
            endpoint += IPtext.Text;
            if (cNode == null)
                cNode = new ChatNode(isServer);
            cNode.Start(endpoint);
            timer.Start(); //refreshes output lists every 100ms
        }

        private void disconnectBtn_Click(object sender, EventArgs e)
        {
            if (cNode != null)
                cNode.Stop(endpoint);
            endpoint = "tcp://";
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            receiveTxtbox.Text = cNode.RefreshOutput();
            receiveTxtbox.SelectionStart = receiveTxtbox.Text.Length;
            receiveTxtbox.ScrollToCaret();
        }

        private void sendTxtbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sendBtn_Click(this, new EventArgs());
            }
        }

        private void btn_Quit_Click(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }
    }
}