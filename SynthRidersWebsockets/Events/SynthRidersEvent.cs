using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;

namespace SynthRidersWebsockets.Events
{
    public class SynthRidersEvent<T>
    {
        public string eventType;
        public T data;

        public SynthRidersEvent(string eventType, T data)
        {
            this.eventType = eventType;
            this.data = data;
        }
    }
}
