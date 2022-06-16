using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class RevocationReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RevocationReason",
                table: "Certificates",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevocationReason",
                table: "Certificates");
        }
    }
}
