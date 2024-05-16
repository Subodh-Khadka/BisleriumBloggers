using Domain.Bislerium;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace MVC.Frontend.Controllers
{
    public class BlogController : Controller
    {
        private readonly HttpClient _httpClient;

        public BlogController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [Authorize(Roles ="Blogger")]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {

                return Unauthorized();
            }

            var userId = userIdClaim.Value;

            var accessToken = Request.Cookies["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "User");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


            var response = await _httpClient.GetAsync($"https://localhost:7241/GetAllBlogsUserId/{userId}");
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

        [Authorize(Roles = "Blogger")]
        public IActionResult Test()
        {
            return View();
        }


        [Authorize(Roles ="Blogger")]
        [HttpPost]
        public async Task<IActionResult> Test(Blog newBlog, IFormFile image)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = userIdClaim?.Value;

                if (newBlog == null)
                {
                    return BadRequest("Invalid blog data.");
                }

                var accessToken = Request.Cookies["AccessToken"];

                if (string.IsNullOrEmpty(accessToken))
                {
                    return RedirectToAction("Login", "User");
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var formData = new MultipartFormDataContent();

                    formData.Add(new StringContent(newBlog.Title), "Title");
                    formData.Add(new StringContent(newBlog.Description), "Description");
                    formData.Add(new StringContent(newBlog.UserId ?? userId), "UserId");
                    formData.Add(new StringContent(newBlog.Popularity?.ToString() ?? "0"), "Popularity");
                    formData.Add(new StringContent(newBlog.UpVote?.ToString() ?? "0"), "UpVote");
                    formData.Add(new StringContent(newBlog.DownVote?.ToString() ?? "0"), "DownVote");
                    formData.Add(new StringContent(newBlog.CommentCount?.ToString() ?? "0"), "CommentCount");

                    if (image != null && image.Length > 0)
                    {
                        if (image.Length > 3 * 1024 * 1024)
                        {
                            ModelState.AddModelError("Image", "The image size must be less than or equal to 3 MB.");
                            return View(newBlog);
                        }

                        var imageContent = new StreamContent(image.OpenReadStream());
                        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                        formData.Add(imageContent, "image", image.FileName);
                    }

                    var response = await httpClient.PostAsync("https://localhost:7241/AddBlog", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                return View(newBlog);
            }
        }


        [Authorize(Roles = "Blogger")]
        public async Task<IActionResult> Edit(Guid blogId)
        {
             var accessToken = Request.Cookies["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "User");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"https://localhost:7241/GetBlogById/{blogId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var blog = JsonConvert.DeserializeObject<Blog>(content);

                return View(blog);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "Blogger")]
        [HttpPost]
        public async Task<IActionResult> EditBlog(Guid blogId, Blog updatedBlog, IFormFile image)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim?.Value;

            if (updatedBlog == null)
            {
                return BadRequest("Invalid blog data.");
            }

            try
            {
                // Set authorization header for the _httpClient instance
                var accessToken = Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(accessToken))
                {
                    return RedirectToAction("Login", "User");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Retrieve the existing blog to get the old content
                var existingBlogResponse = await _httpClient.GetAsync($"https://localhost:7241/GetBlogById/{blogId}");

                if (existingBlogResponse.IsSuccessStatusCode)
                {
                    var existingBlogJson = await existingBlogResponse.Content.ReadAsStringAsync();
                    var existingBlog = JsonConvert.DeserializeObject<Blog>(existingBlogJson);

                    var formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(existingBlog.Id.ToString()), "Id");
                    formData.Add(new StringContent(blogId.ToString()), "blogId");
                    formData.Add(new StringContent(updatedBlog.Title), "Title");
                    formData.Add(new StringContent(updatedBlog.Description), "Description");
                    formData.Add(new StringContent(updatedBlog.UserId ?? userId), "UserId");
                    formData.Add(new StringContent(updatedBlog.Popularity?.ToString() ?? "0"), "Popularity");
                    formData.Add(new StringContent(updatedBlog.UpVote?.ToString() ?? "0"), "UpVote");
                    formData.Add(new StringContent(updatedBlog.DownVote?.ToString() ?? "0"), "DownVote");

                    if (image != null && image.Length > 0)
                    {
                        var imageContent = new StreamContent(image.OpenReadStream());
                        formData.Add(imageContent, "image", image.FileName);
                    }

                    var response = await _httpClient.PutAsync("https://localhost:7241/UpdateBlog", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        var historyData = new
                        {
                            EntityId = existingBlog.Id,
                            EntityType = "Blog",
                            Action = "Edit",
                            UpdatedBy = userId,
                            UpdatedDate = DateTime.Now,
                            OldVContent = existingBlog.Description,
                            NewContent = updatedBlog.Description,
                        };

                        var jsonContent = new StringContent(JsonConvert.SerializeObject(historyData), Encoding.UTF8, "application/json");

                        var historyResponse = await _httpClient.PostAsync("https://localhost:7241/api/History/AddHistory", jsonContent);
                        if (!historyResponse.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Error", "Home");
                        }

                        return RedirectToAction("Index", "Blog");
                    }
                    else
                    {
                        return View("Error", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error", "Home");
            }
        }


        [Authorize(Roles = "Blogger")]
        [HttpPost]
        public async Task<IActionResult> Delete(Guid blogId)
        {
            var accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "User");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.DeleteAsync($"https://localhost:7241/DeleteBlog/{blogId}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Blog");
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                return View("Error");
            }
        }
    }
}
