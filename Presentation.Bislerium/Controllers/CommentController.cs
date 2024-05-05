using Application.Bislerium;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Bislerium.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICommentService _commentService;

        public CommentController(ApplicationDbContext db, ICommentService commentService)
        {
            _db = db;
            _commentService = commentService;
        }


        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            

            if (comment == null)
            {
                return BadRequest("Comment Null");
            }

            var newComment = await _commentService.AddComment(comment);
            return Ok(newComment);
        }


        [HttpGet("GetAllComments")]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await _commentService.GetAllComments();
            return Ok(comments);
        }

        [HttpGet("GetCommentById/{id}")]
        public async Task<IActionResult> GetCommentById(Guid id)
        {
            var comment = await _commentService.GetCommentById(id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        [HttpPut("UpdateComment")]
        public async Task<IActionResult> UpdateComment([FromBody] Comment comment)
        {
            if (comment == null)
            {
                return BadRequest("Invalid blog data");
            }

            var updatedComment = await _commentService.UpdateComment(comment);
            return Ok(updatedComment);
        }

        [HttpDelete("DeleteComment/{id}")]
        public async Task<IActionResult> DeleteComment(Guid Id)
        {
            var result = await _commentService.DeleteComment(Id);
            return Ok("CommentDeleted");
        }

        [HttpGet("GetCommentsByBlogId")]
        public async Task<IActionResult> GetCommentsByBlogId(Guid blogId)
        {
            var comments = await _commentService.GetCommentsByBlogId(blogId);
            return Ok(comments);
        }

        [HttpPut("UpdateCommentUpVote/{commentId}")]
        public async Task<IActionResult> UpdateCommentUpVote(Guid commentId,string userId)
        {
            try
            {
                var updateComment = await _commentService.UpdateCommentUpVote(commentId,userId);
                return Ok(updateComment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Blog not found");
            }
        }

        [HttpPut("UpdateCommentDownVote/{commentId}")]
        public async Task<IActionResult> UpdateCommentDownVote(Guid commentId, string userId)
        {
            try
            {
                var updateComment = await _commentService.UpdateCommentDownVote(commentId, userId);
                return Ok(updateComment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Blog not found");
            }
        }

    }
}
