using Application.Bislerium;
using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Infrastructure.Bislerium.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Presentation.Bislerium.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : Controller
    {


        private readonly ApplicationDbContext _db;
        private readonly IHistoryService _historyService;

        public HistoryController(ApplicationDbContext db, IHistoryService historyService)
        {
            _db = db;
            _historyService = historyService;
        }

        [HttpGet("{entityType}/{entityId}")]
        public async Task<IActionResult> GetEntityHistory(string entityType, Guid entityId)
        {
            try
            {
                var history = await _historyService.GetEntityHistory(entityId, entityType);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving history: {ex.Message}");
            }
        }

        [HttpPost("AddHistory")]
        public async Task<IActionResult> AddHistory([FromBody] UpdateHistory history)
        {
            try
            {
                await _historyService.AddHistory(history);
                return Ok("History added successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding history: {ex.Message}");
            }
        }

        [HttpGet("GetAllHistory")]
        public async Task<IActionResult> GetAllHistory()
        {
            try
            {
                var history = await _historyService.GetAllHistory();
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving all history: {ex.Message}");
            }
        }

    }

}

