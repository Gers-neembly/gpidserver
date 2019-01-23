using Microsoft.EntityFrameworkCore.Migrations;

namespace Neembly.GPIDServer.Persistence.Migrations
{
    public partial class ProfileMobileNo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileNo",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePrefix",
                table: "Players",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileNo",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "MobilePrefix",
                table: "Players");
        }
    }
}
