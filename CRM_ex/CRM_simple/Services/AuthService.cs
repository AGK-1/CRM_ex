using CRM.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CRM_simple.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthService(IConfiguration configuration, AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }

        public async Task<string> Login(LoginModel model)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Check if the user exists
            if (user == null)
            {
                throw new UnauthorizedAccessException("User does not exist");
            }

            // Check if the password is valid
            var result = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!result)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Token generation logic
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Email, user.Email)
             }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public string Username { get; set; }
        }
    }
}
