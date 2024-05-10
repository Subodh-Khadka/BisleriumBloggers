using Domain.Bislerium;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Bislerium.MVC.Controllers
{
    [Authorize(Roles = "Blogger")]
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
                var existingCommentResponse = await _httpClient.GetAsync($"https://localhost:7241/GetCommentById/{comment.Id}");
                if (!existingCommentResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                var existingCommentJson = await existingCommentResponse.Content.ReadAsStringAsync();
                var existingComment = JsonConvert.DeserializeObject<Comment>(existingCommentJson);

                var response = await _httpClient.PutAsJsonAsync($"https://localhost:7241/UpdateComment", comment);

                if (response.IsSuccessStatusCode)
                {
                    var historyData = new
                    {
                        EntityId = existingComment.Id,
                        EntityType = "Comment",
                        Action = "Edit",
                        UpdatedBy = comment.UserId,
                        UpdatedDate = DateTime.Now,
                        OldVContent = existingComment.Content,
                        NewContent = comment.Content
                    };

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(historyData), Encoding.UTF8, "application/json");
                    var historyResponse = await _httpClient.PostAsync("https://localhost:7241/api/History/AddHistory", jsonContent);

                    if (!historyResponse.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Error", "Home"); 
                    }
                }

                return RedirectToAction(nameof(HomeController.BlogDetail), "Home", new { blogId = blogId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
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
