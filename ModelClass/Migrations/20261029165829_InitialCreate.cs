using Microsoft.EntityFrameworkCore.Migrations;
using ModelClass.Models;

#nullable disable

namespace ModelClass.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = DatabaseSchema.GetForMigrations();
            migrationBuilder.EnsureSchema(
                name: schema);

            migrationBuilder.CreateSequence(
                name: "building_id_seq",
                schema: schema);

            migrationBuilder.CreateSequence(
                name: "city_id_seq",
                schema: schema);

            migrationBuilder.CreateSequence(
                name: "county_id_seq",
                schema: schema);

            migrationBuilder.CreateSequence(
                name: "layers_id_seq",
                schema: schema);

            migrationBuilder.CreateSequence(
                name: "map_info_id_seq",
                schema: schema);

            migrationBuilder.CreateSequence(
                name: "neighborhood_id_seq",
                schema: schema);

            migrationBuilder.CreateSequence(
                name: "user_id_seq",
                schema: schema);

            migrationBuilder.CreateTable(
                name: "building",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "building_id_seq")),
                    number = table.Column<decimal>(type: "numeric", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    coordinate = table.Column<string>(type: "text", nullable: true),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    feature_type = table.Column<string>(type: "text", nullable: true),
                    located_city = table.Column<string>(type: "text", nullable: true),
                    building_no = table.Column<decimal>(type: "numeric", nullable: true),
                    floor_num = table.Column<decimal>(type: "numeric", nullable: true),
                    is_floor_ava = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("building_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "city",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "city_id_seq")),
                    number = table.Column<decimal>(type: "numeric", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    coordinate = table.Column<string>(type: "text", nullable: true),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    feature_type = table.Column<string>(type: "text", nullable: true),
                    located_city = table.Column<string>(type: "text", nullable: true),
                    license_plate = table.Column<decimal>(type: "numeric", nullable: true),
                    population = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("city_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "county",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "county_id_seq")),
                    number = table.Column<decimal>(type: "numeric", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    coordinate = table.Column<string>(type: "text", nullable: true),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    feature_type = table.Column<string>(type: "text", nullable: true),
                    located_city = table.Column<string>(type: "text", nullable: true),
                    population = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("county_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "layers",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "layers_id_seq")),
                    layer_id = table.Column<decimal>(type: "numeric", nullable: true),
                    user_id = table.Column<decimal>(type: "numeric", nullable: true),
                    priority = table.Column<decimal>(type: "numeric", nullable: true),
                    layer = table.Column<string>(type: "text", nullable: true),
                    is_open = table.Column<bool>(type: "boolean", nullable: true),
                    min_zoom = table.Column<decimal>(type: "numeric", nullable: true),
                    max_zoom = table.Column<decimal>(type: "numeric", nullable: true),
                    group_id = table.Column<decimal>(type: "numeric", nullable: true),
                    group = table.Column<string>(type: "text", nullable: true),
                    tag = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("layers_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "map_info",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "map_info_id_seq")),
                    feature_type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("map_info_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "neighborhood",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "neighborhood_id_seq")),
                    number = table.Column<decimal>(type: "numeric", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    coordinate = table.Column<string>(type: "text", nullable: true),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    feature_type = table.Column<string>(type: "text", nullable: true),
                    located_city = table.Column<string>(type: "text", nullable: true),
                    located_county = table.Column<string>(type: "text", nullable: true),
                    population = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("neighborhood_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "user_id_seq")),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "user_user_name_key",
                schema: schema,
                table: "user",
                column: "user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var schema = DatabaseSchema.GetForMigrations();
            migrationBuilder.DropTable(
                name: "building",
                schema: schema);

            migrationBuilder.DropTable(
                name: "city",
                schema: schema);

            migrationBuilder.DropTable(
                name: "county",
                schema: schema);

            migrationBuilder.DropTable(
                name: "layers",
                schema: schema);

            migrationBuilder.DropTable(
                name: "map_info",
                schema: schema);

            migrationBuilder.DropTable(
                name: "neighborhood",
                schema: schema);

            migrationBuilder.DropTable(
                name: "user",
                schema: schema);

            migrationBuilder.DropSequence(
                name: "building_id_seq",
                schema: schema);

            migrationBuilder.DropSequence(
                name: "city_id_seq",
                schema: schema);

            migrationBuilder.DropSequence(
                name: "county_id_seq",
                schema: schema);

            migrationBuilder.DropSequence(
                name: "layers_id_seq",
                schema: schema);

            migrationBuilder.DropSequence(
                name: "map_info_id_seq",
                schema: schema);

            migrationBuilder.DropSequence(
                name: "neighborhood_id_seq",
                schema: schema);

            migrationBuilder.DropSequence(
                name: "user_id_seq",
                schema: schema);
        }
    }
}
