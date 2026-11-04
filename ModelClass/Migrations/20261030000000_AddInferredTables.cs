using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ModelClass.Models;
using WebApi;

#nullable disable

namespace ModelClass.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20261030000000_AddInferredTables")]
    public partial class AddInferredTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = DatabaseSchema.GetForMigrations();
            migrationBuilder.EnsureSchema(name: schema);

            migrationBuilder.CreateSequence(name: "transformer_id_seq", schema: schema);
            migrationBuilder.CreateSequence(name: "street_id_seq", schema: schema);
            migrationBuilder.CreateSequence(name: "straight_id_seq", schema: schema);
            migrationBuilder.CreateSequence(name: "subscriber_id_seq", schema: schema);

            migrationBuilder.CreateTable(
                name: "transformer",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "transformer_id_seq")),
                    number = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    coordinate = table.Column<string>(type: "text", nullable: true),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    feature_type = table.Column<string>(type: "text", nullable: true),
                    capacity = table.Column<string>(type: "text", nullable: true),
                    located_city = table.Column<string>(type: "text", nullable: true),
                    located_county = table.Column<string>(type: "text", nullable: true),
                    located_neighborhood = table.Column<string>(type: "text", nullable: true),
                    streetnumber = table.Column<int>(type: "integer", nullable: true),
                    streetname = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => table.PrimaryKey("transformer_pkey", x => x.id));

            migrationBuilder.CreateTable(
                name: "street",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "street_id_seq")),
                    number = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    coordinate = table.Column<string>(type: "text", nullable: true),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    feature_type = table.Column<string>(type: "text", nullable: true),
                    located_city = table.Column<string>(type: "text", nullable: true),
                    located_county = table.Column<string>(type: "text", nullable: true),
                    located_neighborhood = table.Column<string>(type: "text", nullable: true),
                    streetnumber = table.Column<int>(type: "integer", nullable: true),
                    streetname = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => table.PrimaryKey("street_pkey", x => x.id));

            migrationBuilder.CreateTable(
                name: "straight",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "straight_id_seq")),
                    number = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    coordinate = table.Column<string>(type: "text", nullable: true),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    feature_type = table.Column<string>(type: "text", nullable: true),
                    located_city = table.Column<string>(type: "text", nullable: true),
                    straight_type = table.Column<string>(type: "text", nullable: true),
                    streetnumber = table.Column<int>(type: "integer", nullable: true),
                    streetname = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => table.PrimaryKey("straight_pkey", x => x.id));

            migrationBuilder.CreateTable(
                name: "subscriber",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false, defaultValueSql: DatabaseSchema.NextValue(schema, "subscriber_id_seq")),
                    number = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    coordinate = table.Column<string>(type: "text", nullable: true),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    feature_type = table.Column<string>(type: "text", nullable: true),
                    located_city = table.Column<string>(type: "text", nullable: true),
                    located_county = table.Column<string>(type: "text", nullable: true),
                    located_neighborhood = table.Column<string>(type: "text", nullable: true),
                    streetnumber = table.Column<int>(type: "integer", nullable: true),
                    streetname = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => table.PrimaryKey("subscriber_pkey", x => x.id));

            migrationBuilder.CreateTable(
                name: "layer_group",
                schema: schema,
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false),
                    @group = table.Column<string>(name: "group", type: "text", nullable: true)
                },
                constraints: table => table.PrimaryKey("layer_group_pkey", x => x.id));

            migrationBuilder.CreateTable(
                name: "user_layer_settings",
                schema: schema,
                columns: table => new
                {
                    layerid = table.Column<decimal>(type: "numeric", nullable: true),
                    userid = table.Column<decimal>(type: "numeric", nullable: true),
                    priority = table.Column<decimal>(type: "numeric", nullable: true),
                    isopen = table.Column<bool>(type: "boolean", nullable: true),
                    minzoom = table.Column<decimal>(type: "numeric", nullable: true),
                    maxzoom = table.Column<decimal>(type: "numeric", nullable: true)
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var schema = DatabaseSchema.GetForMigrations();

            migrationBuilder.DropTable(name: "user_layer_settings", schema: schema);
            migrationBuilder.DropTable(name: "layer_group", schema: schema);
            migrationBuilder.DropTable(name: "subscriber", schema: schema);
            migrationBuilder.DropTable(name: "straight", schema: schema);
            migrationBuilder.DropTable(name: "street", schema: schema);
            migrationBuilder.DropTable(name: "transformer", schema: schema);

            migrationBuilder.DropSequence(name: "subscriber_id_seq", schema: schema);
            migrationBuilder.DropSequence(name: "straight_id_seq", schema: schema);
            migrationBuilder.DropSequence(name: "street_id_seq", schema: schema);
            migrationBuilder.DropSequence(name: "transformer_id_seq", schema: schema);
        }
    }
}
