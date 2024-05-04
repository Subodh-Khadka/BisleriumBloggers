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

namespace MVC.Frontend.Controllers
{
    public class BlogController : Controller
    {
        private readonly HttpClient _httpClient;

        public BlogController(HttpClient httpClient)
        {
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

        public IActionResult Test()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Test(Blog newBlog, IFormFile image)
        {
            if (newBlog == null)
            {
                return BadRequest("Invalid blog data.");
            }

            try
            {
                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(newBlog.Title), "Title");
                formData.Add(new StringContent(newBlog.Description), "Description");
                formData.Add(new StringContent(newBlog.UserId ?? "cb2030a7-9ce3-43a7-9827-39255dc1f185"), "UserId");
                formData.Add(new StringContent(newBlog.Popularity?.ToString() ?? "1"), "Popularity");
                formData.Add(new StringContent(newBlog.UpVote?.ToString() ?? "1"), "UpVote");
                formData.Add(new StringContent(newBlog.DownVote?.ToString() ?? "1"), "DownVote");

                if (image != null && image.Length > 0)
                {
                    var imageContent = new StreamContent(image.OpenReadStream());
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                    formData.Add(imageContent, "image", image.FileName); // Ensure the field name is "image"
                }

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync("https://localhost:7241/AddBlog", formData);

                    // Logging for troubleshooting
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
                // Log the exception
                Console.WriteLine($"Exception: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                return View(newBlog);
            }
        }



    }
}
