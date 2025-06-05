using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PBL3.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace PBL3.Controllers
{
    [Route("restaurant")]
    public class RestaurantController : Controller
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IConfiguration _config;

        public RestaurantController(
            IRestaurantService restaurantService,
            IConfiguration config)
        {
            _restaurantService = restaurantService ?? throw new ArgumentNullException(nameof(restaurantService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }        /// <summary>
        /// Restaurant detail page accessed via SEO-friendly slug
        /// Route: /restaurant/{slug}
        /// Slug format: "{name-with-dashes}-{id}"
        /// </summary>
        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            try
            {
                // Extract ID from slug (format: "restaurant-name-123")
                var restaurantId = ExtractIdFromSlug(slug);
                if (restaurantId == null)
                {
                    return NotFound();
                }

                // Get restaurant details
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(restaurantId.Value);
                if (restaurant == null)
                {
                    return NotFound();
                }

                // Pass Mapbox token to view for map display
                ViewData["MapboxToken"] = _config["Mapbox:AccessToken"];

                return View(restaurant);
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, "Error loading restaurant details");
            }
        }        /// <summary>
        /// Legacy route handler for old URL pattern: /Restaurant/Details/{id}
        /// Redirects to the new SEO-friendly slug-based URL
        /// </summary>
        [HttpGet("/Restaurant/Details/{id:int}")]
        public async Task<IActionResult> DetailsById(int id)
        {
            try
            {
                // Get restaurant to build the slug
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
                if (restaurant == null)
                {
                    return NotFound();
                }

                // URL encode the slug to handle Vietnamese characters properly
                var encodedSlug = Uri.EscapeDataString(restaurant.Slug);
                
                // Redirect to the new slug-based URL
                return RedirectPermanent($"/restaurant/{encodedSlug}");
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, "Error loading restaurant details");
            }
        }

        /// <summary>
        /// Extract restaurant ID from SEO-friendly slug
        /// Expected format: "restaurant-name-with-dashes-{id}"
        /// </summary>
        private int? ExtractIdFromSlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return null;

            // Split by dash and get the last part (should be the ID)
            var parts = slug.Split('-');
            if (parts.Length < 2)
                return null;

            var lastPart = parts[^1]; // C# 8.0 syntax for last element
            
            if (int.TryParse(lastPart, out int id))
            {
                return id;
            }

            return null;
        }
    }
}
