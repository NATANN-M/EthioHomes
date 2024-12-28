namespace EthioHomes.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public string PropertyType { get; set; }
        public string Status { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; } // Foreign key to Users table

        // List of associated image paths
        public List<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public List<string> ImagePaths { get; set; }
    }
}
