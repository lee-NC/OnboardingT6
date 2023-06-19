using Demo.Services.UserService.Entity.Api.Entities;

namespace Demo.Services.UserService.Entity.Api.Model;

public class UserResponse
{
    public string Id { get; set; }
    public string Username { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public string? Sex { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string StatusDesc { get; set; }
    public UserEntity.Statuses Status { get; set; }

    public static UserResponse MapFromModel(UserEntity userEntity)
    {
        return new UserResponse
        {
            Id = userEntity.Id.ToString(),
            Username = userEntity.UserPass.Username,
            DateCreated = userEntity.DataCreated,
            DateUpdated = userEntity.DateUpdated,
            Sex = userEntity.Sex.ToString(),
            LastName = userEntity.LastName,
            FirstName = userEntity.FirstName,
            DateOfBirth = userEntity.DateOfBirth,
            Email = userEntity.Email,
            Role = userEntity.Role.ToString(),
            StatusDesc = userEntity.StatusDesc,
            Status = userEntity.Status
        };
    }
}