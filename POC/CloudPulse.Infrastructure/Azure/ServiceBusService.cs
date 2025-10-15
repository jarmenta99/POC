using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CloudPulse.Domain.Azure.Interfaces;
using CloudPulse.Domain.Enums;
using CloudPulse.Domain.Models;
using CloudPulse.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudPulse.Infrastructure.Azure;

public class ServiceBusService(IOptions<ConnectionSettings> connectionSettings, ILogger<ServiceBusService> logger) : IServiceBusService
{
    private readonly ConnectionSettings _connectionSettings = connectionSettings.Value ?? throw new ArgumentNullException(nameof(connectionSettings));
    private readonly ILogger<ServiceBusService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

    public async Task<IReadOnlyList<ServiceBusPeekedMessage>> PeekMessagesAsync(PeekMessageRequest peekMessageRequest)
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

            var results = await receiver.PeekMessagesAsync(peekMessageRequest.MaxMessages);

            // Project to DTO with Body as string
            var dtoResults = results.Select(m => new ServiceBusPeekedMessage
            {
                MessageId = m.MessageId,
                Message = m.Body.ToString(), // Converts BinaryData to string
                CorrelationId = m.CorrelationId ?? string.Empty,
                SessionId = m.SessionId,
                Subject = m.Subject,
                DeliveryCount = m.DeliveryCount,
                DeadLetterErrorDescription = m.DeadLetterErrorDescription,
                DeadLetterReason = m.DeadLetterReason,
                EnqueuedTime = m.EnqueuedTime.UtcDateTime,
                SequenceNumber = m.SequenceNumber
            }).ToList();

            return dtoResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error peeking messages");
            throw;
        }
    }

    public async Task DeleteMessagesAsync(DeleteMessageRequest deleteMessageRequest)
    {
        try
        {
            var busClient = GetServiceBusClient(deleteMessageRequest.AzureEnvironment);
            var receiver = busClient.CreateReceiver(
                deleteMessageRequest.TopicName,
                deleteMessageRequest.SubscriptionName,
                new ServiceBusReceiverOptions()
                {
                    SubQueue = deleteMessageRequest.DeadLetter ? SubQueue.DeadLetter : SubQueue.None
                }
            );

            var minSeqNumber = deleteMessageRequest.SequenceNumbers.Min();
            var maxSeqNumber = deleteMessageRequest.SequenceNumbers.Max();
            int totalMessages = (int)maxSeqNumber - (int)minSeqNumber + 1;

            var messages = await receiver.PeekMessagesAsync(totalMessages, minSeqNumber);
            var dictionary = messages.ToDictionary(m => m.SequenceNumber, m => m);

            foreach (var sequenceNumber in deleteMessageRequest.SequenceNumbers)
            {
                if (dictionary.TryGetValue(sequenceNumber, out var message))
                {
                    await receiver.CompleteMessageAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting messages");
            throw;
        }
    }
}