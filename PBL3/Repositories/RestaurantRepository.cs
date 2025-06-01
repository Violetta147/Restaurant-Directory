using PBL3.Models;
using PBL3.Data;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;
using PBL3.Repositories.Interfaces;

namespace PBL3.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly ApplicationDbContext _context;

        public RestaurantRepository(ApplicationDbContext context)
        {
            _context = context;
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
            var query = _context.Restaurants.AsQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    r.Category.ToLower().Contains(searchTerm) ||
                    r.Address.ToLower().Contains(searchTerm));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.Category.Contains(category));
            }

            // Get page of results
            return await query.OrderByDescending(r => r.Rating)
                .ToPagedListAsync(page, pageSize);
        }

        public async Task<List<Restaurant>> GetRestaurantsInAreaAsync(
            double lat,
            double lng,
            double radius = 3.0,
            string searchTerm = "",
            string category = "")
        {
            // Simple query to get restaurants - in a real app, you might use
            // spatial queries or PostGIS for more accurate filtering
            var query = _context.Restaurants.AsQueryable();

            // Apply text search if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    r.Category.ToLower().Contains(searchTerm) ||
                    r.Address.ToLower().Contains(searchTerm));
            }

            // Apply category filter if provided
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.Category.Contains(category));
            }

            // Get results
            return await query.ToListAsync();
        }

        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            return await _context.Restaurants.FindAsync(id);
        }
    }
}