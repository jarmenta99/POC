
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

    //Gets messages from a session enabled queue
    /*[Function(nameof(QuerySvcBusSessionEnabled))]
    public async Task Run(
        [ServiceBusTrigger("sessiontestqueue",
        Connection = "CrewSharedServiceBusConnectionSessionId",
        IsSessionsEnabled = true)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        Console.WriteLine($"[SessionId: {message.SessionId}, SequNumber: {message.SequenceNumber}, {message.Body.ToString()}]");
        await messageActions.CompleteMessageAsync(message);
    }*/

    [Function(nameof(QuerySvcBusSessionEnabledSubscription))]
    public async Task QuerySvcBusSessionEnabledSubscription(
        [ServiceBusTrigger(
            "feedtransactionmanager",
            "jgtestSessionEnabled",
            Connection = "ServiceBusConnectionString",
            IsSessionsEnabled = true)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        Console.WriteLine($"[SequNumber: {message.SequenceNumber}, {message.Body.ToString()}]");
        await messageActions.CompleteMessageAsync(message);
    }
}