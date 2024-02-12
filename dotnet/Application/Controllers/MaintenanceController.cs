using Application.GroheApiClasses;
using Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

// api controller that lets you get the current alarm state
[ApiController]
[Route("api/[controller]")]
public class MaintenanceController(IApiClientLockQueue apiClientLockQueue) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeviceState>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaintenanceState(long? locationId = null, string applianceId = null, long? batteryWarningLevel = null, long? seenOffsetWarningHours = null)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<BaseAppliance>(locationId, applianceId);
        if (appliances.Count == 0) return NotFound();

        var results = appliances.Values.Select(sense => new DeviceState
        {
            Id = sense.Id,
            Name = sense.Name,
            Type = sense.Type,
            Location = sense.Location.Name,
            BatteryLevel = sense.Status.TryGetValue("battery", out var value) ? value : null,
            BatteryWarning = batteryWarningLevel != null ? sense.Status.GetValueOrDefault("battery") < batteryWarningLevel : null,
            LastSeen = sense.LastSeen?.ToUniversalTime(),
            LastSeenWarning = seenOffsetWarningHours != null ? sense.LastSeen?.ToUniversalTime() < DateTime.UtcNow.AddHours(-seenOffsetWarningHours.Value) : null
        });

        return Json(results);
    }
    
    [HttpGet("combined")]
    [ProducesResponseType(typeof(DeviceStateCombined), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCombinedMaintenanceState(long? locationId = null, string applianceId = null, long? batteryWarningLevel = null, long? seenOffsetWarningHours = null)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<BaseAppliance>(locationId, applianceId);
        if (appliances.Count == 0) return NotFound();

        var batteryLevels = appliances.Values.Where(sense => sense.Status.ContainsKey("battery")).Select(sense => sense.Status["battery"]).ToList();
        var timestamps = appliances.Values.Where(sense => sense.LastSeen != null).Select(sense => sense.LastSeen.Value).ToList();
        
        var result = new DeviceStateCombined
        {
            LowestBatteryLevel = batteryLevels.Count != 0 ? batteryLevels.Min() : null,
            BatteryWarning = batteryWarningLevel != null ? batteryLevels.Count != 0 && batteryLevels.Min() < batteryWarningLevel : null,
            LowestLastSeen = timestamps.Count != 0 ? timestamps.Min().ToUniversalTime() : null,
            LastSeenWarning = seenOffsetWarningHours != null ? timestamps.Count != 0 && timestamps.Min().ToUniversalTime() < DateTime.UtcNow.AddHours(-seenOffsetWarningHours.Value) : null
        };

        return Json(result);
    }
    
    [JsonTypeName("DeviceStateCombined")]
    private class DeviceStateCombined
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public long? LowestBatteryLevel { get; init; }
        public DateTime? LowestLastSeen { get; init; }
        public bool? BatteryWarning { get; init; }
        public bool? LastSeenWarning { get; init; }
        
        // ReSharper disable once UnusedMember.Local - it's used in serialization
        public bool? Warning
        {
            get
            {
                if (BatteryWarning == null && LastSeenWarning == null) return null;
                return BatteryWarning == true || LastSeenWarning == true;
            }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
    
    [JsonTypeName("DeviceState")]
    private class DeviceState
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string Id { get; init; }
        public long? BatteryLevel { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool? BatteryWarning { get; init; }
        public bool? LastSeenWarning { get; init; }
        public string Name { get; init; }
        public string Location { get; init; }
        public ApplianceType Type { get; init; }
        
        // ReSharper disable once UnusedMember.Local - it's used in serialization
        public bool? Warning
        {
            get
            {
                if (BatteryWarning == null && LastSeenWarning == null) return null;
                return BatteryWarning == true || LastSeenWarning == true;
            }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}