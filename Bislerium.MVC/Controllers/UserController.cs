using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Collections.Specialized.BitVector32;
using static AccountController;

namespace Bislerium.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        public UserController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7241/Register", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again later.");
                return View(model);
            }
        }


        public async Task<IActionResult> Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginVm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7241/Login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse.Flag)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadToken(loginResponse.Token) as JwtSecurityToken;

                    var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var name = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                    var email = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    var role = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                 new Claim(ClaimTypes.Name, name),
                 new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Email, email),
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    if (role == "Blogger")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Server error. Please try again later.");
                return View(model);
            }
        }

        [Authorize]
        public async Task<IActionResult> UserProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var response = await _httpClient.GetAsync($"https://localhost:7241/UserProfile/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var userProfile = await response.Content.ReadFromJsonAsync<UserProfileVm>();

                    return View(userProfile);
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UserProfile(UserProfileVm updatedProfile, IFormFile profilePicture)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            try
            {
                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(userId), "userId");
                formData.Add(new StringContent(updatedProfile.FullName), "FullName");
                formData.Add(new StringContent(updatedProfile.Email), "Email");
                formData.Add(new StringContent(updatedProfile.Bio ?? ""), "Bio");
                formData.Add(new StringContent(updatedProfile.DateOfBirth?.ToString("yyyy-MM-dd") ?? ""), "DateOfBirth");
                formData.Add(new StringContent(updatedProfile.Address ?? ""), "Address");

                if (profilePicture != null && profilePicture.Length > 0)
                {
                    var imageContent = new StreamContent(profilePicture.OpenReadStream());
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(profilePicture.ContentType);
                    formData.Add(imageContent, "profilePicture", profilePicture.FileName);
                }

                var response = await _httpClient.PostAsync($"https://localhost:7241/UpdateUser/{userId}", formData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("UserProfile");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, errorResponse);
                    return View(updatedProfile);
                }
            }
            catch (Exception ex)
            {
                return View(updatedProfile);
            }
        }

        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found.");
            }

            try
            {
                var json = JsonConvert.SerializeObject(model);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"https://localhost:7241/changePassword/{Uri.EscapeDataString(userId)}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Password has been changed successfully.";
                    return RedirectToAction("ChangePassword");
                }
                else
                {
                    TempData["SuccessMessage"] = "Change Password failed";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _httpClient.PostAsync($"https://localhost:7241/forgot-password?email={model.Email}", null);

            if (response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                TempData["SuccessMessage"] = message;
                return RedirectToAction("ResetPassword", new { email = model.Email });

            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }
        }


        public IActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordVm { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string token, string newPassword, string confirmPassword)
        {
            try
            {
                var requestData = new
                {
                    Email = email,
                    Token = token,
                    NewPassword = newPassword,
                    ConfirmPassword = confirmPassword,
                };

                var response = await _httpClient.PostAsJsonAsync("https://localhost:7241/reset-password", requestData);

                if (response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return View();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost("delete-user/{userId}")]  
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var response = await _httpClient.DeleteAsync($"https://localhost:7241/delete-user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                //await _signInManager.SignOutAsync(); 
                return RedirectToAction("Index", "Home"); 
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, errorMessage);
                return View("Error");
            }
        }
    }
}
