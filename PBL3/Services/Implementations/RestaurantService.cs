using PBL3.Models;
using PBL3.Services.Interfaces;
using PBL3.Data;
using X.PagedList;
using Microsoft.EntityFrameworkCore;
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
        public Task<IEnumerable<Restaurant>> GetRestaurantsAsync()
        {
            throw new NotImplementedException();
        }
        public Task<Restaurant?> GetRestaurantByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Restaurant>> SearchRestaurantsAsync(
                string searchTerm,
                IEnumerable<int>? cuisineTypeIds = null,
                IEnumerable<int>? tagIds = null,
                decimal? minPrice = null,
                decimal? maxPrice = null,
                string? sortBy = null,
                int pageNumber = 1,
                int pageSize = 10)
        {
            var query = _context.Restaurants
                .Include(r => r.Address)
                .Include(r => r.Reviews)
                .Include(r => r.Photos)
                .Include(r => r.RestaurantCuisines)
                    .ThenInclude(rc => rc.CuisineType)
                .Include(r => r.RestaurantTags)
                    .ThenInclude(rt => rt.Tag)
                .Include(r => r.OperatingHours)
                .Include(r => r.MenuItems)
                .AsQueryable();

            // Apply text search with relevance scoring
                if (searchTerm.Length < 3)
                {
                    //log warning and return empty result if search term is too short
                    _logger.LogWarning("Độ dài < 3 là quá ngắn để có ý nghĩa: {searchTerm}", searchTerm);
                    return Enumerable.Empty<Restaurant>();
                }
                //Normalize search term to lower case for case-insensitive comparison
                searchTerm = searchTerm.ToLower();

                // Create a relevance-based query with scoring
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    r.Description.ToLower().Contains(searchTerm) ||
                    r.MenuItems.Any(mi => mi.Name.ToLower().Contains(searchTerm)) ||
                    r.RestaurantCuisines.Any(rc => rc.CuisineType.Name.ToLower().Contains(searchTerm)) ||
                    r.RestaurantTags.Any(rt => rt.Tag.Name.ToLower().Contains(searchTerm))
                )
                .OrderByDescending(r =>
                    // Relevance scoring - higher score = more relevant
                    (r.Name.ToLower().Contains(searchTerm) ? 100 : 0) +                    // Highest priority
                    (r.Name.ToLower().StartsWith(searchTerm) ? 50 : 0) +                   // Name starts with term
                    (r.Description.ToLower().Contains(searchTerm) ? 30 : 0) +              // Description match
                    (r.MenuItems.Count(mi => mi.Name.ToLower().Contains(searchTerm)) * 20) + // Menu items (multiply by count)
                    (r.RestaurantCuisines.Count(rc => rc.CuisineType.Name.ToLower().Contains(searchTerm)) * 15) + // Cuisine type
                    (r.RestaurantTags.Count(rt => rt.Tag.Name.ToLower().Contains(searchTerm)) * 10)              // Tags
                )
                .ThenByDescending(r => r.AverageRating)  // Secondary sort by rating
                .ThenByDescending(r => r.ReviewCount);   // Tertiary sort by review count

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
                       // Apply sorting - with searchTerm always present
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
            // Execute query and apply pagination
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public Task<IEnumerable<Restaurant>> SearchRestaurantsByLocationAsync(
            string? addressQuery,
            double? latitude = null,
            double? longitude = null,
            double? radiusInKm = 5.0,
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<Restaurant>> SearchRestaurantsAdvancedAsync(
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
            )
        {
            throw new NotImplementedException();
        }
    }
}