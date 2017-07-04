using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TSM.Data.Migrations
{
    public partial class Mig_02 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LeaveType",
                table: "Leaves",
                newName: "LeaveTypeID");

            migrationBuilder.AlterColumn<string>(
                name: "LeaveTypeID",
                table: "Leaves",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_LeaveTypeID",
                table: "Leaves",
                column: "LeaveTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_LeaveTypes_LeaveTypeID",
                table: "Leaves",
                column: "LeaveTypeID",
                principalTable: "LeaveTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_LeaveTypes_LeaveTypeID",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_LeaveTypeID",
                table: "Leaves");

            migrationBuilder.RenameColumn(
                name: "LeaveTypeID",
                table: "Leaves",
                newName: "LeaveType");

            migrationBuilder.AlterColumn<string>(
                name: "LeaveType",
                table: "Leaves",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
