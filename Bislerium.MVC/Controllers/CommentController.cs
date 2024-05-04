using Domain.Bislerium;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Bislerium.MVC.Controllers
{
    public class CommentController : Controller
    {
        private readonly HttpClient _httpClient;

        public CommentController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Edit(Guid commentId)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7241/GetCommentById/{commentId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var comment = JsonConvert.DeserializeObject<Comment>(content);

                return View(comment);
            }
            else
            {
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Comment comment, Guid blogId)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"https://localhost:7241/UpdateComment", comment);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(HomeController.BlogDetail), "Home", new { blogId = blogId });

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
        public async Task<IActionResult> DeleteComment(Guid commentId,Guid blogId)
        {
            
            try
            {
                var response = await _httpClient.DeleteAsync($"https://localhost:7241/DeleteComment/{commentId}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(HomeController.BlogDetail), "Home", new { blogId = blogId });
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

        public IActionResult Index()
        {
            return View();
        }
    }
}
