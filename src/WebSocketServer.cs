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
    internal abstract class WebSocketServer : IHostedService
    {
        protected readonly MelonLogger.Instance logger;
        protected bool isConnected = false;
        protected EventWaitHandle sendWait = new EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

        private readonly string url;
        private readonly HttpListener httpListener;

        public WebSocketServer(MelonLogger.Instance logger, string host, int port)
        {
            this.logger = logger;

            url = $"http://{host}:{port}/";
            httpListener = new();
            httpListener.Prefixes.Add(url);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.Msg("Starting http listener...");
                httpListener.Start();
                logger.Msg("Started");
                while (!cancellationToken.IsCancellationRequested)
                {
                    var context = await httpListener.GetContextAsync()
                        .WithCancellationToken(cancellationToken);
                    if (context is null)
                    {
                        logger.Warning("Null listener context; exiting main loop");
                        return;
                    }

                    if (!context.Request.IsWebSocketRequest)
                    {
                        logger.Warning("Not a websocket request; ignoring");
                        context.Response.Abort();
                    }
                    else
                    {
                        logger.Msg("Accepting websocket request");
                        var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null)
                            .WithCancellationToken(cancellationToken);

                        if (webSocketContext is null)
                        {
                            logger.Warning("Websocket context is null, cannot connect");
                            continue;
                        }

                        string clientId = Guid.NewGuid().ToString();
                        WebSocket webSocket = webSocketContext.WebSocket;

                        logger.Msg($"Starting handlers for new client {clientId}");
                        _ = Task.Run(async () => await SendLoop(clientId, webSocket, cancellationToken));
                        _ = Task.Run(async () => await ReceiveLoop(clientId, webSocket, cancellationToken));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to start websocket at {url}", ex);
            }
        }

        private async Task SendLoop(string clientId, WebSocket webSocket, System.Threading.CancellationToken cancellationToken)
        {
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Wait until we have a message to send
                    sendWait.WaitOne();

                    // Send all available messages
                    while (true)
                    {
                        var nextMessage = NextMessageToSend(clientId);
                        if (nextMessage == null)
                        {
                            sendWait.Reset();
                            logger.Msg($"No more messages to send");
                            break;
                        }
                        else
                        {
                            logger.Msg($"Sending message '{nextMessage}' to client {clientId}");
                            await webSocket.SendAsync(
                                Encoding.ASCII.GetBytes(nextMessage),
                                WebSocketMessageType.Text,
                                endOfMessage: true,
                                cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failure in send loop for client {clientId}", ex);
                }
            }
        }

        private async Task ReceiveLoop(string clientId, WebSocket webSocket, CancellationToken cancellationToken)
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
                    HandleReceive(clientId, stringBuilder.ToString());
                    stringBuilder = new StringBuilder();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Msg("Stopping websocket server...");
            if (httpListener == null)
                return Task.CompletedTask;
            
            if (!isConnected)
                return Task.CompletedTask;

            isConnected = false;
            try
            {
                logger.Msg("Stopping listener...");
                httpListener.Stop();
                logger.Msg("Stopped listener");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to stop websocket server!", ex);
            }

            return Task.CompletedTask;
        }

        abstract protected string NextMessageToSend(string clientId);
        abstract protected void HandleReceive(string clientId, string message);
    }
}
