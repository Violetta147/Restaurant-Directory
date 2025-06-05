using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PBL3.ViewModels.Search
{
    // Distance Category Enum with Display attributes
    public enum DistanceCategory
    {
        [Display(Name = "Bird's-eye View")]
        BirdseyeView,
        
        [Display(Name = "Within 1 km")]
        OneKm,
        
        [Display(Name = "Within 3 km")]
        ThreeKm,
        
        [Display(Name = "Within 5 km")]
        FiveKm,
        
        [Display(Name = "Within 10 km")]
        TenKm
    }

    // Sort Option Enum with Display attributes
    public enum SortOption
    {
        [Display(Name = "Relevance")]
        Relevance,
        
        [Display(Name = "Distance: Nearest first")]
        Distance,
        
        [Display(Name = "Rating: Highest first")]
        Rating
    }

    public static class SearchConstants
    {
        // Backward compatibility constants (for existing code)
        public const string Distance_BirdseyeView_Value = "BirdseyeView";
        public const string Distance_1km_Value = "1km";
        public const string Distance_3km_Value = "3km";
        public const string Distance_5km_Value = "5km";
        public const string Distance_10km_Value = "10km";
        
        public const string Sort_Relevance_Value = "Relevance";
        public const string Sort_Distance_Value = "Distance";
        public const string Sort_Rating_Value = "Rating";

        // Extension methods for enum to string conversion
        public static string ToValue(this DistanceCategory distance)
        {
            return distance switch
            {
                DistanceCategory.BirdseyeView => "BirdseyeView",
                DistanceCategory.OneKm => "1km",
                DistanceCategory.ThreeKm => "3km",
                DistanceCategory.FiveKm => "5km",
                DistanceCategory.TenKm => "10km",
                _ => "BirdseyeView"
            };
        }

        public static string ToValue(this SortOption sort)
        {
            return sort switch
            {
                SortOption.Relevance => "Relevance",
                SortOption.Distance => "Distance",
                SortOption.Rating => "Rating",
                _ => "Relevance"
            };
        }

        // Parse string back to enum
        public static DistanceCategory ParseDistanceCategory(string value)
        {
            return value switch
            {
                "BirdseyeView" => DistanceCategory.BirdseyeView,
                "1km" => DistanceCategory.OneKm,
                "3km" => DistanceCategory.ThreeKm,
                "5km" => DistanceCategory.FiveKm,
                "10km" => DistanceCategory.TenKm,
                _ => DistanceCategory.BirdseyeView
            };
        }

        public static SortOption ParseSortOption(string value)
        {
            return value switch
            {
                "Relevance" => SortOption.Relevance,
                "Distance" => SortOption.Distance,
                "Rating" => SortOption.Rating,
                _ => SortOption.Relevance
            };
        }

        // Generate SelectListItems from enums
        public static List<SelectListItem> DistanceOptions => 
            System.Enum.GetValues<DistanceCategory>()
                .Select(d => new SelectListItem 
                { 
                    Value = d.ToValue(), 
                    Text = d.GetDisplayName() 
                })
                .ToList();

        public static List<SelectListItem> SortOptions => 
            System.Enum.GetValues<SortOption>()
                .Select(s => new SelectListItem 
                { 
                    Value = s.ToValue(), 
                    Text = s.GetDisplayName() 
                })
                .ToList();
    }

    // Extension method to get Display attribute value
    public static class EnumExtensions
    {
        public static string GetDisplayName(this System.Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetField(enumValue.ToString())
                ?.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;
            
            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}