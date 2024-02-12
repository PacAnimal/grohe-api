using Application.GroheApiClasses;
using Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable InvertIf

namespace Application.Controllers;

// api controller that lets you get the current alarm state
[ApiController]
[Route("api/[controller]")]
public class NotificationsController(IApiClientLockQueue apiClientLockQueue) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApiNotification>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(long? locationId = null)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var notifications = await apiClient.GetNotifications(locationId);
        return Json(notifications.Values.OrderByDescending(n => n.Timestamp).Select(GetApiModel));
    }
    
    [HttpGet("next/{lastUnixTime:long}")]
    [ProducesResponseType(typeof(ApiNotification), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNextNotification(long lastUnixTime, long? locationId, bool markAsRead = false, bool delete = false)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var notifications = await apiClient.GetNotifications(locationId);
        var next = notifications.Values.OrderBy(n => n.Timestamp).FirstOrDefault(n => n.Timestamp.ToUnixTime() > lastUnixTime);
        if (next == null) return NotFound();
        if (delete)
        {
            await apiClient.DeleteNotification(next.Id);
            next.IsRead |= markAsRead; // if you want it marked as read, we'll give you that, even though it's gone now
        }
        else if (markAsRead && !next.IsRead)
        {
            await apiClient.MarkNotificationAsRead(next);
            next.IsRead = true;
        }
        return Json(GetApiModel(next));
    }

    private static ApiNotification GetApiModel(Notification n) => new()
    {
        Id = n.Id,
        Timestamp = n.Timestamp,
        UnixTime = n.Timestamp.ToUnixTime(),
        Appliance = n.ApplianceName,
        ApplianceType = n.ApplianceType,
        Location = n.LocationName,
        Message = n.Message,
        IsRead = n.IsRead
    };
    
    [JsonTypeName("Notification")]
    private class ApiNotification
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string Id { get; init; }
        public DateTime Timestamp { get; init; }
        public long UnixTime { get; init; }
        public string Appliance { get; init; }
        public ApplianceType ApplianceType { get; set; }
        public string Location { get; init; }
        public string Message { get; init; }
        public bool IsRead { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }

    [HttpDelete("{notificationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)] // HTTP 503 for failure
    public async Task<IActionResult> DeleteNotification(string notificationId)
    {
        await using var apiClientLock = await apiClientLockQueue.GetLock();
        var apiClient = apiClientLock.ApiClient;

        var success = await apiClient.DeleteNotification(notificationId);
        return success ? Ok() : StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed to delete notification");
    }
}