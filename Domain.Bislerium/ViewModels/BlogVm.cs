using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Bislerium.ViewModels
{
    public class BlogVm
    {
        public Guid id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }     
        public double Popularity { get; set; }
    }
}
