using CloudPulse.Domain.Enums;
using CloudPulse.Domain.Models;

namespace CloudPulse.Domain.Azure.Interfaces;

public interface IServiceBusService
{
    Task<ServiceBusTopicsResult> GetServiceBusTopicsResultAsync(AzureEnvironment azureEnvironment);
    Task<IReadOnlyList<ServiceBusPeekedMessage>> PeekMessagesAsync(PeekMessageRequest peekMessageRequest);
    Task DeleteMessagesAsync(PeekMessageRequest peekMessageRequest, IEnumerable<string> messageIds);
}
