using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Threading;
using MelonLoader;
using Newtonsoft.Json;

namespace SynthRidersWebsockets.Events
{
    public class SREventsWebSocketClient : AbstractWebSocketClient
    {
        ISynthRidersEventHandler eventHandler;

        public SREventsWebSocketClient(MelonLogger.Instance logger, string host, int port, ISynthRidersEventHandler eventHandler)
            : base(logger, host, port)
        {
            this.eventHandler = eventHandler;
        }

        protected override void HandleReceive(string messageJson)
        {
            //logger.Msg("Received message");
            try
            {
                // Parse top layer as generic to get type, then parse the specific message data if needed.
                var genericEvent = JsonConvert.DeserializeObject<SynthRidersEvent<object>>(messageJson);
                if (genericEvent.eventType == "SongStart")
                {
                    var songStart = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSongStart>>(messageJson);
                    eventHandler.OnSongStart(songStart.data);
                }
                else if (genericEvent.eventType == "SongEnd")
                {
                    var songEnd = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSongEnd>>(messageJson);
                    eventHandler.OnSongEnd(songEnd.data);
                }
                else if (genericEvent.eventType == "PlayTime")
                {
                    var playTime = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataPlayTime>>(messageJson);
                    eventHandler.OnPlayTime(playTime.data);
                }
                else if (genericEvent.eventType == "NoteHit")
                {
                    var noteHit = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataNoteHit>>(messageJson);
                    eventHandler.OnNoteHit(noteHit.data);
                }
                else if (genericEvent.eventType == "NoteMiss")
                {
                    var noteMiss = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataNoteMiss>>(messageJson);
                    eventHandler.OnNoteMiss(noteMiss.data);
                }
                else if (genericEvent.eventType == "EnterSpecial")
                {
                    // No extra data in payload
                    eventHandler.OnEnterSpecial();
                }
                else if (genericEvent.eventType == "CompleteSpecial")
                {
                    // No extra data in payload
                    eventHandler.OnCompleteSpecial();
                }
                else if (genericEvent.eventType == "FailSpecial")
                {
                    // No extra data in payload
                    eventHandler.OnFailSpecial();
                }
                else if (genericEvent.eventType == "SceneChange")
                {
                    var sceneChange = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSceneChange>>(messageJson);
                    eventHandler.OnSceneChange(sceneChange.data);
                }
                else if (genericEvent.eventType == "ReturnToMenu")
                {
                    eventHandler.OnReturnToMenu();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to parse message {messageJson}", ex);
            }
        }
    }
}
