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
    internal class SREventsWebSocketServer : AbstractWebSocketServer
    {
        private readonly ConcurrentQueue<string> messagesToSend = new();

        public SREventsWebSocketServer(MelonLogger.Instance logger, string host, int port)
            : base(logger, host, port)
        {
        }

        protected override void HandleReceive(string clientId, string message)
        {
            // No server-side behaviors from sent messages at this time, so just log it if it happens
            logger.Msg($"Server received message '{message}' from client '{clientId}'");
        }

        public void QueueMessage(string message)
        {
            // In case we use this mod without any clients, limit the size of the queue
            if (messagesToSend.Count > 100)
            {
                logger.Warning("Server message queue full; do you have a client receiving messages? Ignoring new message.");
            }
            else
            {
                messagesToSend.Enqueue(message);
            }

            // Let the client receive messages
            sendWait.Set();
        }

        protected override string NextMessageToSend()
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
