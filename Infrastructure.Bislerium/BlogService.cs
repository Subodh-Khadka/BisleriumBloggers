using Application.Bislerium;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Bislerium
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _db;

        public BlogService(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Blog> AddBlog(Blog blog, IFormFile image)
        {
            if (blog == null)
            {
                throw new ArgumentNullException(nameof(blog));
            }

            if (image != null && image.Length > 0)
            {

                var fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine("wwwroot", "Images", "BlogImages", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                blog.Image = $"/Images/BlogImages/{fileName}";
            }

            var result = await _db.Blogs.AddAsync(blog);
            await _db.SaveChangesAsync();
            return result.Entity;
        }


        public async Task<Blog> DeleteBlog(Guid id)
        {
            var blog = await _db.Blogs.FindAsync(id);

            if (blog == null)
            {
                throw new ArgumentNullException($"{nameof(blog)}");
            }
            else
            {
                var result = _db.Blogs.Remove(blog);
                await _db.SaveChangesAsync();
                return result.Entity;
            }

        }

        public async Task<IEnumerable<Blog>> GetAllBlogs()
        {
            var allBlog = await _db.Blogs.ToListAsync();
            return allBlog;
        }

        public async Task<Blog> GetBlogById(Guid id)
        {
            var blog = await _db.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            return blog;
        }

        public async Task<Blog> UpdateBlog(Guid blogId, Blog blog, IFormFile image)
        {
            var existingBlog = await _db.Blogs.FindAsync(blogId);

            if (existingBlog == null)
            {
                throw new KeyNotFoundException("Blog not found");
            }

            existingBlog.Title = blog.Title;
            existingBlog.Description = blog.Description;
            existingBlog.UpVote = blog.UpVote;
            existingBlog.DownVote = blog.DownVote;
            existingBlog.UpdatedDateTime = DateTime.Now;
            existingBlog.CreatedDateTime = existingBlog.CreatedDateTime;

            // Check if a new image is provided
            if (image != null && image.Length > 0)
            {
                var fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine("wwwroot", "Images", "BlogImages", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                existingBlog.Image = $"/Images/BlogImages/{fileName}";
            }

            _db.Update(existingBlog);
            await _db.SaveChangesAsync();
            return existingBlog;
        }

        public async Task<Blog> UpdateBlogUpVote(Guid blogId)
        {
            var blog = await _db.Blogs.FindAsync(blogId);

            if (blog == null)
            {
                throw new KeyNotFoundException("Blog not found");
            }

            blog.UpVote++;

            _db.Update(blog);
            await _db.SaveChangesAsync();

            return blog;
        }


        public async Task<Blog> UpdateBlogDownVote(Guid blogId)
        {
            var blog = await _db.Blogs.FindAsync(blogId);

            if (blog == null)
            {
                throw new KeyNotFoundException("Blog not found");
            }

            blog.DownVote++;

            _db.Update(blog);
            await _db.SaveChangesAsync();

            return blog;
        }

        public async Task<IEnumerable<Blog>> GetAllBlogsUserId(string userId)
        {
            
            var blogs = await _db.Blogs
                .Where(b => b.UserId == userId) 
                .ToListAsync();

            return blogs;
        }

        public async Task<IEnumerable<Blog>> GetSortedBlogs(string sortBy)
        {
            IQueryable<Blog> query = _db.Blogs.AsQueryable();

            switch (sortBy.ToLower())
            {
                case "popularity":
                    query = query.OrderByDescending(b => b.Popularity);
                    break;
                case "recency":
                    query = query.OrderByDescending(b => b.CreatedDateTime);
                    break;
                case "random":
                    query = query.OrderBy(b => Guid.NewGuid());
                    break;
                default:
                    query = query.OrderByDescending(b => b.CreatedDateTime);
                    break;
            }

            return await query.ToListAsync();
        }


    }
}


