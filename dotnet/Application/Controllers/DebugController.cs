using Cathedral.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController(OrderedSemaphore<IApiClient> apiClientSemaphore) : Controller
{
    [HttpGet("details/sense/{applianceId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSenseDetails(string applianceId)
    {
        using var apiClientLock = await apiClientSemaphore.WaitForDisposable();
        var apiClient = apiClientLock.Value;

        var details = await apiClient.GetSenseDetails(applianceId);
        return Json(details);
    }
    
    [HttpGet("details/senseguard/{applianceId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSenseGuardDetails(string applianceId)
    {
        using var apiClientLock = await apiClientSemaphore.WaitForDisposable();
        var apiClient = apiClientLock.Value;

        var details = await apiClient.GetSenseGuardDetails(applianceId);
        return Json(details);
    }
}