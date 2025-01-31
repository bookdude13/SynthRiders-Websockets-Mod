﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets.Events
{
    public interface ISynthRidersEventHandler
    {
        void OnSongStart(EventDataSongStart data);
        void OnSongEnd(EventDataSongEnd data);
        void OnPlayTime(EventDataPlayTime data);
        void OnNoteHit(EventDataNoteHit data);
        void OnNoteMiss(EventDataNoteMiss data);
        void OnEnterSpecial();
        void OnCompleteSpecial();
        void OnFailSpecial();
        void OnSceneChange(EventDataSceneChange data);
        void OnReturnToMenu();
    }
}
