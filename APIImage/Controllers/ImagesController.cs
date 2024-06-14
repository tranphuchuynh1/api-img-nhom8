using APIImage.Data;
using APIImage.Models.Domain;
using APIImage.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly ImageContext _context;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(ImageContext context, ILogger<ImagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Read,Write")]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var images = await _context.Images
                .OrderBy(i => i.ImageId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(images);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Read,Write")]
        public async Task<ActionResult<Image>> GetImage(int id)
        {
            try
            {
                var image = await _context.Images.FindAsync(id);

                if (image == null)
                {
                    _logger.LogWarning("Image with id {ImageId} not found", id);
                    return NotFound();
                }

                return image;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image with id {ImageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Write,Read")]
        public async Task<ActionResult<Image>> PostImage([FromBody] ImageUploadDto imageDto)
        {
            if (string.IsNullOrEmpty(imageDto.ImageUrl))
                return BadRequest("ImageUrl is required.");

            var image = new Image
            {
                Title = imageDto.Title,
                Description = imageDto.Description,
                ImageUrl = imageDto.ImageUrl,
                CategoryId = imageDto.CategoryId,
                UploadDate = DateTime.UtcNow
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetImage", new { id = image.ImageId }, image);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Write,Read")]
        public async Task<IActionResult> PutImage(int id, Image image)
        {
            if (id != image.ImageId)
            {
                return BadRequest();
            }

            _context.Entry(image).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageExists(id))
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
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("search")]
        [Authorize(Roles = "Read,Write")]
        public async Task<ActionResult<IEnumerable<Image>>> SearchImages([FromQuery] string? title, [FromQuery] string? description, [FromQuery] int? categoryId)
        {
            var query = _context.Images.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(i => i.Title.Contains(title));
            }

            if (!string.IsNullOrEmpty(description))
            {
                query = query.Where(i => i.Description.Contains(description));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == categoryId.Value);
            }

            var images = await query.ToListAsync();
            return Ok(images);
        }

        private bool ImageExists(int id)
        {
            return _context.Images.Any(e => e.ImageId == id);
        }
    }
}
