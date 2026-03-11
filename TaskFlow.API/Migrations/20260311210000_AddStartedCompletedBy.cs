using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.API.Migrations
{
    public partial class AddStartedCompletedBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StartedById",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedById",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StartedById",
                table: "Tasks",
                column: "StartedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CompletedById",
                table: "Tasks",
                column: "CompletedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_StartedById",
                table: "Tasks",
                column: "StartedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_CompletedById",
                table: "Tasks",
                column: "CompletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_StartedById",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_CompletedById",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_StartedById",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CompletedById",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StartedById",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CompletedById",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Tasks");
        }
    }
}
