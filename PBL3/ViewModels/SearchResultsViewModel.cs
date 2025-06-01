using PBL3.Models;
using X.PagedList;

namespace PBL3.ViewModels
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;
        public string LocationQuery { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public LocationViewModel Center { get; set; } = new LocationViewModel();
        public IPagedList<Restaurant>? Restaurants { get; set; }
    }
}