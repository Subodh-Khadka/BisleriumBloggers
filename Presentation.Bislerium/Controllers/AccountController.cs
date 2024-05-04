using Domain.Bislerium.ViewModels;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Bislerium;

namespace Presentation.Student.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AccountController(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
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
                // If the role doesn't exist, return error
                return BadRequest("Invalid role specified.");
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVm model)
        {
            var authService = HttpContext.RequestServices.GetService<IAuthService>(); 
            var token = await authService.Login(model); 

            if (token != null)
            {
                return Ok(new { token });
            }
            else
            {
                return BadRequest("Invalid email or password.");
            }
        }


        [HttpGet("getUser"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginVm model)
        //{
        //    var user = await _userManager.FindByEmailAsync(model.Email);
        //    if (user != null)
        //    {
        //        var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
        //        if (result.Succeeded)
        //        {

        //            var roles = await _userManager.GetRolesAsync(user);
        //            var role = roles.FirstOrDefault();
        //            var username = user.UserName;

        //            var claims = new List<Claim>()
        //            {
        //                new Claim(ClaimTypes.Name, username),
        //                new Claim(ClaimTypes.Role, role),
        //            };

        //            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        //            var authProperties = new AuthenticationProperties
        //            {
        //                IsPersistent = false
        //            };

        //            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        //            return Ok("logged");
        //        }
        //    }

        //    return Unauthorized("Invalid email or password");
        //}

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet("getAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            return Ok(allUsers);
        }

        [HttpDelete("deleteUser")]
        public async Task<IActionResult> Delete(string userId) 
        {
            var existingUser = await _userManager.FindByIdAsync(userId);

            if (existingUser == null) 
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(existingUser);
            if (result.Succeeded)
            {
                return Ok("user deleted");
            }
            return BadRequest(result.Errors);   
        }


        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser(string userId, UpdateUserVM updateUserVM)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateUserVM.FullName))
            {
                existingUser.FullName = updateUserVM.FullName;
            }

            var result = await _userManager.UpdateAsync(existingUser);
            if (result.Succeeded)
            {
                return Ok("User Information Updated");
            }
            else
            {
                return BadRequest(result.Errors);
            }
            
        }

    }
}