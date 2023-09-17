using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets
{
    internal interface IWebSocketServer
    {
        public void Start();
        public void Stop();
        public void Send(string message);
    }
}
