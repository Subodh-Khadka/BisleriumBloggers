using Application.Bislerium;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Bislerium
{


    public class AuthService : IAuthService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public AuthService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<string> Login(LoginVm model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email.ToUpper());
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ("User null");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "anonymous")
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("8/wVwKtv3Sl4eAjhZtg3wIUuQ/+KqWkx/jRsAlx6pXe1+UBBhcRqy6BtsTpQFmo+JZ6XQVbBYNDC1wuSdnImHw=="));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenGenerator = new JwtSecurityToken(
                issuer: "https://localhost:7241",
                audience: "https://localhost:7241",
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds,
                claims: claims
            );

            var tokenResult = new JwtSecurityTokenHandler();
            var token = tokenResult.WriteToken(tokenGenerator);
            return token;
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
