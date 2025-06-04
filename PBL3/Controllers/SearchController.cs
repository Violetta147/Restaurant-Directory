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
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string query = "", // from navbar
            string location = "", // from navbar

            /*IF EMPTY OR NULL THEN IT NAVIGATED FROM HOMEPAGE*/
            string selectedCategory = "", // from filter panel 
            string selectedDistanceCategory = SearchConstants.Distance_BirdseyeView_Value,
            string selectedSortOption = SearchConstants.Sort_Relevance_Value,
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
                var srvm = new SearchResultsViewModel
                {
                    Query = query,
                    Location = location,
                    UserLat = lat, // User's input location for distance calc
                    UserLng = lng,
                    SelectedCategory = selectedCategory,
                    SelectedDistanceCategory = selectedDistanceCategory,
                    SelectedSortOption = selectedSortOption,
                    MinRating = minRating ?? 4,
                    PriceRange = priceRange ?? "$$",
                    IsOpenNow = isOpenNow
                };

                // Populate categories for the dropdown filter
                var uniqueCategories = await _restaurantService.GetUniqueRestaurantCategoriesAsync();
                srvm.Categories = uniqueCategories.Select(c => new SelectListItem { Value = c, Text = c }).ToList();
                // Add an "All" option for categories
                srvm.Categories.Insert(0, new SelectListItem { Value = "", Text = "All Categories" });

                var searchResults = await _restaurantService.SearchRestaurantsAsViewModelsAsync(
                    query, selectedCategory, selectedDistanceCategory, selectedSortOption, 
                    page, pageSize, lat, lng, 
                    minRating, priceRange, isOpenNow);
                    
                srvm.Restaurants = searchResults;

                ViewData["MapboxToken"] = _config["Mapbox:AccessToken"];

                // Set current search parameters as ViewData for form persistence
                // ViewData for form persistence - adjust as needed for the new filter panel
                ViewData["CurrentQuery"] = query;
                ViewData["CurrentLocation"] = location;
                ViewData["SelectedCategory"] = selectedCategory;
                ViewData["SelectedDistanceCategory"] = selectedDistanceCategory;
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
            string selectedDistanceCategory = SearchConstants.Distance_BirdseyeView_Value,
            string selectedSortOption = SearchConstants.Sort_Relevance_Value,
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
                var searchResults = await _restaurantService.SearchRestaurantsAsViewModelsAsync(
                    query, selectedCategory, selectedDistanceCategory, selectedSortOption,
                    page, pageSize, lat, lng,
                    minRating, priceRange, isOpenNow);

                return PartialView("_RestaurantListPartial", searchResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading restaurants");
            }
        }
    }
}