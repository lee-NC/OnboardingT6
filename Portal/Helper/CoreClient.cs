using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace Demo.Portal.Helper;

public interface ICoreClient
{
    ApiRespoinse Query(string? accessToken, string uri, object parameter, out string message, Method method);
    string QueryStr(string accessToken, string uri, object parameter, out string message, Method method);

    ApiRespoinse QueryDataFromApi(string uri, object parameter, object loginInfo, string stringLoginInfo,
        out string message);

    T? QueryDataFromApi<T>(string uri, object parameter, object loginInfo, string stringLoginInfo,
        out string message);

    string QueryData(string uri, object parameter, object loginInfo, string stringLoginInfo, out string message);

    ApiRespoinse QueryDataBase64(string uri, object parameter, string stringLoginInfo, out string message);
    string QueryStrBase64(string uri, object parameter, string stringLoginInfo, out string message);

    ApiRespoinse QueryNoToken(string uri, object parameter, out string message);
    T QueryNoToken<T>(string uri, object parameter, out string message) where T : class;
    string QueryStrNoToken(string uri, object parameter, out string message);

    string QueryNoTokenWithFile(string username, string password, string uri, byte[] file, string contentType,
        out string message);

    byte[] SignTsq(string username, string password, string uri, byte[] file, string contentType,
        out string message);
}

public class CoreClient : ICoreClient
{
    private readonly ILogger<CoreClient> _log;
    private readonly CoreApiSetting _setting;

    public CoreClient(
        IOptions<CoreApiSetting> setting,
        ILogger<CoreClient> logger
    )
    {
        _setting = setting?.Value;
        _log = logger;
    }

    /// <summary>
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="uri"></param>
    /// <param name="parameter"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public ApiRespoinse Query(string accessToken, string uri, object parameter, out string message, Method method)
    {
        message = "";
        var respContent = QueryStr(accessToken, uri, parameter, out message, method);
        if (respContent == null) return null;
        try
        {
            return JsonConvert.DeserializeObject<ApiRespoinse>(respContent);
        }
        catch (Exception)
        {
            return new ApiRespoinse
            {
                Code = 500,
                Message = "No response"
            };
        }
    }


    public ApiRespoinse QueryDataFromApi(string uri, object parameter, object loginInfo, string stringLoginInfo,
        out string message)
    {
        message = "";
        var respContent = QueryData(uri, parameter, loginInfo, stringLoginInfo, out message);
        if (respContent == null) return null;
        try
        {
            var resp = JsonConvert.DeserializeObject<ApiRespoinse>(respContent);
            return resp;
        }
        catch (Exception)
        {
            return new ApiRespoinse
            {
                Code = 500,
                Message = "No response"
            };
        }
    }

    public ApiRespoinse QueryDataBase64(string uri, object parameter, string stringLoginInfo, out string message)
    {
        message = "";
        var respContent = QueryStrBase64(uri, parameter, stringLoginInfo, out message);
        if (respContent == null) return null;
        try
        {
            var resp = JsonConvert.DeserializeObject<ApiRespoinse>(respContent);
            return resp;
        }
        catch (Exception)
        {
            return new ApiRespoinse
            {
                Code = 500,
                Message = "No response"
            };
        }
    }

    public ApiRespoinse QueryNoToken(string uri, object parameter, out string message)
    {
        message = "";
        var respContent = QueryStrNoToken(uri, parameter, out message);
        if (respContent == null) return null;
        try
        {
            var resp = JsonConvert.DeserializeObject<ApiRespoinse>(respContent);
            return resp;
        }
        catch (Exception)
        {
            return new ApiRespoinse
            {
                Code = 500,
                Message = "No response"
            };
        }
    }

