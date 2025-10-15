using CloudPulse.Domain.Enums;

namespace CloudPulse.Domain.Models;

public class DeleteMessageRequest
{
    public AzureEnvironment AzureEnvironment { get; set; }
    
    public required string TopicName { get; set; }
    
    public required string SubscriptionName { get; set; }
    public bool IsSessionEnabled { get; set; }
    public IEnumerable<long> SequenceNumbers { get; set; } = Enumerable.Empty<long>();

    public bool DeadLetter { get; set; } = false;
}
