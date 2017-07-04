using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TSM.Data.Migrations
{
    public partial class Mig_05 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Today",
                table: "Leaves",
                newName: "ToDate");

            migrationBuilder.RenameColumn(
                name: "FromDay",
                table: "Leaves",
                newName: "FromDate");

            migrationBuilder.RenameColumn(
                name: "ApprovedDay",
                table: "Leaves",
                newName: "ApprovedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "Leaves",
                newName: "Today");

            migrationBuilder.RenameColumn(
                name: "FromDate",
                table: "Leaves",
                newName: "FromDay");

            migrationBuilder.RenameColumn(
                name: "ApprovedDate",
                table: "Leaves",
                newName: "ApprovedDay");
        }
    }
}
