namespace CloudPulse.Infrastructure.Configuration;

public class ConnectionSettings
{
    public required string QaConnectionString { get; set; }
    
    public required string TestConnectionString { get; set; }
}
