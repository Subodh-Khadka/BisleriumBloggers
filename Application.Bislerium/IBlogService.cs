using Domain.Bislerium;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Bislerium
{
    public interface IBlogService
    {
        Task<Blog> AddBlog(Blog blog, IFormFile image);
        Task<IEnumerable<Blog>> GetAllBlogs();
        Task<Blog> GetBlogById(Guid id);
        Task<Blog> UpdateBlog( Guid blogId, Blog blog, IFormFile image);
        Task<Blog> DeleteBlog(Guid id);
        Task<Blog> UpdateBlogUpVote(Guid id, string userId); 
        Task<Blog> UpdateBlogDownVote(Guid id, string userId);
        Task<IEnumerable<Blog>> GetAllBlogsUserId(string userId);
        Task<IEnumerable<Blog>> GetSortedBlogs(string sortBy);
        Task<double> CalculateBlogPopularity(Guid blogId);

        Task<IEnumerable<Blog>> GetBlogsByMonth(string month);

        Task<IEnumerable<Blog>> GetTop10BlogPosts(); 
        Task<IEnumerable<Blog>> GetTop10Bloggers();

        Task<IEnumerable<Blog>> GetTop10BlogPosts(string month);
        Task<IEnumerable<Blog>> GetTop10Bloggers(string month);


    }
}
