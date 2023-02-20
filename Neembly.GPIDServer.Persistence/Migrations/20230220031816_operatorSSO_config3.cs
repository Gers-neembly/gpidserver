using Microsoft.EntityFrameworkCore.Migrations;

namespace Neembly.GPIDServer.Persistence.Migrations
{
    public partial class operatorSSO_config3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "OperatorSSO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OperatorId",
                table: "OperatorSSO",
                nullable: false,
                defaultValue: 0);
        }
    }
}
