namespace CinemaSystem.Core.Tickets;

public class StudentTicket : Ticket
{
    public StudentTicket(int showingId, decimal pricePaid)
        : base(showingId, pricePaid)
    {
    }
}