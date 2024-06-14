using Microsoft.AspNetCore.Http;

namespace MVCImage.Models
{
    public class ImageUploadDto
    {
        public int ImageId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } // Thêm thuộc tính này
        public int CategoryId { get; set; }
    }
}
