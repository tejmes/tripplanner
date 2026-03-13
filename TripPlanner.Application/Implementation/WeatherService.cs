using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.Abstraction;
using static System.Net.WebRequestMethods;
using TripPlanner.Application.Dtos;
using System.Net.Http.Json;
using System.Net.Http;
using System.Globalization;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Implementation
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient httpClient;
        private readonly string timeZone;
        private readonly string baseUrl;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            timeZone = configuration["OpenMeteo:TimeZone"] ?? "auto";
            baseUrl = configuration["OpenMeteo:BaseUrl"] ?? throw new ArgumentNullException("OpenMeteo:BaseUrl");
        }

        public async Task<Dictionary<DateTime, WeatherDto>> GetForecast(double latitude, double longitude, DateTime startDate, DateTime endDate)
        {
            var url = $"{baseUrl}"
                + $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}"
                + $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}"
                + "&daily=temperature_2m_max,temperature_2m_min,weathercode"
                + $"&start_date={startDate:yyyy-MM-dd}"
                + $"&end_date={endDate:yyyy-MM-dd}"
                + $"&timezone={Uri.EscapeDataString(timeZone)}";

            try
            {
                var resp = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
                var result = new Dictionary<DateTime, WeatherDto>();
                if (resp?.daily?.time != null)
                {
                    for (int i = 0; i < resp.daily.time.Length; i++)
                    {
                        var date = DateTime.Parse(resp.daily.time[i]);
                        result[date] = new WeatherDto
                        {
                            Date = date,
                            TemperatureMax = resp.daily.temperature_2m_max[i],
                            TemperatureMin = resp.daily.temperature_2m_min[i],
                            WeatherCode = resp.daily.weathercode[i]
                        };
                    }
                }
                return result;
            }
            catch
            {
                return new Dictionary<DateTime, WeatherDto>();
            }
        }

        public async Task ApplyForecastToTripDays(List<TripDayViewModel> tripDays, LocationViewModel destinationLocation)
        {
            if (tripDays == null || tripDays.Count == 0) return;

            var start = tripDays.Min(d => d.Date.Value.Date);
            var end = tripDays.Max(d => d.Date.Value.Date);

            var forecast = await GetForecast(
                destinationLocation.Latitude,
                destinationLocation.Longitude,
                start,
                end);

            foreach (var day in tripDays)
            {
                if (day.Date.HasValue
                    && forecast.TryGetValue(day.Date.Value.Date, out var w))
                {
                    day.TempMax = w.TemperatureMax;
                    day.TempMin = w.TemperatureMin;
                    day.WeatherCode = w.WeatherCode;
                }
            }
        }

        private class OpenMeteoResponse
        {
            public Daily daily { get; set; }
            public class Daily
            {
                public string[] time { get; set; } = Array.Empty<string>();
                public double[] temperature_2m_max { get; set; } = Array.Empty<double>();
                public double[] temperature_2m_min { get; set; } = Array.Empty<double>();
                public int[] weathercode { get; set; } = Array.Empty<int>();
            }
        }
    }
}
