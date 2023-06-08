namespace test.Model;

public class User
{
    public required string Username { get; set; }
    public string? Password { get; set; }
    public required string Role { get; set; }
}