using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaSystem.Booking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingsAndTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShowingId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Showings_ShowingId",
                        column: x => x.ShowingId,
                        principalTable: "Showings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookedTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookingId = table.Column<int>(type: "INTEGER", nullable: false),
                    TicketType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PricePaid = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookedTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookedTickets_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookedTickets_BookingId",
                table: "BookedTickets",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ShowingId",
                table: "Bookings",
                column: "ShowingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookedTickets");

            migrationBuilder.DropTable(
                name: "Bookings");
        }
    }
}
