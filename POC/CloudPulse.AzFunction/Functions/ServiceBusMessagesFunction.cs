using CloudPulse.Domain.Azure.Interfaces;
using CloudPulse.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CloudPulse.AzFunction.Functions;

public class ServiceBusMessagesFunction
{
    private readonly ILogger<ServiceBusMessagesFunction> _logger;
    private readonly IServiceBusService _serviceBusService;

    public ServiceBusMessagesFunction(ILogger<ServiceBusMessagesFunction> logger, IServiceBusService serviceBusService)
    {
        _logger = logger;
        _serviceBusService = serviceBusService;
    }

    [Function("Messages")]
    public async Task<HttpResponseData> GetMessages([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<PeekMessageRequest>(requestBody);
            
            if (data == null)
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid request body" });
                return badResponse;
            }

            var messages = await _serviceBusService.PeekMessagesAsync(data);
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(messages);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMessages function");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }
}