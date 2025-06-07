using PBL3.Models;
using X.PagedList;

namespace PBL3.Services.Interfaces
{
    public interface IRestaurantService
    {
        /// <summary>
        /// Lấy chi tiết thông tin của một nhà hàng dựa trên ID.
        /// </summary>
        /// <param name="id">ID của nhà hàng.</param>
        /// <returns>Đối tượng Restaurant hoặc null nếu không tìm thấy.</returns>
        Task<Restaurant?> GetRestaurantByIdAsync(int id); // Sử dụng nullable reference types (C# 8+)

        Task<IPagedList<Restaurant>> SearchRestaurantsAdvancedAsync(
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
        // Task<IEnumerable<Restaurant>> GetRestaurantsByLocationAsync(double latitude, double longitude, double radiusInKm); // Tìm nhà hàng gần vị trí
        
        /// <summary>
        /// Chuẩn hóa địa chỉ và tọa độ với giá trị mặc định cho Đà Nẵng nếu không được cung cấp
        /// </summary>
        /// <param name="address">Địa chỉ đầu vào, có thể null hoặc rỗng</param>
        /// <param name="latitude">Tọa độ vĩ độ đầu vào, có thể null</param>
        /// <param name="longitude">Tọa độ kinh độ đầu vào, có thể null</param>
        /// <param name="radiusInKm">Bán kính tìm kiếm đầu vào, có thể null</param>
        /// <returns>Tuple chứa địa chỉ, tọa độ vĩ độ, kinh độ và bán kính đã chuẩn hóa</returns>
        (string address, double latitude, double longitude, double radiusInKm) NormalizeLocationParameters(
            string? address, double? latitude = null, double? longitude = null, string? maxDistance = null);

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
        /// <summary>
        /// Gets all cuisine types ordered by name
        /// </summary>
        /// <returns>A list of cuisine types</returns>
        Task<List<CuisineType>> GetAllCuisineTypesAsync();
    }
}

