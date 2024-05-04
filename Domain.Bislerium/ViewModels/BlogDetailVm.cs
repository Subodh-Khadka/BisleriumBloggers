using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Bislerium.ViewModels
{
    public class BlogDetailVm
    {
        public string? UserId { get; set; }
        public Blog Blog { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
