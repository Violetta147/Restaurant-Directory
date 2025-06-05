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
using static PBL3.ViewModels.Search.SearchConstants;

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
            string selectedDistanceCategory = null, // Will use enum default
            string selectedSortOption = null, // Will use enum default
            int page = 1,
            int pageSize = 10,
            double? lat = null, // User's location lat
            double? lng = null, // User's location lng
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false)
        {
            // Set enum-based defaults
            selectedDistanceCategory ??= DistanceCategory.BirdseyeView.ToValue();
            selectedSortOption ??= SortOption.Relevance.ToValue();
            
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

            // Get all matching restaurants without sorting (we'll handle sorting in service layer)
            var allRestaurants = await GetFilteredRestaurantsAsync(
                searchTerm, selectedCategory, lat, lng, actualRadiusKm, 
                minRating, priceRange, isOpenNow);
            
            // Calculate relevance scores and prepare for sorting
            var rankedRestaurants = CalculateRelevanceScores(allRestaurants, searchTerm, lat, lng);
            
            // Apply proper sorting with priority: Distance, MinRating, PriceRange
            var sortedRestaurants = ApplyBusinessSorting(rankedRestaurants, selectedSortOption, lat, lng);
            
            // Apply pagination
            var totalCount = sortedRestaurants.Count;
            var pagedRestaurants = sortedRestaurants
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            // Tạo đối tượng Location của người dùng nếu có tọa độ để tính khoảng cách
            Point userLocation = null;
            if (lat.HasValue && lng.HasValue)
            {
                userLocation = new Point(lng.Value, lat.Value) { SRID = 4326 };
            }
            
            // Convert to ViewModels
            var viewModels = pagedRestaurants.Select(r => new RestaurantViewModel
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
                // Convert from NetTopologySuite.Geometries.Point to coordinates
                Latitude = r.Location?.Y ?? 0,
                Longitude = r.Location?.X ?? 0,
                // Calculate distance if user location is available
                Distance = userLocation != null && r.Location != null 
                    ? r.Location.Distance(userLocation) / 1000 // Convert from meters to km
                    : null,
                // Handle opening hours
                OpeningTime = r.OpeningTime,
                ClosingTime = r.ClosingTime,
                IsOpen = IsRestaurantOpenNow(r.OpeningTime, r.ClosingTime)
            }).ToList();
            
            // Create IPagedList from viewModels
            return new StaticPagedList<RestaurantViewModel>(
                viewModels,
                page,
                pageSize,
                totalCount);
        }

        /// <summary>
        /// Get filtered restaurants from repository without pagination or sorting
        /// </summary>
        private async Task<List<Restaurant>> GetFilteredRestaurantsAsync(
            string searchTerm, string selectedCategory, double? lat, double? lng, 
            double? actualRadiusKm, double? minRating, string priceRange, bool isOpenNow)
        {
            var query = await _restaurantRepository.GetPagedRestaurantsAsync(
                searchTerm, selectedCategory, 1, int.MaxValue, lat, lng, 
                actualRadiusKm, "Relevance", // Use basic sorting, we'll override later
                minRating, priceRange, isOpenNow);
            
            return query.ToList();
        }

        /// <summary>
        /// Calculate relevance scores for search results based on full-text search matching
        /// </summary>
        private List<RestaurantWithScore> CalculateRelevanceScores(List<Restaurant> restaurants, string searchTerm, double? lat, double? lng)
        {
            var restaurantsWithScores = new List<RestaurantWithScore>();
            bool hasSearchTerm = !string.IsNullOrWhiteSpace(searchTerm);
            
            foreach (var restaurant in restaurants)
            {
                var score = new RelevanceScore();
                
                if (hasSearchTerm)
                {
                    var searchTermLower = searchTerm.ToLower();
                    var searchWords = searchTermLower.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    // Score based on exact name match (highest priority)
                    if (restaurant.Name.ToLower().Contains(searchTermLower))
                    {
                        score.NameMatchScore = restaurant.Name.ToLower() == searchTermLower ? 100 : 50;
                    }
                    
                    // Score based on category match
                    if (restaurant.Category.ToLower().Contains(searchTermLower))
                    {
                        score.CategoryMatchScore = 30;
                    }
                    
                    // Score based on description match
                    if (restaurant.Description.ToLower().Contains(searchTermLower))
                    {
                        score.DescriptionMatchScore = 20;
                    }
                    
                    // Score based on keywords match
                    if (restaurant.Keywords.ToLower().Contains(searchTermLower))
                    {
                        score.KeywordMatchScore = 25;
                    }
                    
                    // Bonus for multiple word matches
                    var matchedWords = searchWords.Count(word => 
                        restaurant.Name.ToLower().Contains(word) ||
                        restaurant.Category.ToLower().Contains(word) ||
                        restaurant.Description.ToLower().Contains(word) ||
                        restaurant.Keywords.ToLower().Contains(word));
                    
                    score.MultiWordBonus = matchedWords > 1 ? matchedWords * 5 : 0;
                }
                
                // Calculate distance score if location is available
                if (lat.HasValue && lng.HasValue && restaurant.Location != null)
                {
                    var userLocation = new Point(lng.Value, lat.Value) { SRID = 4326 };
                    var distanceKm = restaurant.Location.Distance(userLocation) / 1000;
                    
                    // Closer restaurants get higher scores (inverse relationship)
                    score.DistanceScore = Math.Max(0, 100 - (distanceKm * 2)); // Penalty increases with distance
                }
                
                restaurantsWithScores.Add(new RestaurantWithScore 
                { 
                    Restaurant = restaurant, 
                    RelevanceScore = score 
                });
            }
            
            return restaurantsWithScores;
        }

        /// <summary>
        /// Apply business sorting with correct priority: Distance, MinRating, PriceRange
        /// </summary>
        private List<Restaurant> ApplyBusinessSorting(List<RestaurantWithScore> restaurantsWithScores, string selectedSortOption, double? lat, double? lng)
        {
            IEnumerable<RestaurantWithScore> sortedQuery = restaurantsWithScores;
            
            switch (selectedSortOption?.ToLower())
            {
                case "distance":
                    if (lat.HasValue && lng.HasValue)
                    {
                        var userLocation = new Point(lng.Value, lat.Value) { SRID = 4326 };
                        sortedQuery = restaurantsWithScores
                            .OrderBy(r => r.Restaurant.Location?.Distance(userLocation) ?? double.MaxValue)
                            .ThenByDescending(r => r.Restaurant.Rating) // Secondary: Higher rating first
                            .ThenBy(r => ConvertPriceLevelToInt(r.Restaurant.PriceLevel)); // Tertiary: Lower price first
                    }
                    else
                    {
                        // Fallback to relevance if no location
                        sortedQuery = restaurantsWithScores
                            .OrderByDescending(r => r.RelevanceScore.TotalScore)
                            .ThenByDescending(r => r.Restaurant.Rating);
                    }
                    break;
                    
                case "rating":
                    sortedQuery = restaurantsWithScores
                        .OrderByDescending(r => r.Restaurant.Rating) // Primary: Higher rating first
                        .ThenBy(r => lat.HasValue && lng.HasValue ? 
                            r.Restaurant.Location?.Distance(new Point(lng.Value, lat.Value) { SRID = 4326 }) ?? double.MaxValue : 
                            0) // Secondary: Closer distance
                        .ThenBy(r => ConvertPriceLevelToInt(r.Restaurant.PriceLevel)); // Tertiary: Lower price first
                    break;
                    
                case "nameaz":
                    sortedQuery = restaurantsWithScores
                        .OrderBy(r => r.Restaurant.Name)
                        .ThenByDescending(r => r.Restaurant.Rating);
                    break;
                    
                case "nameza":
                    sortedQuery = restaurantsWithScores
                        .OrderByDescending(r => r.Restaurant.Name)
                        .ThenByDescending(r => r.Restaurant.Rating);
                    break;
                    
                case "relevance":
                default:
                    // Primary sort by relevance score, then by business priorities
                    sortedQuery = restaurantsWithScores
                        .OrderByDescending(r => r.RelevanceScore.TotalScore) // Primary: Relevance
                        .ThenBy(r => lat.HasValue && lng.HasValue ? 
                            r.Restaurant.Location?.Distance(new Point(lng.Value, lat.Value) { SRID = 4326 }) ?? double.MaxValue : 
                            0) // Secondary: Distance (closer first)
                        .ThenByDescending(r => r.Restaurant.Rating) // Tertiary: Rating (higher first)
                        .ThenBy(r => ConvertPriceLevelToInt(r.Restaurant.PriceLevel)); // Quaternary: Price (lower first)
                    break;
            }
            
            return sortedQuery.Select(r => r.Restaurant).ToList();
        }

        public (string processedQuery, string processedLocation, string processedDistanceCategory, double? processedLat, double? processedLng, int validatedPage) 
            ProcessSearchParameters(string query, string location, string selectedDistanceCategory, double? lat, double? lng, int page = 1)
        {
            // Set default values according to user specifications
            var processedQuery = string.IsNullOrEmpty(query) ? "" : query;
            var processedLocation = location;
            var processedDistanceCategory = selectedDistanceCategory;
            var processedLat = lat;
            var processedLng = lng;
            
            // Validate page parameter - business rule: page must be >= 1
            var validatedPage = page <= 0 ? 1 : page;

            // Apply default behavior when location is empty
            if (string.IsNullOrEmpty(processedLocation))
            {
                processedLocation = "Liên Chiểu Đà Nẵng";
                
                // Set default coordinates to null as specified
                processedLat = null;
                processedLng = null;
            }

            return (processedQuery, processedLocation, processedDistanceCategory, processedLat, processedLng, validatedPage);
        }
    }

    #region Helper Classes for Relevance Scoring

    /// <summary>
    /// Container for restaurant with calculated relevance score
    /// </summary>
    public class RestaurantWithScore
    {
        public Restaurant Restaurant { get; set; }
        public RelevanceScore RelevanceScore { get; set; }
    }

    /// <summary>
    /// Relevance scoring system for search results
    /// </summary>
    public class RelevanceScore
    {
        public double NameMatchScore { get; set; } = 0;
        public double CategoryMatchScore { get; set; } = 0;
        public double DescriptionMatchScore { get; set; } = 0;
        public double KeywordMatchScore { get; set; } = 0;
        public double MultiWordBonus { get; set; } = 0;
        public double DistanceScore { get; set; } = 0;
        
        public double TotalScore => NameMatchScore + CategoryMatchScore + DescriptionMatchScore + 
                                   KeywordMatchScore + MultiWordBonus + DistanceScore;
    }

    #endregion
}