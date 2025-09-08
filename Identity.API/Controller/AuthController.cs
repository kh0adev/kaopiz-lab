using System.Security.Claims;
using Identity.API.Data.Model;
using Identity.API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controller;

[Route("api")]
[ApiController]
public class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    TokenService tokenService,
    IHttpContextAccessor contextAccessor)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest? registerModel)
    {
        if (registerModel == null)
        {
            return BadRequest(new { message = "Invalid data" });
        }

        var existingUser = await userManager.FindByEmailAsync(registerModel.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "Email is already in use" });
        }

        var user = new AppUser()
        {
            Email = registerModel.Email,
            UserName = registerModel.UserName,
            NormalizedUserName = registerModel.Email.ToUpper(),
        };

        var result = await userManager.CreateAsync(user, registerModel.Password);
        await userManager.AddToRoleAsync(user, nameof(AppRole.Admin));
        if (result.Succeeded)
        {
            return Ok(new { message = "User registered successfully" });
        }

        return BadRequest(new { message = "Registration failed", errors = result.Errors });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid login attempt" });
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid login attempt" });
        }

        var refreshToken = string.Empty;
        if (request.RememberMe)
        {
            refreshToken = tokenService.CreateRefreshToken(user);
        }

        var token = await tokenService.GenerateAccessToken(user);

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new { token });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var token = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { message = "No refresh token" });
        }
        
        var refreshToken = await tokenService.RefreshToken(token);
        if (refreshToken.IsValid())
        {
            var newAccessToken = await tokenService.GenerateAccessToken(refreshToken.AppUser);
            var newRefreshToken = tokenService.RefreshRefreshToken(refreshToken.AppUser.Id);
            
            Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            return Ok(new { newAccessToken });
        }

        return Unauthorized(new { message = "Invalid token attempt" });
    }

    [HttpDelete("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("refresh_token");
        await tokenService.RevokeRefreshToken(GetCurrentUserId());
        return NoContent();
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.FindByIdAsync(GetCurrentUserId());
        if (user is not null)
        {
            return Ok(user);
        }
        
        return NotFound();
    }

    private string GetCurrentUserId()
    {
        return contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    public record RefreshRequest(string Token);

    public record RegisterRequest(string Email, string UserName, string Password);

    public record LoginRequest(string Email, string Password, bool RememberMe);
}