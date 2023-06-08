using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.Model;

namespace test.Controller;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet("Admins")]
    // [Authorize(Roles = "Administrator")]
    [Authorize(Policy = "admin")]
    public IActionResult AdminsEndpoint()
    {
        var currentUser = GetCurrentUser();

        return Ok($"Hi {currentUser.Username}, you are an {currentUser.Role}");
    }

    [HttpGet("Sellers")]
    [Authorize(Policy = "seller")]
    public IActionResult SellersEndpoint()
    {
        var currentUser = GetCurrentUser();

        return Ok($"Hi {currentUser.Username}, you are a {currentUser.Role}");
    }

    [HttpGet("AdminsAndSellers")]
    [Authorize(Policy = "all")]
    public IActionResult AdminsAndSellersEndpoint()
    {
        var currentUser = GetCurrentUser();

        return Ok($"Hi {currentUser.Username}, you are an {currentUser.Role}");
    }

    [HttpGet("Public")]
    public IActionResult Public()
    {
        return Ok("Hi, you're on public property");
    }

    private User GetCurrentUser()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;

        if (identity != null)
        {
            var userClaims = identity.Claims;
            return new User
            {
                Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value
            };
        }

        return null;
    }
}