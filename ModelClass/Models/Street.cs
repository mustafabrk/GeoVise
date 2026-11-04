namespace ModelClass.Models
{
    public class Street
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Coordinate { get; set; }
        public string Measurement { get; set; }
        public string FeatureType { get; set; }
        public string LocatedCity { get; set; }
        public string LocatedCounty { get; set; }
        public string LocatedNeighborhood { get; set; }
    }
}