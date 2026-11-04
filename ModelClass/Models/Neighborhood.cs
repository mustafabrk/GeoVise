namespace ModelClass.Models
{
    public class Neighborhood
    {
        public decimal Id { get; set; }
        public decimal? Number { get; set; }
        public string? Name { get; set; }
        public string? Coordinate { get; set; }
        public string? Measurement { get; set; }
        public string? FeatureType { get; set; }
        public string? LocatedCity { get; set; }
        public string? LocatedCounty { get; set; }
        public decimal? Population { get; set; }
    }
}