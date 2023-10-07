using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Neembly.GPIDServer.Persistence.Migrations
{
    public partial class operatorSSO_config : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperatorSSO",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OperatorId = table.Column<int>(nullable: false),
                    AuthProvider = table.Column<string>(maxLength: 50, nullable: true),
                    Parameters = table.Column<string>(maxLength: 4096, nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorSSO", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatorSSO");
        }
    }
}
