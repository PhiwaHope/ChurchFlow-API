using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChurchFlow.api.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ChurchFlowDbContext _context;
    private readonly PasswordHasher<Committee> _passwordHasher;
    private readonly IConfiguration _config;

    public AuthController(ChurchFlowDbContext context, PasswordHasher<Committee> passwordHasher, IConfiguration config)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var committeeMember = await _context.Committees
            .FirstOrDefaultAsync(c => c.Email == request.Email);

        if (committeeMember == null)
            return Unauthorized("Invalid email or password");

        var result = _passwordHasher.VerifyHashedPassword(
            committeeMember, committeeMember.Password, request.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid email or password");

        var claims = new List<Claim>
        {
            new Claim("nameid", committeeMember.ComId.ToString()),
            new Claim("name", committeeMember.Name),
            new Claim("role", committeeMember.Position)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}