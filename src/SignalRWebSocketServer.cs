using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets
{
    internal class SignalRWebSocketServer : IWebSocketServer
    {
        private string host;
        private int port;

        public SignalRWebSocketServer(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Start()
        {
            var connectionString = $"ws://{host}:{port}";
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }
    }
}
