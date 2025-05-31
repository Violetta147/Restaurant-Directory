
using PBL3.Models;
namespace PBL3.ViewModel
{
    public class MapIndexViewModel
    {
        // Dữ liệu để hiển thị marker
        public List<RestaurantViewModel> Restaurants { get; set; } = new List<RestaurantViewModel>();        // Thông tin người dùng/lọc tìm kiếm
        public LocationViewModel Center { get; set; } = new LocationViewModel();
        public string Category { get; set; } = "";

        // Default map center coordinates
        //Da Nang city
        public double DefaultLat { get; set; } = 16.0471;
        public double DefaultLng { get; set; } = 108.2062;
        public double SearchRadius { get; set; } = 3.0; // km
    }
}
