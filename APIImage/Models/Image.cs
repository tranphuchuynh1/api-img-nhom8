namespace APIImage.Models.Domain
{
    public class Image
    {
        public int ImageId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } // Add this property
        public DateTime UploadDate { get; set; }
        public int CategoryId { get; set; }
    }
}
