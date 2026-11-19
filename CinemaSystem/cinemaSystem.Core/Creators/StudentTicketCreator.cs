using CinemaSystem.Core.Tickets;

namespace CinemaSystem.Core.Creators;

public class StudentTicketCreator : TicketCreator
{
    protected override Ticket CreateTicket(
        int showingId, decimal pricePaid)
    {
        return new StudentTicket(showingId, pricePaid);
    }
}