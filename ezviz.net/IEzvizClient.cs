using System.Runtime.CompilerServices;
using ezviz.net.domain;
using ezviz.net.util;

[assembly: InternalsVisibleTo("ezviz.net.tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ezviz.net
{
    public interface IEzvizClient
    {
        Task<string?> GetAlarmImageBase64(Alarm alarm);
        Task<IEnumerable<Camera>> GetCameras(CancellationToken stoppingToken = default);
        Task<DefenceMode> GetDefenceMode();
        Task<EzvizUser> Login(string username, string password, string? region = null);
        Task SetDefenceMode(DefenceMode mode);

        public bool LogAllResponses { get; set; }

        public Task EnablePushNotifications(IPushNotificationLogger logger, Action<Alarm> messageHandler);
        public Task CheckPushConnection();
        public Task Shutdown();
    }
}