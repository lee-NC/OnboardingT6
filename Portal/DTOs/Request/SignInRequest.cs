using System.ComponentModel.DataAnnotations;

namespace Demo.ApiGateway.DTOs;

public class SignInRequest
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}