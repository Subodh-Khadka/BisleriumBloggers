using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Domain.Bislerium.ViewModels;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace Bislerium.MVC.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;

        public AdminController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string month)
        {
            var accessToken = Request.Cookies["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "User");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            try
            {
                string url = "https://localhost:7241/dashboard";

                if (!string.IsNullOrEmpty(month))
                {
                    url += $"?month={month}";
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var dashboardData = JsonConvert.DeserializeObject<AdminDashboardDetail>(content);

                    ViewBag.IsAllTime = string.IsNullOrEmpty(month);
                    return View(dashboardData);
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index1(string month)
        {
            var accessToken = Request.Cookies["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "User");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            try
            {
                string url = "https://localhost:7241/dashboard";

                if (!string.IsNullOrEmpty(month))
                {
                    url += $"?month={month}";
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var dashboardData = JsonConvert.DeserializeObject<AdminDashboardDetail>(content);

                    ViewBag.IsAllTime = string.IsNullOrEmpty(month);
                    return View(dashboardData);
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index2(string month)
        {
             var accessToken = Request.Cookies["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "User");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            try
            {
                string url = "https://localhost:7241/dashboard";

                if (!string.IsNullOrEmpty(month))
                {
                    url += $"?month={month}";
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var dashboardData = JsonConvert.DeserializeObject<AdminDashboardDetail>(content);

                    ViewBag.IsAllTime = string.IsNullOrEmpty(month);
                    return View(dashboardData);
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("UserId");

            return RedirectToAction("Index", "Home");
        }
    }
}

