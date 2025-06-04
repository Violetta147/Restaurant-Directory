using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace PBL3.ViewModels.Search
{
    public static class SearchConstants
    {
        // Distance
        public const string Distance_BirdseyeView_Value = "BirdseyeView";
        public const string Distance_BirdseyeView_Text = "Bird's-eye View";
        public const string Distance_1km_Value = "1km";
        public const string Distance_1km_Text = "Within 1 km";
        public const string Distance_3km_Value = "3km";
        public const string Distance_3km_Text = "Within 3 km";  
        public const string Distance_5km_Value = "5km";
        public const string Distance_5km_Text = "Within 5 km";
        public const string Distance_10km_Value = "10km";
        public const string Distance_10km_Text = "Within 10 km";

        public static readonly List<SelectListItem> DistanceOptions = new()
        {
            new SelectListItem { Value = Distance_BirdseyeView_Value, Text = Distance_BirdseyeView_Text },
            new SelectListItem { Value = Distance_1km_Value, Text = Distance_1km_Text },
            new SelectListItem { Value = Distance_3km_Value, Text = Distance_3km_Text },
            new SelectListItem { Value = Distance_5km_Value, Text = Distance_5km_Text },
            new SelectListItem { Value = Distance_10km_Value, Text = Distance_10km_Text }
        };

        // Sort
        public const string Sort_Relevance_Value = "Relevance";
        public const string Sort_Relevance_Text = "Relevance";
        public const string Sort_Distance_Value = "Distance";
        public const string Sort_Distance_Text = "Distance: Nearest first";
        public const string Sort_Rating_Value = "Rating";
        public const string Sort_Rating_Text = "Rating: Highest first";

        public static readonly List<SelectListItem> SortOptions = new()
        {
            new SelectListItem { Value = Sort_Relevance_Value, Text = Sort_Relevance_Text },
            new SelectListItem { Value = Sort_Distance_Value, Text = Sort_Distance_Text },
            new SelectListItem { Value = Sort_Rating_Value, Text = Sort_Rating_Text },
        };
    }
}