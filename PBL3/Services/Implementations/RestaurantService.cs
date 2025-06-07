using PBL3.Models;
using PBL3.Services.Interfaces;
using PBL3.Data;
using X.PagedList;
using Microsoft.EntityFrameworkCore;
using System;

namespace PBL3.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RestaurantService> _logger;

        public RestaurantService(ApplicationDbContext context, ILogger<RestaurantService> logger)
        {
            _context = context;
            _logger = logger;
        } 
        
        //Search by both searchTerm and addressQuery
        public async Task<IPagedList<Restaurant>> SearchRestaurantsAdvancedAsync(
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
                int page = 1,
                int pageSize = 10
            )
        {
            // Only include essential data for search results - remove heavy collections
            var query = _context.Restaurants
                .Include(r => r.Address)
                .Include(r => r.RestaurantCuisines)
                    .ThenInclude(rc => rc.CuisineType)
                .Include(r => r.RestaurantTags)
                    .ThenInclude(rt => rt.Tag)
                .Include(r => r.OperatingHours)
                .AsQueryable();

            // Apply text search if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (searchTerm.Length < 3)
                {
                    _logger.LogWarning("Độ dài < 3 là quá ngắn để có ý nghĩa: {searchTerm}", searchTerm);
                    // Return empty paged list for invalid search terms
                    return new StaticPagedList<Restaurant>(new List<Restaurant>(), page, pageSize, 0);
                }

                // Normalize search term to lower case for case-insensitive comparison
                searchTerm = searchTerm.ToLower();                // Apply text search filtering (removed MenuItems search for performance)
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    r.Description.ToLower().Contains(searchTerm) ||
                    r.RestaurantCuisines.Any(rc => rc.CuisineType.Name.ToLower().Contains(searchTerm)) ||
                    r.RestaurantTags.Any(rt => rt.Tag.Name.ToLower().Contains(searchTerm))
                );
            }

            // Apply address text search if provided
            if (!string.IsNullOrEmpty(addressQuery))
            {
                query = query.Where(r =>
                    r.Address != null && (
                        r.Address.AddressLine1.Contains(addressQuery) ||
                        r.Address.Ward.Contains(addressQuery) ||
                        r.Address.District.Contains(addressQuery) ||
                        r.Address.City.Contains(addressQuery) ||
                        r.Address.Country.Contains(addressQuery)
                    )
                );
            }

            // Apply cuisine filters
            if (cuisineTypeIds != null && cuisineTypeIds.Any())
            {
                query = query.Where(r =>
                    r.RestaurantCuisines.Any(rc => cuisineTypeIds.Contains(rc.CuisineTypeId)));
            }

            // Apply tag filters
            if (tagIds != null && tagIds.Any())
            {
                query = query.Where(r =>
                    r.RestaurantTags.Any(rt => tagIds.Contains(rt.TagId)));
            }

            // Apply price filters
            if (minPrice.HasValue)
            {
                query = query.Where(r => r.MinTypicalPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(r => r.MaxTypicalPrice <= maxPrice.Value);
            }

            // Apply sorting 
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "highestrated" => query.OrderByDescending(r => r.AverageRating),
                    "mostreviewed" => query.OrderByDescending(r => r.ReviewCount),
                    "relevance" => query.OrderByDescending(r => r.AverageRating * 0.7 + r.ReviewCount * 0.3),
                    _ => query.OrderByDescending(r => r.AverageRating)
                };
            }            else
            {
                // Default sorting - use relevance formula for consistent behavior
                query = query.OrderByDescending(r => r.AverageRating * 0.7 + r.ReviewCount * 0.3);
            }

            // If we have coordinates, we need to filter and sort by distance in memory
            List<Restaurant> finalResults;
            int totalCount;

            if (latitude.HasValue && longitude.HasValue)
            {
                double searchLat = latitude.Value;
                double searchLng = longitude.Value;
                double searchRadius = radiusInKm ?? 5.0;

                // Execute base query to get filtered results before distance calculation
                var filteredRestaurants = await query.ToListAsync();                // Calculate distances and apply distance filter
                var restaurantsWithDistance = filteredRestaurants
                    .Where(r => r.Address != null && r.Address.Latitude.HasValue && r.Address.Longitude.HasValue)
                    .Select(r => new
                    {
                        Restaurant = r,
                        Distance = CalculateDistance(
                            searchLat,
                            searchLng,
                            r.Address.Latitude!.Value,
                            r.Address.Longitude!.Value)
                    })
                    .Where(x => x.Distance <= searchRadius)
                    // Chỉ sắp xếp theo khoảng cách khi sortBy rỗng hoặc là "distance"
                    .ToList();
                //log ghi ra tất cả nhà hàng để biết số lượng nhà hàng
                _logger.LogInformation($"Tổng số nhà hàng tìm thấy: {restaurantsWithDistance.Count}");

                totalCount = restaurantsWithDistance.Count;                // Áp dụng sắp xếp dựa trên tuỳ chọn người dùng (không ưu tiên khoảng cách)
                var sortedResults = sortBy?.ToLower() switch
                {
                    "highestrated" => restaurantsWithDistance.OrderByDescending(x => x.Restaurant.AverageRating),
                    "mostreviewed" => restaurantsWithDistance.OrderByDescending(x => x.Restaurant.ReviewCount),
                    "relevance" => restaurantsWithDistance.OrderByDescending(x => x.Restaurant.AverageRating * 0.7 + x.Restaurant.ReviewCount * 0.3),
                    _ => restaurantsWithDistance.OrderByDescending(x => x.Restaurant.AverageRating * 0.7 + x.Restaurant.ReviewCount * 0.3) // Mặc định theo relevance
                };
                
                // Apply pagination
                finalResults = sortedResults
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.Restaurant)
                    .ToList();
            }
            else
            {
                // No geo filtering needed, execute query directly with pagination
                totalCount = await query.CountAsync();
                finalResults = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // Return as paged list
            return new StaticPagedList<Restaurant>(finalResults, page, pageSize, totalCount);
        }
        // Helper method to calculate distance between two coordinates using the Haversine formula
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0; // Earth's radius in kilometers

            // Convert degrees to radians
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            // Haversine formula
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadiusKm * c;

            return distance; // Distance in kilometers
        }

        // Convert degrees to radians
        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }        public Task<List<CuisineType>> GetAllCuisineTypesAsync()
        {
            return _context.CuisineTypes
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        
        /// <summary>
        /// Chuẩn hóa địa chỉ và tọa độ với giá trị mặc định cho Đà Nẵng
        /// </summary>
        public (string address, double latitude, double longitude, double radiusInKm) NormalizeLocationParameters(
            string? address, double? latitude = null, double? longitude = null, string? maxDistance = null)
        {
            // Mặc định địa chỉ là Đà Nẵng nếu không có địa chỉ
            if (string.IsNullOrEmpty(address))
            {
                address = "Đà Nẵng";
            }
            
            double normalizedLatitude = latitude ?? 16.0544; // Tọa độ trung tâm Đà Nẵng
            double normalizedLongitude = longitude ?? 108.2022;
            
            double radiusInKm = 5.0;
            
            if (!string.IsNullOrEmpty(maxDistance) && double.TryParse(maxDistance, out double parsedDistance))
            {
                radiusInKm = parsedDistance;
            }
            
            return (address, normalizedLatitude, normalizedLongitude, radiusInKm);
        }

        public Task<Restaurant?> GetRestaurantByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
