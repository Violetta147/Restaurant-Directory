using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL3.Models;
using X.PagedList;

namespace PBL3.ViewModels.Search
{
    public class SearchResultsViewModel
    {
        // Tham số tìm kiếm cơ bản
        public string Query { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        
        // Dữ liệu vị trí (tọa độ của địa điểm người dùng nhập, nếu có)
        public double? UserLat { get; set; } 
        public double? UserLng { get; set; }

        // Thuộc tính bản đồ (tọa độ trung tâm bản đồ để hiển thị)
        public double MapCenterLat { get; set; } = 16.0471; // Mặc định là Đà Nẵng
        public double MapCenterLng { get; set; } = 108.2062;
        
        // Kết quả
        public IPagedList<RestaurantViewModel> Restaurants { get; set; }

        // --- Filters --- 
        // Category Filter
        public string SelectedCategory { get; set; } = string.Empty;
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        // Distance Filter
        public string SelectedDistanceCategory { get; set; } = SearchConstants.Distance_BirdseyeView_Value; // Default value
        public List<SelectListItem> DistanceOptions { get; set; } = SearchConstants.DistanceOptions;

        // Sort Options
        public string SelectedSortOption { get; set; } = SearchConstants.Sort_Relevance_Value; // Default value
        public List<SelectListItem> SortOptions { get; set; } = SearchConstants.SortOptions;
        
        // SearchMode (có thể vẫn giữ lại nếu muốn có logic tìm kiếm khác nhau ở backend)
        public SearchMode SearchMode { get; set; } = SearchMode.Natural; 

        public bool MatchAllTerms { get; set; } = false; // Mặc định OR thay vì AND
        public bool UseExactPhrase { get; set; } = false; // Không tìm cụm từ chính xác
        public bool SearchInDescription { get; set; } = false; // Không tìm trong mô tả
        
        // Bộ lọc khác
        public double? MinRating { get; set; } // Lọc theo xếp hạng tối thiểu
        public string PriceRange { get; set; } = ""; // Lọc theo mức giá
        public bool IsOpenNow { get; set; } = false; // Lọc nhà hàng đang mở cửa
        
        // Sắp xếp và phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Các tùy chọn dropdown
        public Dictionary<string, string> PriceRangeOptions { get; set; } // Keep if used for a price filter UI
        
        // Thuộc tính hỗ trợ hiển thị
        public bool HasSearchFilters => 
            !string.IsNullOrEmpty(Query) ||
            !string.IsNullOrEmpty(SelectedCategory) ||
            MinRating.HasValue ||
            !string.IsNullOrEmpty(PriceRange) ||
            IsOpenNow;
            
        public string CurrentFiltersSummary
        {
            get
            {
                var filters = new List<string>();
                
                if (!string.IsNullOrEmpty(Query))
                    filters.Add($"\"{Query}\"");
                    
                if (!string.IsNullOrEmpty(SelectedCategory))
                    filters.Add($"thể loại: {SelectedCategory}");
                    
                if (MinRating.HasValue)
                    filters.Add($"đánh giá: {MinRating.Value}+ sao");
                    
                if (!string.IsNullOrEmpty(PriceRange))
                    filters.Add($"giá: {PriceRange}");
                    
                if (IsOpenNow)
                    filters.Add("đang mở cửa");
                    
                return string.Join(" • ", filters);
            }
        }
    }
}