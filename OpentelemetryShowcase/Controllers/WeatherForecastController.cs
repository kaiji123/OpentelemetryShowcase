using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace OpentelemetryShowcase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            using var activity = new Activity("GetWeatherForecast").Start();

            activity?.SetTag("WeatherForecast.Count", 5);

            var forecast = Enumerable.Range(1, 5).Select(index =>
            {
                var weatherForecast = new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };

                activity?.AddEvent(new ActivityEvent("GeneratedForecast", tags: new ActivityTagsCollection {
                    { "Date", weatherForecast.Date.ToString() },
                    { "Temperature", weatherForecast.TemperatureC },
                    { "Summary", weatherForecast.Summary }
                }));

                return weatherForecast;
            })
            .ToArray();

            _logger.LogInformation("Generated {Count} weather forecasts", forecast.Length);

            return forecast;
        }
    }
}