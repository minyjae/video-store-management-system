using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_ticket_booking_system.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Movies_Title",
                table: "Movies");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Title",
                table: "Movies",
                column: "Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Movies_Title",
                table: "Movies");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Title",
                table: "Movies",
                column: "Title");
        }
    }
}
