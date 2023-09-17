/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using UnityEngine.Networking.PlayerConnection;
using MelonLoader;
using System.IO;

namespace SynthRidersWebsockets
{
    public class DotNetCoreWebSocketServer : IWebSocketServer
    {
        private WebSocket server;

        // Define a receive buffer to hold data received on the WebSocket connection. The buffer will be reused as we only need to hold on to the data
        // long enough to send it back to the sender.
        private byte[] serverBuffer = new byte[2048];
        private Stream serverStream;


        public void Start(string host)
        {
            MelonLogger.Msg($"[Websocket] Starting socket server on: ws://{host}/");

            serverStream = new MemoryStream(serverBuffer, true);

            // TODO consider adding KeepAliveInterval
            WebSocketCreationOptions options = new WebSocketCreationOptions
            {
                IsServer = true,
            };
            server = WebSocket.CreateFromStream(serverStream, options);


            *//*server = new WebSocketServer("ws://" + host);
            server.AddWebSocketService<EventSocket>("/");
            server.Start();*//*
        }

        public void Stop()
        {
            if (server != null) server.Stop();
        }

        public void Send(string message)
        {
            if (server == null || eventSocket == null) return;
            eventSocket.SendBroadcast(message);
        }

        public class EventSocket : WebSocketBehavior
        {
            public void SendBroadcast(string msg)
            {
                Sessions.Broadcast(msg);
            }

            protected override void OnOpen()
            {
                if (eventSocket == null) eventSocket = this;
            }

            protected override void OnError(WebSocketSharp.ErrorEventArgs e)
            {
                if (eventSocket == null) eventSocket = this;
            }

            protected override void OnClose(CloseEventArgs e)
            {
                if (eventSocket == null) eventSocket = this;
            }

            protected override void OnMessage(MessageEventArgs e)
            {
            }
        }
    }
}
*/