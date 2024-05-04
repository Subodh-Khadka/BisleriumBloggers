﻿using Application.Bislerium;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Bislerium.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlogService _blogService;

        public BlogController(ApplicationDbContext db, IBlogService blogService)
        {
            _db = db;
            _blogService = blogService;
        }

        [HttpPost("AddBlog")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddBlog([FromForm] Blog blog, IFormFile image)
        {
            blog.Id = Guid.NewGuid();
            blog.CreatedDateTime = DateTime.Now;
            blog.UpdatedDateTime = DateTime.Now;

            if (blog == null)
            {
                return BadRequest("Blog object is null.");
            }

            var newBlog = await _blogService.AddBlog(blog,image);
            return Ok(newBlog);
        }

        [HttpGet("GetAllBlogs")]
        public async Task<IActionResult> GetAllBlogs()
        {
            var blogs = await _blogService.GetAllBlogs();
            return Ok(blogs);
        }

        [HttpGet("GetBlogById/{id}")]
        public async Task<IActionResult> GetBlogById(Guid id)
        {
            var blog = await _blogService.GetBlogById(id);
            if (blog == null)
            {
                return NotFound(); 
            }
            return Ok(blog);
        }

        [HttpPut("UpdateBlog")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBlog([FromBody] Blog blog, IFormFile image)
        {

            if (blog == null)
            {
                return BadRequest("Invalid blog data");
            }

            var updatedBlog = await _blogService.UpdateBlog(blog, image);
            return Ok(updatedBlog);
        }


        [HttpPut("UpdateBlogVotes")]
        public async Task<IActionResult> UpdateBlogVotes([FromBody] Blog blog)
        {
            if (blog == null)
            {
                return BadRequest("Invalid blog data");
            }

            var updatedBlog = await _blogService.UpdateBlogVotes(blog);
            return Ok(updatedBlog);
        }

        [HttpDelete("DeleteBlog/{id}")]
        public async Task<IActionResult> DeleteBlog(Guid id)
        {
            var result = await _blogService.DeleteBlog(id);
            return Ok("BlogDeleted"); 
        }

    }
}
