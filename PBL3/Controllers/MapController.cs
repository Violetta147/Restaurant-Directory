using Microsoft.AspNetCore.Mvc;
using PBL3.Services;
using System.Threading.Tasks;
using System.Linq;
using PBL3.Services.Interfaces;

namespace PBL3.Controllers
{
    public class MapController : Controller
    {
        private readonly IRestaurantService _restaurantService;

        public MapController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRestaurantsJson(double lat, double lng, double radius = 3.0, string q = "", string category = "")
        {
            // Get restaurants within radius for map display
            var restaurants = await _restaurantService.GetRestaurantsForMapAsync(
                lat, lng, radius, q, category);

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
                        rating = r.Rating
                    }
                })
            };

            return Json(geoJson);
        }
    }
}