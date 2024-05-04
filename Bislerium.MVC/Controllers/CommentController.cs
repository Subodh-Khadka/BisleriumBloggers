using Domain.Bislerium;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Bislerium.MVC.Controllers
{
    public class CommentController : Controller
    {
        private readonly HttpClient _httpClient;

        public CommentController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid blogId, string content)
        {
            try
            {
                var comment = new Comment
                {
                    BlogId = blogId,
                    Content = content,

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

    }
}

