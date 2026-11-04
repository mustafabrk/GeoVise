using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ModelClass.Models;
using WebApi;

#nullable disable

namespace ModelClass.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20261030014527_AlignDatabaseNames")]
    public partial class AlignDatabaseNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = DatabaseSchema.GetForMigrations();

            foreach (var table in new[] { "city", "county", "neighborhood", "building" })
            {
                migrationBuilder.AddColumn<decimal>(
                    name: "street_number",
                    schema: schema,
                    table: table,
                    type: "numeric",
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "street_name",
                    schema: schema,
                    table: table,
                    type: "text",
                    nullable: true);
            }

            foreach (var table in new[] { "transformer", "street", "straight", "subscriber" })
            {
                migrationBuilder.RenameColumn(
                    name: "streetnumber",
                    schema: schema,
                    table: table,
                    newName: "street_number");

                migrationBuilder.RenameColumn(
                    name: "streetname",
                    schema: schema,
                    table: table,
                    newName: "street_name");
            }

            migrationBuilder.RenameColumn(
                name: "layerid",
                schema: schema,
                table: "user_layer_settings",
                newName: "layer_id");
            migrationBuilder.RenameColumn(
                name: "userid",
                schema: schema,
                table: "user_layer_settings",
                newName: "user_id");
            migrationBuilder.RenameColumn(
                name: "isopen",
                schema: schema,
                table: "user_layer_settings",
                newName: "is_open");
            migrationBuilder.RenameColumn(
                name: "minzoom",
                schema: schema,
                table: "user_layer_settings",
                newName: "min_zoom");
            migrationBuilder.RenameColumn(
                name: "maxzoom",
                schema: schema,
                table: "user_layer_settings",
                newName: "max_zoom");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("This migration is intentionally irreversible to avoid dropping columns or data.");
        }
    }
}
