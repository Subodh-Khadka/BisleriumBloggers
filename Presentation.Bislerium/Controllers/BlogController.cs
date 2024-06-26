﻿using Application.Bislerium;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Bislerium.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlogService _blogService;
        private readonly IEmailSender _emailSender;

        public BlogController(ApplicationDbContext db, IBlogService blogService, IEmailSender emailSender)
        {
            _db = db;
            _blogService = blogService;
            _emailSender = emailSender;
        }

        [Authorize(Roles = "Blogger")]
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

        [Authorize]
        [HttpPut("UpdateBlog")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBlog(Guid blogId, Blog blog, IFormFile image)
        {

            if (blog == null)
            {
                return BadRequest("Invalid blog data");
            }

            var updatedBlog = await _blogService.UpdateBlog(blogId, blog, image);
            return Ok(updatedBlog);
        }


        [Authorize(Roles = "Blogger")]
        [HttpPut("UpdateBlogUpVote/{id}")]
        public async Task<IActionResult> UpdateBlogUpVote(Guid id, string userId)
        {
            var blog = await _db.Blogs.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);   
            var blogOwner = blog.User.Email;

            try
            {
                var updatedBlog = await _blogService.UpdateBlogUpVote(id, userId);
                await _emailSender.SendEmailAsync(blogOwner, "New Upvote", $"Your blog post has received a new upvote by {user.FullName}!");
                return Ok(updatedBlog);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Blog not found");
            }
        }


        [Authorize(Roles = "Blogger")]
        [HttpPut("UpdateBlogDownVote/{id}")]
        public async Task<IActionResult> UpdateBlogDownVote(Guid id, string userId)
        {
            var blog = await _db.Blogs.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            var blogOwner = blog.User.Email;

            try
            {
                var updatedBlog = await _blogService.UpdateBlogDownVote(id, userId);
                await _emailSender.SendEmailAsync(blogOwner, "New DownVote", $"Your blog post has received a new downvote by {user.FullName}!");
                return Ok(updatedBlog);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Blog not found");
            }
        }


        [Authorize(Roles = "Blogger")]
        [HttpDelete("DeleteBlog/{id}")]
        public async Task<IActionResult> DeleteBlog(Guid id)
        {
            var result = await _blogService.DeleteBlog(id);
            return Ok("BlogDeleted"); 
        }

        [Authorize(Roles ="Blogger")]
        [HttpGet("GetAllBlogsUserId/{userId}")]
        public async Task<IActionResult> GetAllBlogsUserId(string userId)
        {
            var result = await _blogService.GetAllBlogsUserId(userId);
            return Ok(result);
        }

        [HttpGet("GetSortedBlogs")]
        public async Task<IActionResult> GetSortedBlogs(string sortBy)
        {
            var sortedBlogs = await _blogService.GetSortedBlogs(sortBy);
            return Ok(sortedBlogs);
        }


        [HttpPut("CalculateBlogPopularity/{blogId}")]
        public async Task<IActionResult> CalculateBlogPopularity(Guid blogId)
        {
            try
            {
                var popularity = await _blogService.CalculateBlogPopularity(blogId);
                return Ok(popularity);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Blog with ID {blogId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while calculating blog popularity: {ex.Message}");
            }
        }

    }
}
