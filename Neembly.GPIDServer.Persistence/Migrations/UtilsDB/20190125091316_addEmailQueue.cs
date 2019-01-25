using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Neembly.GPIDServer.Persistence.Migrations.UtilsDB
{
    public partial class addEmailQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OperatorId = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(maxLength: 100, nullable: true),
                    IsHtml = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Receipients = table.Column<string>(maxLength: 2000, nullable: true),
                    CarbonCopies = table.Column<string>(maxLength: 2000, nullable: true),
                    BlindCarbonCopies = table.Column<string>(maxLength: 2000, nullable: true),
                    Sender = table.Column<string>(maxLength: 256, nullable: true),
                    Status = table.Column<string>(maxLength: 256, nullable: true),
                    FailedMessage = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueues", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailQueues");
        }
    }
}
