using System;
using System.Threading.Tasks;

namespace PBL3.Services.Interfaces
{
    public interface IGeoLocationService
    {
        /// <summary>
        /// Gets the geographic coordinates (latitude and longitude) for a given address using Mapbox API
        /// </summary>
        /// <param name="address">The address to geocode</param>
        /// <returns>A tuple containing the latitude and longitude coordinates</returns>
        Task<(double latitude, double longitude)> GetCoordinatesFromAddressAsync(string address);
    }
}