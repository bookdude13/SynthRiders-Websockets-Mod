using MelonLoader;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;
using EventWaitHandle = System.Threading.EventWaitHandle;

namespace SynthRidersWebsockets
{
    // Heavily inspired by https://stackoverflow.com/questions/30490140/how-to-work-with-system-net-websockets-without-asp-net
    internal abstract class AbstractWebSocketClient : IHostedService
    {
        protected readonly MelonLogger.Instance logger;
        protected bool isConnected = false;
        protected EventWaitHandle sendWait = new EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

        private readonly Uri uri;
        private readonly ClientWebSocket client;

        public AbstractWebSocketClient(MelonLogger.Instance logger, string host, int port)
        {
            this.logger = logger;

            var builder = new UriBuilder
            {
                Scheme = "ws",
                Host = host,
                Port = port,
                Path = "/",
            };
            uri = builder.Uri;

            logger.Msg("Creating client web socket");
            client = new ClientWebSocket();
            // TODO maybe tweak options, keepalive, etc.
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.Msg($"Connecting client to websocket");
                await client.ConnectAsync(uri, cancellationToken);

                logger.Msg($"Starting handlers for client");
                _ = Task.Run(async () => await ReceiveLoop(client, cancellationToken));
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to start websocket client at {uri}", ex);
                await Task.FromException(ex);
            }
        }

        private async Task ReceiveLoop(ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];
            var stringBuilder = new StringBuilder(2048);
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                WebSocketReceiveResult receiveResult =
                    await webSocket.ReceiveAsync(buffer, cancellationToken);
                if (receiveResult.Count == 0)
                    return;

                stringBuilder.Append(Encoding.ASCII.GetString(buffer, 0, receiveResult.Count));
                if (receiveResult.EndOfMessage)
                {
                    HandleReceive(stringBuilder.ToString());
                    stringBuilder = new StringBuilder();
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Msg("Stopping websocket server...");
            if (client == null)
            {
                await Task.CompletedTask;
            }

            if (client.State != WebSocketState.Open || client.State != WebSocketState.Connecting)
            {
                logger.Msg("Client not open and not connecting; skipping close");
                await Task.CompletedTask;
            }

            try
            {
                logger.Msg("Stopping client...");
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal stop", cancellationToken);
                logger.Msg("Stopped client");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to stop websocket server!", ex);
                await Task.FromException(ex);
            }
        }

        abstract protected void HandleReceive(string message);
    }
}
