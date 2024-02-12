using System.Globalization;
using Application.GroheApiClasses;
using Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController(IApiClientLockQueue apiClientLockQueue) : Controller
{
    [HttpGet("aggregated/{applianceId}/{aggregation}")]
    [ProducesResponseType(typeof(AggregatedData), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSenseDetails(string applianceId, ApiAggregation aggregation, DateTime from, DateTime to)
    {
        if (from > to) return BadRequest("Time machine not implemented yet");
        if (from == DateTime.MinValue) return BadRequest("From is not set");
        if (to == DateTime.MinValue) return BadRequest("To is not set");
        
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;
        
        if (!(await apiClient.GetAppliances()).TryGetValue(applianceId, out var appliance)) return NotFound("Appliance not found");

        var groheAggregation = aggregation switch
        {
            ApiAggregation.Hour => Aggregation.Hour,
            ApiAggregation.Day => Aggregation.Day,
            ApiAggregation.Week => Aggregation.Week,
            ApiAggregation.Month => Aggregation.Month,
            ApiAggregation.Year => Aggregation.Year,
            _ => throw new ArgumentOutOfRangeException(nameof(aggregation), aggregation, null)
        };
        
        var payload = (await apiClient.GetAggregatedData(applianceId, groheAggregation, from, to)).Data;
        
        var result = new AggregatedData
        {
            ApplianceId = appliance.Id,
            ApplianceName = appliance.Name,
            Type = appliance.Type,
            Aggregation = aggregation
        };
        
        foreach (var measurement in payload.Measurements ?? [])
        {
            DataPoint dataPoint = appliance.Type switch
            {
                ApplianceType.SenseGuard => new SenseGuardMeasurement
                {
                    FlowRate = measurement.FlowRate,
                    Pressure = measurement.Pressure,
                    TemperatureGuard = measurement.TemperatureGuard
                },
                ApplianceType.Sense => new SenseMeasurement
                {
                    Temperature = measurement.Temperature,
                    Humidity = measurement.Humidity
                },
                ApplianceType.Unknown => throw new Exception("Appliance type unknown"),
                _ => throw new Exception($"Appliance type out of range: {(int)appliance.Type}")
            };
            SetMeasurementDate(dataPoint, aggregation, measurement.When);
            result.Measurements.Add(dataPoint);
        }
        
        foreach (var withdrawal in payload.Withdrawals ?? [])
        {
            var dataPoint = new SenseGuardWithdrawal
            {
                WaterConsumption = withdrawal.WaterConsumption,
                HotWaterShare = withdrawal.HotWaterShare,
                WaterCost = withdrawal.WaterCost,
                EnergyCost = withdrawal.EnergyCost
            };
            SetMeasurementDate(dataPoint, aggregation, withdrawal.When);
            result.Withdrawals.Add(dataPoint);
        }
        
        return Json(result);
    }

    public class AggregatedData
    {
        public string ApplianceId { get; init; }
        public string ApplianceName { get; init; }
        public ApplianceType Type { get; init; }
        public ApiAggregation Aggregation { get; init; }
        // ReSharper disable CollectionNeverQueried.Global
        public List<object> Measurements { get; init; } = []; // collection of <object> is a dirty hack to make System.Text.Json serialize the derived types
        public List<object> Withdrawals { get; init; } = []; // collection of <object> is a dirty hack to make System.Text.Json serialize the derived types
        // ReSharper restore CollectionNeverQueried.Global
    }

    public abstract class DataPoint
    {
        public int Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public int? Hour { get; set; }
        public int? WeekNumber { get; set; }
    }
    
    public class SenseGuardMeasurement : DataPoint
    {
        public decimal? FlowRate { get; init; }
        public decimal? Pressure { get; init; }
        public decimal? TemperatureGuard { get; init; }
    }
    
    public class SenseMeasurement : DataPoint
    {
        public decimal? Temperature { get; init; }
        public long? Humidity { get; init; }
    }
    
    public class SenseGuardWithdrawal : DataPoint
    {
        public decimal? WaterConsumption { get; init; }
        public decimal? HotWaterShare { get; init; }
        public decimal? WaterCost { get; init; }
        public decimal? EnergyCost { get; init; }
    }

    [JsonTypeName("Aggregation")]
    public enum ApiAggregation
    {
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    private static void SetMeasurementDate(DataPoint dataPoint, ApiAggregation aggregation, string when)
    {
        switch (aggregation)
        {
            case ApiAggregation.Hour:
            {
                var date = DateTime.ParseExact(when, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                dataPoint.Year = date.Year;
                dataPoint.Month = date.Month;
                dataPoint.Day = date.Day;
                dataPoint.Hour = date.Hour;
                break;
            }
            case ApiAggregation.Day:
            {
                var split = SplitWhen(when);
                dataPoint.Year = split[0];
                dataPoint.Month = split[1];
                dataPoint.Day = split[2];
                break;
            }
            case ApiAggregation.Week:
            {
                var split = SplitWhen(when);
                dataPoint.Year = split[0];
                dataPoint.WeekNumber = split[1];
                break;
            }
            case ApiAggregation.Month:
            {
                var split = SplitWhen(when);
                dataPoint.Year = split[0];
                dataPoint.Month = split[1];
                break;
            }
            case ApiAggregation.Year:
            {
                var split = SplitWhen(when);
                dataPoint.Year = split[0];
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(aggregation), aggregation, null);
        }
        return;
        
        List<int> SplitWhen(string s) => s.Split('-').Select(int.Parse).ToList();
    }
}