using Application.Bislerium;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Http;
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
    public class HistoryService : IHistoryService
    {
        private readonly ApplicationDbContext _db;

        public HistoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddHistory(UpdateHistory history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            _db.UpdateHistories.Add(history);
            await _db.SaveChangesAsync();
        }

        public async  Task<IEnumerable<UpdateHistory>> GetAllHistory()
        {
            return await _db.UpdateHistories.ToListAsync();
        }

        public async Task<IEnumerable<UpdateHistory>> GetEntityHistory(Guid entityId, string entityType)
        {
            return await _db.UpdateHistories
                 .Where(history => history.EntityId == entityId && history.EntityType == entityType)
                 .ToListAsync();
        }
    }
}

