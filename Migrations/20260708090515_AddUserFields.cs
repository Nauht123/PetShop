using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GioiTinh",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "DiaChi",
                table: "Users",
                newName: "DiaChi2");

            migrationBuilder.AddColumn<string>(
                name: "DiaChi1",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiemTichLuy",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgaySinh",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChi1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiemTichLuy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NgaySinh",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "DiaChi2",
                table: "Users",
                newName: "DiaChi");

            migrationBuilder.AddColumn<string>(
                name: "GioiTinh",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
