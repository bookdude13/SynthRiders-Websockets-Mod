using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets.Events
{
    public class EventDataNoteMiss
    {
        public int multiplier = 1; // multiplier before miss reset
        public float lifeBarPercent = 1.0f;
        public float playTimeMS = 0.0f; // Current play time

        public EventDataNoteMiss(int multiplier, float lifeBarPercent, float playTimeMS)
        {
            this.multiplier = multiplier;
            this.lifeBarPercent = lifeBarPercent;
            this.playTimeMS = playTimeMS;
        }
    }
}
