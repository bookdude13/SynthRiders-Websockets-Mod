﻿using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Il2Cpp;
using Il2Cppsynth;
using Il2CppSynth.Utils;

using UnityEngine;
using UnityEngine.Events;

using MelonLoader;

using SynthRidersWebsockets.Harmony;
using SynthRidersWebsockets.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

/*
* Todo:
* - Emit event if song failed.
* - Emit event is song is quit.
* - Previous high score
* - Emit what hand note was for (left, right, special one-hand, special two-hand)
* - Score on specific hits
*/
namespace SynthRidersWebsockets
{
    public class WebsocketMod : MelonMod
    {
        // If set, creates a client as well that logs out messages as they are received
        private static readonly bool USE_TEST_CLIENT = false;

        public static WebsocketMod Instance;
        private static GameControlManager gameControlManager;
        private static SREventsWebSocketServer webSocketServer;
        private static SREventsWebSocketClient testClient;
        private static IHost server;

        public static MelonPreferences_Category connectionCategory;

        /**
         * Keep track of last time play time was emitted so we only emit once per second.
         */
        private float lastPlayTimeEventMS = 0;
        private float currentPlayTimeMS = 0.0f;
        private float healthPercent = 1.0f;

        public override void OnInitializeMelon() {
            Instance = this;
            connectionCategory = MelonPreferences.CreateCategory("Connection");
            string host = connectionCategory.CreateEntry<string>("Host", "localhost").Value;
            int port = connectionCategory.CreateEntry<int>("Port", 9000).Value;

            if (USE_TEST_CLIENT)
            {
                var eventHandler = new LoggingSynthRidersEventHandler(LoggerInstance);
                testClient = new SREventsWebSocketClient(LoggerInstance, host, port, eventHandler);
            }

            LoggerInstance.Msg("[Websocket] Starting Websocket server");
            webSocketServer = new SREventsWebSocketServer(LoggerInstance, host, port);
            webSocketServer.ClientConnected += this.ClientConnected;

            HostBuilder builder = new();
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            server = builder
                .ConfigureServices(services =>
                {
                    services.AddHostedService(provider =>
                    {
                        return webSocketServer;
                    });

                    if (USE_TEST_CLIENT && testClient != null)
                    {
                        services.AddHostedService(provider =>
                        {
                            return testClient;
                        });
                    }
                })
                .Build();
            
            // Kick off server in background
            _ = server.RunAsync();

            // Patch _after_ the server is started and can handle messages
            LoggerInstance.Msg("Applying Harmony patches");
            RuntimePatch.PatchAll();
        }

        protected void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            // NOTE: At present, this just broadcasts to all connected sockets as it was easier to implement.
            // As an optimization, it should emit this only to the newly-connected socket.
            if (gameControlManager != null && gameControlManager == GameControlManager.s_instance)
            {
                // We're "in-game", so let the client know about the current song data.
                LoggerInstance.Msg("ClientConnected: Emitting OnSongStart");
                OnSongStart();
            } else
            {
                // Emit the "Return to Menu" event on connect.  This is to cover a state where
                // a websocket client lost a connection while playing and reconnected when it wasn't.
                // Prevents a state of a client showing song data when it shouldn't.
                LoggerInstance.Msg("ClientConnected: Emitting return to menu");
                EmitReturnToMenuEvent();
            }
            
        }

        public override async void OnApplicationQuit()
        {
            await server?.StopAsync();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            EventDataSceneChange sceneChangeEvent = new EventDataSceneChange(sceneName);
            Send(new SynthRidersEvent<EventDataSceneChange>("SceneChange", sceneChangeEvent));
        }

        public override void OnUpdate()
        {
            // If song is playing, check the last play time.  If it's advanced at least one second, emit an update.
            if (gameControlManager != null && gameControlManager == GameControlManager.s_instance)
            {
                if (gameControlManager.SongIsPlaying)
                {
                    this.currentPlayTimeMS = gameControlManager.PlayTimeMS;
                    if (currentPlayTimeMS - lastPlayTimeEventMS > 999)
                    {
                        EventDataPlayTime playTimeEvent = new EventDataPlayTime(currentPlayTimeMS);
                        Send(new SynthRidersEvent<EventDataPlayTime>("PlayTime", playTimeEvent));
                        this.lastPlayTimeEventMS = currentPlayTimeMS;
                    }
                }
                else
                {
                    this.lastPlayTimeEventMS = 0.0f;
                }
            }
        }

        public void EmitReturnToMenuEvent()
        {
            Send(new SynthRidersEvent<object>("ReturnToMenu", new object()));
        }

        public void GameManagerInit()
        {
            if (gameControlManager == GameControlManager.s_instance) return;
            gameControlManager = GameControlManager.s_instance;
            try
            {
                LoggerInstance.Msg("Adding stage events!");
                StageEvents stageEvents = new StageEvents();
                stageEvents.OnSongStart = new UnityEvent();
                stageEvents.OnSongStart.AddListener(new System.Action(() => OnSongStart()));
                stageEvents.OnSongEnd = new UnityEvent();
                stageEvents.OnSongEnd.AddListener(new System.Action(() => OnSongEnd()));
                stageEvents.OnNoteHit = new UnityEvent();
                stageEvents.OnNoteHit.AddListener(new System.Action(() => OnNoteHit()));
                stageEvents.OnNoteFail = new UnityEvent();
                stageEvents.OnNoteFail.AddListener(new System.Action(() => OnNoteFail()));
                stageEvents.OnEnterSpecial = new UnityEvent();
                stageEvents.OnEnterSpecial.AddListener(new System.Action(() => OnEnterSpecial()));
                stageEvents.OnCompleteSpecial = new UnityEvent();
                stageEvents.OnCompleteSpecial.AddListener(new System.Action(() => OnCompleteSpecial()));
                stageEvents.OnFailSpecial = new UnityEvent();
                stageEvents.OnFailSpecial.AddListener(new System.Action(() => OnFailSpecial()));
                GameControlManager.UpdateStageEventList(stageEvents);
            }
            catch (Exception e)
            {
                LoggerInstance.Msg(e.Message);
            }
        }
        
