using ezviz.net.domain;

namespace ezviz.net
{
    public interface IEzvizClient
    {
        Task<string?> GetAlarmImageBase64(Alarm alarm);
        Task<IEnumerable<Camera>> GetCameras(CancellationToken stoppingToken = default);
        Task<DefenceMode> GetDefenceMode();
        Task<EzvizUser> Login(string username, string password, string? region = null);
        Task SetDefenceMode(DefenceMode mode);
    }
}