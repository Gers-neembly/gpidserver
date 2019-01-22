using Microsoft.EntityFrameworkCore.Migrations;

namespace Neembly.GPIDServer.Persistence.Migrations
{
    public partial class registerStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationStatus",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationStatus",
                table: "AspNetUsers");
        }
    }
}
