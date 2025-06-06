using Microsoft.AspNetCore.Mvc;
using PBL3.Models;
using PBL3.ViewModel;
using PBL3.Services.Interfaces;
using PBL3.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; // Add this for .Select() and .FirstOrDefault()
using System.Threading.Tasks;
using System.Collections; // Add this for async operations
using X.PagedList;

namespace PBL3.Controllers
{    public class SearchController : Controller
    {   
        private readonly IRestaurantService _restaurantService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        
        public SearchController(IRestaurantService restaurantService, ApplicationDbContext context, IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _restaurantService = restaurantService ?? throw new ArgumentNullException(nameof(restaurantService));
            _context = context;
        }        public async Task<IActionResult> Index(string searchTerm = "", string Address= "", int page = 1, int pageSize = 10, IEnumerable<int>? tagIds = null, IEnumerable<int>? cuisineTypeIds = null, string sortBy = "relevance", decimal? minPrice = null, decimal? maxPrice = null, string maxDistance = "")
        {
            //Mapbox token
            ViewBag.MapboxToken = _config["Mapbox:AccessToken"];            // Populate ViewBag.CuisineTypes for the filter dropdown
            ViewBag.CuisineTypes = await _context.CuisineTypes
                .OrderBy(c => c.Name)
                .ToListAsync();

            var svm = new SearchViewModel
            {
                SearchTerm = searchTerm,
                Address = Address,
                Page = page,
                PageSize = pageSize,
                TagIds = tagIds,
                CuisineTypeIds = cuisineTypeIds,                SortBy = sortBy,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MaxDistance = maxDistance
            };            // Decide which search method to use based on provided parameters
            if (!string.IsNullOrEmpty(searchTerm) && !string.IsNullOrEmpty(Address))
            {
                // Both search term and location provided - use advanced search
                svm.RestaurantCards = await SearchByTermAndLocationAsync(
                    searchTerm, 
                    Address,
                    svm.Lat, 
                    svm.Lng, 
                    5.0, // Default radius in km
                    cuisineTypeIds,
                    tagIds,
                    minPrice,
                    maxPrice,
                    sortBy,
                    page,
                    pageSize
                );
            }
            else if (!string.IsNullOrEmpty(searchTerm))
            {
                // Only search term provided
                svm.RestaurantCards = await SearchByTermAsync(svm.SearchTerm);
            }
            else if (!string.IsNullOrEmpty(Address))
            {
                // Only location provided
                svm.RestaurantCards = await SearchByLocationAsync(
                    Address,
                    svm.Lat,
                    svm.Lng,
                    page,
                    pageSize
                );
            }
            else
            {
                // No search criteria - show all restaurants
                svm.RestaurantCards = await GetAllRestaurantsAsync(page, pageSize);
            }

            return View(svm);}
        
        // The private helper methods below might be less necessary if the Index action handles all cases
        // and directly populates the SearchViewModel.
        // If you keep them, they should also be async Task and map to RestaurantCardViewModel.
        
        private async Task<IPagedList<RestaurantCardViewModel>> SearchByTermAsync(string searchTerm)
        {
            var pagedRestaurants = await _restaurantService.SearchRestaurantsAsync(searchTerm);
            
            var restaurantCards = pagedRestaurants.Select(r => new RestaurantCardViewModel
            {
                Id = r.Id,
                Name = r.Name,
                CardImageUrl = r.MainImageUrl ?? "/images/default-restaurant.png",
                CuisineSummary = r.RestaurantCuisines?.Select(rc => rc.CuisineType.Name).ToList<string?>() ?? new List<string?>(),
                AverageRating = r.AverageRating,
                ReviewCount = r.ReviewCount,
                FullAddress = r.Address?.FullAddress ?? "",
                OperatingHours = r.OperatingHours
            }).ToList();
            
            // Return as IPagedList with correct pagination info
            return new StaticPagedList<RestaurantCardViewModel>(
                restaurantCards, 
                pagedRestaurants.PageNumber, 
                pagedRestaurants.PageSize, 
                pagedRestaurants.TotalItemCount
            );
        }        private async Task<IPagedList<RestaurantCardViewModel>> GetAllRestaurantsAsync(int page, int pageSize)
        {
            // Use the service layer instead of accessing context directly
            var pagedRestaurants = await _restaurantService.GetAllRestaurantsPaginatedAsync(page, pageSize);

            var restaurantCards = pagedRestaurants.Select(r => new RestaurantCardViewModel
            {
                Id = r.Id,
                Name = r.Name,
                CardImageUrl = r.MainImageUrl ?? "/images/default-restaurant.png",
                CuisineSummary = r.RestaurantCuisines?.Select(rc => rc.CuisineType.Name).ToList<string?>() ?? new List<string?>(),
                AverageRating = r.AverageRating,
                ReviewCount = r.ReviewCount,
                FullAddress = r.Address?.FullAddress ?? "",
                OperatingHours = r.OperatingHours
            }).ToList();

            return new StaticPagedList<RestaurantCardViewModel>(
                restaurantCards,
                pagedRestaurants.PageNumber,
                pagedRestaurants.PageSize,
                pagedRestaurants.TotalItemCount
            );
        }

        private async Task<IPagedList<RestaurantCardViewModel>> SearchByLocationAsync(
            string address,
            double? latitude,
            double? longitude,
            int page,
            int pageSize)
        {
            // Use the service implementation
            var pagedRestaurants = await _restaurantService.SearchRestaurantsByLocationAsync(
                address, 
                latitude,
                longitude,
                5.0, // default radius
                page,
                pageSize);
                
            // Map to view model
            var restaurantCards = pagedRestaurants.Select(r => new RestaurantCardViewModel
            {
                Id = r.Id,
                Name = r.Name,
                CardImageUrl = r.MainImageUrl ?? "/images/default-restaurant.png",
                CuisineSummary = r.RestaurantCuisines?.Select(rc => rc.CuisineType.Name).ToList<string?>() ?? new List<string?>(),
                AverageRating = r.AverageRating,
                ReviewCount = r.ReviewCount,
                FullAddress = r.Address?.FullAddress ?? "",
                OperatingHours = r.OperatingHours
            }).ToList();
            
            // Return as IPagedList
            return new StaticPagedList<RestaurantCardViewModel>(
                restaurantCards,
                pagedRestaurants.PageNumber,
                pagedRestaurants.PageSize,
                pagedRestaurants.TotalItemCount
            );
        }
        
        private async Task<IPagedList<RestaurantCardViewModel>> SearchByTermAndLocationAsync(
            string searchTerm,
            string address,
            double? latitude,
            double? longitude,
            double? radiusInKm,
            IEnumerable<int>? cuisineTypeIds,
            IEnumerable<int>? tagIds,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            int page,
            int pageSize)
        {
            // Use advanced search from service
            var pagedRestaurants = await _restaurantService.SearchRestaurantsAdvancedAsync(
                searchTerm,
                address,
                latitude,
                longitude,
                radiusInKm,
                cuisineTypeIds,
                tagIds,
                minPrice,
                maxPrice,
                sortBy,
                page,
                pageSize);
                
            // Map results to view model
            var restaurantCards = pagedRestaurants.Select(r => new RestaurantCardViewModel
            {
                Id = r.Id,
                Name = r.Name,
                CardImageUrl = r.MainImageUrl ?? "/images/default-restaurant.png",
                CuisineSummary = r.RestaurantCuisines?.Select(rc => rc.CuisineType.Name).ToList<string?>() ?? new List<string?>(),
                AverageRating = r.AverageRating,
                ReviewCount = r.ReviewCount,
                FullAddress = r.Address?.FullAddress ?? "",
                OperatingHours = r.OperatingHours
            }).ToList();
            
            // Return as IPagedList
            return new StaticPagedList<RestaurantCardViewModel>(
                restaurantCards,
                pagedRestaurants.PageNumber,
                pagedRestaurants.PageSize,
                pagedRestaurants.TotalItemCount
            );
        }
    }
}