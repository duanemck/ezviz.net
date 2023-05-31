namespace ezviz.net.util
{
    public interface IPushNotificationLogger
    {
        void LogInformation(string content, params object[] formatParams);
    }
}
