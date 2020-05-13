﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;
using SampleApp.Shared.AzureAssets.Abstraction;
using SampleApp.Shared.ProcessEntities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Shared.AzureAssets
{
    public class ServiceBusFactory : IServiceBusFactory
    {
        static IQueueClient queueClient;
        public async Task SendMessage<T>(T message, AzureMessageContext context)
        {
            try
            {
                var mgmtQueueClient = new ManagementClient(context.ConnectionString);

                var queues = await mgmtQueueClient.GetQueuesAsync();

                if (queues.Count == 0 ||
                    (!queues.Any(x => x.Path == context.QueueOrTopicName.ToLower())))
                {
                    await mgmtQueueClient.CreateQueueAsync(context.QueueOrTopicName);
                }

                queueClient = new QueueClient(context.ConnectionString, context.QueueOrTopicName);

                queueClient.ServiceBusConnection.TransportType = TransportType.AmqpWebSockets;

                var sbMessage = FormatMessage(message);

                queueClient.SendAsync(sbMessage).Wait();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Message FormatMessage<T>(T queueMessage)
        {
            Message message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(queueMessage)))
            {
                MessageId = Guid.NewGuid().ToString(),
                SessionId = Guid.NewGuid().ToString()
            };

            return message;
        }
    }
}
