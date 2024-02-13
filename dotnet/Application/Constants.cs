namespace Application;

public static class Constants
{
    public const string ApiBaseAddress = "https://idp2-apigw.cloud.grohe.com";
    public static readonly TimeSpan NotificationPollInterval = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan SnoozeRefreshDelay = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan SnoozeTimeout = TimeSpan.FromSeconds(180);
    public static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan NotificationCacheDuration = TimeSpan.FromHours(2); // we can cache notifications for a long time, as the NotificationPollerService will flush the cache when it detects a new notification
}