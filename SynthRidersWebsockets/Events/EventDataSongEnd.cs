using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets.Events
{
    public class EventDataSongEnd
    {
        public string song = "";
        public int perfect = 0;
        public int normal = 0;
        public int bad = 0;
        public int fail = 0;
        public int highestCombo = 0;

        public EventDataSongEnd(string song, int perfect, int normal, int bad, int fail, int highestCombo)
        {
            this.song = song;
            this.perfect = perfect;
            this.normal = normal;
            this.bad = bad;
            this.fail = fail;
            this.highestCombo = highestCombo;
        }
    }
}
