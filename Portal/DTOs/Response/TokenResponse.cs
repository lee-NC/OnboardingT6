using System.Text.Json.Serialization;

namespace Demo.ApiGateway.DTOs.Response;

public class TokenResponse
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }

    public static TokenResponse MapFromModel(string username, string userId, string token)
    {
        return new TokenResponse
        {
            Username = username,
            UserId = userId,
            Token = token
        };
    }
}