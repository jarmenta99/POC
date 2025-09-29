
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
namespace ServiceBusSession.Func;

public class QuerySvcBusSessionEnabled
{
    private readonly ILogger<QuerySvcBusSessionEnabled> _logger;

    public QuerySvcBusSessionEnabled(ILogger<QuerySvcBusSessionEnabled> logger)
    {
        _logger = logger;
    }

    [Function(nameof(QuerySvcBusSessionEnabled))]
    public async Task Run(
        [ServiceBusTrigger("sessiontestqueue",
        Connection = "CrewSharedServiceBusConnectionSessionId",
        IsSessionsEnabled = true)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        Console.WriteLine($"[SessionId: {message.SessionId}, SequNumber: {message.SequenceNumber}, {message.Body.ToString()}]");
        await messageActions.CompleteMessageAsync(message);
    }
}