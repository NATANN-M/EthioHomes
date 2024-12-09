namespace EthioHomes.Models
{
    public class Property
    {
        public int Id { get; set; }
        public int UserId { get; set; } // not included in Ui used as Forign key
        public string Title { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public string PropertyType { get; set; }
        public string Status { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string Description { get; set; }
    }
}
