using Azure.Core.Serialization;
using CloudPulse.Domain.Azure.Interfaces;
using CloudPulse.Infrastructure.Azure;
using CloudPulse.Infrastructure.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Services
    .AddApplicationInsightsTelemetryWorkerService(options =>
    {
        options.EnableDependencyTrackingTelemetryModule = true;
        options.EnablePerformanceCounterCollectionModule = true;
        options.ConnectionString = Environment.GetEnvironmentVariable("AppInsights_ConnStr");
    })
    .ConfigureFunctionsApplicationInsights();

builder.Services.Configure<WorkerOptions>(options =>
{
    options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
    {
        PropertyNamingPolicy = null // This keeps PascalCase
    });
});

builder.Services.Configure<ConnectionSettings>(options =>
{
    // Get logger from DI and log the exception
    var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("Startup");
    
    try
    {
        options.QaConnectionString = Environment.GetEnvironmentVariable("CrewSharedServiceBusConnectionQA") ?? throw new Exception("CrewSharedServiceBusConnectionQA environment variable is not set");
    } catch (Exception ex)
    {
        logger.LogError(ex, "Error setting CrewSharedServiceBusConnectionQA");
        Console.WriteLine(ex.Message);
    }

    try
    {
        options.TestConnectionString = Environment.GetEnvironmentVariable("CrewSharedServiceBusConnectionTest") ?? throw new Exception("CrewSharedServiceBusConnectionTest environment variable is not set");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error setting CrewSharedServiceBusConnectionTest");
        Console.WriteLine(ex.Message);
    }
});

builder.Services.AddSingleton<IServiceBusService, ServiceBusService>();

await builder.Build().RunAsync();
