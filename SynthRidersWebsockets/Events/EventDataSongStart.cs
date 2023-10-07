using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets.Events
{
    public class EventDataSongStart
    {
        public string song = "";
        public string difficulty = "";
        public string author = "";
        public string beatMapper = "";
        public float length = 0.0f;
        public float bpm = 0.0f;
        public string albumArt = "";

        public EventDataSongStart(string song, string difficulty, string author, string beatMapper, float length, float bpm, string albumArt = null)
        {
            this.song = song;
            this.difficulty = difficulty;
            this.author = author;
            this.beatMapper = beatMapper;
            this.length = length;
            this.bpm = bpm;
            this.albumArt = albumArt;
        }
    }
}
