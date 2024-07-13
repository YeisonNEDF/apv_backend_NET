using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APV.Migrations
{
    public partial class initia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paciente_Veterinarios_VeterinarioId",
                table: "Paciente");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Paciente",
                table: "Paciente");

            migrationBuilder.RenameTable(
                name: "Paciente",
                newName: "Pacientes");

            migrationBuilder.RenameIndex(
                name: "IX_Paciente_VeterinarioId",
                table: "Pacientes",
                newName: "IX_Pacientes_VeterinarioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pacientes",
                table: "Pacientes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Veterinarios_VeterinarioId",
                table: "Pacientes",
                column: "VeterinarioId",
                principalTable: "Veterinarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Veterinarios_VeterinarioId",
                table: "Pacientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pacientes",
                table: "Pacientes");

            migrationBuilder.RenameTable(
                name: "Pacientes",
                newName: "Paciente");

            migrationBuilder.RenameIndex(
                name: "IX_Pacientes_VeterinarioId",
                table: "Paciente",
                newName: "IX_Paciente_VeterinarioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Paciente",
                table: "Paciente",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Paciente_Veterinarios_VeterinarioId",
                table: "Paciente",
                column: "VeterinarioId",
                principalTable: "Veterinarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
