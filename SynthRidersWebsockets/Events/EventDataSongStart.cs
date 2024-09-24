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
        
        // Modifiers and game modes
        public bool noFailEnabled = false;
        public bool spinEnabled = false;
        public int spinMode = 0; // 0 = 90, 1 = 180, 2 = 360, 3 = 360+
        public int spinIntensity = 0; // 1 = wild, 2 = mild, 0 = styled (yes, not intuitive - but it's what the game does)
        public bool spiralEnabled = false;
        public int spiralIntensity = 0; // 1 = mild, 2 = styled, 3 = chuck (wild)
        public bool obstaclesEnabled = false;
        public int noteJumpSpeed = 0; // 0 = 1 NJS, 1 = 2 NJS, 2 = 3 NJS
        public bool suddenDeathEnabled = false;
        public bool vanishNotesEnabled = false;
        public bool prismaticNotesEnabled = false;
        public int noteSize = 0; // 0 = default, 1 = small, 2 = big
        public bool oneHandModeEnabled = false;
        public bool isExperienceStage = false;
        public bool isForceMode = false; // false == rhythm mode, true == force mode
        public bool haloEnabled = false;

        public EventDataSongStart(
            string song, 
            string difficulty, 
            string author, 
            string beatMapper, 
            float length, 
            float bpm, 
            string albumArt = null,
            bool noFailEnabled = false,
            bool spinEnabled = false,
            int spinMode = 0,
            int spinIntensity = 0,
            bool spiralEnabled = false,
            int spiralIntensity = 0,
            bool vanishNotesEnabled = false,
            bool obstaclesEnabled = false,
            int noteJumpSpeed = 0,
            bool suddenDeathEnabled = false,
            bool prismaticNotesEnabled = false,
            int noteSize = 0,
            bool oneHandModeEnabled = false,
            bool isExperienceStage = false,
            bool isForceMode = false,
            bool haloEnabled = false)
        {
            this.song = song;
            this.difficulty = difficulty;
            this.author = author;
            this.beatMapper = beatMapper;
            this.length = length;
            this.bpm = bpm;
            this.albumArt = albumArt;

            this.noFailEnabled = noFailEnabled;
            this.spinEnabled = spinEnabled;
            this.spinMode = spinMode;
            this.spinIntensity = spinIntensity;
            this.spiralEnabled = spiralEnabled;
            this.spiralIntensity = spiralIntensity;
            this.vanishNotesEnabled = vanishNotesEnabled;
            this.obstaclesEnabled = obstaclesEnabled;
            this.noteJumpSpeed = noteJumpSpeed;
            this.suddenDeathEnabled = suddenDeathEnabled;
            this.prismaticNotesEnabled = prismaticNotesEnabled;
            this.noteSize = noteSize;
            this.oneHandModeEnabled = oneHandModeEnabled;
            this.isExperienceStage = isExperienceStage;
            this.isForceMode = isForceMode;
            this.haloEnabled = haloEnabled;
        }
    }
}
