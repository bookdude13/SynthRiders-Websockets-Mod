using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets.Events
{
    public class EventDataPlayTime
    {
        public float playTimeMS = 0.0f;
        public EventDataPlayTime(float playTimeMS)
        {
            this.playTimeMS = playTimeMS;
        }
    }
}
