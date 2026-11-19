using CinemaSystem.Core.Creators;
using CinemaSystem.Core.Tickets;

namespace CinemaSystem.Tests;

public class TicketCreatorTests
{
    [Fact]
    public void IssueTicket_AdultCreator_ReturnsAdultTicket()
    {
        TicketCreator creator = new AdultTicketCreator();

        Ticket ticket = creator.IssueTicket(1, 12m);

        Assert.IsType<AdultTicket>(ticket);
        Assert.Equal(1, ticket.ShowingId);
        Assert.Equal(12m, ticket.PricePaid);
        Assert.NotEqual(Guid.Empty, ticket.Id);
    }

    [Fact]
    public void IssueTicket_StudentCreator_ReturnsStudentTicket()
    {
        TicketCreator creator = new StudentTicketCreator();

        Ticket ticket = creator.IssueTicket(2, 10.56m);

        Assert.IsType<StudentTicket>(ticket);
        Assert.Equal(2, ticket.ShowingId);
        Assert.Equal(10.56m, ticket.PricePaid);
        Assert.NotEqual(Guid.Empty, ticket.Id);
    }

    [Fact]
    public void IssueTicket_ChildCreator_ReturnsChildTicket()
    {
        TicketCreator creator = new ChildTicketCreator();

        Ticket ticket = creator.IssueTicket(3, 8.40m);

        Assert.IsType<ChildTicket>(ticket);
        Assert.Equal(3, ticket.ShowingId);
        Assert.Equal(8.40m, ticket.PricePaid);
        Assert.NotEqual(Guid.Empty, ticket.Id);
    }
    [Theory]
[InlineData(0)]
[InlineData(-1)]
public void IssueTicket_InvalidShowingId_ThrowsException(int showingId)
{
    TicketCreator creator = new AdultTicketCreator();

    Assert.Throws<ArgumentOutOfRangeException>(() =>
        creator.IssueTicket(showingId, 10m));
}

[Fact]
public void IssueTicket_NegativePrice_ThrowsException()
{
    TicketCreator creator = new StudentTicketCreator();

    Assert.Throws<ArgumentOutOfRangeException>(() =>
        creator.IssueTicket(1, -1m));
}

[Fact]
public void IssueTicket_TwoTickets_HaveDifferentIds()
{
    TicketCreator creator = new ChildTicketCreator();

    Ticket first = creator.IssueTicket(1, 7m);
    Ticket second = creator.IssueTicket(1, 7m);

    Assert.NotEqual(first.Id, second.Id);
}
}