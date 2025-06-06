using PBL3.Models;
using PBL3.ViewModels.Search;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;
using static PBL3.ViewModels.Search.SearchConstants;

namespace PBL3.Services.Interfaces
{
    public interface IRestaurantService
    {
        /// <summary>
        /// Lấy danh sách nhà hàng phân trang với các tùy chọn tìm kiếm nâng cao
        /// </summary>
        Task<IPagedList<Restaurant>> GetPagedRestaurantsAsync(
            string searchTerm = "",
            string selectedCategory = "",
            int page = 1,
            int pageSize = 10,
            double? lat = null,
            double? lng = null,
            double? actualRadiusKm = null,
            string? selectedSortOption = null, // Will default to Relevance
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false);

        /// <summary>
        /// Lấy danh sách nhà hàng trong khu vực bản đồ với các tùy chọn tìm kiếm nâng cao
        /// </summary>
        Task<List<RestaurantViewModel>> GetRestaurantsForMapAsync(
            double lat,
            double lng,
            string searchTerm = "",
            string selectedCategory = "",
            string? selectedDistanceCategory = null, // Will default to BirdseyeView
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false);

        /// <summary>
        /// Lấy thông tin chi tiết nhà hàng theo ID
        /// </summary>
        Task<RestaurantViewModel> GetRestaurantByIdAsync(int id);
        
        /// <summary>
        /// Tìm kiếm nhà hàng và chuyển đổi thành RestaurantViewModel cho hiển thị
        /// </summary>
        Task<IPagedList<RestaurantViewModel>> SearchRestaurantsAsViewModelsAsync(
            string searchTerm = "",
            string selectedCategory = "",
            string selectedDistanceCategory = SearchConstants.Distance_BirdseyeView_Value,
            string selectedSortOption = SearchConstants.Sort_Relevance_Value,
            int page = 1,
            int pageSize = 10,
            double? lat = null,
            double? lng = null,
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false);
        
        /// <summary>
        /// Lấy danh sách các danh mục nhà hàng duy nhất
        /// </summary>
        Task<List<string>> GetUniqueRestaurantCategoriesAsync();
        
        /// <summary>
        /// Xử lý và chuẩn hóa các tham số tìm kiếm với các giá trị mặc định phù hợp
        /// </summary>
        (string processedQuery, string processedLocation, string processedDistanceCategory, double? processedLat, double? processedLng, int validatedPage) 
        ProcessSearchParameters(string query, string location, string selectedDistanceCategory, double? lat, double? lng, int page = 1);
    }
}