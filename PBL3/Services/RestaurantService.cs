using PBL3.Models;
using PBL3.Repositories.Interfaces;
using PBL3.Services.Interfaces;
using PBL3.ViewModels.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using NetTopologySuite.Geometries;

namespace PBL3.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository ?? throw new ArgumentNullException(nameof(restaurantRepository));
        }

        public async Task<IPagedList<Restaurant>> GetPagedRestaurantsAsync(
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
            bool isOpenNow = false)
        {
            return await _restaurantRepository.GetPagedRestaurantsAsync(
                searchTerm, selectedCategory, page, pageSize, lat, lng, 
                actualRadiusKm, selectedSortOption, 
                minRating, priceRange, isOpenNow);
        }

        public async Task<List<RestaurantViewModel>> GetRestaurantsForMapAsync(
            double lat, // Map center lat
            double lng, // Map center lng
            string searchTerm = "",
            string selectedCategory = "",
            string selectedDistanceCategory = "BirdseyeView", // For map, this might define the initial view area or a filter
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false)
        {
            // Convert selectedDistanceCategory to actualRadiusKm
            double? actualRadiusKm = null;
            switch (selectedDistanceCategory)
            {
                case "1km": actualRadiusKm = 1.0; break;
                case "3km": actualRadiusKm = 3.0; break;
                case "5km": actualRadiusKm = 5.0; break;
                case "10km": actualRadiusKm = 10.0; break;
                case "BirdseyeView":
                default: actualRadiusKm = 3.0; break; // Default to 3km for map view if not specified or birdseye
            }

            var restaurants = await _restaurantRepository.GetRestaurantsInAreaAsync(
                lat, lng, actualRadiusKm ?? 3.0, // Ensure a default if somehow null
                searchTerm, selectedCategory, 
                minRating, priceRange, isOpenNow);

            var userLocation = new Point(lng, lat) { SRID = 4326 };

            return restaurants.Select(r => new RestaurantViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Address = r.Address,
                Category = r.Category,
                Rating = r.Rating,
                PriceLevel = ConvertPriceLevelToInt(r.PriceLevel),
                ImageUrl = r.ImageUrl,
                Latitude = r.Location.Y,
                Longitude = r.Location.X,
                Distance = r.Location.Distance(userLocation) / 1000, // Distance in km
                IsOpen = IsRestaurantOpenNow(r.OpeningTime, r.ClosingTime)
            }).ToList();
        }

        public async Task<RestaurantViewModel> GetRestaurantByIdAsync(int id)
        {
            var restaurant = await _restaurantRepository.GetRestaurantByIdAsync(id);
            if (restaurant == null)
                return null;

            return new RestaurantViewModel
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
                Category = restaurant.Category,
                Description = restaurant.Description,
                Rating = restaurant.Rating,
                PriceLevel = ConvertPriceLevelToInt(restaurant.PriceLevel),
                ImageUrl = restaurant.ImageUrl,
                Phone = restaurant.Phone,
                Website = restaurant.Website,
                Latitude = restaurant.Location.Y,
                Longitude = restaurant.Location.X,
                OpeningTime = restaurant.OpeningTime,
                ClosingTime = restaurant.ClosingTime,
                Keywords = restaurant.Keywords,
                IsOpen = IsRestaurantOpenNow(restaurant.OpeningTime, restaurant.ClosingTime)
            };
        }

        public async Task<List<string>> GetUniqueRestaurantCategoriesAsync()
        {
            return await _restaurantRepository.GetUniqueRestaurantCategoriesAsync();
        }
        
        public async Task<IPagedList<RestaurantViewModel>> SearchRestaurantsAsViewModelsAsync(
            string searchTerm = "",
            string selectedCategory = "",
            string selectedDistanceCategory = "BirdseyeView",
            string selectedSortOption = "Relevance",
            int page = 1,
            int pageSize = 10,
            double? lat = null, // User's location lat
            double? lng = null, // User's location lng
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false)
        {
            // Convert selectedDistanceCategory to actualRadiusKm
            double? actualRadiusKm = null;
            switch (selectedDistanceCategory)
            {
                case "1km": actualRadiusKm = 1.0; break;
                case "3km": actualRadiusKm = 3.0; break;
                case "5km": actualRadiusKm = 5.0; break;
                case "10km": actualRadiusKm = 10.0; break;
                case "BirdseyeView":
                    if (lat.HasValue && lng.HasValue) 
                    {
                        actualRadiusKm = 50000; // Effectively a global search, but allows distance calculation/sorting
                    } 
                    else 
                    {
                        actualRadiusKm = null; // No location, no radius
                    }
                    break;
                default: actualRadiusKm = null; break; 
            }

            // Lấy kết quả từ repository
            var restaurants = await _restaurantRepository.GetPagedRestaurantsAsync(
                searchTerm, selectedCategory, page, pageSize, lat, lng, 
                actualRadiusKm, selectedSortOption, 
                minRating, priceRange, isOpenNow);
            
            // Tạo đối tượng Location của người dùng nếu có tọa độ để tính khoảng cách
            Point userLocation = null;
            if (lat.HasValue && lng.HasValue)
            {
                userLocation = new Point(lng.Value, lat.Value) { SRID = 4326 };
            }
            
            // Chuyển đổi từ Restaurant sang RestaurantViewModel
            var viewModels = restaurants.Select(r => new RestaurantViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Address = r.Address,
                Description = r.Description,
                Category = r.Category,
                Keywords = r.Keywords,
                Rating = r.Rating,
                ReviewCount = r.ReviewCount,
                PriceLevel = ConvertPriceLevelToInt(r.PriceLevel),
                Phone = r.Phone,
                Website = r.Website,
                ImageUrl = r.ImageUrl,
                // Chuyển đổi từ NetTopologySuite.Geometries.Point sang tọa độ
                Latitude = r.Location?.Y ?? 0,
                Longitude = r.Location?.X ?? 0,
                // Tính khoảng cách nếu có vị trí người dùng
                Distance = userLocation != null && r.Location != null 
                    ? r.Location.Distance(userLocation) / 1000 // Chuyển đổi từ m sang km
                    : null,
                // Xử lý giờ mở cửa
                OpeningTime = r.OpeningTime,
                ClosingTime = r.ClosingTime,
                IsOpen = IsRestaurantOpenNow(r.OpeningTime, r.ClosingTime)
            }).ToList();
            
            // Tạo IPagedList từ danh sách viewModels
            return new StaticPagedList<RestaurantViewModel>(
                viewModels,
                restaurants.PageNumber,
                restaurants.PageSize,
                restaurants.TotalItemCount);
        }

        #region Private Helper Methods

        private bool IsRestaurantOpenNow(TimeSpan? openingTime, TimeSpan? closingTime)
        {
            if (!openingTime.HasValue || !closingTime.HasValue)
                return false;

            var now = DateTime.Now.TimeOfDay;
            
            // Handle restaurants that are open overnight (e.g., 6 PM to 2 AM)
            if (closingTime.Value < openingTime.Value)
            {
                return now >= openingTime.Value || now <= closingTime.Value;
            }
            
            return now >= openingTime.Value && now <= closingTime.Value;
        }

        #endregion
        
        // Hàm chuyển đổi từ chuỗi PriceLevel ("$", "$$", "$$$", "$$$$") sang số nguyên (1-4)
        private int ConvertPriceLevelToInt(string priceLevel)
        {
            return priceLevel?.Length ?? 0;
        }
    }
}