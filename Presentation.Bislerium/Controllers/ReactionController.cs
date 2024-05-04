using Application.Bislerium;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Bislerium.Controllers
{
    public class ReactionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IReactionService _reactionService;

        public ReactionController(ApplicationDbContext db, IReactionService reactionService)
        {
            _db = db;
            _reactionService = reactionService;
        }

        [HttpPost("AddReaction")]
        public async Task<IActionResult> AddReaction([FromBody] Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException();

            var newReaction = await _reactionService.AddReaction(reaction);
            return Ok(newReaction);
        }
    }
}
