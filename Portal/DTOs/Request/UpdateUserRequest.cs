using System.ComponentModel.DataAnnotations;
using Demo.Common.Utils;

namespace Demo.ApiGateway.DTOs;

public class UpdateUserRequest
{
    [Required] public string UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public GenderEnum? Sex { get; set; }

    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}