using PBL3.Models;
using PBL3.ViewModels;
using X.PagedList;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBL3.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<IPagedList<Restaurant>> GetPagedRestaurantsAsync(
            string searchTerm = "",
            string category = "",
            int page = 1,
            int pageSize = 10,
            double? lat = null,
            double? lng = null,
            double radius = 3.0);

        Task<List<RestaurantViewModel>> GetRestaurantsForMapAsync(
            double lat,
            double lng,
            double radius = 3.0,
            string searchTerm = "",
            string category = "");

        Task<RestaurantViewModel> GetRestaurantByIdAsync(int id);
    }
}