using AuthService.DTO;
using AuthService.Exceptions;
using AuthService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthManager _authManager;

        public AuthController(IAuthManager authService)
        {
            _authManager = authService;
        }

        [AllowAnonymous]
        [EnableRateLimiting("standard")]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authManager.RegisterAsync(request);
                return Ok(result);
            }
            catch (AuthException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authManager.LoginAsync(request);
                return Ok(result);
            }
            catch (AuthException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [EnableRateLimiting("standard")]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authManager.RefreshAsync(request);
                return Ok(result);
            }
            catch (AuthException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                await _authManager.LogoutAsync(request);
                return Ok(new { message = "Logged out successfully" });
            }
            catch (AuthException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            var result = await _authManager.GetCurrentUserAsync(userId);

            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authManager.ForgotPasswordAsync(request);
            return Ok(new { message = "If this email exists, a reset link has been sent." });
        }

        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _authManager.ResetPasswordAsync(request);
                return Ok(new { message = "Password updated successfully." });
            }
            catch (AuthException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            var version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            return Ok(new { version });
        }
    }

}