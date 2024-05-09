using Application.Bislerium;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Bislerium.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetAdminDashboardData(string month)
        {
            try
            {
                if (string.IsNullOrEmpty(month))
                {
                    // If month is not provided, fetch all-time data
                    var allTimeData = await _adminService.GetAllTimeData();
                    if (allTimeData == null)
                    {
                        return NotFound("Dashboard data not found.");
                    }

                    
                    return Ok(allTimeData);
                }
                else
                {
                    // If month is provided, fetch monthly data
                    var monthlyData = await _adminService.GetMonthlyData(month);
                    if (monthlyData == null)
                    {
                        return NotFound($"Dashboard data for {month} not found.");
                    }

                    ViewBag.IsAllTime = false;
                    return Ok(monthlyData);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }

}
