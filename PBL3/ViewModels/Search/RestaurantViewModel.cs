using System;
using PBL3.ViewModels.Search;

namespace PBL3.ViewModels.Search
{
    public class RestaurantViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Thông tin vị trí
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string Address { get; set; } = string.Empty;
        public double? Distance { get; set; } // Khoảng cách tính bằng km từ vị trí tìm kiếm
        
        // Thông tin phân loại
        public string Category { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        
        // Thông tin đánh giá và giá cả
        public double Rating { get; set; }
        public int ReviewCount { get; set; } //cần tính toán
        public int PriceLevel { get; set; } // 1 = $, 2 = $$, 3 = $$$, 4 = $$$$
        
        // Thông tin liên hệ
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        
        // Thông tin giờ mở cửa
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }
        public bool IsOpen { get; set; } // Trạng thái mở/đóng cửa hiện tại sẽ có logic để chủ nhà hàng lật flag này


        /*----Displayer helper properties----*/
        
        // Hiển thị giá
        public string PriceDisplay 
        { 
            get 
            { 
                return PriceLevel > 0 ? new string('$', PriceLevel) : "$";  //Tạo ra một chuỗi ký tự $ lặp lại đúng bằng giá trị của PriceLevel
            } 
        }
        
        // Hiển thị khoảng cách
        public string DistanceDisplay
        {
            get 
            { 
                return Distance.HasValue 
                    ? $"{Math.Round(Distance.Value, 1)} km" 
                    : string.Empty; 
            }
        }
        
        // SEO-friendly URL slug
        public string Slug
        {
            get
            {
                return $"{Name.ToLower().Replace(" ", "-")}-{Id}";
            }
        }
        
        // Hiển thị giờ đóng cửa
        public string ClosingTimeDisplay
        {
            get
            {
                return ClosingTime?.ToString(@"hh\:mm") ?? string.Empty;
            }
        }
        
        // Hiển thị giờ mở cửa
        public string OpeningTimeDisplay
        {
            get
            {
                return OpeningTime?.ToString(@"hh\:mm") ?? string.Empty;
            }
        }
        
        // Kiểm tra xem nhà hàng có đang mở cửa không
        public bool IsCurrentlyOpen
        {
            get
            {
                if (!OpeningTime.HasValue || !ClosingTime.HasValue)
                    return IsOpen;
                    
                var now = DateTime.Now.TimeOfDay;
                var isOvernight = ClosingTime.Value < OpeningTime.Value;
                
                return isOvernight 
                    ? now >= OpeningTime.Value || now <= ClosingTime.Value
                    : now >= OpeningTime.Value && now <= ClosingTime.Value;
            }
        }
    }
}