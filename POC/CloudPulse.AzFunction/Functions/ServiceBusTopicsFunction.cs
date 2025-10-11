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

    [Function("GetTopics")]
    public async Task<HttpResponseData> GetTopics([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        try
        {
            /*
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UserInput>(requestBody);
            */
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var results = await _serviceBusService.GetServiceBusTopicsResultAsync(AzureEnvironment.Qa);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(results);
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