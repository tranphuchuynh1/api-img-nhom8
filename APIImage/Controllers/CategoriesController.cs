using APIImage.Data;
using APIImage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ImageContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ImageContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles ="Read,Write")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.Include(c => c.Images).ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles ="Read,Write")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.Include(c => c.Images).FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                _logger.LogWarning("Category with id {CategoryId} not found", id);
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        [Authorize(Roles = "Write,Read")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            if (category == null)
            {
                return BadRequest("Category is null");
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Write,Read")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest("Category ID mismatch");
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Write,Read")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
