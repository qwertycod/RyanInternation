using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.   KeyVault.Secrets;
using Homework.Models;
using Homework.Repository.Interfaces;
using Homework.Repository.Models;
using Homework.Services.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Homework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtHelper _jwtHelper;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthController> _logger;
        private const int JWT_timeout_minute = 2;

        private static Dictionary<string, Repository.Models.RefreshToken> _userRefreshTokens = new(); // username -> token

        public AuthController(JwtHelper jwtHelper, IAuthService authService,  IUnitOfWork unitOfWork, ILogger<AuthController> logger)
        {
            _jwtHelper = jwtHelper;
            _authService = authService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginRequest request)
        //{
        //    if (request == null)
        //    {
        //        return Unauthorized();
        //    }

        //    // next logic assumes user is detail is ok

        //    var claims = new List<Claim>()
        //    {
        //        new Claim(ClaimTypes.Name, request.UserName),
        //        new Claim(ClaimTypes.Email, request.UserName)
        //    };

        //    var identity = new ClaimsIdentity(claims, "Cookie");
        //    var principal = new ClaimsPrincipal(identity);

        //    await HttpContext.SignInAsync("HomeworkAppCookie", principal);

        //    return Ok(new { UserName = request.UserName });
        //}


        // it used httpContext SignInAsync method, its cookie name is HomeworkAppCookie
        // [Authorize] works with this 
        [HttpPost("loginByCookie")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            // 1. Validate user credentials (replace with your actual user validation logic)
            if (model.UserName == "string" && model.Password == "string")
            {
                // 2. Create Claims for the authenticated user
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, model.UserName), // Unique identifier
                new Claim(ClaimTypes.Name, model.UserName), // Display name
                new Claim(ClaimTypes.Role, "User"), // Example role
                // Add any other claims you want to store in the cookie
                new Claim("CustomClaimType", "CustomValue")
            };

                // 3. Create a ClaimsIdentity
                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    "HomeworkAppCookie" // IMPORTANT: Use the same scheme name as configured in Program.cs
                );

                // 4. Create a ClaimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // 5. Define Authentication Properties (e.g., for "Remember Me")
                var authProperties = new AuthenticationProperties
                {
                    // IsPersistent = true, // Set to true for a persistent cookie (e.g., "Remember Me")
                    // If true, the cookie will survive browser restarts based on ExpireTimeSpan
                    // You can set ExpireTimeSpan in your AddCookie options or here.
                    // ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30), // Example: cookie expires in 30 minutes
                };

                // 6. Sign in the user - THIS IS WHERE THE COOKIE IS ISSUED!
                await HttpContext.SignInAsync(
                    "HomeworkAppCookie", // IMPORTANT: Use the same scheme name
                    claimsPrincipal,
                    authProperties
                );

                return Ok(new { message = "Login successful!" });
            }

            return Unauthorized(new { message = "Invalid credentials." });
        }


        [HttpPost("login")]
        //// [Authorize] works with this also
        public async Task<IActionResult> Login1([FromBody] LoginRequest request)
        {
            if (request.UserName != null) // Simulate login
            {
              var loginResponse = _authService.Login(request.UserName, request.Password);

                if (loginResponse != null && loginResponse.Token != null)
                {
                    var refreshTokenString = GenerateRefreshToken();
                    var refreshToken = new Homework.Repository.Models.RefreshToken
                    {
                        Token = refreshTokenString,
                        Username = request.UserName,
                        Expires = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime()).AddDays(7), // Set expiration to 7 days
                    };
                    var res = _unitOfWork.UserRepository.AddRefreshToken(refreshToken);

                    Response.Cookies.Append("jwt-token-value", loginResponse.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddMinutes(JWT_timeout_minute)
                    });
                    Response.Cookies.Append("refresh-token-value", refreshTokenString, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddMinutes(60)
                    });

                    StoreRefreshToken(loginResponse.Username, refreshTokenString);

                    //var client = new SecretClient(new Uri("https://testkv230625r.vault.azure.net/"), new DefaultAzureCredential());
                    //await client.SetSecretAsync("MySecret", "MySecretValue");

                    _logger.LogInformation(loginResponse.Username + " Logged In!");
                    return Ok(new { Expiry = 3, message = "Logged In!" });
                }
                else
                {
                    return Unauthorized(loginResponse);
                }
            }

            return Unauthorized();
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser(RegsterDetail regsterDetail)
        {
            var UserDetail = new Homework.Repository.Models.UserDetail { 
                Username = regsterDetail.Username,
                Email = regsterDetail.Email,
                PasswordHash = regsterDetail.Password,
                Phone = regsterDetail.Phone,            
                StudentId = regsterDetail.StudentId
            };

            var call = await _authService.Register(UserDetail);

            return Ok();
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        //public string GenerateRefreshToken(string username)
        //{
        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.Name, username),
        //        new Claim("token_type", "refresh")
        //    };

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: _config["Jwt:Issuer"],
        //        audience: _config["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddDays(7),
        //        signingCredentials: creds
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        private void StoreRefreshToken(string username, string refreshToken)
        {
            _userRefreshTokens[username] = new Repository.Models.RefreshToken
            {
                Token = refreshToken,
                Expires = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),// or shorter
                Username = username
            };
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if(_userRefreshTokens == null || (_userRefreshTokens.Count == 0))
            {
                await Logout();
                return BadRequest();
            }

            var refreshToken = Request.Cookies["refresh-token-value"];
            var token = Request.Cookies["jwt-token-value"];

            var username =   _userRefreshTokens
                    .FirstOrDefault(kv => kv.Value != null && kv.Value.Token == refreshToken).Key;

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            //// ✅ Validate and extract username from refresh token
            //var principal = _jwtHelper.ValidateRefreshToken(token);
            //if (principal == null)
            //    return Unauthorized();

            //var username = principal.Identity?.Name;
            //if (string.IsNullOrEmpty(username))
            //    return Unauthorized();

            var stored = _userRefreshTokens[username];
            if (stored != null && stored.Token != null && stored.Expires > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                // ✅ Generate new JWT access token
                var newJwt = _jwtHelper.GenerateToken(username);
                var newRefreshToken = GenerateRefreshToken();

                // ✅ Update refresh token storage
                StoreRefreshToken(username, newRefreshToken);


                // ✅ Set new JWT in cookie
                Response.Cookies.Append("jwt-token-value", newJwt, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(JWT_timeout_minute)
                });

                Response.Cookies.Append("refresh-token-value", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return Ok(new { message = "Token refreshed" });
            }
            else
            {
                               // Refresh token is invalid or expired
                return Unauthorized(new { message = "Invalid or expired refresh token" });  
            }
        }

        [HttpGet("antiforgery-token")]
        public IActionResult GetXsrfToken([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
            {
                HttpOnly = false,               // JavaScript must read it
                SameSite = SameSiteMode.None,    // or None
                Secure = false                  // Allow over HTTP for localhost dev
            });
            return NoContent();
        }

        [Authorize]
        [HttpGet("secure")]
        public IActionResult SecureData()
        {
            return Ok($"You are authenticated as: {User.Identity.Name}");
        }   

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var cookies = Request.Cookies;

            foreach (var cookie in cookies)
            {
                Console.WriteLine($"Cookie: {cookie.Key} = {cookie.Value}");
            }

            // Or access a specific cookie by name
            if (Request.Cookies.TryGetValue("HomeworkAppCookie", out var cookieValue))
            {
                Console.WriteLine($"HomeworkAppCookie value: {cookieValue}");
            }

            var studentData = await _unitOfWork.StudentRepository.GetAllStudentsAsync();

            var username = User.FindFirstValue(ClaimTypes.Name);

            var user = await _unitOfWork.UserRepository.GetByUserNameAsync(username);

            _logger.LogInformation(" me ______________________________ ");

            return Ok(new { username = username, studentId = user.StudentId });
        }

        [AllowAnonymous]
        [HttpGet("/testme")]
        public IActionResult TestMe()
        {
            return Ok(new { value = "OK" });
        }

        //[HttpPost("logout")]
        //public async Task<IActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync("HomeworkAppCookie");
        //    return Ok();
        //}

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("HomeworkAppCookie");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/", // <-- Important: must match!
            };

            var token = Request.Cookies["jwt-token-value"];
            if (!string.IsNullOrEmpty(token))
            {
                var principal = _jwtHelper.ValidateRefreshToken(token);
                if (principal != null)
                {
                    var username = principal.Identity?.Name;
                    _userRefreshTokens[username] = null;
                }
            }
            else
            {

                Response.Cookies.Delete("jwt-token-value", cookieOptions);
                Response.Cookies.Delete("refresh-token-value", cookieOptions);
                Response.Cookies.Delete("XSRF-TOKEN", new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });
            }
            return Ok(new { message = "Logged out" });
        }

    }
}
