using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Serialization.Json;

namespace Demo.Portal.Helper;

public interface IApiClient
{
    T Query<T>(string enpoint, object req, string access_token);
}

public abstract class ApiClient : IApiClient
{
    private readonly ILogger<ApiClient> _logger;
    private readonly SslClientSetting _ssl;

    public ApiClient(
        ILogger<ApiClient> logger,
        IOptions<SslClientSetting> ssl
    )
    {
        _logger = logger;
        _ssl = ssl.Value;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enpoint"></param>
    /// <param name="req"></param>
    /// <param name="access_token"></param>
    /// <returns></returns>
    public T Query<T>(string enpoint, object req, string access_token)
    {
        ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;

        var reqParam = "";
        var serializer = new JsonSerializer();
        try
        {
            reqParam = serializer.Serialize(req);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Cannot serialize parameter: {ex.Message}", ex);
            return default;
        }

        RestRequest request = new(Method.POST)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(req);
        request.AddHeader("Authorization", $"Bearer {access_token}");
        request.AddHeader("Client-Agent", "Web");

        RestClient client = new(enpoint)
        {
            //ClientCertificates = new X509CertificateCollection() { certificates }
        };
        IRestResponse response = null;
        try
        {
            response = client.Execute(request);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connect gateway error: {ex.Message}", ex);
        }

        if (response == null || response.ErrorException != null)
        {
            _logger.LogError("Service return null response");
            return default;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError($"Status Code={response.StatusCode}. Status Content: {response.Content}");
            return default;
        }

        var respContent = response.Content;
        try
        {
            return serializer.Deserialize<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError("Service return error: " + ex.Message, ex);
            return default;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="cert"></param>
    /// <param name="chain"></param>
    /// <param name="policyErrors"></param>
    /// <returns></returns>
    public static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain,
        SslPolicyErrors policyErrors)
    {
        var result = true;

        return result;
    }

    public class ApiResponse
    {
        //public Guid Code { get; set; }
        public int Code { get; set; }
        public string CodeDesc { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }
    }
}