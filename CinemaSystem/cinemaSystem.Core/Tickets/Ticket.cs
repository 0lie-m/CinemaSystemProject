namespace CinemaSystem.Core.Tickets;

public abstract class Ticket
{
    public Guid Id { get; } = Guid.NewGuid();
    public int ShowingId { get; }
    public decimal PricePaid { get; }

    protected Ticket(int showingId, decimal pricePaid)
    {
        ShowingId = showingId;
        PricePaid = pricePaid;
    }
}