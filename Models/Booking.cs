namespace EthioHomes.Models;

public class Booking
{
    public string Name { get; set; }
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PropertyId { get; set; }
    public DateTime BookingDate { get; set; }
    public string Status { get; set; }  // Pending, Approved, Rejected
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool AgreementAccepted { get; set; }
    public bool IsPaid { get; set; }

    public Property Property { get; set; }

      public string CheckInStatus { get; set; } // NEW
    public DateTime? CheckedInAt { get; set; } // NEW
    public DateTime? CheckedOutAt { get; set; } // NEW
    public string StayStatus { get; set; }

    public string RenterName { get; set; }
    public string PropertyName { get; set; }

}
