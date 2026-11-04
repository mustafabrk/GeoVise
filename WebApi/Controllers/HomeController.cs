using Microsoft.AspNetCore.Mvc;
using ModelClass.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]

    public class HomeController : ControllerBase
    {
        //PostgreSQL veritabanı bağlantı dizesi oluşturdum
        private readonly string connString;
        public HomeController(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Set ConnectionStrings:DefaultConnection in configuration.");
            var schema = DatabaseSchema.Get(configuration);
            connString = new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = schema }.ConnectionString;
        }
        public class FIleUpload
        {
            public IFormFile files { get; set; }
        }

        #region Liste ve Info Get İşlemleri
        // ---> GET İSTEKLERİ
        [HttpGet] //Hepsini getir
        public List<ToDo> Get()
        {
            return ReadDB();
        }
        [HttpGet] //Id'ye göre getir
        public ToDo GetById(int id)
        {
            var line = ReadDB().FirstOrDefault(x => x.Id == id);  //id sorgusuna göre bir eleman getirir  
            return line;
        }
        [HttpGet] //İsme göre getir
        public ToDo GetByName(string name)
        {
            var line = ReadDB().FirstOrDefault(x => x.Name == name);  //id sorgusuna göre bir eleman getirir  
            return line;
        }
        [HttpGet] //Numaraya göre getir
        public ToDo GetByNumber(int number)
        {
            var line = ReadDB().FirstOrDefault(x => x.Number == number);  //numara sorgusuna göre bir eleman getirir  
            return line;
        }
        [HttpGet] //Sunucu taraflı sayfalama ve arama işlemlerine göre getir



        public BSTableResp<List<ToDo>> PagingDB(int limit, int offset, string? sortOrder, string? sortField, string? search)
        {
            BSTableResp<List<ToDo>> resp = new BSTableResp<List<ToDo>>();
            var data = ReadDB();

            if (!string.IsNullOrEmpty(search))  //Sütunlara özel arama çubukları eklersem bu if'lerden kurtulabilirim
            {
                var query = data.AsQueryable().Where(x => x.Name == search);
                if (query.ToList().Count == 0)
                {
                    query = data.AsQueryable().Where(x => x.Number.ToString() == search);
                    if (query.ToList().Count == 0)
                    {
                        query = data.AsQueryable().Where(x => x.Id.ToString() == search);
                        if (query.ToList().Count == 0)
                        {
                            query = data.AsQueryable().Where(x => x.LocatedCity == search);
                        }
                    }
                }
                data = query.ToList();
            }

            resp.total = data.Count;
            resp.totalNotFiltered = data.Count();
            resp.rows = data.Skip(offset).Take(limit).ToList();     //Skip metodu örneğin 3. sayfadaysak önceki verileri atlamamızı sağlıyor
                                                                    //Take metodu da aldığı değer kadar veriyi getiriyor.
            return resp;
        }



        [HttpGet]
        public City GetCityById(int id)
        {
            var line = new City();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                //SQL sorgusunu oluşturdum
                string getCityLine = $"SELECT * FROM city WHERE id=" + id;
                using (NpgsqlCommand cmd = new NpgsqlCommand(getCityLine, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new City()
                            {
                                Id = reader.GetDecimal(reader.GetOrdinal("id")),
                                Number = ReadNullableDecimal(reader, "number"),
                                Name = ReadNullableString(reader, "name"),
                                Measurement = ReadNullableString(reader, "measurement"),
                                FeatureType = ReadNullableString(reader, "feature_type"),
                                LocatedCity = ReadNullableString(reader, "located_city"),
                                Coordinate = coordinate,
                                LicensePlate = ReadNullableDecimal(reader, "license_plate"),
                                Population = ReadNullableDecimal(reader, "population")
                            };

                            line = data;
                        }
                    }
                }

                //Bağlantıyı kapat
                conn.Close();
            }

            return line;
        }
        [HttpGet]
        public County GetCountyById(int id)
        {
            var line = new County();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                //SQL sorgusunu oluşturdum
                string getCountyLine = $"SELECT * FROM county WHERE id=" + id;
                using (NpgsqlCommand cmd = new NpgsqlCommand(getCountyLine, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new County()
                            {
                                Id = reader.GetDecimal(reader.GetOrdinal("id")),
                                Number = ReadNullableDecimal(reader, "number"),
                                Name = ReadNullableString(reader, "name"),
                                Measurement = ReadNullableString(reader, "measurement"),
                                FeatureType = ReadNullableString(reader, "feature_type"),
                                LocatedCity = ReadNullableString(reader, "located_city"),
                                Coordinate = coordinate,
                                Population = ReadNullableDecimal(reader, "population")
                            };

                            line = data;
                        }
                    }
                }

                //Bağlantıyı kapat
                conn.Close();
            }

            return line;
        }
        [HttpGet]
        public Neighborhood GetNeighborhoodById(int id)
        {
            var line = new Neighborhood();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                //SQL sorgusunu oluşturdum
                string getNeighborhoodLine = $"SELECT * FROM neighborhood WHERE id=" + id;
                using (NpgsqlCommand cmd = new NpgsqlCommand(getNeighborhoodLine, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new Neighborhood()
                            {
                                Id = reader.GetDecimal(reader.GetOrdinal("id")),
                                Number = ReadNullableDecimal(reader, "number"),
                                Name = ReadNullableString(reader, "name"),
                                Measurement = ReadNullableString(reader, "measurement"),
                                FeatureType = ReadNullableString(reader, "feature_type"),
                                LocatedCity = ReadNullableString(reader, "located_city"),
                                LocatedCounty = ReadNullableString(reader, "located_county"),
                                Coordinate = coordinate,
                                Population = ReadNullableDecimal(reader, "population")
                            };

                            line = data;
                        }
                    }
                }

                //Bağlantıyı kapat
                conn.Close();
            }

            return line;
        }
        [HttpGet]
        public Building GetBuildingById(int id)
        {
            var line = new Building();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                //SQL sorgusunu oluşturdum
                string getBuildingLine = $"SELECT * FROM building WHERE id=" + id;
                using (NpgsqlCommand cmd = new NpgsqlCommand(getBuildingLine, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new Building()
                            {
                                Id = reader.GetDecimal(reader.GetOrdinal("id")),
                                Number = ReadNullableDecimal(reader, "number"),
                                Name = ReadNullableString(reader, "name"),
                                Measurement = ReadNullableString(reader, "measurement"),
                                FeatureType = ReadNullableString(reader, "feature_type"),
                                LocatedCity = ReadNullableString(reader, "located_city"),
                                Coordinate = coordinate,
                                BuildingNo = ReadNullableDecimal(reader, "building_no"),
                                FloorNum = ReadNullableDecimal(reader, "floor_num"),
                                IsFloorAva = ReadNullableDecimal(reader, "is_floor_ava")
                            };

                            line = data;
                        }
                    }
                }

                //Bağlantıyı kapat
                conn.Close();
            }

            return line;
        }
        [HttpGet]
        public Transformer GetTransformerById(int id)
        {
            var line = new Transformer();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                //SQL sorgusunu oluşturdum
                string getTransformerLine = $"SELECT * FROM transformer WHERE id=" + id;
                using (NpgsqlCommand cmd = new NpgsqlCommand(getTransformerLine, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new Transformer()
                            {
                                Id = reader.GetInt16(reader.GetOrdinal("id")),
                                Number = reader.GetInt32(reader.GetOrdinal("number")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Measurement = reader.GetString(reader.GetOrdinal("measurement")),
                                FeatureType = reader.GetString(reader.GetOrdinal("feature_type")),
                                LocatedCity = reader.IsDBNull(reader.GetOrdinal("located_city")) ? null : reader.GetString(reader.GetOrdinal("located_city")),
                                Coordinate = coordinate,
                                LocatedCounty = reader.IsDBNull(reader.GetOrdinal("located_county")) ? null : reader.GetString(reader.GetOrdinal("located_county")),
                                LocatedNeighborhood = reader.IsDBNull(reader.GetOrdinal("located_neighborhood")) ? null : reader.GetString(reader.GetOrdinal("located_neighborhood")),
                                Capacity = reader.GetString(reader.GetOrdinal("capacity"))
                            };

                            line = data;
                        }
                    }
                }

                //Bağlantıyı kapat
                conn.Close();
            }

            return line;
        }
        [HttpGet]
        public Street GetStreetById(int id)
        {
            var line = new Street();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                //SQL sorgusunu oluşturdum
                string getStreetLine = $"SELECT * FROM street WHERE id=" + id;
                using (NpgsqlCommand cmd = new NpgsqlCommand(getStreetLine, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new Street()
                            {
                                Id = reader.GetInt16(reader.GetOrdinal("id")),
                                Number = reader.GetInt32(reader.GetOrdinal("number")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Measurement = reader.GetString(reader.GetOrdinal("measurement")),
                                FeatureType = reader.GetString(reader.GetOrdinal("feature_type")),
                                LocatedCity = reader.IsDBNull(reader.GetOrdinal("located_city")) ? null : reader.GetString(reader.GetOrdinal("located_city")),
                                LocatedCounty = reader.IsDBNull(reader.GetOrdinal("located_county")) ? null : reader.GetString(reader.GetOrdinal("located_county")),
                                LocatedNeighborhood = reader.IsDBNull(reader.GetOrdinal("located_neighborhood")) ? null : reader.GetString(reader.GetOrdinal("located_neighborhood")),
                                Coordinate = coordinate,
                            };

                            line = data;
                        }
                    }
                }

                //Bağlantıyı kapat
                conn.Close();
            }

            return line;
        }
        [HttpGet]
        public Straight GetStraightById(int id)
        {
            var line = new Straight();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                //SQL sorgusunu oluşturdum
                string getStraightLine = $"SELECT * FROM straight WHERE id=" + id;
                using (NpgsqlCommand cmd = new NpgsqlCommand(getStraightLine, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new Straight()
                            {
                                Id = reader.GetInt16(reader.GetOrdinal("id")),
                                Number = reader.GetInt32(reader.GetOrdinal("number")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Measurement = reader.GetString(reader.GetOrdinal("measurement")),
                                FeatureType = reader.GetString(reader.GetOrdinal("feature_type")),
                                LocatedCity = reader.IsDBNull(reader.GetOrdinal("located_city")) ? null : reader.GetString(reader.GetOrdinal("located_city")),
                                Coordinate = coordinate,
                                StraightType = reader.GetString(reader.GetOrdinal("straight_type"))
                            };

                            line = data;
                        }
                    }
                }

                //Bağlantıyı kapat
                conn.Close();
            }

            return line;
        }
        [HttpGet]
        public Subscriber GetSubscriberById(int id)
        {
            var line = new Subscriber();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                //SQL sorgusunu oluşturdum
                string getSubscriberLine = $"SELECT * FROM subscriber WHERE id=" + id;
                using (NpgsqlCommand cmd = new NpgsqlCommand(getSubscriberLine, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new Subscriber()
                            {
                                Id = reader.GetInt16(reader.GetOrdinal("id")),
                                Number = reader.GetInt32(reader.GetOrdinal("number")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Measurement = reader.GetString(reader.GetOrdinal("measurement")),
                                FeatureType = reader.GetString(reader.GetOrdinal("feature_type")),
                                LocatedCity = reader.IsDBNull(reader.GetOrdinal("located_city")) ? null : reader.GetString(reader.GetOrdinal("located_city")),
                                Coordinate = coordinate,
                                LocatedCounty = reader.IsDBNull(reader.GetOrdinal("located_county")) ? null : reader.GetString(reader.GetOrdinal("located_county")),
                                LocatedNeighborhood = reader.IsDBNull(reader.GetOrdinal("located_neighborhood")) ? null : reader.GetString(reader.GetOrdinal("located_neighborhood")),
                            };

                            line = data;
                        }
                    }
                }

                //Bağlantıyı kapat
                conn.Close();
            }

            return line;
        }
        #endregion
        #region Liste ve Info Post-Put-Delete İşlemleri
        [HttpPost] //İle Gönder
        public City PostCity(City line)
        {
            string sql;

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Veritabanına bağlantı kuruldu

                sql = "INSERT INTO city (number, name, coordinate, measurement, feature_type, located_city, license_plate, population)" +
                      " VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    AddParameter(cmd, "param1", line.Number);
                    AddParameter(cmd, "param2", line.Name);
                    AddParameter(cmd, "param3", line.Coordinate);
                    AddParameter(cmd, "param4", line.Measurement);
                    AddParameter(cmd, "param5", line.FeatureType);
                    AddParameter(cmd, "param6", line.LocatedCity);
                    AddParameter(cmd, "param7", line.LicensePlate);
                    AddParameter(cmd, "param8", line.Population);

                    cmd.ExecuteNonQuery();
                }
            }
            return line;
        }
        [HttpPost] //İlçeye Gönder
        public County PostCounty(County line)
        {
            string sql;

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Veritabanına bağlantı kuruldu

                sql = "INSERT INTO county (number, name, coordinate, measurement, feature_type, located_city, population) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    AddParameter(cmd, "param1", line.Number);
                    AddParameter(cmd, "param2", line.Name);
                    AddParameter(cmd, "param3", line.Coordinate);
                    AddParameter(cmd, "param4", line.Measurement);
                    AddParameter(cmd, "param5", line.FeatureType);
                    AddParameter(cmd, "param6", line.LocatedCity);
                    AddParameter(cmd, "param7", line.Population);

                    cmd.ExecuteNonQuery();
                }
            }
            return line;
        }
        [HttpPost] //Mahalleye Gönder
        public Neighborhood PostNeighborhood(Neighborhood line)
        {
            string sql;

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Veritabanına bağlantı kuruldu

                sql = "INSERT INTO neighborhood (number, name, coordinate, measurement, feature_type, located_city, located_county, population) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    AddParameter(cmd, "param1", line.Number);
                    AddParameter(cmd, "param2", line.Name);
                    AddParameter(cmd, "param3", line.Coordinate);
                    AddParameter(cmd, "param4", line.Measurement);
                    AddParameter(cmd, "param5", line.FeatureType);
                    AddParameter(cmd, "param6", line.LocatedCity);
                    AddParameter(cmd, "param7", line.LocatedCounty);
                    AddParameter(cmd, "param8", line.Population);

                    cmd.ExecuteNonQuery();
                }
            }
            return line;
        }
        [HttpPost] //Binaya Gönder
        public Building PostBuilding(Building line)
        {
            string sql;

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Veritabanına bağlantı kuruldu

                sql = "INSERT INTO building (number, name, coordinate, measurement, feature_type, located_city, building_no, floor_num) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    AddParameter(cmd, "param1", line.Number);
                    AddParameter(cmd, "param2", line.Name);
                    AddParameter(cmd, "param3", line.Coordinate);
                    AddParameter(cmd, "param4", line.Measurement);
                    AddParameter(cmd, "param5", line.FeatureType);
                    AddParameter(cmd, "param6", line.LocatedCity);
                    AddParameter(cmd, "param7", line.BuildingNo);
                    AddParameter(cmd, "param8", line.FloorNum);

                    cmd.ExecuteNonQuery();
                }
            }
            return line;
        }
        [HttpPost] //Trafoya Gönder
        public Transformer PostTransformer(Transformer line)
        {
            string sql;

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Veritabanına bağlantı kuruldu

                sql = "INSERT INTO transformer (number, name, coordinate, measurement, feature_type, capacity, located_city, located_county, located_neighborhood) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    AddParameter(cmd, "param1", line.Number);
                    AddParameter(cmd, "param2", line.Name);
                    AddParameter(cmd, "param3", line.Coordinate);
                    AddParameter(cmd, "param4", line.Measurement);
                    AddParameter(cmd, "param5", line.FeatureType);
                    AddParameter(cmd, "param6", line.Capacity);
                    AddParameter(cmd, "param7", line.LocatedCity);
                    AddParameter(cmd, "param8", line.LocatedCounty);
                    AddParameter(cmd, "param9", line.LocatedNeighborhood);

                    cmd.ExecuteNonQuery();
                }
            }
            return line;
        }
        [HttpPost] //Sokağa Gönder
        public Street PostStreet(Street line)
        {
            string sql;

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Veritabanına bağlantı kuruldu

                sql = "INSERT INTO street (number, name, coordinate, measurement, feature_type, located_city, located_county, located_neighborhood) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    AddParameter(cmd, "param1", line.Number);
                    AddParameter(cmd, "param2", line.Name);
                    AddParameter(cmd, "param3", line.Coordinate);
                    AddParameter(cmd, "param4", line.Measurement);
                    AddParameter(cmd, "param5", line.FeatureType);
                    AddParameter(cmd, "param6", line.LocatedCity);
                    AddParameter(cmd, "param7", line.LocatedCounty);
                    AddParameter(cmd, "param8", line.LocatedNeighborhood);

                    cmd.ExecuteNonQuery();
                }
            }
            return line;
        }
        [HttpPost] //Direğe Gönder
        public Straight PostStraight(Straight line)
        {
            string sql;

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Veritabanına bağlantı kuruldu

                sql = "INSERT INTO straight (number, name, coordinate, measurement, feature_type, located_city, straight_type) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    AddParameter(cmd, "param1", line.Number);
                    AddParameter(cmd, "param2", line.Name);
                    AddParameter(cmd, "param3", line.Coordinate);
                    AddParameter(cmd, "param4", line.Measurement);
                    AddParameter(cmd, "param5", line.FeatureType);
                    AddParameter(cmd, "param6", line.LocatedCity);
                    AddParameter(cmd, "param7", line.StraightType);

                    cmd.ExecuteNonQuery();
                }
            }
            return line;
        }
        [HttpPost] //Aboneye Gönder
        public Subscriber PostSubscriber(Subscriber line)
        {
            string sql;

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Veritabanına bağlantı kuruldu

                sql = "INSERT INTO subscriber (number, name, coordinate, measurement, feature_type, located_city, located_county, located_neighborhood) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    AddParameter(cmd, "param1", line.Number);
                    AddParameter(cmd, "param2", line.Name);
                    AddParameter(cmd, "param3", line.Coordinate);
                    AddParameter(cmd, "param4", line.Measurement);
                    AddParameter(cmd, "param5", line.FeatureType);
                    AddParameter(cmd, "param6", line.LocatedCity);
                    AddParameter(cmd, "param7", line.LocatedCounty);
                    AddParameter(cmd, "param8", line.LocatedNeighborhood);

                    cmd.ExecuteNonQuery();
                }
            }
            return line;
        }



        [HttpPut] //DÜZENLİYOR
        public bool Put(ToDo line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = TableSelector(line.FeatureType, "UPDATE", "SET street_number = @streetNumber, street_name = @streetName," +
                        " measurement = @measurement, feature_type = @featureType, coordinate = @coordinate WHERE id = " + line.Id);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "streetNumber", line.Number);
                        AddParameter(cmd, "streetName", line.Name);
                        AddParameter(cmd, "measurement", line.Measurement);
                        AddParameter(cmd, "featureType", line.FeatureType);
                        AddParameter(cmd, "coordinate", line.Coordinate);

                        //cmd.ExecuteNonQuery();

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }
        [HttpPut] //DÜZENLİYOR
        public bool PutCity(City line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = TableSelector(line.FeatureType, "UPDATE", "SET number = @number, name = @name, coordinate = @coordinate," +
                    " measurement = @measurement, feature_type = @featureType, located_city = @locatedCity, license_plate = @licensePlate, population = @population WHERE id = " + line.Id);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "@number", line.Number);
                        AddParameter(cmd, "@name", line.Name);
                        AddParameter(cmd, "@coordinate", line.Coordinate);
                        AddParameter(cmd, "@measurement", line.Measurement);
                        AddParameter(cmd, "@featureType", line.FeatureType);
                        AddParameter(cmd, "@locatedCity", line.LocatedCity);
                        AddParameter(cmd, "@licensePlate", line.LicensePlate);
                        AddParameter(cmd, "@population", line.Population);

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }
        [HttpPut] //DÜZENLİYOR
        public bool PutCounty(County line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = TableSelector(line.FeatureType, "UPDATE", "SET number = @number, name = @name, coordinate = @coordinate," +
                        " measurement = @measurement, feature_type = @featureType, located_city = @locatedCity, population = @population WHERE id = " + line.Id);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "@number", line.Number);
                        AddParameter(cmd, "@name", line.Name);
                        AddParameter(cmd, "@coordinate", line.Coordinate);
                        AddParameter(cmd, "@measurement", line.Measurement);
                        AddParameter(cmd, "@featureType", line.FeatureType);
                        AddParameter(cmd, "@locatedCity", line.LocatedCity);
                        AddParameter(cmd, "@population", line.Population);

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }
        [HttpPut] //DÜZENLİYOR
        public bool PutNeighborhood(Neighborhood line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = TableSelector(line.FeatureType, "UPDATE", "SET number = @number, name = @name, coordinate = @coordinate," +
                        " measurement = @measurement, feature_type = @featureType, located_city = @locatedCity, located_county = @locatedCounty, population = @population WHERE id = " + line.Id);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "@number", line.Number);
                        AddParameter(cmd, "@name", line.Name);
                        AddParameter(cmd, "@coordinate", line.Coordinate);
                        AddParameter(cmd, "@measurement", line.Measurement);
                        AddParameter(cmd, "@featureType", line.FeatureType);
                        AddParameter(cmd, "@locatedCity", line.LocatedCity);
                        AddParameter(cmd, "@locatedCounty", line.LocatedCounty);
                        AddParameter(cmd, "@population", line.Population);

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }
        [HttpPut] //DÜZENLİYOR
        public bool PutBuilding(Building line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = TableSelector(line.FeatureType, "UPDATE", "SET number = @number, name = @name, coordinate = @coordinate," +
                    " measurement = @measurement, feature_type = @featureType, located_city = @locatedCity, building_no = @buildingNo, floor_num = @floorNum WHERE id = " + line.Id);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "@number", line.Number);
                        AddParameter(cmd, "@name", line.Name);
                        AddParameter(cmd, "@coordinate", line.Coordinate);
                        AddParameter(cmd, "@measurement", line.Measurement);
                        AddParameter(cmd, "@featureType", line.FeatureType);
                        AddParameter(cmd, "@locatedCity", line.LocatedCity);
                        AddParameter(cmd, "@buildingNo", line.BuildingNo);
                        AddParameter(cmd, "@floorNum", line.FloorNum);

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }
        [HttpPut] //DÜZENLİYOR
        public bool PutTransformer(Transformer line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = TableSelector(line.FeatureType, "UPDATE", "SET number = @number, name = @name, coordinate = @coordinate, measurement = @measurement, " +
                    "feature_type = @featureType, capacity = @capacity, located_city = @locatedCity, located_county = @locatedCounty, located_neighborhood = @locatedNeighborhood WHERE id = " + line.Id);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "@number", line.Number);
                        AddParameter(cmd, "@name", line.Name);
                        AddParameter(cmd, "@coordinate", line.Coordinate);
                        AddParameter(cmd, "@measurement", line.Measurement);
                        AddParameter(cmd, "@featureType", line.FeatureType);
                        AddParameter(cmd, "@capacity", line.Capacity);
                        AddParameter(cmd, "@locatedCity", line.LocatedCity);
                        AddParameter(cmd, "@locatedCounty", line.LocatedCounty);
                        AddParameter(cmd, "@locatedNeighborhood", line.LocatedNeighborhood);

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }
        [HttpPut] //DÜZENLİYOR
        public bool PutStreet(Street line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = TableSelector(line.FeatureType, "UPDATE", "SET number = @number, name = @name, coordinate = @coordinate, measurement = @measurement, " +
                    "feature_type = @featureType, located_city = @locatedCity, located_county = @locatedCounty, located_neighborhood = @locatedNeighborhood WHERE id = " + line.Id);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "@number", line.Number);
                        AddParameter(cmd, "@name", line.Name);
                        AddParameter(cmd, "@coordinate", line.Coordinate);
                        AddParameter(cmd, "@measurement", line.Measurement);
                        AddParameter(cmd, "@featureType", line.FeatureType);
                        AddParameter(cmd, "@locatedCity", line.LocatedCity);
                        AddParameter(cmd, "@locatedCounty", line.LocatedCounty);
                        AddParameter(cmd, "@locatedNeighborhood", line.LocatedNeighborhood);

                        //cmd.ExecuteNonQuery();

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }
        [HttpPut] //DÜZENLİYOR
        public bool PutStraight(Straight line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = TableSelector(line.FeatureType, "UPDATE", "SET number = @number, name = @name, coordinate = @coordinate, measurement = @measurement, " +
                    "feature_type = @featureType, located_city = @locatedCity, straight_type = @straightType WHERE id = " + line.Id);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "@number", line.Number);
                        AddParameter(cmd, "@name", line.Name);
                        AddParameter(cmd, "@coordinate", line.Coordinate);
                        AddParameter(cmd, "@measurement", line.Measurement);
                        AddParameter(cmd, "@featureType", line.FeatureType);
                        AddParameter(cmd, "@locatedCity", line.LocatedCity);
                        AddParameter(cmd, "@straightType", line.StraightType);

                        //cmd.ExecuteNonQuery();

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }
        [HttpPut] //DÜZENLİYOR
        public bool PutSubscriber(Subscriber line)/////
        {
            bool status = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = "UPDATE subscriber SET number = @number, name = @name, coordinate = @coordinate, measurement = @measurement, " +
                    "feature_type = @featureType, located_city = @locatedCity, located_county = @locatedCounty, located_neighborhood = @locatedNeighborhood WHERE id = " + line.Id;

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) // güncelleme sorgusunu oluşturun
                    {
                        AddParameter(cmd, "@number", line.Number);
                        AddParameter(cmd, "@name", line.Name);
                        AddParameter(cmd, "@coordinate", line.Coordinate);
                        AddParameter(cmd, "@measurement", line.Measurement);
                        AddParameter(cmd, "@featureType", line.FeatureType);
                        AddParameter(cmd, "@locatedCity", line.LocatedCity);
                        AddParameter(cmd, "@locatedCounty", line.LocatedCounty);
                        AddParameter(cmd, "@locatedNeighborhood", line.LocatedNeighborhood);

                        //cmd.ExecuteNonQuery();

                        int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                        if (rowsAffected > 0) //En az 1 satır etkilenmişse güncelleme başarılıdır
                        {
                            status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                status = false;
            }

            return status;
        }



        [HttpDelete] //Sil
        public bool Delete(int id, string featureType)
        {
            // NpgsqlConnection nesnesi oluştur
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                // Bağlantıyı aç
                conn.Open();

                // Sorguyu hazırla
                string sql = TableSelector(featureType, "DELETE FROM", "WHERE id = @Id");
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    // Parametre ekleyerek sorguyu güvence altına alıyorum
                    AddParameter(cmd, "Id", id);
                    cmd.ExecuteNonQuery();
                }
                // Bağlantıyı kapat
                conn.Close();
            }
            return true;    //BURADA KONTROL İŞLEMİ GERÇEKLEŞTİR
        }
        #endregion
        #region Katman İşlemleri
        [HttpGet]
        public List<string> GetLayerList(int id)
        {
            var layerList = new List<string>();

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                string getLayer = "SELECT l.layer " +
                    "FROM layers l " +
                    $"LEFT JOIN (SELECT * FROM user_layer_settings WHERE user_id = {id}) uls ON uls.layer_id = l.id " +
                    $"WHERE (uls.user_id = {id} OR uls.user_id IS NULL) AND uls.user_id IS NOT NULL " +
                    "ORDER BY COALESCE(uls.priority, l.priority) ASC";
                using (NpgsqlCommand cmd = new NpgsqlCommand(getLayer, conn))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string layer = reader.GetString(0);
                            layerList.Add(layer);
                        }
                    }
                }

                conn.Close();
            }

            return layerList;
        }
        [HttpPatch]
        public bool ChangeLayerVisible(List<Layers> uls)
        {
            bool result = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    foreach (var settings in uls)
                    {
                        string updateLayer = $"UPDATE user_layer_settings SET is_open = @isopen WHERE layer_id = @layerId AND user_id = @id";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(updateLayer, conn))
                        {
                            AddParameter(cmd, "@isopen", settings.IsOpen);
                            AddParameter(cmd, "@layerId", settings.LayerId);
                            AddParameter(cmd, "@id", settings.UserId);

                            int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                            result = rowsAffected > 0;
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        [HttpDelete]
        public bool DeleteOldLayerSettings(int id)
        {
            bool result = false;

            try
            {
                // NpgsqlConnection nesnesi oluştur
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    // Bağlantıyı aç
                    conn.Open();

                    // Sorguyu hazırla
                    string sql = $"DELETE FROM user_layer_settings WHERE user_id = {id}"; //AND is_open = true
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    // Bağlantıyı kapat
                    conn.Close();
                }
                return true;    //BURADA KONTROL İŞLEMİ GERÇEKLEŞTİR
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        [HttpPost]
        public bool PostNewLayerSettings(List<Layers> layers)
        {
            bool result = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    foreach (var layer in layers)
                    {
                        string sql = "INSERT INTO user_layer_settings (layer_id, user_id, priority, is_open, min_zoom, max_zoom) VALUES (@param2, @param3, @param4, @param5, @param6, @param7)";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                        {
                            //AddParameter(cmd, "param1", layer.Id);
                            AddParameter(cmd, "param2", layer.LayerId);
                            AddParameter(cmd, "param3", layer.UserId);
                            AddParameter(cmd, "param4", layer.Priority);
                            AddParameter(cmd, "param5", layer.IsOpen);
                            AddParameter(cmd, "param6", layer.MinZoom);
                            AddParameter(cmd, "param7", layer.MaxZoom);

                            int rowsAffected = cmd.ExecuteNonQuery(); //Sorguyu çalıştır ve etkilenen satır sayısını al

                            result = rowsAffected > 0;
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        [HttpGet]
        public List<Layers> DefaultLayerSettings(int id)
        {
            List<Layers> layers = new List<Layers>();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                string getAll = "SELECT l.id, l.group_id, l.layer, l.tag, lg.\"group\", " +
                                "COALESCE(uls.is_open, false) AS is_open, " +
                                $"COALESCE(uls.user_id, {id}) AS user_id, " +
                                "COALESCE(uls.min_zoom, l.min_zoom) AS min_zoom, " +
                                "COALESCE(uls.max_zoom, l.max_zoom) AS max_zoom, " +
                                "COALESCE(uls.priority, l.priority) AS priority " +
                                "FROM layers l " +
                                "LEFT JOIN layer_group lg ON l.group_id = lg.id " +
                                $"LEFT JOIN user_layer_settings uls ON uls.layer_id = l.id AND uls.user_id = {id} " +
                                "ORDER BY COALESCE(l.priority) ASC;";
                using (NpgsqlCommand cmd = new NpgsqlCommand(getAll, conn))
                {
                    //Sorguyu çalıştırdım ve sonuçları okudum
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Bilgileri sütunlara göre listeye ekledim
                            var data = new Layers()
                            {
                                Id = reader.GetDecimal(reader.GetOrdinal("id")),
                                UserId = ReadNullableDecimal(reader, "user_id"),//--
                                Priority = ReadNullableDecimal(reader, "priority"),//
                                Layer = ReadNullableString(reader, "layer"),//
                                IsOpen = reader.GetBoolean(reader.GetOrdinal("is_open")),//
                                MinZoom = ReadNullableDecimal(reader, "min_zoom"),//
                                MaxZoom = ReadNullableDecimal(reader, "max_zoom"),//
                                GroupId = ReadNullableDecimal(reader, "group_id"),//
                                Group = ReadNullableString(reader, "group"),//--
                                Tag = ReadNullableString(reader, "tag")//
                            };

                            layers.Add(data);
                        };
                    };
                };

                //Bağlantıyı kapat
                conn.Close();
            }

            return layers;
        }
        [HttpGet]
        public List<Layers> GetLayerParams(int id)
        {
            var layerList = new List<Layers>();

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                const string getLayerParams =
                    "SELECT uls.layer_id, uls.user_id, uls.priority, uls.min_zoom, uls.max_zoom, l.tag, lg.\"group\" " +
                    "FROM layers l " +
                    "INNER JOIN user_layer_settings uls ON uls.layer_id = l.id " +
                    "LEFT JOIN layer_group lg ON lg.id = l.group_id " +
                    "WHERE uls.is_open = true AND uls.user_id = @user_id " +
                    "ORDER BY uls.priority ASC";
                using (NpgsqlCommand cmd = new NpgsqlCommand(getLayerParams, conn))
                {
                    AddParameter(cmd, "user_id", id);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = new Layers()
                            {
                                LayerId = ReadNullableDecimal(reader, "layer_id"),
                                UserId = ReadNullableDecimal(reader, "user_id"),
                                Priority = ReadNullableDecimal(reader, "priority"),
                                MinZoom = ReadNullableDecimal(reader, "min_zoom"),
                                MaxZoom = ReadNullableDecimal(reader, "max_zoom"),
                                Tag = ReadNullableString(reader, "tag"),
                                Group = ReadNullableString(reader, "group")
                            };

                            layerList.Add(data);
                        }
                    }
                }
            }

            return layerList;
        }
        #endregion
        #region Kullanıcı Girişi ve Haritadan Seçme İstekleri
        [HttpGet]
        public ActionResult<List<MapInfo>> GetInfoMap(string wkt, string layers, double zoomLevel)
        {
            var supportedLayers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["city"] = "İl",
                ["county"] = "İlçe",
                ["neighborhood"] = "Mahalle",
                ["building"] = "Bina"
            };
            var layersArray = layers.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (layersArray.Length == 0 || layersArray.Any(layer => !supportedLayers.ContainsKey(layer)))
            {
                return BadRequest("Only layers defined by the current database schema are supported.");
            }

            if (zoomLevel <= 0)
            {
                return BadRequest("zoomLevel must be greater than zero.");
            }

            var list = new List<MapInfo>();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                // WKT verisini bir geometri objesine dönüştür
                var wktReader = new NetTopologySuite.IO.WKTReader();
                var geometry = wktReader.Read(wkt);

                if (zoomLevel > 120)
                {
                    zoomLevel *= zoomLevel;
                }

                double bufferDistance = 200000 / zoomLevel * 2; //Tamponlama mesafesi
                var bufferedGeometry = geometry.Buffer(bufferDistance);

                // Tüm tablolar için aynı sorgu yapılıyor
                foreach (string layer in layersArray)
                {
                    var getMap = $"SELECT id, feature_type, coordinate FROM \"{layer}\"";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(getMap, conn))
                    {
                        //Sorguyu çalıştırdım ve sonuçları okudum
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var coordinate = ReadNullableString(reader, "coordinate");
                                if (coordinate is null)
                                {
                                    continue;
                                }

                                var resultGeometry = new WKTReader().Read(coordinate);
                                if (bufferedGeometry.Intersects(resultGeometry))
                                {
                                    var data = new MapInfo()
                                    {
                                        Id = reader.GetDecimal(reader.GetOrdinal("id")),
                                        FeatureType = supportedLayers[layer],
                                    };

                                    list.Add(data);
                                }
                            }
                        }
                    }
                }
                //Bağlantıyı kapat
                conn.Close();
            }
            return list;
        }
        #endregion
        #region Fonksiyonlar
        private List<ToDo> ReadDB()
        {
            var list = new List<ToDo>();

            // NpgsqlConnection nesnesi oluşturdum
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                //Bağlantıyı aç
                conn.Open();

                // Tüm tablolar için aynı sorgu yapılıyor
                string[] tables = { "city", "county", "neighborhood", "building" };
                foreach (string table in tables)
                {
                    //SQL sorgusunu oluşturdum
                    string getAll = $"SELECT * FROM {table}";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(getAll, conn))
                    {
                        //Sorguyu çalıştırdım ve sonuçları okudum
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string? coordinate = reader.IsDBNull(reader.GetOrdinal("coordinate")) ? null : reader.GetString(reader.GetOrdinal("coordinate"));

                                //Bilgileri sütunlara göre listeye ekledim
                                var data = new ToDo()
                                {
                                    Id = reader.GetDecimal(reader.GetOrdinal("id")),
                                    Number = ReadNullableDecimal(reader, "number"),
                                    Name = ReadNullableString(reader, "name"),
                                    Measurement = ReadNullableString(reader, "measurement"),
                                    FeatureType = ReadNullableString(reader, "feature_type"),
                                    LocatedCity = ReadNullableString(reader, "located_city"),
                                    Coordinate = coordinate
                                };

                                list.Add(data);
                            }
                        }
                    }
                }
                //Bağlantıyı kapat
                conn.Close();
            }

            return list;
        }

        private static void AddParameter(NpgsqlCommand command, string name, object? value) =>
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        private static decimal? ReadNullableDecimal(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
        }

        private static string? ReadNullableString(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private string TableSelector(string featureType, string sqlCommand1, string sqlCommand2)
        {
            string sql;
            string tableName;

            switch (featureType)
            {
                case "İl":
                    tableName = " city ";
                    break;
                case "İlçe":
                    tableName = " county ";
                    break;
                case "Mahalle":
                    tableName = " neighborhood ";
                    break;
                case "Sokak":
                    tableName = " street ";
                    break;
                case "Bina":
                    tableName = " building ";
                    break;
                case "Trafo":
                    tableName = " transformer ";
                    break;
                case "Direk":
                    tableName = " straight ";
                    break;
                case "Abone":
                    tableName = " subscriber ";
                    break;
                default:
                    tableName = " ? ";
                    break;
            }

            sql = sqlCommand1 + tableName + sqlCommand2;
            return sql;
        }
        #endregion
    }
}