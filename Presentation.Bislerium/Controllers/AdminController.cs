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

                    return Ok(monthlyData);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        //[HttpGet("dashboard")]
        //public async Task<IActionResult> GetAdminDashboardData()
        //{
        //    try
        //    {
        //        var allTimeData = await _adminService.GetAllTimeData();

        //        if (allTimeData == null)
        //        {
        //            return NotFound("Dashboard data not found.");
        //        }

        //        return Ok(allTimeData);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}


        //[HttpGet("monthlydata")]
        //public async Task<IActionResult> GetMonthlyData(string month)
        //{
        //    try
        //    {
        //        // Call the service method to get monthly dashboard data
        //        var monthlyData = await _adminService.GetMonthlyDashboardData(month);

        //        if (monthlyData == null)
        //        {
        //            return NotFound("Monthly data not found.");
        //        }

        //        return Ok(monthlyData);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}
    }

}
