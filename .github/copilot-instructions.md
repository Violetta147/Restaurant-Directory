# MAPBOX-ONLY RESTAURANT SEARCH SYSTEM

## Core Rules:
- Use MAPBOX ONLY for map rendering, geocoding, and directions
- Always ask and provide a plan before making changes
- Separate UI components into as many div frames as possible
- Use CDN for Mapbox GL JS v3.12.0 (no SDK/npm)
- Always prioritize GPS location over IP location

## Reference Documentation:
- https://docs.mapbox.com/help/tutorials/poi-search-react/
- https://docs.mapbox.com/help/tutorials/optimization-api/
- https://docs.mapbox.com/help/tutorials/local-search-geocoding-api/
- https://docs.mapbox.com/help/tutorials/getting-started-directions-api/
- https://docs.mapbox.com/help/tutorials/geocode-and-sort-stores
- https://docs.mapbox.com/help/tutorials/building-a-store-locator/

## Search System Requirements:

### 1. Search Query Handling (Like Yelp):
- URL Pattern: `/Map/Search?q={food_type_or_restaurant_name}&loc={location}`
- Support exact restaurant name search: "Pizza Hut Landmark 81"
- Support food category search: "pizza", "burger", "sushi"
- Search in Restaurant.Name and Restaurant.Category fields
- Filter by 3km radius from user GPS location

### 2. UI Component Structure (Yelp-style 3-Column Layout):
```html
<!-- Use existing search navbar from _Layout.cshtml -->
<!-- /Map/Index - Search Results Page -->
<div id="yelp-style-results-layout">
    <!-- LEFT PANEL: Categories & Filters -->
    <div id="filters-sidebar-frame">
        <div id="categories-filter-container"><!-- Food categories --></div>
        <div id="price-filter-container"><!-- Price range filters --></div>
        <div id="rating-filter-container"><!-- Rating filters --></div>
        <div id="distance-filter-container"><!-- Distance filters --></div>
    </div>
    
    <!-- CENTER: Restaurant Results List -->
    <div id="restaurant-results-frame">
        <div id="search-results-header"><!-- "Best Pizza near HCMC" --></div>
        <div id="restaurant-list-container"><!-- Restaurant items (NO Get Directions) --></div>
        <div id="pagination-container"><!-- Page navigation --></div>
    </div>
    
    <!-- RIGHT: Map Display -->
    <div id="map-sidebar-frame">
        <div id="mapbox-container"><!-- Main map with restaurant pins --></div>
        <div id="map-controls-overlay"><!-- Map controls --></div>
    </div>
</div>
```

### 3. GPS-First Location Strategy:
- Always request GPS permission on page load
- Use navigator.geolocation.getCurrentPosition()
- Fallback to location input only if GPS denied
- Use GPS coordinates for radius-based filtering

### 4. Mapbox Integration Features:
- **Geocoding**: Convert location input to lat/lng using Mapbox Geocoding API
- **Reverse Geocoding**: Convert lat/lng to address for display
- **Directions**: Use Mapbox Directions API for routing from GPS to restaurant
- **Autocomplete**: Use Mapbox Search API for location suggestions
- **Map Display**: Show restaurants as pins with popup info

### 5. Restaurant Display System:
- Show all restaurants within 3km radius on map as GeoJSON markers
- Display restaurant list in separate results div (NO Get Directions button in listing)
- Each restaurant item shows: Name, Address, Rating only
- Get Directions appears only in map popup or restaurant details view
- NO minimap in search results page (only main map)

### 6. Directions Flow:
1. User clicks on restaurant pin on map → Show popup with restaurant details
2. User clicks "Get Directions" in popup or details view
3. Check GPS permission (prompt if not granted)
4. Get current GPS coordinates
5. Call Mapbox Directions API with GPS → Restaurant coordinates
6. Display route on main map
7. Show turn-by-turn directions if needed

### 7. Business Owner Features:
- Use GPS location for adding new restaurants (when owner claims business)
- Geocoding for address verification during restaurant registration
- Reverse geocoding to display readable addresses

## Controller Structure:
```csharp
// MapController actions:
[HttpGet] Index(string q, string loc) // Yelp-style results page (Returns View)
[HttpGet] GetRestaurants(string q, string loc, double? userLat, double? userLng) // AJAX API (Returns JSON)
[HttpGet] GetDirections(int restaurantId, double userLat, double userLng) // Routing data API (Returns JSON)
[HttpPost] ClaimBusiness() // Owner registration with GPS

// Flow:
// 1. User searches from navbar → Submit to /Map/Index?q=pizza&loc=HCMC
// 2. Index action returns Yelp-style 3-column results page
// 3. JavaScript calls /Map/GetRestaurants AJAX → Returns JSON restaurant data
// 4. Update center results list + right map with pins
// 5. User clicks restaurant pin → Show popup with "Get Directions"
// 6. Click "Get Directions" → AJAX call to /Map/GetDirections → Display route
```

## Updated Plan:
**Phase 1**: Create ViewModels (RestaurantSearchRequest, RestaurantSearchResult)
**Phase 2**: Implement MapController (Index returns Yelp-style results page, GetRestaurants returns JSON)
**Phase 3**: Create Map/Index.cshtml with 3-column Yelp-style layout
**Phase 4**: Add Mapbox GL JS integration with GPS-first location  
**Phase 5**: Implement AJAX GetRestaurants functionality
**Phase 6**: Add restaurant pins with popups containing "Get Directions"
**Phase 7**: Implement filters (categories, price, rating, distance)
**Phase 8**: Implement Mapbox Directions API integration
ngoài ra chỉ được quyền chạy code khi tôi bảo chạy code
hiện tại chỉ được write vào Models những là những file mới không được sửa đổi file cũ ngoại trừ Restaurant.cs

thôi việc làm này quá nguy hiểm trước tiên hãy tạo các model cho dự án đã trước tiên tôi mún thực hiện chức năng loot your business 