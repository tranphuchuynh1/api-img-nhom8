using Microsoft.AspNetCore.Mvc;
using APIImage.Data;
using APIImage.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace APIImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Write")]
    public class HistoriesController : ControllerBase
    {
        private readonly ImageContext _context;

        public HistoriesController(ImageContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<History>>> GetHistories()
        {
            return await _context.Histories.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<History>> PostHistory(History history)
        {
            _context.Histories.Add(history);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHistories), new { id = history.Id }, history);
        }

        // DELETE: api/Histories/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> Clear()
        {
            _context.Histories.RemoveRange(_context.Histories);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
