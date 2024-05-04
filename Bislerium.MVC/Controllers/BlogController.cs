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

namespace MVC.Frontend.Controllers
{
    public class BlogController : Controller
    {
        private readonly HttpClient _httpClient;

        public BlogController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                
                return Unauthorized(); 
            }

            var userId = userIdClaim.Value;

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


        public IActionResult Test()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Test(Blog newBlog, IFormFile image)
        {

            if (image != null && image.Length > 3 * 1024 * 1024)
            {
                ModelState.AddModelError("Image", "The image size must be less than or equal to 3 MB.");
                return View(newBlog); 
            }

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim?.Value;


            if (newBlog == null)
            {
                return BadRequest("Invalid blog data.");
            }

            try
            {
                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(newBlog.Title), "Title");
                formData.Add(new StringContent(newBlog.Description), "Description");
                formData.Add(new StringContent(newBlog.UserId ?? userId), "UserId");
                formData.Add(new StringContent(newBlog.Popularity?.ToString() ?? "0"), "Popularity");
                formData.Add(new StringContent(newBlog.UpVote?.ToString() ?? "0"), "UpVote");
                formData.Add(new StringContent(newBlog.DownVote?.ToString() ?? "0"), "DownVote");

                if (image != null && image.Length > 0)
                {
                    var imageContent = new StreamContent(image.OpenReadStream());
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                    formData.Add(imageContent, "image", image.FileName); 
                }

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync("https://localhost:7241/AddBlog", formData);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, errorResponse);
                        return View(newBlog);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                
                return View(newBlog);
            }
        }


        public async Task<IActionResult> Edit(Guid blogId)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7241/GetBlogById/{blogId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var blog = JsonConvert.DeserializeObject<Blog>(content);

                return View(blog);
            }
            else
            {
                // If the request fails, return an error view or handle the error as needed
                return View("Error");
            }
        }


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
                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(updatedBlog.Id.ToString()), "Id");
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
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                    formData.Add(imageContent, "image", image.FileName);
                }

                using (var client = new HttpClient())
                {
                    var response = await client.PutAsync("https://localhost:7241/UpdateBlog", formData);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, errorResponse);
                        return View(updatedBlog);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return View(updatedBlog);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid blogId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"https://localhost:7241/DeleteBlog/{blogId}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, errorResponse);
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
