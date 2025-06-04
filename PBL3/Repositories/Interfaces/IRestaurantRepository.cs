using PBL3.Models;
using PBL3.ViewModels;
using X.PagedList;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBL3.Repositories.Interfaces
{
    public interface IRestaurantRepository
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
            string selectedSortOption = "Relevance",
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false);

        /// <summary>
        /// Lấy danh sách nhà hàng trong khu vực với các tùy chọn tìm kiếm nâng cao
        /// </summary>
        Task<List<Restaurant>> GetRestaurantsInAreaAsync(
            double lat,
            double lng,
            double actualRadiusKm = 3.0,
            string searchTerm = "",
            string selectedCategory = "",
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false);

        /// <summary>
        /// Lấy thông tin chi tiết nhà hàng theo ID
        /// </summary>
        Task<Restaurant> GetRestaurantByIdAsync(int id);

        /// <summary>
        /// Lấy danh sách các danh mục nhà hàng duy nhất
        /// </summary>
        Task<List<string>> GetUniqueRestaurantCategoriesAsync();
    }
}