using Domain.Bislerium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Bislerium
{
    public interface ICommentService
    {
        Task<Comment> AddComment(Comment comment);
        Task<IEnumerable<Comment>> GetAllComments();
        Task<Comment> GetCommentById(Guid Id);
        Task<Comment> UpdateComment(Comment comment);
        Task<Comment> DeleteComment(Guid Id);  
        Task<IEnumerable<Comment>> GetCommentsByBlogId(Guid BlogId);
        Task<Comment> UpdateCommentUpVote(Guid id, string userId);
        Task<Comment> UpdateCommentDownVote(Guid id, string userId);

        Task<IEnumerable<Comment>> GetCommentsByMonth(string month);
    }
}
