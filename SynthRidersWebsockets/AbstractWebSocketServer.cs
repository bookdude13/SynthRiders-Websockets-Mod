using Il2CppSynth.Versus;
using MelonLoader;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;
using EventWaitHandle = System.Threading.EventWaitHandle;

namespace SynthRidersWebsockets
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public string clientId { get; set; }
    }

    // Heavily inspired by https://stackoverflow.com/questions/30490140/how-to-work-with-system-net-websockets-without-asp-net
    internal abstract class AbstractWebSocketServer : IHostedService
    {
        protected readonly MelonLogger.Instance logger;
        protected bool isConnected = false;
        protected EventWaitHandle sendWait = new EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);
        protected Dictionary<string, WebSocket> clients = new();

        private readonly string url;
        private readonly HttpListener httpListener;

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public AbstractWebSocketServer(MelonLogger.Instance logger, string host, int port)
        {
            this.logger = logger;

            url = $"http://{host}:{port}/";
            httpListener = new();
            httpListener.Prefixes.Add(url);
            logger.Msg($"Listener created for '{url}'");
        }

        protected virtual void OnClientConnected(ClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.Msg("Starting http listener...");
                httpListener.Start();
                logger.Msg("Started");

                // Start looking for new client connections
                _ = Task.Run(async () =>
                {
                    await ConnectionLoop(cancellationToken);
                }, cancellationToken);

                // Set up sending to all connections
                _ = Task.Run(async () => await SendAllLoop(cancellationToken));

                logger.Msg("Done setting up server");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to start websocket at {url}", ex);
                return Task.FromException(ex);
            }
        }

        private async Task ConnectionLoop(CancellationToken cancellationToken)
        {
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
                    clients.Add(clientId, webSocket);
                    
                    logger.Msg($"Starting server receiver for new client {clientId} from {context.Request.RemoteEndPoint.Address}");
                    ClientConnectedEventArgs args = new ClientConnectedEventArgs();
                    args.clientId = clientId;
                    
                    // Trigger a 'connected' event here that the mod itself can choose to emit events for
                    // ex: when connected, emit the SongStart event so the client can get 'caught up' immediately.
                    this.OnClientConnected(args);

                    _ = Task.Run(async () => await ReceiveLoop(clientId, webSocket, cancellationToken));
                    logger.Msg($"Connection setup complete for client {clientId}");
                }
            }
        }

        private async Task SendAllLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //logger.Msg("Waiting for message");

                    // Wait until we have a message to send
                    sendWait.WaitOne();

                    //logger.Msg("Sending messages");
                    // Send all available messages
                    while (true)
                    {
                        var nextMessage = NextMessageToSend();
                        if (nextMessage == null)
                        {
                            sendWait.Reset();
                            //logger.Msg($"No more messages to send");
                            break;
                        }
                        else
                        {
                            foreach (var client in clients)
                            {
                                //logger.Msg($"Sending message to client {client.Key}");
                                await SendToClient(client.Key, nextMessage, cancellationToken);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Failure in send loop", ex);
                }
            }
        }

        private async Task SendToClient(string clientId, string message, CancellationToken cancellationToken)
        {
            var webSocket = clients[clientId];
            if (webSocket.State != WebSocketState.Open)
            {
                logger.Warning("Web socket not open, not sending message");
                return;
            }

            try
            {
                await webSocket.SendAsync(
                                Encoding.ASCII.GetBytes(message),
                                WebSocketMessageType.Text,
                                endOfMessage: true,
                                cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Error($"Failure to send message to client {clientId}", ex);
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

        abstract protected string NextMessageToSend();
        abstract protected void HandleReceive(string clientId, string message);
    }
}
