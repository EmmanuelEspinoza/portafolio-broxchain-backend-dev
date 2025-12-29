using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FondoInversion.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSQLServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adminUsers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    correo = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    password = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    rol = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__adminUse__3213E83F2761EE0B", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eventLog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__eventLog__3213E83F329D075A", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "precio",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fecha_precio = table.Column<DateOnly>(type: "date", nullable: false),
                    precio_mxn = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__precio__3213E83F7DEC59F5", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    rfc = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    curp = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    correo = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    telefono = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user__3213E83F99A70B0B", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "accesoUser",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    intentos = table.Column<int>(type: "int", nullable: false),
                    ultimoIntento = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__accesoUs__3213E83FA656F1CD", x => x.id);
                    table.ForeignKey(
                        name: "FK__accesoUse__userI__59FA5E80",
                        column: x => x.userID,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "flujo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    inversionista_id = table.Column<int>(type: "int", nullable: false),
                    dia_movimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    importe = table.Column<double>(type: "float", nullable: false),
                    comision = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__flujo__3213E83F399E913D", x => x.id);
                    table.ForeignKey(
                        name: "FK__flujo__inversion__4CA06362",
                        column: x => x.inversionista_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    refreshToken = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    expiracion = table.Column<DateTime>(type: "datetime", nullable: false),
                    activo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tokens__3213E83FFD91BCFC", x => x.id);
                    table.ForeignKey(
                        name: "FK__tokens__userID__72C60C4A",
                        column: x => x.userID,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transactionLogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    requestBody = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    userID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__transact__3213E83FA47BDB6D", x => x.id);
                    table.ForeignKey(
                        name: "FK__transacti__userI__5535A963",
                        column: x => x.userID,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_accesoUser_userID",
                table: "accesoUser",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_flujo_inversionista_id",
                table: "flujo",
                column: "inversionista_id");

            migrationBuilder.CreateIndex(
                name: "UQ__precio__E5CD410C546B8B96",
                table: "precio",
                column: "fecha_precio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tokens_userID",
                table: "tokens",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_transactionLogs_userID",
                table: "transactionLogs",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "UQ__user__2A586E0BD88A8B2F",
                table: "user",
                column: "correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accesoUser");

            migrationBuilder.DropTable(
                name: "adminUsers");

            migrationBuilder.DropTable(
                name: "eventLog");

            migrationBuilder.DropTable(
                name: "flujo");

            migrationBuilder.DropTable(
                name: "precio");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "transactionLogs");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
