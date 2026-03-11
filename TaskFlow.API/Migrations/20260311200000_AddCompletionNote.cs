using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.API.Migrations
{
    public partial class AddCompletionNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletionNote",
                table: "Tasks",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionNote",
                table: "Tasks");
        }
    }
}
