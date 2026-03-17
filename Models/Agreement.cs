namespace EthioHomes.Models
{
    public class Agreement
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int OwnerId { get; set; }
        public DateTime AgreementDate { get; set; }
        public bool IsAccepted { get; set; }
    }

}
