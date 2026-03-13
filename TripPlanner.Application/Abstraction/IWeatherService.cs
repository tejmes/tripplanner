using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.Dtos;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface IWeatherService
    {
        Task<Dictionary<DateTime, WeatherDto>> GetForecast(double latitude, double longitude, DateTime startDate, DateTime endDate);
        Task ApplyForecastToTripDays(List<TripDayViewModel> tripDays, LocationViewModel destinationLocation);
    }
}
