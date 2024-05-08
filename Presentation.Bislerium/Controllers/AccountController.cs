using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
    public record LoginResponse(bool Flag, string Token, string Message);
    public record UserSession(string? Id, string? Name, string? Email, string? Role);
    public AccountController(UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager, IConfiguration configuration,
    ApplicationDbContext db,
    SignInManager<ApplicationUser> signInManager)
    {

        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _signInManager = signInManager;
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

            //// If the role doesn't exist, return error
            //return BadRequest("Invalid role specified.");
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
    //[Authorize]
    public async Task<ActionResult<UserProfileVm>> UserProfile(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        // Create a user profile view model based on the user information
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
}