    public string QueryStr(string accessToken, string uri, object parameter, out string message, Method method)
    {
        message = "";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        var client = new RestClient(_setting?.UrlPrefix);
        IRestRequest request = new RestRequest(uri, method, DataFormat.Json);
        request.AddHeader("Content-Type", "application/json; CHARSET=UTF-8");
        request.AddHeader("Authorization", accessToken);
        if (!method.Equals(Method.GET)) request.AddJsonBody(parameter);
        IRestResponse response;

        response = client.Execute(request);
        if (response == null || response.ErrorException != null)
        {
            _log.LogError("Query error {@r}", response);
            message = "Service return null response";
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            message = $"Status Code={response.StatusCode}. Status Content: {response.Content}";
            _log.LogError(message);
            return null;
        }

        return response.Content;
    }


    public string QueryData(string uri, object parameter, object loginInfo, string stringLoginInfo,
        out string message)
    {
        message = "";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        var userPass = stringLoginInfo;
        var plainTextBytes = Encoding.UTF8.GetBytes(userPass);
        var base64UserPass = Convert.ToBase64String(plainTextBytes);

        var client = new RestClient(_setting?.UrlPrefix);
        IRestRequest request = new RestRequest(uri, Method.POST, DataFormat.Json);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic " + base64UserPass);
        request.AddJsonBody(parameter);
        IRestResponse response;

        response = client.Execute(request);
        if (response == null || response.ErrorException != null)
        {
            _log.LogError("Query error {@r}", response);
            message = "Service return null response";
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            message = $"Status Code={response.StatusCode}. Status Content: {response.Content}";
            _log.LogError(message);
            return null;
        }

        return response.Content;
    }

    public string QueryStrBase64(string uri, object parameter, string stringLoginInfo, out string message)
    {
        message = "";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        var userPass = stringLoginInfo;
        var plainTextBytes = Encoding.UTF8.GetBytes(userPass);
        var base64UserPass = Convert.ToBase64String(plainTextBytes);

        var client = new RestClient(_setting?.UrlPrefix);
        IRestRequest request = new RestRequest(uri, Method.POST, DataFormat.Json);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic " + base64UserPass);
        request.AddJsonBody(parameter);
        IRestResponse response;

        response = client.Execute(request);
        if (response == null || response.ErrorException != null)
        {
            _log.LogError("Query error {@r}", response);
            message = "Service return null response";
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            message = $"Status Code={response.StatusCode}. Status Content: {response.Content}";
            _log.LogError(message);
            return null;
        }

        return response.Content;
    }

