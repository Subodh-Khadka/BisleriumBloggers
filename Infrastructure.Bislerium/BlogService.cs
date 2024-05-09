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

        public async Task<Blog> UpdateBlogUpVote(Guid blogId, string userId)
        {
            var blog = await _db.Blogs.FindAsync(blogId);
            if (blog == null)
            {
                throw new KeyNotFoundException("Blog not found");
            }

            var existingUpVote = await _db.Uovotes.FirstOrDefaultAsync(u => u.BlogId == blogId && u.UserId == userId);
            if (existingUpVote == null)
            {
                var existingDownVote = await _db.DownVotes.FirstOrDefaultAsync(d => d.BlogId == blogId && d.UserId == userId);
                if (existingDownVote != null)
                {
                    _db.DownVotes.Remove(existingDownVote);
                    blog.DownVote--;
                }

                var newUpVote = new UpVote
                {
                    Id = Guid.NewGuid(),
                    UpVoteDate = DateTime.Now,
                    UserId = userId,
                    BlogId = blogId
                };
                _db.Uovotes.Add(newUpVote);
                blog.UpVote++;
            }
            else
            {
                _db.Uovotes.Remove(existingUpVote);
                blog.UpVote--;
            }

            _db.Update(blog);
            await _db.SaveChangesAsync();
            return blog;
        }

        public async Task<Blog> UpdateBlogDownVote(Guid blogId, string userId)
        {
            var blog = await _db.Blogs.FindAsync(blogId);
            if (blog == null)
            {
                throw new KeyNotFoundException("Blog not found");
            }

            var existingDownVote = await _db.DownVotes.FirstOrDefaultAsync(d => d.BlogId == blogId && d.UserId == userId);
            if (existingDownVote == null)
            {
                
                var existingUpVote = await _db.Uovotes.FirstOrDefaultAsync(u => u.BlogId == blogId && u.UserId == userId);
                if (existingUpVote != null)
                {
                    _db.Uovotes.Remove(existingUpVote);
                    blog.UpVote--;
                }

                var newDownVote = new DownVote
                {
                    Id = Guid.NewGuid(),
                    DownVoteDate = DateTime.Now,
                    UserId = userId,
                    BlogId = blogId
                };
                _db.DownVotes.Add(newDownVote);
                blog.DownVote++;
            }
            else
            {
                _db.DownVotes.Remove(existingDownVote);
                blog.DownVote--;
            }

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
            
        public async Task<double> CalculateBlogPopularity(Guid blogId)
        {
            var blog = await _db.Blogs.FindAsync(blogId);

            if (blog == null)
            {
                throw new KeyNotFoundException($"Blog with ID {blogId} not found.");
            }

            const int upvoteWeightage = 2;
            const int downvoteWeightage = -1;
            const int commentWeightage = 1;

            double popularity = upvoteWeightage * (blog.UpVote ?? 0) +
                                downvoteWeightage * (blog.DownVote ?? 0) +
                                commentWeightage * (blog.CommentCount ?? 0);

            blog.Popularity = popularity;
            _db.Blogs.Update(blog);
            await _db.SaveChangesAsync();

            return popularity;
        }

        //for admin dashboard
        public async Task<IEnumerable<Blog>> GetBlogsByMonth(string month)
        {
            if (!int.TryParse(month, out int monthNumber))
            {
                throw new ArgumentException("Invalid month format.");
            }

            var blogs = await _db.Blogs
                .Where(b => b.CreatedDateTime.Month == monthNumber)
                .Include(b => b.User)
                .ToListAsync();

            return blogs;
        }

        public async Task<IEnumerable<Blog>> GetTop10BlogPosts()
        {
            var top10Posts = await _db.Blogs.OrderByDescending(b => b.Popularity).Take(10).ToListAsync();
            return top10Posts;
        }

        public async Task<IEnumerable<Blog>> GetTop10Bloggers()
        {
            var top10Bloggers = await _db.Blogs
                .GroupBy(b => b.UserId)
                .Select(g => new { UserId = g.Key, Popularity = g.Sum(b => b.Popularity) })
                .OrderByDescending(g => g.Popularity)
                .Take(10)
                .SelectMany(g => _db.Blogs.Where(b => b.UserId == g.UserId))
                .Include(u => u.User)
                .ToListAsync();
 
            top10Bloggers = top10Bloggers.GroupBy(b => b.UserId).Select(g => g.First()).ToList();

            return top10Bloggers;
        }

        public async Task<IEnumerable<Blog>> GetTop10BlogPosts(string month)
        {
            var blogs = await GetBlogsByMonth(month);
            var top10Posts = blogs.OrderByDescending(b => b.Popularity).Take(10).ToList();
            return top10Posts;
        }

        public async Task<IEnumerable<Blog>> GetTop10Bloggers(string month)
        {
            var blogs = await GetBlogsByMonth(month);
            var top10Bloggers = blogs
                .GroupBy(b => b.UserId)
                .OrderByDescending(g => g.Sum(b => b.Popularity))
                .Take(10)
                .SelectMany(g => g)
                .ToList();

            foreach (var blog in top10Bloggers)
            {
                _db.Entry(blog).Reference(b => b.User).Load();
            }

            top10Bloggers = top10Bloggers.GroupBy(b => b.UserId).Select(g => g.First()).ToList();

            return top10Bloggers;
        }


    }
}

