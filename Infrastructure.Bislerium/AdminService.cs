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

            //top 10 blog posts
            var top10BlogPosts = await _blogService.GetTop10BlogPosts();

            // top 10 bloggers
            var top10Bloggers = await _blogService.GetTop10Bloggers();

            var dashboardData = new AdminDashboardDetail
            {
                TotalBlogPosts = totalBlogPosts,
                TotalUpvotes = (int)totalUpvotes,
                TotalDownvotes = (int)totalDownvotes,
                TotalComments = totalComments,
                Top10BlogPosts = top10BlogPosts,
                Top10Bloggers = top10Bloggers,
            };
            return dashboardData;
        }

        public async Task<AdminDashboardDetail> GetMonthlyData(string month)
        {
            if (!string.IsNullOrEmpty(month))
            {
                var monthlyBlogs = await _blogService.GetBlogsByMonth(month);
                var monthlyBlogPosts = monthlyBlogs.Count();
                var monthlyBlogUpvotes = monthlyBlogs.Sum(blog => blog.UpVote);
                var monthlyBlogDownvotes = monthlyBlogs.Sum(blog => blog.DownVote);

                var monthlyComments = await _commentService.GetCommentsByMonth(month);
                var monthlyCommentCount = monthlyComments.Count();
                var monthlyCommentUpvotes = monthlyComments.Sum(comment => comment.UpVote);
                var monthlyCommentDownvotes = monthlyComments.Sum(comment => comment.DownVote);

                // Get the top 10 blog posts for the specific month
                var top10MonthlyBlogPosts = await _blogService.GetTop10BlogPosts(month);

                // Get the top 10 bloggers for the specific month
                var top10MonthlyBloggers = await _blogService.GetTop10Bloggers(month);

                var totalMonthlyUpvotes = monthlyBlogUpvotes + monthlyCommentUpvotes;
                var totalMonthlyDownvotes = monthlyBlogDownvotes + monthlyCommentDownvotes;
                var dashboardData = new AdminDashboardDetail
                {
                    MonthTotalBlogPosts = monthlyBlogPosts,
                    MonthTotalComments = monthlyCommentCount,
                    MonthTotalUpvotes = (int)totalMonthlyUpvotes,
                    MonthTotalDownvotes = (int)totalMonthlyDownvotes,
                    Top10MonthlyBlogPosts = top10MonthlyBlogPosts,
                    Top10MonthlyBloggers = top10MonthlyBloggers
                };

                return dashboardData;
            }
            else
            {
                return await GetAllTimeData();
            }
        }
    }
}

