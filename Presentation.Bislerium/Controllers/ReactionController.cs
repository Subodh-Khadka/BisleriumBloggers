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

       
    }
}
