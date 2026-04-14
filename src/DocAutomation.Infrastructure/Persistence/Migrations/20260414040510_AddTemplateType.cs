using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocAutomation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Templates",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "TemplateType",
                table: "Deployments",
                type: "int",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Type", table: "Templates");

            migrationBuilder.DropColumn(name: "TemplateType", table: "Deployments");
        }
    }
}
