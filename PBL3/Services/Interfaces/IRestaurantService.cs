using PBL3.Models;

namespace PBL3.Services.Interfaces
{
    public interface IRestaurantService
    {
        // --- Chức năng cho người dùng (User) ---

        /// <summary>
        /// Lấy danh sách tất cả các nhà hàng đang hoạt động.
        /// </summary>
        Task<IEnumerable<Restaurant>> GetRestaurantsAsync();

        /// <summary>
        /// Lấy chi tiết thông tin của một nhà hàng dựa trên ID.
        /// </summary>
        /// <param name="id">ID của nhà hàng.</param>
        /// <returns>Đối tượng Restaurant hoặc null nếu không tìm thấy.</returns>
        Task<Restaurant?> GetRestaurantByIdAsync(int id); // Sử dụng nullable reference types (C# 8+)

        /// <summary>
        /// Tìm kiếm và lọc nhà hàng.
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên nhà hàng, tên món ăn, mô tả...).</param>
        /// <param name="cuisineTypeIds">Danh sách ID loại hình ẩm thực để lọc.</param>
        /// <param name="tagIds">Danh sách ID tag để lọc.</param>
        /// <param name="minPrice">Giá tối thiểu để lọc.</param>
        /// <param name="maxPrice">Giá tối đa để lọc.</param>
        /// <param name="sortBy">Tiêu chí sắp xếp (ví dụ: "rating", "price", "name").</param>
        /// <param name="pageNumber">Số trang (cho phân trang).</param>
        /// <param name="pageSize">Số lượng kết quả trên mỗi trang (cho phân trang).</param>
        /// <returns>Danh sách nhà hàng phù hợp với tiêu chí tìm kiếm/lọc.</returns>
        Task<IEnumerable<Restaurant>> SearchRestaurantsAsync(
            string? searchTerm = null,
            IEnumerable<int>? cuisineTypeIds = null,
            IEnumerable<int>? tagIds = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = null, // Có thể dùng enum thay vì string cho sortBy
            int pageNumber = 1,
            int pageSize = 10
        );
        // Location-based search
        Task<IEnumerable<Restaurant>> SearchRestaurantsByLocationAsync(
                string? addressQuery,
                double? latitude = null,
                double? longitude = null,
                double? radiusInKm = 5.0,
                int pageNumber = 1,
                int pageSize = 10
            );
            
        //Combined search (term + location)
            Task<IEnumerable<Restaurant>> SearchRestaurantsAdvancedAsync(
                string? searchTerm = null,
                string? addressQuery = null,
                double? latitude = null,
                double? longitude = null,
                double? radiusInKm = 5.0,
                IEnumerable<int>? cuisineTypeIds = null,
                IEnumerable<int>? tagIds = null,
                decimal? minPrice = null,
                decimal? maxPrice = null,
                string? sortBy = null,
                int pageNumber = 1,
                int pageSize = 10
            );

        // Có thể thêm các phương thức lọc/tìm kiếm chuyên biệt khác nếu cần
        // Task<IEnumerable<Restaurant>> GetRestaurantsByLocationAsync(double latitude, double longitude, double radiusInKm); // Tìm nhà hàng gần vị trí

        // --- Chức năng cho Admin hoặc Chủ Nhà hàng (RestaurantOwner) ---
        // (Cần cơ chế xác thực và phân quyền để chỉ cho phép người có quyền thực hiện)

        /// <summary>
        /// Tạo mới một nhà hàng.
        /// </summary>
        /// <param name="restaurant">Đối tượng Restaurant cần tạo.</param>
        /// <returns>ID của nhà hàng vừa tạo.</returns>
        // Task<int> CreateRestaurantAsync(Restaurant restaurant); // Hoặc dùng ViewModel/DTO

        /// <summary>
        /// Cập nhật thông tin một nhà hàng.
        /// </summary>
        /// <param name="restaurant">Đối tượng Restaurant với thông tin cập nhật.</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy nhà hàng hoặc lỗi.</returns>
        // Task<bool> UpdateRestaurantAsync(Restaurant restaurant); // Hoặc dùng ViewModel/DTO

        /// <summary>
        /// Xóa một nhà hàng.
        /// </summary>
        /// <param name="id">ID của nhà hàng cần xóa.</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy nhà hàng hoặc lỗi.</returns>
        // Task<bool> DeleteRestaurantAsync(int id); // Cần xử lý OnDelete logic (xóa Reviews, Menus, etc.)

        // Phương thức để lấy các nhà hàng thuộc sở hữu của một User cụ thể
        // Task<IEnumerable<Restaurant>> GetRestaurantsByOwnerIdAsync(string ownerId);

        // Có thể thêm các phương thức quản lý chi tiết hơn (ví dụ: thêm/xóa tag cho nhà hàng, thêm/xóa loại hình ẩm thực...)
        // Task<bool> AddCuisineTypeToRestaurantAsync(int restaurantId, int cuisineTypeId);
        // Task<bool> RemoveCuisineTypeFromRestaurantAsync(int restaurantId, int cuisineTypeId);

    }
}

