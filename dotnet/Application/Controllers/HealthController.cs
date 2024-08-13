using Cathedral.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController(OrderedSemaphore<IApiClient> apiClientSemaphore) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)] // HTTP 503 for timeout
    public async Task<IActionResult> GetHealth()
    {
        using var apiClientLock = await apiClientSemaphore.WaitForDisposable();
        var apiClient = apiClientLock.Value;

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