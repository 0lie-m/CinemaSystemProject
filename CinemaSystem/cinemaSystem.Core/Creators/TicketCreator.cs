using CinemaSystem.Core.Tickets;

namespace CinemaSystem.Core.Creators;

public abstract class TicketCreator
{
    public Ticket IssueTicket(int showingId, decimal pricePaid)
    {
        // Validate shared inputs before delegating to a concrete creator.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(showingId);
        ArgumentOutOfRangeException.ThrowIfNegative(pricePaid);

        return CreateTicket(showingId, pricePaid);
    }

    protected abstract Ticket CreateTicket(
        int showingId, decimal pricePaid);
}