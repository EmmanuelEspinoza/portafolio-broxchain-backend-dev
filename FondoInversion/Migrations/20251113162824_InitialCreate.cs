using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FondoInversion.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "precio",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    fecha_precio = table.Column<DateTime>(type: "DATE", nullable: false),
                    precio_mxn = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    rfc = table.Column<string>(type: "TEXT", nullable: false),
                    Curp = table.Column<string>(type: "TEXT", nullable: false),
                    correo = table.Column<string>(type: "TEXT", nullable: false),
                    telefono = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flujo",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    inversionista_id = table.Column<int>(type: "INTEGER", nullable: false),
                    dia_movimiento = table.Column<DateTime>(type: "DATE", nullable: false),
                    importe = table.Column<double>(type: "REAL", nullable: false),
                    Comision = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flujo", x => x.id);
                    table.ForeignKey(
                        name: "FK_flujo_user_inversionista_id",
                        column: x => x.inversionista_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_flujo_inversionista_id",
                table: "flujo",
                column: "inversionista_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flujo");

            migrationBuilder.DropTable(
                name: "precio");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
