using DocumentEditing.Libs;
using DocumentEditing.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DocumentEditing.Controllers.API
{
    [Route("Api/Auth")]
    [ApiController]
    public class AuthController : Controller
    {
        public AuthController()
        {
        }

        // POST /Auth/Login
        /// <summary>
        /// User authentification
        /// </summary>
        /// <param name="loginModel">Model with login (in field email:))</param>
        /// <returns>Object with user creds and JWT-token</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginModel)
        {
            if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Email))
            {
                return BadRequest(new { message = "Username is required" });
            }

            // Just two roles now
            var role = loginModel.Email.ToLower() == "guest" ? "Guest" : "Admin";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, loginModel.Email),
                new Claim(ClaimTypes.Role, role)
            };

            // Encoding
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_1234567890987654321"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "DocumentEditing",
                audience: "DocumentEditing",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { Token = tokenString, Username = loginModel.Email, Role = role });
        }

        // POST /api/auth/logout
        public IActionResult Logout()
        {
            return Ok();
        }
    }
}
