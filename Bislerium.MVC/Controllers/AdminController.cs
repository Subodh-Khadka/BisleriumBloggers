using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Domain.Bislerium.ViewModels;
using Newtonsoft.Json;
using System.Net.Http;

namespace Bislerium.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;

        public AdminController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<IActionResult> Register()
        {
            return View();
        }

        public async Task<IActionResult> Index(string month)
        {
            try
            {
                string url = "https://localhost:7241/dashboard";

                // If month is provided, append it to the URL
                if (!string.IsNullOrEmpty(month))
                {
                    url += $"?month={month}";
                }

                // Fetch dashboard data from the API
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var dashboardData = JsonConvert.DeserializeObject<AdminDashboardDetail>(content);
                    return View(dashboardData);
                }
                else
                {
                    // Handle unsuccessful response
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return View("Error");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}






//public async Task<IActionResult> Index()
//{
//    try
//    {
//        // Fetch all-time dashboard data from the API
//        var response = await _httpClient.GetAsync($"https://localhost:7241/dashboard");

//        if (response.IsSuccessStatusCode)
//        {
//            var content = await response.Content.ReadAsStringAsync();
//            var allTimeData = JsonConvert.DeserializeObject<AdminDashboardDetail>(content);
//            return View(allTimeData);
//        }
//        else
//        {
//            // Handle unsuccessful response
//            return View("Error");
//        }
//    }
//    catch (Exception ex)
//    {
//        // Handle exception
//        return View("Error");
//    }
//}