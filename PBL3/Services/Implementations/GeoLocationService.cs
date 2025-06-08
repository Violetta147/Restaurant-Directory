using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PBL3.Services.Interfaces;

namespace PBL3.Services.Implementations
{
    public class GeoLocationService : IGeoLocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _mapboxAccessToken;
        private readonly ILogger<GeoLocationService> _logger;
        private readonly string _sessionToken; // Session token for Mapbox SearchBox API

        public GeoLocationService(HttpClient httpClient, IConfiguration configuration, ILogger<GeoLocationService> logger)
        {
            _httpClient = httpClient;
            _mapboxAccessToken = configuration["Mapbox:AccessToken"] ?? 
                throw new InvalidOperationException("Mapbox:AccessToken is not configured in app settings");
            _logger = logger;
            
            // Generate a random session token that will persist for the lifetime of this service instance
            _sessionToken = Guid.NewGuid().ToString();
            _logger.LogInformation("Created Mapbox session token: {SessionToken}", _sessionToken);
        }

        public async Task<(double latitude, double longitude)> GetCoordinatesFromAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                _logger.LogWarning("Empty address provided to GetCoordinatesFromAddressAsync");
                // Default coordinates for Da Nang
                return (16.047079, 108.206230);
            }

            try
            {                // Step 1: Call the Mapbox SearchBox API suggest endpoint
                var suggestUrl = $"https://api.mapbox.com/search/searchbox/v1/suggest?q={Uri.EscapeDataString(address)}&session_token={_sessionToken}&access_token={_mapboxAccessToken}";
                _logger.LogInformation("Using suggest URL with session token: {Url}", suggestUrl);                var suggestResponse = await _httpClient.GetAsync(suggestUrl);
                
                // Check for failure and add debug breakpoint
                if (!suggestResponse.IsSuccessStatusCode)
                {
                    var errorContent = await suggestResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Mapbox API call failed with status: {Status}, Response: {Response}", 
                        suggestResponse.StatusCode, errorContent);
                    
                    // When debugging is enabled, stop execution here
                    System.Diagnostics.Debugger.Break();
                }
                
                suggestResponse.EnsureSuccessStatusCode();

                var suggestContent = await suggestResponse.Content.ReadAsStringAsync();
                var suggestData = JsonDocument.Parse(suggestContent);

                // Check if we have suggestions
                var suggestions = suggestData.RootElement.GetProperty("suggestions").EnumerateArray();
                
                if (!suggestions.MoveNext())
                {
                    _logger.LogWarning("No suggestions found for address: {Address}", address);
                    // Default coordinates for Da Nang
                    return (16.047079, 108.206230);
                }

                // Get the first suggestion's ID
                string id = suggestions.Current.GetProperty("mapbox_id").GetString() ?? 
                    throw new InvalidOperationException("Could not get mapbox_id from suggestion");                // Step 2: Call the retrieve endpoint to get details
                var retrieveUrl = $"https://api.mapbox.com/search/searchbox/v1/retrieve/{id}?session_token={_sessionToken}&access_token={_mapboxAccessToken}";
                _logger.LogInformation("Retrieve request URL: {Url}", retrieveUrl);                var retrieveResponse = await _httpClient.GetAsync(retrieveUrl);
                
                // Check for failure and add debug breakpoint
                if (!retrieveResponse.IsSuccessStatusCode)
                {
                    var errorContent = await retrieveResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Mapbox retrieve API call failed with status: {Status}, Response: {Response}", 
                        retrieveResponse.StatusCode, errorContent);
                    
                    // When debugging is enabled, stop execution here
                    System.Diagnostics.Debugger.Break();
                }
                
                retrieveResponse.EnsureSuccessStatusCode();

                var retrieveContent = await retrieveResponse.Content.ReadAsStringAsync();
                var retrieveData = JsonDocument.Parse(retrieveContent);

                // Extract coordinates from the first feature
                var features = retrieveData.RootElement.GetProperty("features").EnumerateArray();
                
                if (!features.MoveNext())
                {
                    _logger.LogWarning("No features found for suggestion ID: {Id}", id);
                    // Default coordinates for Da Nang
                    return (16.047079, 108.206230);
                }

                var coordinates = features.Current.GetProperty("geometry").GetProperty("coordinates").EnumerateArray();
                
                // Mapbox returns coordinates as [longitude, latitude]
                var longitude = coordinates.MoveNext() ? coordinates.Current.GetDouble() : 108.206230;
                var latitude = coordinates.MoveNext() ? coordinates.Current.GetDouble() : 16.047079;

                _logger.LogInformation("Successfully geocoded address: {Address} to coordinates: ({Lat}, {Lng})", 
                    address, latitude, longitude);
                
                return (latitude, longitude);
            }            catch (Exception ex)
            {
                _logger.LogError(ex, "Error geocoding address: {Address}", address);
                
                // When debugging is enabled, stop execution here
                System.Diagnostics.Debugger.Break();
                
                // Default coordinates for Da Nang
                return (16.047079, 108.206230);
            }
        }
    }
}