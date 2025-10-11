namespace CloudPulse.Domain.Models;

public class ServiceBusSubscriptionInfo
{
    public required string SubscriptionName { get; set; }

    public bool IsSessionEnabled { get; set; }

    public long ActiveMessageCount { get; set; }
    
    public long DeadLetterMessageCount { get; set; }
}
