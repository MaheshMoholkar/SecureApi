using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    public IConfiguration _config;

    public record AuthenticationData(string? UserName, string? Password);
    public record UserData(int UserId, string UserName, string Role);
    public AuthenticationController(IConfiguration config)
    {
        _config = config;
    }

    // api/Authentication/token
    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<string> Authenticate([FromBody] AuthenticationData data)
    {
        var user = ValidateCredentials(data);

        if (user == null)
        {
            return Unauthorized();
        }

        var token = GenerateToken(user);
        return Ok(token);
    }

    private string GenerateToken(UserData user)
    {
        var secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(
                _config.GetValue<string>("Authentication:SecretKey")));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new();
        claims.Add(new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.UniqueName, user.UserName));
        claims.Add(new("Role", user.Role));

        var token = new JwtSecurityToken(
            _config.GetValue<string>("Authentication:Issuer"),
            _config.GetValue<string>("Authentication:Audience"),
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddSeconds(30),
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UserData ValidateCredentials(AuthenticationData data)
    {
        // Demo purpose only
        if (CompareValues(data.UserName, "admin") && CompareValues(data.Password, "password"))
        {
            return new UserData(1, data.UserName!, "admin");
        }

        return null;
    }

    private bool CompareValues(string? actual, string expected)
    {
        if (actual is not null)
        {
            if (actual.Equals(expected /*, StringComparison.InvariantCultureIgnoreCase*/))
            {
                return true;
            }
        }

        return false;
    }
}
