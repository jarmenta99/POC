using CloudPulse.Domain.Azure.Interfaces;
using CloudPulse.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CloudPulse.AzFunction.Functions;

public class ServiceBusTopicsFunction
{
    private readonly ILogger<ServiceBusTopicsFunction> _logger;
    private readonly IServiceBusService _serviceBusService;

    public ServiceBusTopicsFunction(ILogger<ServiceBusTopicsFunction> logger, IServiceBusService serviceBusService)
    {
        _logger = logger;
        _serviceBusService = serviceBusService;
    }

    [Function("Topics")]
    public async Task<HttpResponseData> GetTopics([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation($"Topics requested");

            // Get AzureEnvironment from query string, default to Qa if not provided
            var envStr = req.Query["environment"];
            AzureEnvironment environment = AzureEnvironment.Qa;
            if (!string.IsNullOrWhiteSpace(envStr) && Enum.TryParse(envStr, true, out AzureEnvironment parsedEnv))
            {
                environment = parsedEnv;
            }

            var results = await _serviceBusService.GetServiceBusTopicsResultAsync(environment);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(results);

            _logger.LogInformation($"Topics processed");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTopics function");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            
            return errorResponse;
        }
    }
}