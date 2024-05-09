using Application.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium;
using Infrastructure.Bislerium.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Bislerium.ViewModels.AdminDashboardDetail;
using static System.Net.Mime.MediaTypeNames;

namespace Infrastructure.Bislerium
{
    public class AdminService : IAdminService
    {
        private readonly IBlogService _blogService;
        private readonly ICommentService _commentService;
        private readonly ApplicationDbContext _db;

        public AdminService(IBlogService blogService, ICommentService commentService, ApplicationDbContext db)
        {
            _blogService = blogService;
            _commentService = commentService;
            _db = db;
        }

        public async Task<AdminDashboardDetail> GetAllTimeData()
        {
            var allBlogs = await _blogService.GetAllBlogs();
            var totalBlogPosts = allBlogs.Count();

            var totalUpvotes = allBlogs.Sum(blog => blog.UpVote);
            var totalDownvotes = allBlogs.Sum(blog => blog.DownVote);

            var allComments = _commentService.GetAllComments().Result;
            var totalComments = allComments.Count();

            var dashboardData = new AdminDashboardDetail
            {
                TotalBlogPosts = totalBlogPosts,
                TotalUpvotes = (int)totalUpvotes,
                TotalDownvotes = (int)totalDownvotes,
                TotalComments = totalComments
            };
            return dashboardData;
        }

        public async Task<AdminDashboardDetail> GetMonthlyData(string month)
        {
            if (!string.IsNullOrEmpty(month))
            {
                var monthlyBlogs = await _blogService.GetBlogsByMonth(month);
                var monthlyBlogPosts = monthlyBlogs.Count();
                var monthlyUpvotes = monthlyBlogs.Sum(blog => blog.UpVote);
                var monthlyDownvotes = monthlyBlogs.Sum(blog => blog.DownVote);

                var monthlyComments = await _commentService.GetCommentsByMonth(month);
                var monthlyCommentCount = monthlyComments.Count();

                // Create the dashboard data object for the specified month
                var dashboardData = new AdminDashboardDetail
                {
                    MonthTotalBlogPosts = monthlyBlogPosts,
                    MonthTotalComments = monthlyCommentCount
                };

                //    MonthTotalUpvotes = monthlyUpvotes,
                //    MonthTotalDownvotes = monthlyDownvotes,

                return dashboardData;
            }
            else
            {
                // If month is not provided, return all-time data
                return await GetAllTimeData();
            }
        }
    }
}



