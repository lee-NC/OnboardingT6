using System.ComponentModel.DataAnnotations;
using Demo.Common.Utils;

namespace Demo.ApiGateway.DTOs;

public class AddUserRequest
{
    [Required] public string Username { get; set; }
    [Required] public string Password { get; set; }
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] public GenderEnum Sex { get; set; }
    public string Email { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public RoleEnum? Role { get; set; }
    public string? CompanyId { get; set; }
}