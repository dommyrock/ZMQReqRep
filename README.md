# ZeroMQTestApps
Used documentation: http://zguide.zeromq.org/

This is basic client- server / REQ -REP chat app made in .NET windows forms  using ZMQ library . 
1) We bind server/REP zsocket (localhost + any(free) port) by pressing connect btn
2) We connect client /REQ zsocket to server /REP socket 
2.1)client sends $$$$ClientREADY$$$$ frame to zsocket
2.2)Server receives $$$$ClientREADY$$$$ frame on zsocket and sends $$$$ServerREADY$$$$ frame
.... process repeats in the background on separate thereads

**IMPORTANT ***  if Server Unbinds all msgs by client will never be received by server (even on server re-bind)
                 if Client Disconnects , server sends messages, client will receive all messages
sent by server while client was disconnected !!!
---------------------------------------------------