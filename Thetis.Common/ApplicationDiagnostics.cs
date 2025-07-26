using System.Diagnostics.Metrics;

namespace Thetis.Common;

public static class ApplicationDiagnostics
{
    public const string ServiceName = "thetis-web";
    public static readonly Meter Meter = new(ServiceName);
    
    public static Counter<long> UserLoginsCounter = Meter.CreateCounter<long>("user.logins");
}