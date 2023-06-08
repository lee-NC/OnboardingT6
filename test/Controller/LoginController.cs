using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using test.Model;

namespace test.Controller;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IConfiguration _config;

    public LoginController(IConfiguration config)
    {
        _config = config;
    }

    private static readonly List<User> Users = new()
    {
        new User() { Username = "admin", Password = "password", Role = "Administrator" },
        new User() { Username = "seller", Password = "password", Role = "Seller" },
    };

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login([FromBody] UserLogin userLogin)
    {
        var user = Authenticate(userLogin);

        if (user != null)
        {
            var token = Generate(user);
            var map = new Dictionary<string, string>();
            map.Add("token", token);
            return Ok(map);
        }

        return NotFound("User not found");
    }

    private string Generate(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private User Authenticate(UserLogin userLogin)
    {
        var currentUser = Users.FirstOrDefault(o =>
            o.Username.ToLower() == userLogin.Username.ToLower() && o.Password == userLogin.Password);

        if (currentUser != null)
        {
            return currentUser;
        }

        return null;
    }
}