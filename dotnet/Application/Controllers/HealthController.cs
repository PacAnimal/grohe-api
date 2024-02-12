using Application.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController(IApiClientLockQueue apiClientLockQueue) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)] // HTTP 503 for timeout
    public async Task<IActionResult> GetHealth()
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var applianceCount = 0;
        try
        {
            var appliances = await apiClient.GetAppliances();
            applianceCount = appliances.Count;
        }
        catch (Exception)
        {
            // ignored
        }
        
        return applianceCount != 0 ? Ok() : StatusCode(StatusCodes.Status503ServiceUnavailable, "API is unhealthy");
    }

}