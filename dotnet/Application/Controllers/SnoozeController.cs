using Application.GroheApiClasses;
using Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

// api controller that lets you get the current alarm state
[ApiController]
[Route("api/[controller]")]
public class SnoozeController(IApiClientLockQueue apiClientLockQueue) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SnoozeState>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSnoozeState(long? locationId = null, string applianceId = null)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<SenseGuardAppliance>(locationId, applianceId);
        if (appliances.Count == 0) return NotFound();

        return Json(appliances.Values.Select(GetApiModel));
    }
    
    [HttpPut]
    [ProducesResponseType(typeof(IEnumerable<SnoozeState>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<SnoozeState>), StatusCodes.Status207MultiStatus)] // some valves failed
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetSnoozeState(long minutes, long? locationId = null, string applianceId = null)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<SenseGuardAppliance>(locationId, applianceId);
        if (appliances.Count == 0) return NotFound("No valves found");

        var someSnoozeFailed = false;
        var results = new List<SnoozeState>();
        foreach (var valve in appliances.Values)
        {
            var state = GetApiModel(valve);
            results.Add(state);
            switch (minutes)
            {
                case >= 1 when await apiClient.SnoozeGuard(valve.Id, minutes):
                    state.Snoozing = true;
                    state.SnoozeSeconds = minutes * 60;
                    state.SnoozeUntil = DateTime.UtcNow.AddMinutes(minutes); // could be off by a few seconds, but that's fine
                    break;
                case 0 when await apiClient.WakeGuard(valve.Id):
                    state.Snoozing = false;
                    state.SnoozeSeconds = 0;
                    state.SnoozeUntil = null;
                    break;
                default:
                    someSnoozeFailed = true;
                    break;
            }
        }
        return someSnoozeFailed
            ? StatusCode(StatusCodes.Status207MultiStatus, results)
            : Json(results);
    }
    
    private static SnoozeState GetApiModel(SenseGuardAppliance valve)
    {
        var snoozedUntil = valve.SnoozedUntil?.ToUniversalTime();
        var snoozing = valve.SnoozedUntil > DateTime.UtcNow;
        return new SnoozeState
        {
            Id = valve.Id,
            Snoozing = snoozing,
            SnoozeSeconds = snoozing ? Math.Max((long)(snoozedUntil! - DateTime.UtcNow).Value.TotalSeconds, 0) : 0,
            SnoozeUntil = snoozing ? snoozedUntil : null
        };
    }
    
    [JsonTypeName("SnoozeState")]
    private class SnoozeState
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string Id { get; init; }
        public bool Snoozing { get; set; }
        public DateTime? SnoozeUntil { get; set; }
        public long SnoozeSeconds { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
    
    [HttpGet("single")]
    [ProducesResponseType(typeof(SnoozeState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSingleSnoozeState()
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<SenseGuardAppliance>();
        return appliances.Count switch
        {
            0 => NotFound("No valve found"),
            1 => Json(GetApiModel(appliances.Values.First())),
            _ => BadRequest("More than one valve found")
        };
    }
    
    [HttpPut("single")]
    [ProducesResponseType(typeof(SnoozeState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SetSingleSnoozeState(long minutes)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<SenseGuardAppliance>();
        switch (appliances.Count)
        {
            case 0:
                return NotFound("No valve found");
            case > 1:
                return BadRequest("More than one valve found");
        }

        var valve = appliances.Values.First();
        var state = GetApiModel(valve);
        switch (minutes)
        {
            case >= 1 when await apiClient.SnoozeGuard(valve.Id, minutes):
                state.Snoozing = true;
                state.SnoozeSeconds = minutes * 60;
                state.SnoozeUntil = DateTime.UtcNow.AddMinutes(minutes); // could be off by a few seconds, but that's fine
                return Json(state);
            case 0 when await apiClient.WakeGuard(valve.Id):
                state.Snoozing = false;
                state.SnoozeSeconds = 0;
                state.SnoozeUntil = null;
                return Json(state);
            default:
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed to set snooze state");
        }
    }
}