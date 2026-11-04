using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModelClass.Models;

namespace WebApi;

public class MyDbContext : DbContext
{
    private readonly string _schema;

    public MyDbContext(DbContextOptions<MyDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _schema = DatabaseSchema.Get(configuration);
    }

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<County> Counties => Set<County>();
    public DbSet<Layers> Layers => Set<Layers>();
    public DbSet<MapInfo> MapInfos => Set<MapInfo>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_schema);

        modelBuilder.HasSequence<long>("building_id_seq", _schema);
        modelBuilder.HasSequence<long>("city_id_seq", _schema);
        modelBuilder.HasSequence<long>("county_id_seq", _schema);
        modelBuilder.HasSequence<long>("layers_id_seq", _schema);
        modelBuilder.HasSequence<long>("map_info_id_seq", _schema);
        modelBuilder.HasSequence<long>("neighborhood_id_seq", _schema);
        modelBuilder.HasSequence<long>("user_id_seq", _schema);

        modelBuilder.Entity<Building>(entity =>
        {
            entity.ToTable("building");
            entity.HasKey(x => x.Id).HasName("building_pkey");
            entity.Property(x => x.Id).HasColumnName("id").HasColumnType("numeric")
                .HasDefaultValueSql(DatabaseSchema.NextValue(_schema, "building_id_seq"));
            entity.Property(x => x.Number).HasColumnName("number").HasColumnType("numeric");
            entity.Property(x => x.Name).HasColumnName("name").HasColumnType("text");
            entity.Property(x => x.Coordinate).HasColumnName("coordinate").HasColumnType("text");
            entity.Property(x => x.Measurement).HasColumnName("measurement").HasColumnType("text");
            entity.Property(x => x.FeatureType).HasColumnName("feature_type").HasColumnType("text");
            entity.Property(x => x.LocatedCity).HasColumnName("located_city").HasColumnType("text");
            entity.Property(x => x.BuildingNo).HasColumnName("building_no").HasColumnType("numeric");
            entity.Property(x => x.FloorNum).HasColumnName("floor_num").HasColumnType("numeric");
            entity.Property(x => x.IsFloorAva).HasColumnName("is_floor_ava").HasColumnType("numeric");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("city");
            entity.HasKey(x => x.Id).HasName("city_pkey");
            entity.Property(x => x.Id).HasColumnName("id").HasColumnType("numeric")
                .HasDefaultValueSql(DatabaseSchema.NextValue(_schema, "city_id_seq"));
            entity.Property(x => x.Number).HasColumnName("number").HasColumnType("numeric");
            entity.Property(x => x.Name).HasColumnName("name").HasColumnType("text");
            entity.Property(x => x.Coordinate).HasColumnName("coordinate").HasColumnType("text");
            entity.Property(x => x.Measurement).HasColumnName("measurement").HasColumnType("text");
            entity.Property(x => x.FeatureType).HasColumnName("feature_type").HasColumnType("text");
            entity.Property(x => x.LocatedCity).HasColumnName("located_city").HasColumnType("text");
            entity.Property(x => x.LicensePlate).HasColumnName("license_plate").HasColumnType("numeric");
            entity.Property(x => x.Population).HasColumnName("population").HasColumnType("numeric");
        });

        modelBuilder.Entity<County>(entity =>
        {
            entity.ToTable("county");
            entity.HasKey(x => x.Id).HasName("county_pkey");
            entity.Property(x => x.Id).HasColumnName("id").HasColumnType("numeric")
                .HasDefaultValueSql(DatabaseSchema.NextValue(_schema, "county_id_seq"));
            entity.Property(x => x.Number).HasColumnName("number").HasColumnType("numeric");
            entity.Property(x => x.Name).HasColumnName("name").HasColumnType("text");
            entity.Property(x => x.Coordinate).HasColumnName("coordinate").HasColumnType("text");
            entity.Property(x => x.Measurement).HasColumnName("measurement").HasColumnType("text");
            entity.Property(x => x.FeatureType).HasColumnName("feature_type").HasColumnType("text");
            entity.Property(x => x.LocatedCity).HasColumnName("located_city").HasColumnType("text");
            entity.Property(x => x.Population).HasColumnName("population").HasColumnType("numeric");
        });

        modelBuilder.Entity<Layers>(entity =>
        {
            entity.ToTable("layers");
            entity.HasKey(x => x.Id).HasName("layers_pkey");
            entity.Property(x => x.Id).HasColumnName("id").HasColumnType("numeric")
                .HasDefaultValueSql(DatabaseSchema.NextValue(_schema, "layers_id_seq"));
            entity.Property(x => x.LayerId).HasColumnName("layer_id").HasColumnType("numeric");
            entity.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("numeric");
            entity.Property(x => x.Priority).HasColumnName("priority").HasColumnType("numeric");
            entity.Property(x => x.Layer).HasColumnName("layer").HasColumnType("text");
            entity.Property(x => x.IsOpen).HasColumnName("is_open").HasColumnType("boolean");
            entity.Property(x => x.MinZoom).HasColumnName("min_zoom").HasColumnType("numeric");
            entity.Property(x => x.MaxZoom).HasColumnName("max_zoom").HasColumnType("numeric");
            entity.Property(x => x.GroupId).HasColumnName("group_id").HasColumnType("numeric");
            entity.Property(x => x.Group).HasColumnName("group").HasColumnType("text");
            entity.Property(x => x.Tag).HasColumnName("tag").HasColumnType("text");
        });

        modelBuilder.Entity<MapInfo>(entity =>
        {
            entity.ToTable("map_info");
            entity.HasKey(x => x.Id).HasName("map_info_pkey");
            entity.Property(x => x.Id).HasColumnName("id").HasColumnType("numeric")
                .HasDefaultValueSql(DatabaseSchema.NextValue(_schema, "map_info_id_seq"));
            entity.Property(x => x.FeatureType).HasColumnName("feature_type").HasColumnType("text");
        });

        modelBuilder.Entity<Neighborhood>(entity =>
        {
            entity.ToTable("neighborhood");
            entity.HasKey(x => x.Id).HasName("neighborhood_pkey");
            entity.Property(x => x.Id).HasColumnName("id").HasColumnType("numeric")
                .HasDefaultValueSql(DatabaseSchema.NextValue(_schema, "neighborhood_id_seq"));
            entity.Property(x => x.Number).HasColumnName("number").HasColumnType("numeric");
            entity.Property(x => x.Name).HasColumnName("name").HasColumnType("text");
            entity.Property(x => x.Coordinate).HasColumnName("coordinate").HasColumnType("text");
            entity.Property(x => x.Measurement).HasColumnName("measurement").HasColumnType("text");
            entity.Property(x => x.FeatureType).HasColumnName("feature_type").HasColumnType("text");
            entity.Property(x => x.LocatedCity).HasColumnName("located_city").HasColumnType("text");
            entity.Property(x => x.LocatedCounty).HasColumnName("located_county").HasColumnType("text");
            entity.Property(x => x.Population).HasColumnName("population").HasColumnType("numeric");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(x => x.Id).HasName("user_pkey");
            entity.Property(x => x.Id).HasColumnName("id").HasColumnType("numeric")
                .HasDefaultValueSql(DatabaseSchema.NextValue(_schema, "user_id_seq"));
            entity.Property(x => x.UserName).HasColumnName("user_name").HasColumnType("text").IsRequired();
            entity.Property(x => x.Password).HasColumnName("password").HasColumnType("text").IsRequired();
            entity.HasIndex(x => x.UserName).IsUnique().HasDatabaseName("user_user_name_key");
            entity.Ignore(x => x.SessionDuration);
        });

        base.OnModelCreating(modelBuilder);
    }

}
