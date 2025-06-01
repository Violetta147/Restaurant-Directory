namespace PBL3.ViewModels
{
    public class RestaurantViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Rating { get; set; }
    }
}