using Azure.Core;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _emailSender;
    public record LoginResponse(bool Flag, string Token, string Message);
    public record UserSession(string? Id, string? Name, string? Email, string? Role);
    public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
    RoleManager<IdentityRole> roleManager, IConfiguration configuration,
    ApplicationDbContext db,
    SignInManager<ApplicationUser> signInManager)
    {

        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _db = db;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            UserName = model.Email,
            Email = model.Email
        };

        // Check if the specified role exists
        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            // Assign the specified role to the user
            await _userManager.AddToRoleAsync(user, "Blogger");

            return Ok("User registered successfully.");
        }

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Assign the specified role to the user
            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok("User registered successfully.");
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("Login")]
    public async Task<LoginResponse> Login([FromBody] LoginVm loginUser)
    {

        var result = await _signInManager.PasswordSignInAsync(loginUser.Email,
        loginUser.Password, false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var getUser = await _userManager.FindByEmailAsync(loginUser.Email);
            var getUserRole = await _userManager.GetRolesAsync(getUser);
            var userSession = new UserSession(getUser.Id, getUser.UserName,
            getUser.Email, getUserRole.First());
            string token = GenerateToken(userSession);
            return new LoginResponse(true, token!, "Login completed");
        }
        else
        {
            return new LoginResponse(false, null!, "Login not completed");
        }
    }

    private string GenerateToken(UserSession user)
    {
        var securityKey = new
        SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey,
        SecurityAlgorithms.HmacSha256);
        var userClaims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
                };
        var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: userClaims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials

        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("UserProfile/{userId}")]
    public async Task<ActionResult<UserProfileVm>> UserProfile(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        var userProfile = new UserProfileVm
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            ProfilePictureUrl = user.ProfilePictureUrl,
        };

        return userProfile;
    }

    [Authorize]
    [HttpPost("UpdateUser/{userId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> EditUserProfile(string userId, UserProfileVm updatedProfile, IFormFile profilePicture)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.FullName = updatedProfile.FullName;
            user.Email = updatedProfile.Email;
            user.DateOfBirth = updatedProfile.DateOfBirth;
            user.Address = updatedProfile.Address;
            user.Bio = updatedProfile.Bio;

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(profilePicture.FileName)}";
                var filePath = Path.Combine("wwwroot", "Images", "ProfilePictures", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = $"/Images/ProfilePictures/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok("User updated successfully.");
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating user: {ex.Message}");
        }
    }

    [Authorize(Roles ="Admin")]
    [HttpGet("GetUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var allUsers = await _db.ApplicationUsers.ToListAsync();
        return Ok(allUsers);
    }


    [Authorize]
    [HttpPut("changePassword/{userId}")]
    public async Task<IActionResult> ChangePassword(string userId, [FromBody] ChangePasswordViewModel model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(user, model.PreviousPassword, model.NewPassword);
        if (result.Succeeded)
        {
            return Ok("Password has been changed!.");
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Ok("User does not exist");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        try
        {
            // Send the reset token to the user's email
            await _emailSender.SendEmailAsync(email, "Reset password", $"Your password reset token is: {token}");

            return Ok("Reset token sent to your email.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while sending the email: {ex.Message}");
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVm model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded)
        {
            return Ok("Password reset completed.");
        }

        return BadRequest(ModelState);
    }

    [Authorize]
    [HttpDelete("delete-user/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return Ok("User deleted successfully.");
        }

        return BadRequest(result.Errors);
    }


}

