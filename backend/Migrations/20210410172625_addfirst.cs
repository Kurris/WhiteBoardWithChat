using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WhiteBoard.Migrations
{
    public partial class addfirst : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Onlines",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Creator = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(maxLength: 14, nullable: false),
                    Modifier = table.Column<int>(nullable: true),
                    ModifyTime = table.Column<DateTime>(maxLength: 14, nullable: true),
                    RoomCode = table.Column<long>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Onlines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Creator = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(maxLength: 14, nullable: false),
                    Modifier = table.Column<int>(nullable: true),
                    ModifyTime = table.Column<DateTime>(maxLength: 14, nullable: true),
                    RoomCode = table.Column<long>(nullable: false),
                    RoomName = table.Column<string>(nullable: true),
                    Owner = table.Column<int>(nullable: false),
                    IsNeedPassword = table.Column<bool>(nullable: false),
                    Password = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Creator = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(maxLength: 14, nullable: false),
                    Modifier = table.Column<int>(nullable: true),
                    ModifyTime = table.Column<DateTime>(maxLength: 14, nullable: true),
                    UserName = table.Column<string>(nullable: true),
                    RealName = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    SignalRConnectionId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Onlines");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
