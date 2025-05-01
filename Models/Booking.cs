namespace EthioHomes.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PropertyId { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }  // Pending, Approved, Rejected
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Property Property { get; set; } //  access to Property details

    }

}

