using Microsoft.AspNetCore.Mvc;
using PBL3.Services;
using PBL3.Services.Interfaces;
using PBL3.ViewModels;
using System.Threading.Tasks;
using X.PagedList;

namespace PBL3.Controllers
{
    public class SearchController : Controller
    {
        private readonly IRestaurantService _restaurantService;

        public SearchController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        public async Task<IActionResult> Index(string q = "", string location = "", string category = "", int page = 1)
        {
            // Default page size
            const int pageSize = 10;

            // Get restaurants matching search criteria
            var restaurants = await _restaurantService.GetPagedRestaurantsAsync(
                q, category, page, pageSize);

            // Build view model
            var viewModel = new SearchResultsViewModel
            {
                Query = q,
                LocationQuery = location,
                Category = category,
                Restaurants = restaurants,
                Center = new LocationViewModel
                {
                    // Default center coordinates for Da Nang
                    Latitude = 16.0471,
                    Longitude = 108.2062
                }
            };

            return View(viewModel);
        }
    }
}