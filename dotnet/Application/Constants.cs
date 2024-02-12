namespace Application;

public static class Constants
{
    public const string ApiBaseAddress = "https://idp2-apigw.cloud.grohe.com";
    public static readonly TimeSpan NotificationPollInterval = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan SnoozeRefreshDelay = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan SnoozeTimeout = TimeSpan.FromSeconds(180);
}