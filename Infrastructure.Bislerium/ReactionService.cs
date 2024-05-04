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

         public async Task<Reaction> AddReaction(Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException();

            var result = await _db.Reactions.AddAsync(reaction);
            await _db.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<IEnumerable<Reaction>> GetAllReactions()
        {
            var result = await _db.Reactions.ToListAsync();
            return result;
        }
    }
}
