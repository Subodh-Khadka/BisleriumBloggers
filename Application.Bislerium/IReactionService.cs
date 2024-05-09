using Domain.Bislerium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Bislerium
{
    public interface IReactionService
    {
        Task<int> GetUpvoteCountForBlog(Guid blogId);
        Task<int> GetDownvoteCountForBlog(Guid blogId);
        Task<int> GetUpvoteCountForComment(Guid commentId);
        Task<int> GetDownvoteCountForComment(Guid commentId);
    }
}
