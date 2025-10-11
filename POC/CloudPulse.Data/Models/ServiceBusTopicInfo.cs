namespace CloudPulse.Domain.Models;

public class ServiceBusTopicInfo
{
    public required string TopicName { get; set; }
    public required List<ServiceBusSubscriptionInfo> Subscriptions { get; set; }
}
