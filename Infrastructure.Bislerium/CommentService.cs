using Application.Bislerium;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Authorization;
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
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _db;

        public CommentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Comment> AddComment(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }

            var blog = await _db.Blogs.FindAsync(comment.BlogId);

            if (blog == null)
            {
                throw new KeyNotFoundException($"Blog with ID {comment.BlogId} not found.");
            }

            var result = await _db.Comments.AddAsync(comment);
            await _db.SaveChangesAsync();

            blog.CommentCount++;
            _db.Blogs.Update(blog);

            return result.Entity;
        }



        public async Task<Comment> DeleteComment(Guid Id)
        {
            var comment = await _db.Comments.FindAsync(Id);

            if (comment == null)
            {
                throw new ArgumentNullException($"{nameof(comment)}");
            }
            else
            {
                var result = _db.Comments.Remove(comment);
                await _db.SaveChangesAsync();
                return result.Entity;
            }

        }

        
        public async Task<IEnumerable<Comment>> GetAllComments()
        {
            var allComment = await _db.Comments.ToListAsync();
            return allComment;
        }

        public async Task<Comment> GetCommentById(Guid Id)
        {
            var comment = await _db.Comments
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == Id);

            return comment;
        }

        public async Task<Comment> UpdateComment(Comment comment)
        {
            var existingComment = await _db.Comments.FindAsync(comment.Id);

            if (existingComment == null)
            {
                throw new KeyNotFoundException("Student Not Found");
            }

            existingComment.Content = comment.Content;
            existingComment.UpVote = comment.UpVote;
            existingComment.DownVote = comment.DownVote;
            

            _db.SaveChanges();
            return existingComment;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByBlogId(Guid blogId)
        {
            var result = await _db.Comments.Where(c => c.BlogId == blogId).ToListAsync();
            return result;

        }


        public async Task<Comment> UpdateCommentUpVote(Guid commentId, string userId)
        {
            var comment = await _db.Comments.FindAsync(commentId);

            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }

            var existingUpVote = await _db.CommentUpVotes.FirstOrDefaultAsync(u => u.CommentId == commentId && u.UserId == userId);
            if (existingUpVote == null)
            {
                var existingDownVote = await _db.CommentDownVotes.FirstOrDefaultAsync(d => d.CommentId == commentId && d.UserId == userId);
                if (existingDownVote != null)
                {
                    _db.CommentDownVotes.Remove(existingDownVote);
                    comment.DownVote--;
                }

                var newUpVote = new CommentUpVote
                {
                    Id = Guid.NewGuid(),
                    UpVoteDate = DateTime.Now,
                    UserId = userId,
                    CommentId = commentId
                };
                _db.CommentUpVotes.Add(newUpVote);
                comment.UpVote++;
            }
            else
            {
                _db.CommentUpVotes.Remove(existingUpVote);
                comment.UpVote--;
            }

            _db.Update(comment);
            await _db.SaveChangesAsync();

            return comment;
        }

        public async Task<Comment> UpdateCommentDownVote(Guid commentId, string userId)
        {
            var comment = await _db.Comments.FindAsync(commentId);

            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }

            var existingDownVote = await _db.CommentDownVotes.FirstOrDefaultAsync(d => d.CommentId == commentId && d.UserId == userId);
            if (existingDownVote == null)
            {
                var existingUpVote = await _db.CommentUpVotes.FirstOrDefaultAsync(u => u.CommentId == commentId && u.UserId == userId);
                if (existingUpVote != null)
                {
                    _db.CommentUpVotes.Remove(existingUpVote);
                    comment.UpVote--;
                }

                var newDownVote = new CommentDownVote
                {
                    Id = Guid.NewGuid(),
                    DownVoteDate = DateTime.Now,
                    UserId = userId,
                    CommentId = commentId
                };
                _db.CommentDownVotes.Add(newDownVote);
                comment.DownVote++;
            }
            else
            {
                _db.CommentDownVotes.Remove(existingDownVote);
                comment.DownVote--;
            }

            _db.Update(comment);
            await _db.SaveChangesAsync();

            return comment;
        }
    }
}


