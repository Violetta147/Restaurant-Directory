using PBL3.Models;
using PBL3.ViewModels;
using PBL3.Repositories;
using X.PagedList;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using PBL3.Services.Interfaces;
using PBL3.Repositories.Interfaces;

namespace PBL3.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _repository;

        public RestaurantService(IRestaurantRepository repository)
        {
            _repository = repository;
        }

        public async Task<IPagedList<Restaurant>> GetPagedRestaurantsAsync(
            string searchTerm = "",
            string category = "",
            int page = 1,
            int pageSize = 10,
            double? lat = null,
            double? lng = null,
            double radius = 3.0)
        {
            return await _repository.GetPagedRestaurantsAsync(
                searchTerm, category, page, pageSize, lat, lng, radius);
        }

        public async Task<List<RestaurantViewModel>> GetRestaurantsForMapAsync(
            double lat,
            double lng,
            double radius = 3.0,
            string searchTerm = "",
            string category = "")
        {
            var restaurants = await _repository.GetRestaurantsInAreaAsync(
                lat, lng, radius, searchTerm, category);

            return restaurants.Select(r => new RestaurantViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Address = r.Address,
                Category = r.Category,
                Rating = r.Rating
            }).ToList();
        }

        public async Task<RestaurantViewModel> GetRestaurantByIdAsync(int id)
        {
            var restaurant = await _repository.GetRestaurantByIdAsync(id);
            if (restaurant == null) return null;

            return new RestaurantViewModel
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Latitude = restaurant.Latitude,
                Longitude = restaurant.Longitude,
                Address = restaurant.Address,
                Category = restaurant.Category,
                Rating = restaurant.Rating
            };
        }
    }
}