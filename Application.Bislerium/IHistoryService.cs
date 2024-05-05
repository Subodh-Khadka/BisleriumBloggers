using Domain.Bislerium;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Bislerium
{
    public interface IHistoryService
    {
        Task AddHistory(UpdateHistory history);
        Task<IEnumerable<UpdateHistory>> GetEntityHistory(Guid entityId, string entityType);
        Task<IEnumerable<UpdateHistory>> GetAllHistory();
    }
}
