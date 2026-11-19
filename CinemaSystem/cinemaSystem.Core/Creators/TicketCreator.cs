using CinemaSystem.Core.Tickets;

namespace CinemaSystem.Core.Creators;

public abstract class TicketCreator
{
    public Ticket IssueTicket(int showingId, decimal pricePaid)
    {
        if (showingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(showingId));
        }

        if (pricePaid < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pricePaid));
        }

        return CreateTicket(showingId, pricePaid);
    }

    protected abstract Ticket CreateTicket(
        int showingId, decimal pricePaid);
}