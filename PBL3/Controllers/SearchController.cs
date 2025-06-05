using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL3.Services.Interfaces;
using PBL3.ViewModels;
using PBL3.ViewModels.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using static PBL3.ViewModels.Search.SearchConstants;

namespace PBL3.Controllers
{
    public class SearchController : Controller
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IConfiguration _config;

        public SearchController(
            IRestaurantService restaurantService,
            IConfiguration config)
        {
            _restaurantService = restaurantService ?? throw new ArgumentNullException(nameof(restaurantService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }        [HttpGet]
        public async Task<IActionResult> Index(
            string query = "", // from navbar
            string location = "", // from navbar

            /*IF EMPTY OR NULL THEN IT NAVIGATED FROM HOMEPAGE*/
            string selectedCategory = "", // from filter panel 
            string? selectedDistanceCategory = null, // Will be set to default below
            string? selectedSortOption = null, // Will be set to default below
            int page = 1,
            int pageSize = 10,
            double? lat = null, // User's geocoded location for distance filtering
            double? lng = null, // User's geocoded location for distance filtering
            double? minRating = null, // from filter panel 
            string priceRange = "", // from filter panel 
            bool isOpenNow = false) // from filter panel
        {
            try
            {
                // Set enum-based defaults
                selectedDistanceCategory ??= DistanceCategory.BirdseyeView.ToValue();
                selectedSortOption ??= SortOption.Relevance.ToValue();
                // Process search parameters through service layer (business logic + validation)
                var (processedQuery, processedLocation, processedDistanceCategory, processedLat, processedLng, validatedPage) = 
                    _restaurantService.ProcessSearchParameters(query, location, selectedDistanceCategory, lat, lng, page);

                // Get search results first to determine total pages
                var searchResults = await _restaurantService.SearchRestaurantsAsViewModelsAsync(
                    processedQuery, selectedCategory, processedDistanceCategory, selectedSortOption, 
                    validatedPage, pageSize, processedLat, processedLng, 
                    minRating, priceRange, isOpenNow);

                // If page is too high (beyond available results), redirect to last valid page
                if (searchResults != null && searchResults.PageCount > 0 && validatedPage > searchResults.PageCount)
                {
                    var queryString = Request.QueryString.Value ?? "";
                    var updatedQuery = queryString.Replace($"page={validatedPage}", $"page={searchResults.PageCount}");
                    if (string.IsNullOrEmpty(updatedQuery) || updatedQuery == "?")
                    {
                        updatedQuery = $"?page={searchResults.PageCount}";
                    }
                    return Redirect($"/Search{updatedQuery}");
                }
                var srvm = new SearchResultsViewModel
                {
                    Query = processedQuery,
                    Location = processedLocation,
                    UserLat = processedLat, // User's input location for distance calc
                    UserLng = processedLng,
                    MapCenterLat = processedLat ?? 16.0471, // Use user location or default to Da Nang
                    MapCenterLng = processedLng ?? 108.2062,
                    SelectedCategory = selectedCategory,
                    SelectedDistanceCategory = processedDistanceCategory,
                    SelectedSortOption = selectedSortOption,
                    MinRating = minRating, // Don't set default - let user choose
                    PriceRange = priceRange, // Don't set default - let user choose  
                    IsOpenNow = isOpenNow
                };

                // Populate categories for the dropdown filter
                var uniqueCategories = await _restaurantService.GetUniqueRestaurantCategoriesAsync();
                srvm.Categories = uniqueCategories.Select(c => new SelectListItem { 
                    Value = c, 
                    Text = c,
                    Selected = c == selectedCategory 
                }).ToList();
                // Add an "All" option for categories
                srvm.Categories.Insert(0, new SelectListItem { 
                    Value = "", 
                    Text = "All Categories",
                    Selected = string.IsNullOrEmpty(selectedCategory)
                });

                // Fix Sort options to maintain state persistence
                srvm.SortOptions = SearchConstants.SortOptions.Select(s => new SelectListItem {
                    Value = s.Value,
                    Text = s.Text,
                    Selected = s.Value == selectedSortOption
                }).ToList();

                // Fix Distance options to maintain state persistence  
                srvm.DistanceOptions = SearchConstants.DistanceOptions.Select(d => new SelectListItem {
                    Value = d.Value,
                    Text = d.Text,
                    Selected = d.Value == selectedDistanceCategory
                }).ToList();

                srvm.Restaurants = searchResults;

                ViewData["MapboxToken"] = _config["Mapbox:AccessToken"];

                // Set current search parameters as ViewData for form persistence
                // ViewData for form persistence - adjust as needed for the new filter panel
                ViewData["CurrentQuery"] = processedQuery;
                ViewData["CurrentLocation"] = processedLocation;
                ViewData["SelectedCategory"] = selectedCategory;
                ViewData["SelectedDistanceCategory"] = processedDistanceCategory;
                ViewData["SelectedSortOption"] = selectedSortOption;

                return View(srvm);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error searching restaurants");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRestaurants(
            string query = "",
            string location = "",
            string selectedCategory = "",
            string? selectedDistanceCategory = null, // Will use enum default
            string? selectedSortOption = null, // Will use enum default
            int page = 1,
            int pageSize = 10,
            double? lat = null,
            double? lng = null,
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false)
        {
            try
            {
                // Set enum-based defaults
                selectedDistanceCategory ??= DistanceCategory.BirdseyeView.ToValue();
                selectedSortOption ??= SortOption.Relevance.ToValue();
                // Process search parameters through service layer (business logic + validation)
                var (processedQuery, processedLocation, processedDistanceCategory, processedLat, processedLng, validatedPage) = 
                    _restaurantService.ProcessSearchParameters(query, location, selectedDistanceCategory, lat, lng, page);

                var searchResults = await _restaurantService.SearchRestaurantsAsViewModelsAsync(
                    processedQuery, selectedCategory, processedDistanceCategory, selectedSortOption,
                    validatedPage, pageSize, processedLat, processedLng,
                    minRating, priceRange, isOpenNow);

                return PartialView("_RestaurantListPartial", searchResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading restaurants");
            }
        }

        /// <summary>
        /// API endpoint for map to get restaurants as GeoJSON
        /// Replaces the old MapController
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRestaurantsJson(
            double lat, 
            double lng, 
            string selectedDistanceCategory = null, // Will use 3km default for map
            string query = "", 
            string selectedCategory = "",
            double? minRating = null,
            string priceRange = "",
            bool isOpenNow = false)
        {
            try
            {
                // Set default for map display
                selectedDistanceCategory ??= DistanceCategory.ThreeKm.ToValue();

                // Apply default behavior for map when no specific search criteria
                if (string.IsNullOrEmpty(query) && string.IsNullOrEmpty(selectedCategory))
                {
                    // If coordinates are Da Nang center and no specific distance set, default to 3km
                    if (Math.Abs(lat - 16.0471) < 0.001 && Math.Abs(lng - 108.2062) < 0.001 
                        && selectedDistanceCategory == DistanceCategory.BirdseyeView.ToValue())
                    {
                        selectedDistanceCategory = DistanceCategory.ThreeKm.ToValue();
                    }
                }

                // Get restaurants for map display
                var restaurants = await _restaurantService.GetRestaurantsForMapAsync(
                    lat, lng, query, selectedCategory, selectedDistanceCategory, 
                    minRating, priceRange, isOpenNow);

                // Format as GeoJSON for Mapbox
                var geoJson = new
                {
                    type = "FeatureCollection",
                    features = restaurants.Select(r => new
                    {
                        type = "Feature",
                        geometry = new
                        {
                            type = "Point",
                            coordinates = new[] { r.Longitude, r.Latitude }
                        },
                        properties = new
                        {
                            id = r.Id,
                            name = r.Name,
                            address = r.Address,
                            category = r.Category,
                            rating = r.Rating,
                            slug = r.Slug
                        }
                    })
                };

                return Json(geoJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading restaurants for map");
            }
        }
    }
}