using Microsoft.AspNetCore.Http;
using Serilog;

namespace Demo.Common.Logging;

public class EnrichDiagnosticContext
{
    public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        //var request = httpContext.Request;

        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    }
}