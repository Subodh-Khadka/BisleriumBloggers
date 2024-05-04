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
        Task<Blog> UpdateBlog(Blog blog, IFormFile image);
        Task<Blog> DeleteBlog(Guid id);
        Task<Blog> UpdateBlogUpVote(Guid id); 
        Task<Blog> UpdateBlogDownVote(Guid id); 
    }
}
