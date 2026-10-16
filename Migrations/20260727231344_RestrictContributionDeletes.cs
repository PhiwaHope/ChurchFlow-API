using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchFlow.api.Migrations
{
    /// <inheritdoc />
    public partial class RestrictContributionDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_Events_EventId",
                table: "Contributions");

            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_Members_MemberId",
                table: "Contributions");

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_Events_EventId",
                table: "Contributions",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_Members_MemberId",
                table: "Contributions",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_Events_EventId",
                table: "Contributions");

            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_Members_MemberId",
                table: "Contributions");

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_Events_EventId",
                table: "Contributions",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_Members_MemberId",
                table: "Contributions",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
