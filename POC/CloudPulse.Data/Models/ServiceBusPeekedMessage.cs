namespace CloudPulse.Domain.Models;

public class ServiceBusPeekedMessage
{
    public required string Message { get; set; }

    public required string MessageId { get; set; }

    public string? SessionId { get; set; }

    public long SequenceNumber { get; set; }

    public required string CorrelationId { get; set; }

    public string? Subject { get; set; }

    public int DeliveryCount { get; set; }

    public DateTime EnqueuedTime { get; set; }

    public string? DeadLetterReason { get; set; }

    public string? DeadLetterErrorDescription { get; set; }
}
