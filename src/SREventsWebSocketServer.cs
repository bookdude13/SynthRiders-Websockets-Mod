using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Threading;
using MelonLoader;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace SynthRidersWebsockets
{
    internal class SREventsWebSocketServer : AbstractSRWebSocketCommon
    {
        public static readonly string EventMessageChannelName = "ReceiveMessage";

        public SREventsWebSocketServer(MelonLogger.Instance logger, string host, int port)
            : base(logger, host, port)
        {
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    // Clients register for "ReceiveMessage" and we send to that channel
                    await connection.SendAsync(EventMessageChannelName, message);
                }
                else
                {
                    logger.Warning($"Not connected yet ({connection.State}), cannot send message '{message}'");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to send message '{message}'", ex);
            }
        }
    }
}
