using Application.Bislerium;
using Infrastructure.Bislerium.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Bislerium
{
    public class ReactionService : IReactionService
    {
        private ApplicationDbContext _db;
            
        public ReactionService(ApplicationDbContext db) 
        {
            _db = db;
        }

        public async Task<int> GetUpvoteCountForBlog(Guid blogId)
        {
            var count = await _db.Uovotes.CountAsync(upvote => upvote.BlogId == blogId);
            return count;
        }

        public async Task<int> GetDownvoteCountForBlog(Guid blogId)
        {
            var count = await _db.DownVotes.CountAsync(downvote => downvote.BlogId == blogId);
            return count;
        }

        public async Task<int> GetUpvoteCountForComment(Guid commentId)
        {
            var count = await _db.CommentUpVotes.CountAsync(upvote => upvote.CommentId == commentId);
            return count;
        }

        public async Task<int> GetDownvoteCountForComment(Guid commentId)
        {
            var count = await _db.CommentDownVotes.CountAsync(downvote => downvote.CommentId == commentId);
            return count;
        }
    }
}
