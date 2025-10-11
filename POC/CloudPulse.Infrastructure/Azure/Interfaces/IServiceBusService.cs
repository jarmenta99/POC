using Azure.Messaging.ServiceBus;
using CloudPulse.Domain.Enums;
using CloudPulse.Domain.Models;

namespace CloudPulse.Domain.Azure.Interfaces;

public interface IServiceBusService
{
    Task<ServiceBusTopicsResult> GetServiceBusTopicsResultAsync(AzureEnvironment azureEnvironment);
    Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekMessagesAsync(PeekMessageRequest peekMessageRequest);
    Task DeleteMessagesAsync(PeekMessageRequest peekMessageRequest, IEnumerable<string> messageIds);
}
