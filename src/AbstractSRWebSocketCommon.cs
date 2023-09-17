using MelonLoader;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthRidersWebsockets
{
    internal abstract class AbstractSRWebSocketCommon
    {
        protected MelonLogger.Instance logger;
        protected bool isConnected = false;
        protected HubConnection connection;

        private string url;

        public AbstractSRWebSocketCommon(MelonLogger.Instance logger, string host, int port)
        {
            this.logger = logger;
            url = $"ws://{host}:{port}/";
            connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            connection.Reconnecting += async (exception) =>
            {
                logger.Msg($"Reconnecting ({exception?.Message})");
                await Task.CompletedTask;
            };
            connection.Reconnected += async (connectionId) =>
            {
                logger.Msg($"Reconnected. New connection id: {connectionId}");
                await Task.CompletedTask;
            };
            connection.Closed += async exception =>
            {
                isConnected = false;
                logger.Msg($"Websocket closed ({exception?.Message ?? "intentionally closed"})");
                await Task.CompletedTask;
            };
        }

        public async Task<bool> StartAsync()
        {
            try
            {
                await connection.StartAsync();
                isConnected = true;
                logger.Msg($"Started websocket at {url}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to start websocket at {url}", ex);
                return false;
            }
        }

        public async Task<bool> StopAsync()
        {
            if (connection == null) return true;
            if (!isConnected) return true;

            isConnected = false;
            try
            {
                await connection.DisposeAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to stop websocket!", ex);
                return false;
            }
        }
    }
}
