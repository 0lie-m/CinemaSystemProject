using CinemaSystem.Core.Tickets;

namespace CinemaSystem.Core.Creators;

public class AdultTicketCreator : TicketCreator
{
    protected override Ticket CreateTicket(
        int showingId, decimal pricePaid)
    {
        return new AdultTicket(showingId, pricePaid);
    }
}