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
        }        public Task<IEnumerable<Restaurant>> GetRestaurantsAsync()
        {
            throw new NotImplementedException();
        }        public async Task<IPagedList<Restaurant>> GetAllRestaurantsPaginatedAsync(int pageNumber = 1, int pageSize = 10)
        {
            // Only include essential data for restaurant listing - remove heavy collections
            var query = _context.Restaurants
                .Include(r => r.Address)
                .Include(r => r.RestaurantCuisines)
                    .ThenInclude(rc => rc.CuisineType)
                .Include(r => r.RestaurantTags)
                    .ThenInclude(rt => rt.Tag)
                .Include(r => r.OperatingHours)
                .OrderBy(r => r.Name)
                .AsQueryable();

            // Execute query and get total count for pagination
            var totalCount = await query.CountAsync();
            
            // Get the items for current page
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            // Return as IPagedList
            return new StaticPagedList<Restaurant>(items, pageNumber, pageSize, totalCount);
        }
        public Task<Restaurant?> GetRestaurantByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
          //Search by searchTerm
        public async Task<IPagedList<Restaurant>> SearchRestaurantsAsync(
                string? searchTerm = null,
                IEnumerable<int>? cuisineTypeIds = null,
                IEnumerable<int>? tagIds = null,
                decimal? minPrice = null,
                decimal? maxPrice = null,
                string? sortBy = null,
                int pageNumber = 1,
                int pageSize = 10)
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

            // Handle search term filtering
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (searchTerm.Length < 3)
                {
                    //log warning and return empty result if search term is too short
                    _logger.LogWarning("Độ dài < 3 là quá ngắn để có ý nghĩa: {searchTerm}", searchTerm);
                    // Return empty paged list for invalid search terms
                    return new StaticPagedList<Restaurant>(new List<Restaurant>(), pageNumber, pageSize, 0);
                }

                //Normalize search term to lower case for case-insensitive comparison
                searchTerm = searchTerm.ToLower();                // Create a relevance-based query with scoring (removed MenuItems search for performance)
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    r.Description.ToLower().Contains(searchTerm) ||
                    r.RestaurantCuisines.Any(rc => rc.CuisineType.Name.ToLower().Contains(searchTerm)) ||
                    r.RestaurantTags.Any(rt => rt.Tag.Name.ToLower().Contains(searchTerm))
                )
                .OrderByDescending(r =>
                    // Relevance scoring - higher score = more relevant
                    (r.Name.ToLower().Contains(searchTerm) ? 100 : 0) +                    // Highest priority
                    (r.Name.ToLower().StartsWith(searchTerm) ? 50 : 0) +                   // Name starts with term
                    (r.Description.ToLower().Contains(searchTerm) ? 30 : 0) +              // Description match
                    (r.RestaurantCuisines.Count(rc => rc.CuisineType.Name.ToLower().Contains(searchTerm)) * 15) + // Cuisine type
                    (r.RestaurantTags.Count(rt => rt.Tag.Name.ToLower().Contains(searchTerm)) * 10)              // Tags
                )
                .ThenByDescending(r => r.AverageRating)  // Secondary sort by rating
                .ThenByDescending(r => r.ReviewCount);   // Tertiary sort by review count
            }
            else
            {
                // When no search term provided, order by rating and review count
                query = query.OrderByDescending(r => r.AverageRating)
                           .ThenByDescending(r => r.ReviewCount);
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
                    "recommended" => query, // Keep existing relevance-based sorting from search
                    _ => query // Default to existing relevance-based sorting
                };
            }
            
            // Execute query and get total count for pagination
            var totalCount = await query.CountAsync();
            
            // Get the items for current page
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            // Return as IPagedList
            return new StaticPagedList<Restaurant>(items, pageNumber, pageSize, totalCount);
        }        //Search by addressQuery
        public async Task<IPagedList<Restaurant>> SearchRestaurantsByLocationAsync(
            string? addressQuery,
            double? latitude = null,
            double? longitude = null,
            double? radiusInKm = 5.0,
            int pageNumber = 1,
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

            // Default to Da Nang city center if no coordinates provided
            double searchLat = latitude ?? 16.075000; // Default latitude for Da Nang
            double searchLng = longitude ?? 108.206230; // Default longitude for Da Nang
            double searchRadius = radiusInKm ?? 5.0;
            
            // If we have valid coordinates, filter by distance
            if (latitude.HasValue && longitude.HasValue)
            {
                // We can't efficiently filter by distance in LINQ-to-SQL directly
                // So we'll fetch restaurants and calculate distances in memory
                
                // First, get restaurants with valid coordinates
                var restaurantsWithCoords = await query
                    .Where(r => r.Address != null && r.Address.Latitude.HasValue && r.Address.Longitude.HasValue)
                    .ToListAsync();
                  // Calculate distances and filter by radius
                var filteredRestaurantsWithDistance = restaurantsWithCoords
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
                    .OrderBy(x => x.Distance)
                    .ToList();
                    
                // Get total count for pagination
                int totalCount = filteredRestaurantsWithDistance.Count;
                
                // Apply pagination
                var pagedItems = filteredRestaurantsWithDistance
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.Restaurant)
                    .ToList();
                
                // Return as PagedList
                return new StaticPagedList<Restaurant>(pagedItems, pageNumber, pageSize, totalCount);
            }
            
            // If no valid coordinates or radius provided, fall back to text search on address
            if (!string.IsNullOrEmpty(addressQuery))
            {
                // Search by address text
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
            
            // Execute query and get total count for pagination
            var totalCountText = await query.CountAsync();
            
            // Get the items for current page
            var items = await query
                .OrderBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            // Return as IPagedList
            return new StaticPagedList<Restaurant>(items, pageNumber, pageSize, totalCountText);
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
                int pageNumber = 1,
                int pageSize = 10
            )        {
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
                    return new StaticPagedList<Restaurant>(new List<Restaurant>(), pageNumber, pageSize, 0);
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
                    "recommended" => query.OrderByDescending(r => r.AverageRating * 0.7 + r.ReviewCount * 0.3),
                    _ => query.OrderByDescending(r => r.AverageRating) // Default sort
                };
            }
            else
            {
                // Default sorting
                query = query.OrderByDescending(r => r.AverageRating);
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
                var filteredRestaurants = await query.ToListAsync();

                // Calculate distances and apply distance filter
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
                    .OrderBy(x => x.Distance)
                    .ToList();

                totalCount = restaurantsWithDistance.Count;
                
                // Apply pagination
                finalResults = restaurantsWithDistance
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.Restaurant)
                    .ToList();
            }
            else
            {
                // No geo filtering needed, execute query directly with pagination
                totalCount = await query.CountAsync();
                finalResults = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // Return as paged list
            return new StaticPagedList<Restaurant>(finalResults, pageNumber, pageSize, totalCount);
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
        }
    }
}
