# ZeroMQTestApps
Used documentation: http://zguide.zeromq.org/

This is basic client- server / REQ -REP chat app made in .NET windows forms  using ZMQ library . 
1) We bind server/REP zsocket (localhost + any(free) port) by pressing connect btn
2) We connect client /REQ zsocket to server
2.1)client sends $$$$ClientREADY$$$$ frame to zsocket
2.2)Server receives $$$$ClientREADY$$$$ frame on zsocket and sends $$$$ServerREADY$$$$ frame
 ....and keeps repeating in background (to allow /improvise real time chat experience)
 
**IMPORTANT ***  if Server Unbinds all messages by client will never be received by server (even on server re-bind)
                 if Client Disconnects , server sends messages, client will receive all messages
sent by server while client was disconnected !!!
---------------------------------------------------
**Summary :**
ZeroMQ gives your applications a single socket API to work with, no matter what the actual transport 
(like in-process, inter-process, TCP, or multicast). 
It automatically reconnects to peers as they come and go. It queues messages at both sender and receiver,
 as needed. 
It limits these queues to guard processes against running out of memory. It handles socket errors.
 It does all I/O in background threads. It uses lock-free techniques for talking between nodes, 
so there are never locks, waits, semaphores, or deadlocks.

Pipline --> --> in memory comunication  (distributed work just in one machine (not on network !)
--> if we needed to send to other machines we would change to TCP socket
PUB /SUB --> like a broadcast -- > sending msgs out , and subscriped clients get them when they tune in 
