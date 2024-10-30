using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CRM.Data;
using CRM_simple.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CRM_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public AuthController(UserManager<IdentityUser> userManager, IEmailSender emailSender, AppDbContext context, AuthService authService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Log the input data
          //  Console.WriteLine(model.Name + model.Surname + model.Company_name);

            var user = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email,
               // PhoneNumber = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // var confirmationLink = $"{Request.Scheme}://{Request.Host}/Auth/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
                var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/Auth/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                if (string.IsNullOrEmpty(confirmationLink))
                {
                    return BadRequest("Failed to generate the confirmation link.");
                }

                await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                    $"<a href='{confirmationLink}' style='padding: 10px 20px; color: white; background-color: #28a745; text-decoration: none; border-radius: 5px; font-size: 16px;'>Confirm your email</a>");

                // Call TransferToUsersTable with model properties
               await TransferToUsersTable(user, model.Name, model.Role);

                return Ok("Registration successful. Please check your email to confirm your account.");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return BadRequest(ModelState);
        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("User ID or token is invalid.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok("Email confirmed successfully!");
            }
            else
            {
                return BadRequest("Email confirmation failed.");
            }
        }

        [HttpGet("token")]
        public IActionResult Yelpaze()
        {
            // Retrieve token from session
            var token = HttpContext.Session.GetString("token");
            if (!string.IsNullOrEmpty(token))
            {
               // HttpContext.Session.Remove("token"); // Delete token after retrieval
               return Ok(token);
              //  HttpContext.Session.Clear();
                //return Ok("Token deleted.");
            }
            else
            {
                return NotFound("No token found in session.");
            }
           
        }


        [HttpPost("check-login")]
        public async Task<IActionResult> CheckLogin(LoginModel model)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Check if the user exists
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check if the password matches
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!passwordValid)
            {
                return Unauthorized("Invalid password");
            }

            var claims = new List<Claim>
            {
             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
             };

            // Create a claims identity
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Create the authentication properties and set the cookie to expire in 30 minutes
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Keep the cookie even if the browser is closed
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Set cookie expiration to 30 minutes
            };

            // Sign the user in with cookie authentication
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(claimsIdentity),
                                          authProperties);

            // Return success response
            return Ok("Login successful. Cookie is set for 30 minutes.");
            // If email and password are valid
        }


        [HttpPost("loginkol")]
        public async Task<IActionResult> Loginko([FromBody] LoginModel model)
        {
            try
            {
                var token = await _authService.Login(model);
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("User does not exist");
                }

                // Save token in session
                HttpContext.Session.SetString("token", token);

                return Ok(new
                {
                    email = user.Email,         // Changed from userName to email
                    token = token,               // Changed Token to token (to keep all lowercase)
                    userName = user.UserName     // Distinct name for the user's username
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpPost("loginlog")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                // Retrieve token from the authentication service
                var token = await _authService.Login(model);

                // Find user details using email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Unauthorized("User does not exist");
                }
                HttpContext.Session.SetString("token", token);
                // Return user details with the generated token
                return Ok(new
                {
                    userName = user.Email,
                    Token = token
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Handle other exceptions
            }
        }



        private async Task TransferToUsersTable(IdentityUser identityUser, string name,string role)
        {
            var newUser = new Users
            {
                email = identityUser.Email,
                name = name,                     
                role = role,
            };

            _context.user.Add(newUser);
            await _context.SaveChangesAsync();
        }

        [Authorize]
        [HttpGet("protected-resource")]
        public IActionResult ProtectedResource()
        {
            return Ok("You are authorized to access this resource.");
        }
    }
}

public class RegisterModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}