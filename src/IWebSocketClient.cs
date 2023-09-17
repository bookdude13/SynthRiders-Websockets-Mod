using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets
{
    internal interface IWebSocketClient
    {
        public void OnOpen();
        public void OnMessage();
        public void OnError();
        public void OnClose();
    }
}
