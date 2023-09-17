using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;

namespace SynthRidersWebsockets
{
    internal class Raw_SREventsWebSocketServer : WebSocketServer
    {
        private readonly ConcurrentQueue<string> messagesToSend = new();

        public Raw_SREventsWebSocketServer(MelonLogger.Instance logger, string host, int port)
            : base(logger, host, port)
        {
        }

        protected override void HandleReceive(string clientId, string message)
        {
            // No server-side behaviors from sent messages at this time, so just log it if it happens
            logger.Msg($"Client {clientId} received message '{message}'");
        }

        public void QueueMessage(string message)
        {
            messagesToSend.Enqueue(message);

            // Let the sender pick it up
            sendWait.Set();
        }

        protected override string NextMessageToSend(string clientId)
        {
            if (messagesToSend.IsEmpty)
            {
                return null;
            }

            if (!messagesToSend.TryDequeue(out var message))
            {
                logger.Warning("Failed to dequeue message!");
                return null;
            }

            return message;
        }
    }
}
