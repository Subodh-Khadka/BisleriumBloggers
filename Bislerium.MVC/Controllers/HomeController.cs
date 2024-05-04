using Bislerium.MVC.Models;
using Domain.Bislerium;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace Bislerium.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("https://localhost:7241/GetAllBlogs");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var blogs = JsonConvert.DeserializeObject<List<Blog>>(content);
                return View(blogs);
            }
            else
            {
                return View("Error");
            }
        }

        public async Task<IActionResult> BlogDetail(Guid blogId)
        {
            try
            {
                var blog = await _httpClient.GetFromJsonAsync<Blog>($"https://localhost:7241/GetBlogById/{blogId}");

                if (blog != null)
                {
                    return View(blog);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBlogUpVote(Guid blogId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"https://localhost:7241/UpdateBlogUpVote/{blogId}", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("BlogDetail", new { blogId });
                }
                else
                {
                    // Handle unsuccessful response
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBlogDownVote(Guid blogId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"https://localhost:7241/UpdateBlogDownVote/{blogId}", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("BlogDetail", new { blogId });
                }
                else
                {
                    // Handle unsuccessful response
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddComment(Guid blogId, string content)
        {
            try
            {
                var comment = new Comment
                {
                    BlogId = blogId,
                    Content = content
                };

                var json = JsonConvert.SerializeObject(comment);

                var contentData = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://localhost:7241/AddComment", contentData);


                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("BlogDetail", new { blogId });
                }
                else
                {
                    // Handle unsuccessful response
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the request
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }






        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
