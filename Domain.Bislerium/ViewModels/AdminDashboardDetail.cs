using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Bislerium.ViewModels
{
    public class AdminDashboardDetail
    {

        public int TotalBlogPosts { get; set; }
        public int TotalUpvotes { get; set; }
        public int TotalDownvotes { get; set; }
        public int TotalComments { get; set; }
        //public Dictionary<string, int> MonthlyData { get; set; }

        public int MonthTotalBlogPosts { get; set; }
        public int MonthTotalUpvotes { get; set; }
        public int MonthTotalDownvotes { get; set; }
        public int MonthTotalComments { get; set; }

    }
}
