using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ModelClass.Models;
using WebApi;

#nullable disable

namespace ModelClass.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20261030020633_SeedCodeRequiredLayers")]
    public partial class SeedCodeRequiredLayers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = DatabaseSchema.GetForMigrations();

            migrationBuilder.Sql($"""
                WITH required_layers(layer_name, layer_tag) AS (
                    VALUES
                        ('city', 'city'),
                        ('county', 'county'),
                        ('neighborhood', 'neighborhood'),
                        ('building', 'building'),
                        ('transformer', 'transformer'),
                        ('street', 'street'),
                        ('straight', 'straight'),
                        ('subscriber', 'subscriber')
                )
                INSERT INTO "{schema}".layers (layer, tag)
                SELECT required_layers.layer_name, required_layers.layer_tag
                FROM required_layers
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "{schema}".layers existing_layer
                    WHERE existing_layer.layer = required_layers.layer_name
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("This seed migration is intentionally irreversible to preserve existing layer data.");
        }
    }
}
