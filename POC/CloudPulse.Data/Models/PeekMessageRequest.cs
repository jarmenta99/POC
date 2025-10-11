using CloudPulse.Domain.Enums;

namespace CloudPulse.Domain.Models;

public class PeekMessageRequest
{
    public AzureEnvironment AzureEnvironment { get; set; }
    
    public required string TopicName { get; set; }
    
    public required string SubscriptionName { get; set; }
    
    public int MaxMessages { get; set; }
    
    public bool DeadLetter { get; set; } = false;
}
