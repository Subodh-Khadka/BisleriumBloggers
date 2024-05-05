using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Bislerium
{
    public class UpdateHistory
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; } 
        public string? EntityType { get; set; } 
        public string? Action { get; set; } 
        public string? UpdatedBy { get; set; } 
        public DateTime? UpdatedDate { get; set; } 
        public string? OldVContent { get; set; } 
        public string? NewContent { get; set; }
    }
}
