using PBL3.Models;
using PBL3.Data;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Mvc.Core;
using PBL3.Repositories.Interfaces;
using PBL3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using NetTopologySuite.Geometries;
using System.Text;

namespace PBL3.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly ApplicationDbContext _context;
        private const int EarthRadiusMeters = 6371000; // Bán kính Trái đất tính bằng mét

        public RestaurantRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IPagedList<Restaurant>> GetPagedRestaurantsAsync(
            string searchTerm = "",
            string selectedCategory = "", // Changed from category
            int page = 1,
            int pageSize = 10,
            double? lat = null, // User's current location latitude
            double? lng = null, // User's current location longitude
            double? actualRadiusKm = null, // Calculated radius based on SelectedDistanceCategory
            string selectedSortOption = "Relevance", // New sort option
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false)
        {
            var query = _context.Restaurants.AsQueryable();

            // Áp dụng tìm kiếm không gian nếu có tọa độ
            // Apply text search if searchTerm is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplyTextSearch(query, searchTerm);
            }

            // Apply spatial filter if coordinates and radius are provided
            if (lat.HasValue && lng.HasValue && actualRadiusKm.HasValue)
            {
                query = ApplySpatialFilter(query, lat.Value, lng.Value, actualRadiusKm.Value);
            }

            // Áp dụng các bộ lọc
            query = ApplyFilters(query, selectedCategory, minRating, priceRange, isOpenNow);

            // Sorting logic (Applied AFTER filtering and text search)
            if (lat.HasValue && lng.HasValue && selectedSortOption == "Distance")
            {
                var userLocation = CreatePoint(lat.Value, lng.Value);
                query = query.OrderBy(r => r.Location.Distance(userLocation));
            }
            else if (selectedSortOption == "Rating")
            {
                query = query.OrderByDescending(r => r.Rating).ThenBy(r => r.Id); // Changed AverageRating to Rating
            }
            else if (selectedSortOption == "NameAZ")
            {
                query = query.OrderBy(r => r.Name).ThenBy(r => r.Id);
            }
            else // Default or Relevance (can be improved with actual relevance scores if using more advanced FTS)
            {
                // If using FTS and it provides a rank, sort by that rank here.
                // For now, a stable sort by ID or another relevant field is good if 'Relevance' is chosen but no FTS rank is available.
                query = query.OrderBy(r => r.Id); // Fallback to ID for stable pagination
            }

            // Sử dụng cách tiếp cận async bằng cách tạo PagedList qua phương thức tĩnh
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            int totalCount = await query.CountAsync();
            return new StaticPagedList<Restaurant>(items, page, pageSize, totalCount);
        }

        public async Task<List<Restaurant>> GetRestaurantsInAreaAsync(
            double lat,
            double lng,
            double actualRadiusKm = 3.0, // Changed from radiusKm
            string searchTerm = "",
            string selectedCategory = "", // Changed from category
            // searchIn, matchAllTerms, useExactPhrase, searchInDescription are removed as text search is now standardized
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false)
        {
            var query = _context.Restaurants.AsQueryable();

            // Áp dụng tìm kiếm không gian
            query = ApplySpatialFilter(query, lat, lng, actualRadiusKm);

            // Áp dụng các bộ lọc
            query = ApplyFilters(query, selectedCategory, minRating, priceRange, isOpenNow);

            // Áp dụng tìm kiếm văn bản nếu có từ khóa
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplyTextSearch(query, searchTerm);
            }

            return await query.ToListAsync();
        }

        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            return await _context.Restaurants.FindAsync(id);
        }
        
        // Hàm mới để lấy các danh mục nhà hàng độc nhất
        public async Task<List<string>> GetUniqueRestaurantCategoriesAsync()
        {
            return await _context.Restaurants
                .Select(r => r.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        #region Private Helper Methods

        private IQueryable<Restaurant> ApplySpatialFilter(IQueryable<Restaurant> query, double lat, double lng, double radiusKm)
        {
            var userLocation = CreatePoint(lat, lng);
            return query
                .Where(r => r.Location.Distance(userLocation) <= radiusKm * 1000) // Convert km to meters
                .OrderBy(r => r.Location.Distance(userLocation));
        }

        private IQueryable<Restaurant> ApplyFilters(
            IQueryable<Restaurant> query,
            string category,
            double? minRating,
            string priceRange,
            bool isOpenNow)
        {
            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.Category == category);
            }

            // Filter by minimum rating
            if (minRating.HasValue)
            {
                query = query.Where(r => r.Rating >= minRating.Value);
            }

            // Filter by price range
            if (!string.IsNullOrEmpty(priceRange))
            {
                // $ = 1, $$ = 2, $$$ = 3, $$$$ = 4
                switch (priceRange)
                {
                    case "$":
                        query = query.Where(r => r.PriceLevel == "$");
                        break;
                    case "$$":
                        query = query.Where(r => r.PriceLevel == "$$");
                        break;
                    case "$$$":
                        query = query.Where(r => r.PriceLevel == "$$$");
                        break;
                    case "$$$$":
                        query = query.Where(r => r.PriceLevel == "$$$$");
                        break;
                }
            }

            // Filter by opening hours
            if (isOpenNow)
            {
                var now = DateTime.Now.TimeOfDay;
                query = query.Where(r => r.OpeningTime <= now && r.ClosingTime >= now);
            }

            return query;
        }

        private IQueryable<Restaurant> ApplyTextSearch(IQueryable<Restaurant> query, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return query;
            }

            // Using default behavior for PrepareSearchQuery as matchAllTerms and useExactPhrase are not user-driven anymore for this part.
            string preparedSearchTerm = PrepareSearchQuery(searchTerm, false, false);

            // Search in Name, Description, Keywords, and Category using OR logic with Like
            // Assuming Category is a string property on Restaurant. If it's a related entity, adjust accordingly.
            query = query.Where(r =>
                EF.Functions.Like(r.Name, "%" + preparedSearchTerm + "%") ||
                EF.Functions.Like(r.Description, "%" + preparedSearchTerm + "%") ||
                EF.Functions.Like(r.Keywords, "%" + preparedSearchTerm + "%") ||
                EF.Functions.Like(r.Category, "%" + preparedSearchTerm + "%")
            );
            
            return query;
        }

        private string PrepareSearchQuery(string searchTerm, bool matchAllTerms, bool useExactPhrase)
        {
            // If exact phrase search, add quotes if not already present
            if (useExactPhrase && !searchTerm.StartsWith("\"") && !searchTerm.EndsWith("\""))
            {
                searchTerm = $"\"{searchTerm}\"";
            }
            // If matching all terms, add AND between terms
            else if (matchAllTerms && !searchTerm.Contains(" AND ") && !searchTerm.Contains(" OR "))
            {
                var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                searchTerm = string.Join(" AND ", terms);
            }

            return searchTerm;
        }

        // ApplySearchField and CombineSearchResults are commented out as the new ApplyTextSearch is more direct.
        // They can be removed if the new approach is confirmed to cover all needs.
        /*
        private IQueryable<Restaurant> ApplySearchField<TProperty>(
            IQueryable<Restaurant> currentQuery,
            Expression<Func<Restaurant, TProperty>> propertySelector,
            string searchQuery)
        {
            var query = _context.Restaurants.AsQueryable();
            
            // Use FreeText for natural language search
            query = query.Where(r => EF.Functions.FreeText(propertySelector, searchQuery));
            
            // Combine with current results if any
            if (currentQuery != null)
            {
                var currentIds = currentQuery.Select(r => r.Id);
                var newIds = query.Select(r => r.Id);
                var combinedIds = currentIds.Union(newIds);
                return _context.Restaurants.Where(r => combinedIds.Contains(r.Id));
            }
            
            return query;
        }

        private IQueryable<Restaurant> CombineSearchResults<TProperty>(
            IQueryable<Restaurant> currentQuery,
            Expression<Func<Restaurant, TProperty>> propertySelector,
            string searchQuery,
            bool matchAllTerms)
        {
            var query = ApplySearchField(null, propertySelector, searchQuery);
            
            if (currentQuery == null)
            {
                return query;
            }
            
            var currentIds = currentQuery.Select(r => r.Id);
            var newIds = query.Select(r => r.Id);
            
            // If we need to match all terms, use intersection (AND)
            if (matchAllTerms)
            {
                var intersection = currentIds.Intersect(newIds);
                return _context.Restaurants.Where(r => intersection.Contains(r.Id));
            }
            // Otherwise use union (OR)
            else
            {
                var union = currentIds.Union(newIds);
                return _context.Restaurants.Where(r => union.Contains(r.Id));
            }
        }
        */

        private static Point CreatePoint(double lat, double lng)
        {
            // Create a point with SRID 4326 (WGS84) - X is longitude, Y is latitude
            return new Point(lng, lat) { SRID = 4326 };
        }

        #endregion
    }
}