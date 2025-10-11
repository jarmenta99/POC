using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CloudPulse.Domain.Azure.Interfaces;
using CloudPulse.Domain.Enums;
using CloudPulse.Domain.Models;
using CloudPulse.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CloudPulse.Infrastructure.Azure;

public class ServiceBusService(IOptions<ConnectionSettings> connectionSettings) : IServiceBusService
{
    private readonly ConnectionSettings _connectionSettings = connectionSettings.Value ?? throw new ArgumentNullException(nameof(connectionSettings));

    private string ResolveConnectionString(AzureEnvironment azureEnvironment)
    {
        return azureEnvironment switch
        {
            AzureEnvironment.Test => _connectionSettings.TestConnectionString,
            AzureEnvironment.Qa => _connectionSettings.QaConnectionString,
            _ => throw new ArgumentOutOfRangeException(nameof(azureEnvironment), "Unsupported Azure environment")
        };
    }

    private ServiceBusAdministrationClient GetServiceBusClientAdmin(AzureEnvironment azureEnvironment)
    {
        string connectionString = ResolveConnectionString(azureEnvironment);

        return new ServiceBusAdministrationClient(connectionString);
    }

    private ServiceBusClient GetServiceBusClient(AzureEnvironment azureEnvironment)
    {
        var connectionString = ResolveConnectionString(azureEnvironment);

        var svcOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };

        return new ServiceBusClient(connectionString, svcOptions);
    }

    public async Task<ServiceBusTopicsResult> GetServiceBusTopicsResultAsync(AzureEnvironment azureEnvironment)
    {
        var adminClient = GetServiceBusClientAdmin(azureEnvironment);
        var results = new ServiceBusTopicsResult
        {
            Topics = new List<ServiceBusTopicInfo>()
        };

        await foreach (var topicProperties in adminClient.GetTopicsAsync())
        {
            var topicInfo = new ServiceBusTopicInfo
            {
                TopicName = topicProperties.Name,
                Subscriptions = new List<ServiceBusSubscriptionInfo>()
            };

            // Get subscriptions for the topic
            await foreach (var subscriptionProperties in adminClient.GetSubscriptionsAsync(topicProperties.Name))
            {
                // Get runtime properties for the subscription (includes message counts)
                var runtimeProps = await adminClient.GetSubscriptionRuntimePropertiesAsync(topicProperties.Name, subscriptionProperties.SubscriptionName);

                topicInfo.Subscriptions.Add(new ServiceBusSubscriptionInfo
                {
                    SubscriptionName = subscriptionProperties.SubscriptionName,
                    IsSessionEnabled = subscriptionProperties.RequiresSession,
                    ActiveMessageCount = runtimeProps.Value.ActiveMessageCount,
                    DeadLetterMessageCount = runtimeProps.Value.DeadLetterMessageCount
                });
            }

            topicInfo.Subscriptions = topicInfo.Subscriptions.OrderBy(s => s.SubscriptionName).ToList();

            results.Topics.Add(topicInfo);
        }

        results.Topics = results.Topics.OrderBy(t => t.TopicName).ToList();

        return results;
    }

    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekMessagesAsync(PeekMessageRequest peekMessageRequest)
    {
        try
        {
            var busClient = GetServiceBusClient(peekMessageRequest.AzureEnvironment);
            var receiver = busClient.CreateReceiver(
                peekMessageRequest.TopicName,
                peekMessageRequest.SubscriptionName,
                new ServiceBusReceiverOptions()
                {
                    SubQueue = peekMessageRequest.DeadLetter ? SubQueue.DeadLetter : SubQueue.None
                }
            );

            return await receiver.PeekMessagesAsync(peekMessageRequest.MaxMessages);
        }
        catch (Exception ex)
        {
            // Log the exception (consider using a logging framework)
            Console.WriteLine($"Error peeking messages: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteMessagesAsync(PeekMessageRequest peekMessageRequest, IEnumerable<string> messageIds)
    {
        try
        {
            var busClient = GetServiceBusClient(peekMessageRequest.AzureEnvironment);
            var receiver = busClient.CreateReceiver(
                peekMessageRequest.TopicName,
                peekMessageRequest.SubscriptionName,
                new ServiceBusReceiverOptions()
                {
                    SubQueue = peekMessageRequest.DeadLetter ? SubQueue.DeadLetter : SubQueue.None
                }
            );

            foreach (var messageId in messageIds)
            {
                var messages = await receiver.PeekMessagesAsync(100000);
                var message = messages.FirstOrDefault(m => m.MessageId == messageId);
                if (message != null)
                {
                    await receiver.CompleteMessageAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception (consider using a logging framework)
            Console.WriteLine($"Error peeking messages: {ex.Message}");
            throw;
        }
    }
}