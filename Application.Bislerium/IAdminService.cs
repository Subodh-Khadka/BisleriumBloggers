using Domain.Bislerium.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Bislerium.ViewModels.AdminDashboardDetail;

namespace Application.Bislerium
{
    public interface IAdminService
    {
        Task<AdminDashboardDetail> GetAllTimeData();
        Task<AdminDashboardDetail> GetMonthlyData(string month);
    }
}
