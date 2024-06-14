namespace MVCImage.Models
{
    public class Image
    {
        public int ImageId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } // Thêm thuộc tính này
        public DateTime UploadDate { get; set; }
        public int CategoryId { get; set; }
        public int Rating { get; set; } // Đánh giá từ 1 đến 5


    }
}
