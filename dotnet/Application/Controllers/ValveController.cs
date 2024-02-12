using Application.GroheApiClasses;
using Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable InvertIf

namespace Application.Controllers;

// api controller that lets you get the current alarm state
[ApiController]
[Route("api/[controller]")]
public class ValveController(IApiClientLockQueue apiClientLockQueue) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ValveState>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetValveState(long? locationId = null, string applianceId = null)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<SenseGuardAppliance>(locationId, applianceId);
        if (appliances.Count == 0) return NotFound();

        var results = new List<ValveState>();
        foreach (var valve in appliances.Values)
        {
            var state = new ValveState
            {
                Id = valve.Id,
                Open = await apiClient.GetValveOpen(valve.Id)
            };
            results.Add(state);
        }
        return Json(results);
    }
    
    [HttpPut]
    [ProducesResponseType(typeof(IEnumerable<ValveState>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValveState>), StatusCodes.Status207MultiStatus)] // some valves failed
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetValveState(bool open, long? locationId = null, string applianceId = null)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<SenseGuardAppliance>(locationId, applianceId);
        if (appliances.Count == 0) return NotFound("No valves found");

        var someValvesFailed = false;
        var results = new List<ValveState>();
        foreach (var valve in appliances.Values)
        {
            var state = new ValveState
            {
                Id = valve.Id,
                Open = await apiClient.GetValveOpen(valve.Id)
            };
            results.Add(state);
            if (state.Open == open) continue;
            if (await apiClient.SetValveOpen(valve.Id, open))
            {
                state.Open = open;
            }
            else
            {
                someValvesFailed = true;
            }
        }
        return someValvesFailed
            ? StatusCode(StatusCodes.Status207MultiStatus, results)
            : Json(results);
    }
    
    [JsonTypeName("ValveState")]
    private class ValveState
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string Id { get; init; }
        public bool Open { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
    
    [HttpGet("single")]
    [ProducesResponseType(typeof(ValveState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSingleValveState()
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var appliances = await apiClient.GetAppliances<SenseGuardAppliance>();
        switch (appliances.Count)
        {
            case 0:
                return NotFound("No valve found");
            case 1:
                var valve = appliances.Values.First();
                var state = new ValveState
                {
                    Id = valve.Id,
                    Open = await apiClient.GetValveOpen(valve.Id)
                };
                return Json(state);
            default:
                return BadRequest("More than one valve found");
        }
    }
    
    [HttpPut("single")]
    [ProducesResponseType(typeof(ValveState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SetSingleValveState(bool open)
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
        if (await apiClient.SetValveOpen(valve.Id, open))
        {
            var state = new ValveState
            {
                Id = valve.Id,
                Open = open
            };
            return Json(state);
        }
        return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed to set valve state");
    }
}