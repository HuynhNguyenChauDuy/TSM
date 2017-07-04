using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TSM.Data.Migrations
{
    public partial class Mig_04 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Leaves",
                newName: "ApplicationUserID");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserID",
                table: "Leaves",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_ApplicationUserID",
                table: "Leaves",
                column: "ApplicationUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_AspNetUsers_ApplicationUserID",
                table: "Leaves",
                column: "ApplicationUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_AspNetUsers_ApplicationUserID",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_ApplicationUserID",
                table: "Leaves");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserID",
                table: "Leaves",
                newName: "UserID");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "Leaves",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
