using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.GroheApiClasses;
using Application.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// ReSharper disable InvertIf
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Application;

public interface IApiClient
{
    Task<Dictionary<string, BaseAppliance>> GetAppliances();
    Task<Dictionary<string, T>> GetAppliances<T>(long? locationId = null, string applianceId = null) where T : BaseAppliance;
    Task<bool> DeleteNotification(string notificationId);
    Task<Dictionary<string, Notification>> GetNotifications(long? locationId);
    Task<string> GetLatestNotificationId();
    Task<SenseDetails> GetSenseDetails(string applianceId);
    Task<SenseGuardDetails> GetSenseGuardDetails(string applianceId);
    Task<AggregateData> GetAggregatedData(string applianceId, Aggregation aggregation, DateTime from, DateTime to);
    Task<bool> SnoozeGuard(string applianceId, long minutes);
    Task<bool> WakeGuard(string applianceId);
    Task<bool> GetValveOpen(string applianceId);
    Task<bool> SetValveOpen(string applianceId, bool open);
    Task<bool> MarkNotificationAsRead(Notification notification);
    void FlushCache();
}

public class ApiClient : IApiClient
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    private readonly IConfiguration _config;
    private readonly ILogger<ApiClient> _log;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _authClient;
    private readonly HttpClient _apiClient;
    private AuthTokens _authTokens;

    public ApiClient(IConfiguration config, IMemoryCache cache, ILogger<ApiClient> log)
    {
        _config = config;
        _log = log;
        _cache = cache.WithNamespace(nameof(ApiClient));
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.GZip // the app does this, so we do it too
        };

        // we use a separate client for auth, without all the headers
        _authClient = new HttpClient(handler)
        {
            BaseAddress = (Constants.ApiBaseAddress + "/v3/iot/").ParseUri()
        };
        _authClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _authClient.DefaultRequestHeaders.Add("User-Agent",
            "Dalvik/2.1.0 (Linux; U; Android 12; SM-A426N Build/SP1A.210812.016)");

        // client for general api usage, with a bunch of default headers, masking as the app
        _apiClient = new HttpClient(handler)
        {
            BaseAddress = (Constants.ApiBaseAddress + "/v3/iot/").ParseUri(),
        };
        _apiClient.DefaultRequestHeaders.Add("Device-Type", "smartphone");
        _apiClient.DefaultRequestHeaders.Add("Device-OS", "android");
        _apiClient.DefaultRequestHeaders.Add("App-Version", "1.11.0");
        _apiClient.DefaultRequestHeaders.Add("Accept-Language", "en_US");
        _apiClient.DefaultRequestHeaders.Add("Client-ID", "sense");
        _apiClient.DefaultRequestHeaders.Add("Device-OS-Number", "12");
        _apiClient.DefaultRequestHeaders.Add("User-Agent", "okhttp/4.10.0");
    }

    public async Task Login()
    {
        _log.LogInformation("Logging in...");

        // run a get request on Loginpath
        var response = await _apiClient.GetAsync("oidc/login");
        if (!response.IsSuccessStatusCode)
        {
            _log.LogError("Login failed");
            return;
        }

        // parse the html to retrieve the action of the login form
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "<form[^>]*action=\"([^\"]+)\"");
        var actionUrl =
            match.Success ? match.Groups[1].Value : throw new Exception("Failed to parse login form action");
        actionUrl = WebUtility.HtmlDecode(actionUrl);

        // submit the form
        var formSubmitMessage = new HttpRequestMessage(HttpMethod.Post, actionUrl)
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", _config.GetString("GROHE_USER")),
                new KeyValuePair<string, string>("password", _config.GetString("GROHE_PASS")),
            })
        };
        var loginResponse = await _authClient.SendAsync(formSubmitMessage);
        if (loginResponse.StatusCode != HttpStatusCode.Found)
        {
            throw new Exception("Login failed - incorrect credentials?");
        }

        // get the refresh token
        var refreshTokenLocation = new UriBuilder(loginResponse.Headers.Location!)
                { Scheme = Uri.UriSchemeHttps }
            .Uri; // switching scheme from sense:// to https:// - we'll continue with the apiClient, masking as the app, from here on
        UpdateAuthTokens(await _apiClient.GetFromJsonAsync<AuthTokens>(refreshTokenLocation));
    }

    private void UpdateAuthTokens(AuthTokens authTokens)
    {
        _authTokens = authTokens;
        _apiClient.DefaultRequestHeaders.Remove("Authorization");
        _apiClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _authTokens.AccessToken);
    }

    public void FlushCache()
    {
        _cache.Remove(nameof(GetLocations));
        _cache.Remove(nameof(GetAppliances));
        _cache.Remove(nameof(GetNotifications));
    }

    public async Task<Dictionary<long, Location>> GetLocations() =>
        await _cache.GetOrRefreshAsync(
            nameof(GetLocations),
            async () =>
            {
                _log.LogInformation("Getting locations");
                await RefreshTokenIfNecessary();
                return (await _apiClient.GetFromJsonAsync<List<Location>>("locations")).ToDictionary(l => l.Id, l => l);
            },
            _cacheDuration
        );

    public async Task<Dictionary<string, BaseAppliance>> GetAppliances() =>
        await _cache.GetOrRefreshAsync(
            nameof(GetAppliances),
            async () =>
            {
                _log.LogInformation("Getting appliances");
                await RefreshTokenIfNecessary();
                var appliances = new Dictionary<string, BaseAppliance>();
                var locations = await GetLocations();
                foreach (var location in locations.Values)
                {
                    var rooms = await _apiClient.GetFromJsonAsync<List<Room>>($"locations/{location.Id}/rooms");
                    foreach (var room in rooms)
                    {
                        var baseAppliances = await _apiClient.GetFromJsonAsync<List<BaseAppliance>>(
                            $"locations/{location.Id}/rooms/{room.Id}/appliances", BaseAppliance.JsonSerializerOptions);
                        foreach (var baseAppliance in baseAppliances)
                        {
                            BaseAppliance appliance;
                            switch (baseAppliance.Type)
                            {
                                case ApplianceType.Sense:
                                    appliance = JsonSerializer.Deserialize<SenseAppliance>(baseAppliance.Json);
                                    break;
                                case ApplianceType.SenseGuard:
                                    appliance = JsonSerializer.Deserialize<SenseGuardAppliance>(baseAppliance.Json);
                                    break;
                                case ApplianceType.Unknown:
                                default:
                                    _log.LogWarning(
                                        "Skipping unknown appliance (type:{TypeValue}, name:{Name}) => {Base64String}",
                                        baseAppliance.TypeValue, baseAppliance.Name,
                                        Convert.ToBase64String(Encoding.UTF8.GetBytes(baseAppliance.Json)));
                                    continue;
                            }

                            appliance.Type = baseAppliance.Type;
                            appliance.Json = baseAppliance.Json;
                            appliance.Room = room;
                            appliance.Location = location;

                            var status = await _apiClient.GetFromJsonAsync<List<ApplianceStatus>>(
                                $"locations/{location.Id}/rooms/{room.Id}/appliances/{appliance.Id}/status");
                            appliance.SetStatus(status);
                            appliances.Add(appliance.Id, appliance);
                        }
                    }
                }

                return appliances;
            },
            _cacheDuration
        );


    private async Task<Dictionary<string, Notification>> GetNotifications() =>
        await _cache.GetOrRefreshAsync(
            nameof(GetAppliances),
            async () =>
            {
                _log.LogInformation("Getting notifications");
                await RefreshTokenIfNecessary();

                var locations = await GetLocations();
                var appliances = await GetAppliances();
                var notifications = new Dictionary<string, Notification>();
                var continuationToken = (string)null;
                do
                {
                    var pageSize =
                        notifications.Count == 0
                            ? 20
                            : 10; // this is what the app does... we're totally the app, so we do it too
                    var url = $"profile/notifications?pageSize={pageSize}";
                    if (continuationToken != null)
                    {
                        url += $"&continuationToken={continuationToken}";
                    }

                    var notificationContainer = await _apiClient.GetFromJsonAsync<NotificationContainer>(url);
                    foreach (var notification in notificationContainer.Notifications)
                    {
                        var appliance = appliances.GetValueOrDefault(notification.ApplianceId);
                        notification.ApplianceName = appliance != null ? appliance.Name : "Unknown Appliance";
                        notification.ApplianceType = appliance?.Type ?? ApplianceType.Unknown;
                        notification.Message =
                            NotificationTypes.GetCategoryTypeMessage(notification.Category,
                                notification.NotificationType);
                        notification.LocationName = locations.TryGetValue(notification.LocationId, out var location)
                            ? location.Name
                            : "Unknown Location";
                        notifications[notification.Id] = notification;
                    }

                    continuationToken = notificationContainer.RemainingNotifications != 0
                        ? notificationContainer.ContinuationToken
                        : null;
                } while (continuationToken != null);

                return notifications;
            },
            _cacheDuration
        );

    public async Task<Dictionary<string, Notification>> GetNotifications(long? locationId) => locationId == null
        ? await GetNotifications()
        : (await GetNotifications()).Where(n => n.Value.LocationId == locationId)
        .ToDictionary(n => n.Key, n => n.Value);

    public async Task<string> GetLatestNotificationId()
    {
        // no cache here - we're polling for news
        const string
            url = "profile/notifications?pageSize=1"; // the app uses 20 to poll, even when the user is not lookin at the notifications, but let's be nice here since we're probably running 24/7
        var notificationContainer = await _apiClient.GetFromJsonAsync<NotificationContainer>(url);
        return notificationContainer.Notifications.FirstOrDefault()?.Id;
    }

    public Task<SenseDetails> GetSenseDetails(string applianceId) => _cache.GetOrRefreshAsync(
        $"{nameof(GetSenseDetails)}|{applianceId}",
        async () =>
        {
            _log.LogInformation("Getting appliance details for sense {ApplianceId}", applianceId);
            await RefreshTokenIfNecessary();
            
            var appliance = (await GetAppliances<SenseAppliance>()).GetValueOrDefault(applianceId);
            if (appliance == null)
            {
                _log.LogError("Failed to find sense appliance {ApplianceId}", applianceId);
                return null;
            }

            return await _apiClient.GetFromJsonAsync<SenseDetails>($"locations/{appliance.Location.Id}/rooms/{appliance.Room.Id}/appliances/{appliance.Id}/details");
        },
        _cacheDuration
    );

    public Task<SenseGuardDetails> GetSenseGuardDetails(string applianceId) => _cache.GetOrRefreshAsync(
    $"{nameof(GetSenseGuardDetails)}|{applianceId}",
    async () =>
        {
            _log.LogInformation("Getting appliance details for senseguard {ApplianceId}", applianceId);
            await RefreshTokenIfNecessary();
                
            var appliance = (await GetAppliances<SenseGuardAppliance>()).GetValueOrDefault(applianceId);
            if (appliance == null)
            {
                _log.LogError("Failed to find senseguard appliance {ApplianceId}", applianceId);
                return null;
            }

            return await _apiClient.GetFromJsonAsync<SenseGuardDetails>($"locations/{appliance.Location.Id}/rooms/{appliance.Room.Id}/appliances/{appliance.Id}/details");
        },
        _cacheDuration
    );

    public Task<AggregateData> GetAggregatedData(string applianceId, Aggregation aggregation, DateTime from, DateTime to) => _cache.GetOrRefreshAsync(
        $"{nameof(GetAggregatedData)}|{applianceId}",
        async () =>
        {
            _log.LogInformation("Getting aggregated data for senseguard {ApplianceId}", applianceId);
            await RefreshTokenIfNecessary();
                
            var appliance = (await GetAppliances<BaseAppliance>()).GetValueOrDefault(applianceId);
            if (appliance == null)
            {
                _log.LogError("Failed to find appliance {ApplianceId}", applianceId);
                return null;
            }
            
            var aggregationString = aggregation.ToString().ToLowerInvariant();
            var fromString = from.ToString("yyyy-MM-dd");
            var toString = to.ToString("yyyy-MM-dd");
            
            return await _apiClient.GetFromJsonAsync<AggregateData>($"locations/{appliance.Location.Id}/rooms/{appliance.Room.Id}/appliances/{appliance.Id}/data/aggregated?groupBy={aggregationString}&from={fromString}&to={toString}");
        },
        _cacheDuration
    );

    public async Task<bool> SnoozeGuard(string applianceId, long minutes)
    {
        _log.LogInformation("Snoozing appliance {ApplianceId} for {Minutes} minutes", applianceId, minutes);
        await RefreshTokenIfNecessary();
        try
        {
            var senseGuard = (await GetAppliances<SenseGuardAppliance>()).GetValueOrDefault(applianceId);
            if (senseGuard == null)
            {
                _log.LogError("Failed to find appliance {ApplianceId}", applianceId);
                return false;
            }

            if (senseGuard.SnoozedUntil != null)
            {
                // already snoozed, so let's wake it up first, as that's what the app would have to do
                _log.LogInformation("Appliance {ApplianceId} is already snoozed - waking it up first...", applianceId);
                if (!await WakeGuard(applianceId))
                {
                    _log.LogError("Failed to temporarily wake appliance {ApplianceId}", applianceId);
                    return false;
                }
            }

            var command = new SenseGuardSnoozeCommand
            {
                SnoozeMinutes = minutes
            };
            var putContent = new StringContent(JsonSerializer.Serialize(command), null, "application/json");
            var putResult = await _apiClient.PutAsync($"locations/{senseGuard.Location.Id}/rooms/{senseGuard.Room.Id}/appliances/{senseGuard.Id}/snooze", putContent);
            if (!putResult.IsSuccessStatusCode)
            {
                _log.LogError("Failed to snooze appliance {ApplianceId}", applianceId);
                return false;
            }
            
            // wait for the valve to fall asleep...
            var timeout = DateTime.UtcNow.Add(Constants.SnoozeTimeout);
            var delayMultiplier = 1;
            while (DateTime.UtcNow < timeout)
            {
                FlushCache();
                senseGuard = (await GetAppliances<SenseGuardAppliance>())[applianceId];
                if (senseGuard.SnoozedUntil != null)
                {
                    _log.LogInformation("Appliance {ApplianceId} is snoozing until {SnoozedUntil}", applianceId, senseGuard.SnoozedUntil);
                    return true;
                }
                _log.LogInformation("Waiting...");
                await Task.Delay(Constants.SnoozeRefreshDelay * delayMultiplier++);
            }

            _log.LogError("Failed to snooze appliance {ApplianceId} - timeout", applianceId);
            return false;
        }
        finally
        {
            FlushCache(); // whatever happened, we need to refresh the cache
        }
    }

    public async Task<bool> WakeGuard(string applianceId)
    {
        _log.LogInformation("Waking appliance {ApplianceId}", applianceId);
        await RefreshTokenIfNecessary();
        
        try
        {
            var senseGuard = (await GetAppliances<SenseGuardAppliance>()).GetValueOrDefault(applianceId);
            if (senseGuard == null)
            {
                _log.LogError("Failed to find appliance {ApplianceId}", applianceId);
                return false;
            }
            
            var deleteResult = await _apiClient.DeleteAsync($"locations/{senseGuard.Location.Id}/rooms/{senseGuard.Room.Id}/appliances/{senseGuard.Id}/snooze");
            if (!deleteResult.IsSuccessStatusCode)
            {
                _log.LogError("Failed to wake appliance {ApplianceId}", applianceId);
                return false;
            }
            
            // wait for the valve to wake up...
            var timeout = DateTime.UtcNow.Add(Constants.SnoozeTimeout);
            var delayMultiplier = 1;
            while (DateTime.UtcNow < timeout)
            {
                FlushCache();
                senseGuard = (await GetAppliances<SenseGuardAppliance>())[applianceId];
                if (senseGuard.SnoozedUntil == null)
                {
                    _log.LogInformation("Appliance {ApplianceId} is awake", applianceId);
                    return true;
                }
                _log.LogInformation("Waiting...");
                await Task.Delay(Constants.SnoozeRefreshDelay * delayMultiplier++);
            }

            _log.LogError("Failed to wake appliance {ApplianceId} - timeout", applianceId);
            return false;
        }
        finally
        {
            FlushCache(); // whatever happened, we need to refresh the cache
        }
    }

    public async Task<bool> SetValveOpen(string applianceId, bool open)
    {
        _log.LogInformation("Setting valve for appliance {ApplianceId} to open:{Open}", applianceId, open);
        await RefreshTokenIfNecessary();
        
        try
        {
            var senseGuard = (await GetAppliances<SenseGuardAppliance>()).GetValueOrDefault(applianceId);
            if (senseGuard == null)
            {
                _log.LogError("Failed to find appliance {ApplianceId}", applianceId);
                return false;
            }
            
            var postCommand = await GetCommand(senseGuard);
            postCommand.Command.ValveOpen = open;
            var postContent = new StringContent(JsonSerializer.Serialize(postCommand), Encoding.UTF8, "application/json");
            var postResult = await _apiClient.PostAsync($"locations/{senseGuard.Location.Id}/rooms/{senseGuard.Room.Id}/appliances/{senseGuard.Id}/command", postContent);
            if (!postResult.IsSuccessStatusCode)
            {
                _log.LogError("Failed to open valve for appliance {ApplianceId}", applianceId);
                return false;
            }
            
            return true;
        }
        finally
        {
            FlushCache(); // whatever happened, we need to refresh the cache
        }
    }

    public async Task<bool> GetValveOpen(string applianceId)
    {
        _log.LogInformation("Getting valve state for appliance {ApplianceId}", applianceId);
        await RefreshTokenIfNecessary();
        
        var senseGuard = (await GetAppliances<SenseGuardAppliance>()).GetValueOrDefault(applianceId);
        if (senseGuard == null)
        {
            _log.LogError("Failed to find appliance {ApplianceId}", applianceId);
            return false;
        }
        
        var postCommand = await GetCommand(senseGuard);
        return postCommand.Command.ValveOpen;
    }

    public async Task<bool> MarkNotificationAsRead(Notification notification)
    {
        _log.LogInformation("Marking notification {NotificationId} as read", notification.Id);
        await RefreshTokenIfNecessary();

        try
        {
            var command = new NotificationReadCommand(notification.ApplianceId, notification.Category, notification.NotificationType, notification.Id, notification.Timestamp);
            var putContent = new StringContent(JsonSerializer.Serialize(command), null, "application/json");
            var putResult = await _apiClient.PutAsync($"profile/notifications/{notification.Id}", putContent);
            if (!putResult.IsSuccessStatusCode)
            {
                _log.LogError("Failed to mark notification {NotificationId} as read", notification.Id);
                return false;
            }
        
            return true;   
        }
        finally
        {
            FlushCache(); // whatever happened, we need to refresh the cache
        }
    }

    private async Task<SenseGuardCommand> GetCommand(BaseAppliance appliance)
    {
        var command = await _apiClient.GetFromJsonAsync<SenseGuardCommand>($"locations/{appliance.Location.Id}/rooms/{appliance.Room.Id}/appliances/{appliance.Id}/command");
        command.CommandBase64 = null; // the app doesn't send this back to the server
        command.Timestamp = null; // the app doesn't send this back to the server
        return command;
    }

    private async Task RefreshTokenIfNecessary()
    {
        if (_authTokens.RefreshAt > DateTime.Now) return; // not yet time to refresh
        var refreshMessage = new HttpRequestMessage(HttpMethod.Post, "oidc/token")
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("refresh_token", _authTokens.RefreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", "sense")
            })
        };
        var refreshResponse = await _authClient.SendAsync(refreshMessage);
        UpdateAuthTokens(await refreshResponse.Content.ReadFromJsonAsync<AuthTokens>());
        _log.LogInformation("Refreshed token");
    }

    public async Task<Dictionary<string, T>> GetAppliances<T>(long? locationId = null, string applianceId = null) where T : BaseAppliance
    {
        var appliances = (await GetAppliances()).Where(a => a.Value is T);
        if (applianceId != null) appliances = appliances.Where(a => a.Value.Id == applianceId);
        if (locationId != null) appliances = appliances.Where(a => a.Value.Location.Id == locationId);
        return appliances.ToDictionary(a => a.Key, a => (T)a.Value);
    }

    public async Task<bool> DeleteNotification(string notificationId)
    {
        _log.LogInformation("Deleting notification {NotificationId}", notificationId);
        await RefreshTokenIfNecessary();

        try
        {
            var deleteResult = await _apiClient.DeleteAsync($"profile/notifications/{notificationId}");
            if (!deleteResult.IsSuccessStatusCode)
            {
                _log.LogError("Failed to delete notification {NotificationId}", notificationId);
                return false;
            }
        
            return true;   
        }
        finally
        {
            FlushCache(); // whatever happened, we need to refresh the cache
        }
    }
}