        private void OnSongStart()
        {
            // It'd be better to get this directly from within the game, but it seems
            // artwork isn't populated in the info provider.  This seems to work well enough
            // but do feel free to implement a better option if available.
            string albumArtPath = Directory.GetCurrentDirectory() + "\\SynthRidersUC\\SongStatusImage.png";
            string albumArtEncoded = null;

            if (File.Exists(albumArtPath))
            {
                albumArtEncoded = "data:image/png;base64," + System.Convert.ToBase64String(File.ReadAllBytes(albumArtPath));
            }

            var info = GameControlManager.s_instance.InfoProvider;
            if (info == null)
            {
                LoggerInstance.Msg("Null info provider, skipping OnSongStart");
                return;
            }

            EventDataSongStart songStartEvent = new EventDataSongStart(
                info.TrackName,
                info.CurrentDifficulty.ToString(),
                info.Author,
                info.Beatmapper,
                GameControlManager.CurrentTrackStatic.Song.clip.length,
                GameControlManager.CurrentTrackStatic.TrackBPM,
                albumArtEncoded,
                GameControlManager.NotLoseMode,
                GameControlManager.s_instance.SpinModeEnabled,
                Game_InfoProvider.SpinModeTypeToInt,
                Game_InfoProvider.SpinChallengeModeEnabled,
                GameControlManager.s_instance.SpiralModeEnabled,
                ((int)Game_InfoProvider.CurrentSpiralIntensity),
                Game_InfoProvider.VanishMode > 0 ? true : false,
                GameControlManager.AllowObstacles,
                GameControlManager.SpeedSelected,
                GameControlManager.SuddenDeath > 0 ? true : false,
                GameControlManager.RainbowMode,
                GameControlManager.NoteSize,
                GameControlManager.OneHandMode,
                GameControlManager.s_instance.IsExperienceStage,
                GameControlManager.NoteNeedBePunched,
                Game_InfoProvider.HoloMode
            );

            Send(new SynthRidersEvent<object>("SongStart", songStartEvent));
        }

        private void OnSongEnd()
        {
            var score = Game_ScoreManager.s_instance;
            if (score == null)
            {
                LoggerInstance.Msg("Null score manager, skipping OnSongEnd");
                return;
            }

            var info = GameControlManager.s_instance.InfoProvider;
            if (info == null)
            {
                LoggerInstance.Msg("Null info provider, skipping OnSongEnd");
                return;
            }

            EventDataSongEnd songEndEvent = new EventDataSongEnd(
                info.TrackName,
                info.TotalPerfectNotes,
                info.TotalNormalNotes,
                info.TotalBadNotes,
                info.TotalFailNotes,
                score.MaxCombo);

            Send(new SynthRidersEvent<EventDataSongEnd>("SongEnd", songEndEvent));
        }
        
        private void OnNoteHit()
        {
            var score = Game_ScoreManager.s_instance;
            if (score == null)
            {
                LoggerInstance.Msg("Null score manager, skipping OnNoteHit");
                return;
            }

            EventDataNoteHit noteHitEvent = new EventDataNoteHit(
                score.Score,
                score.CurrentCombo,
                score.NotesCompleted,
                score.TotalMultiplier,
                this.healthPercent,
                currentPlayTimeMS
            );

            Send(new SynthRidersEvent<EventDataNoteHit>("NoteHit", noteHitEvent));
        }
        
        private void OnNoteFail()
        {
            EventDataNoteMiss noteMissEvent = new EventDataNoteMiss(
                GameControlManager.s_instance.ScoreManager.TotalMultiplier,
                this.healthPercent,
                currentPlayTimeMS
            );

            Send(new SynthRidersEvent<EventDataNoteMiss>("NoteMiss", noteMissEvent));
        }

        private void OnEnterSpecial()
        {
            Send(new SynthRidersEvent<object>("EnterSpecial", new object()));
        }

        private void OnCompleteSpecial()
        {
            Send(new SynthRidersEvent<object>("CompleteSpecial", new object()));
        }

        private void OnFailSpecial()
        {
            Send(new SynthRidersEvent<object>("FailSpecial", new object()));
        }

        public void UpdateHealth(float health)
        {
            // Because the game might return a number greater than one (sometimes ex: 1.0004172),
            // We cap it at 1 for 100%.
            if (health > 1.0) {
                this.healthPercent = 1;
            } else if (health < 0.0)
            {
                this.healthPercent = 0;
            } else
            {
                this.healthPercent = health;
            }
        }

        public  void Send<T>(SynthRidersEvent<T> outputEvent)
        {
            webSocketServer?.QueueMessage(JsonConvert.SerializeObject(outputEvent));
        }
    }
}
