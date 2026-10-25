using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmoxarifadoHospitalar.Migrations
{
    /// <inheritdoc />
    public partial class CorrigeNomeUnidadeMedida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnidadeMedidade",
                table: "Materiais",
                newName: "UnidadeMedida");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnidadeMedida",
                table: "Materiais",
                newName: "UnidadeMedidade");
        }
    }
}
