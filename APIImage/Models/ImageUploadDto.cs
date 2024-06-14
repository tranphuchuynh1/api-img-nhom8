namespace APIImage.Models.DTO
{
    public class ImageUploadDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } // Add this property for image URL
        public int CategoryId { get; set; }
    }
}
