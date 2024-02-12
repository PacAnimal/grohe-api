using Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController(IApiClientLockQueue apiClientLockQueue) : Controller
{
    [HttpGet("details/sense/{applianceId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSenseDetails(string applianceId)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var details = await apiClient.GetSenseDetails(applianceId);
        return Json(details);
    }
    
    [HttpGet("details/senseguard/{applianceId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSenseGuardDetails(string applianceId)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var details = await apiClient.GetSenseGuardDetails(applianceId);
        return Json(details);
    }
}