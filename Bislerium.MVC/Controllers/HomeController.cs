using Bislerium.MVC.Models;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace Bislerium.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient, ApplicationDbContext db)
        {
            _logger = logger;
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<IActionResult> Index(string sortBy)
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7241/GetAllBlogs");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var blogs = JsonConvert.DeserializeObject<List<Blog>>(content);

                    // Check if sorting criteria is provided
                    if (!string.IsNullOrEmpty(sortBy))
                    {
                        var sortedResponse = await _httpClient.GetAsync($"https://localhost:7241/GetSortedBlogs?sortBy={sortBy}");

                        if (sortedResponse.IsSuccessStatusCode)
                        {
                            var sortedContent = await sortedResponse.Content.ReadAsStringAsync();
                            var sortedBlogs = JsonConvert.DeserializeObject<List<Blog>>(sortedContent);
                            return View(sortedBlogs);
                        }
                        else
                        {
                            return View("Error");
                        }
                    }
                    else
                    {
                        return View(blogs);
                    }
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> BlogDetail(Guid blogId)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim.Value;
            try
            {
                var blogResponse = await _httpClient.GetFromJsonAsync<Blog>($"https://localhost:7241/GetBlogById/{blogId}");
                var commentsResponse = await _httpClient.GetFromJsonAsync<List<Comment>>($"https://localhost:7241/GetCommentsByBlogId?blogId={blogId}");

                if (blogResponse != null && commentsResponse != null)
                {
                    var viewModel = new BlogDetailVm
                    {
                        Blog = blogResponse,
                        Comments = commentsResponse,
                        UserId = userId,
                    };

                    return View(viewModel);
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
        public async Task<IActionResult> UpdateBlogUpVote(Guid blogId, string userId)
        {
            try
            {
                var requestUrl = $"https://localhost:7241/UpdateBlogUpVote/{blogId}?userId={userId}";

                var response = await _httpClient.PutAsync(requestUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("BlogDetail", new { blogId });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateBlogDownVote(Guid blogId, string userId)
        {
            try
            {
                //var response = await _httpClient.PutAsync($"https://localhost:7241/UpdateBlogDownVote/{blogId}", null);
                var requestUrl = $"https://localhost:7241/UpdateBlogDownVote/{blogId}?userId={userId}";
                var response = await _httpClient.PutAsync(requestUrl, null);

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

        [HttpPost]
        public async Task<IActionResult> UpdateCommentUpVote(Guid commentId)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            var blog = await _db.Blogs.FindAsync(comment.BlogId);
            var blogId = blog.Id;

            try
            {
                var response = await _httpClient.PutAsync($"https://localhost:7241/UpdateCommentUpVote/{commentId}", null);

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
        public async Task<IActionResult> UpdateCommentDownVote(Guid commentId)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            var blog = await _db.Blogs.FindAsync(comment.BlogId);
            var blogId = blog.Id;

            try
            {
                var response = await _httpClient.PutAsync($"https://localhost:7241/UpdateCommentDownVote/{commentId}", null);

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