    public string QueryStrNoToken(string uri, object parameter, out string message)
    {
        message = "";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        var client = new RestClient(_setting?.UrlPrefix);
        IRestRequest request = new RestRequest(uri, Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(parameter);
        IRestResponse response;

        response = client.Execute(request);
        if (response == null || response.ErrorException != null)
        {
            _log.LogError("Query error {@r}", response);
            message = "Service return null response";
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            message = $"Status Code={response.StatusCode}. Status Content: {response.Content}";
            _log.LogError(message);
            return null;
        }

        return response.Content;
    }

    public string QueryNoTokenWithFile(string username, string password, string uri, byte[] file,
        string contentType, out string message)
    {
        message = "";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        var userPass = username + ":" + password;
        var plainTextBytes = Encoding.UTF8.GetBytes(userPass);
        var base64UserPass = Convert.ToBase64String(plainTextBytes);

        var client = new RestClient(_setting?.UrlPrefix);
        IRestRequest request = new RestRequest(uri, Method.POST);
        request.AddHeader("Content-Type", contentType);
        request.AddHeader("Authorization", "Basic " + base64UserPass);
        request.AddParameter(contentType, file, ParameterType.RequestBody);

        IRestResponse response;

        response = client.Execute(request);
        if (response == null || response.ErrorException != null)
        {
            _log.LogError("Query error {@r}", response);
            message = "Service return null response";
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            message = $"Status Code={response.StatusCode}. Status Content: {response.Content}";
            _log.LogError(message);
            return null;
        }

        return response.Content;
    }

    public byte[] SignTsq(string? username, string? password, string uri, byte[] file, string contentType,
        out string message)
    {
        message = "";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        var userPass = username + ":" + password;
        var plainTextBytes = Encoding.UTF8.GetBytes(userPass);
        var base64UserPass = Convert.ToBase64String(plainTextBytes);

        var client = new RestClient(_setting?.UrlPrefix);
        IRestRequest request = new RestRequest(uri, Method.POST);
        request.AddHeader("Content-Type", contentType);
        request.AddHeader("Authorization", "Basic " + base64UserPass);
        //request.AddFile("timestamp_request.tsp", file, "timestamp_request.tsp", "application/timestamp-query");
        request.AddParameter(contentType, file, ParameterType.RequestBody);
        //request.AddFileBytes("timestamp_request.tsp", file, "timestamp_request.tsp", "application/timestamp-query");

        IRestResponse response;

        response = client.Execute(request);
        if (response == null || response.ErrorException != null)
        {
            _log.LogError("Query error {@r}", response);
            message = "Service return null response";
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            message = $"Status Code={response.StatusCode}. Status Content: {response.Content}";
            _log.LogError(message);
            return null;
        }

        return response.RawBytes;
    }

    public T QueryNoToken<T>(string uri, object parameter, out string message) where T : class
    {
        message = "";
        var respContent = QueryStrNoToken(uri, parameter, out message);
        if (respContent == null) return null;

        try
        {
            var resp = JsonConvert.DeserializeObject<T>(respContent);
            return resp;
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    /// <param name="parameter"></param>
    /// <param name="loginInfo"></param>
    /// <param name="stringLoginInfo"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public T? QueryDataFromApi<T>(string uri, object parameter, object loginInfo, string stringLoginInfo,
        out string message)
    {
        var respContent = QueryData(uri, parameter, loginInfo, stringLoginInfo, out message);
        if (respContent == null) return default;

        try
        {
            return JsonConvert.DeserializeObject<T>(respContent);
        }
        catch (Exception)
        {
            return default;
        }
    }
}

internal class InvalidGrant
{
    public string error_description { get; set; }
    public string error { get; set; }
}

public class ApiRespoinse
{
    public int Code { get; set; }
    public string Message { get; set; }
    public object Content { get; set; }
}

public class ApiResponse<T>
{
    [JsonPropertyName("Code")] public int Code { get; set; }
    [JsonPropertyName("Message")] public string? Message { get; set; }
    [JsonPropertyName("Content")] public T? Content { get; set; }
}

public class ApiSigRespoinse
{
    public int code { get; set; }
    public string message { get; set; }
    public ApiTranInfo content { get; set; }
    public string codeDesc { get; set; }
}

public class ApiTranInfo
{
    public string rfTranId { get; set; }

    public List<ApiTranDocument> documents { get; set; }


    public string tranId { get; set; }

    public string sub { get; set; }

    public string credentialId { get; set; }

    public int tranType { get; set; }

    public string tranTypeDesc { get; set; }

    public int tranStatus { get; set; }

    public string tranStatusDesc { get; set; }

    public string reqTime { get; set; }
}

public class ApiTranDocument
{
    public string name { get; set; }
    public string type { get; set; }
    public string size { get; set; }
    public string data { get; set; }
    public string hash { get; set; }
    public string signature { get; set; }

    public string dataSigned { get; set; }
    public string url { get; set; }
}

/// <summary>
/// </summary>
internal class GetTokenBody
{
    public string grant_type { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public string client_id { get; set; }
    public string client_secret { get; set; }
}

/// <summary>
/// </summary>
public class GetTokenResponse
{
    public string id { get; set; }
    public bool is_group { get; set; }
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public DateTime exprise_time { get; set; }

    public string email { get; set; }
    // More properties here
}

public class LoginInfo
{
    public string? username { get; set; }
    public string? password { get; set; }
}