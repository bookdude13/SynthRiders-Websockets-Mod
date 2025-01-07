using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets.Events
{
    public class EventDataNoteHit
    {
        public int score = 0; // score _after_ note hit
        public int combo = 0;
        public int multiplier = 1;
        public float completed = 1.0f; // perfect + normal + bad (all notes hit except fails)
        public float lifeBarPercent = 1.0f;
        public float playTimeMS = 0.0f; // Current play time

        public EventDataNoteHit(int score, int combo, float completed, int multiplier, float lifeBarPercent, float playTimeMS)
        {
            this.score = score;
            this.combo = combo;
            this.completed = completed;
            this.multiplier = multiplier;
            this.lifeBarPercent = lifeBarPercent;
            this.playTimeMS = playTimeMS;
        }
    }
}
