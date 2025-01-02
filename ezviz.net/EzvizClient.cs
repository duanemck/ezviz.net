using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using ezviz.net.exceptions;
using ezviz.net.util;
using ezviz_mqtt.cloud_mqtt;
using Refit;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ezviz.net;
public class EzvizClient : IEzvizClient
{
    private const string DEFAULT_REGION = "apiieu.ezvizlife.com";
    private readonly string[] SUPPORTED_DEVICE_CATEGORIES = new[] { "COMMON", "IPC", "BatteryCamera", "BDoorBell", "XVR", "CatEye" };
    private readonly IRequestResponseLogger requestLogger;
    private string? username;
    private string? password;
    private string? region;

    private LoginArea? apiDetails;
    private LoginSession? session = null!;
    private EzvizUser? user;
    private SystemConfigInfo systemConfig = null!;

    private SessionIdProvider sessionIdProvider = new SessionIdProvider();
    private PushNotificationManager pushManager = null!;

    private IEzvizApi api = null!;

    public bool LogAllResponses { get; set; } = false;

    public EzvizClient(IRequestResponseLogger requestLogger)
    {
        this.requestLogger = requestLogger;
    }

    internal EzvizClient(IEzvizApi api, IRequestResponseLogger requestLogger, SessionIdProvider sessionIdProvider)
    {
        this.requestLogger = requestLogger;
        this.api = api;
        this.sessionIdProvider = sessionIdProvider;
    }

    private void CacheCredentialsAndCreateApi(string username, string password, string? region)
    {
        this.username = username;
        this.password = GetPasswordHash(password);
        this.region = region ?? DEFAULT_REGION;
        sessionIdProvider.Login = GetNewSession;
        api = GetApi(this.region);
    }

    public async Task<EzvizUser> Login(string username, string password, string? region)
    {
        CacheCredentialsAndCreateApi(username, password, region);
        return await Login();
    }

    private async Task<EzvizUser> Login()
    {
        var payload = new Dictionary<string, object>()
        {
            { "account", username ?? "" },
            { "password", password ?? "" },
            { "cuName", Constants.CU_NAME },
            { "msgType", "0" },
            { "feature_code", Constants.FEATURE_CODE}
        };

        LoginResponse response;
        try
        {
            response = await api.Login(payload);
        }
        catch (Exception ex)
        {
            throw new LoginException("Error logging in", ex);
        }

        if (response.Meta.Code == Meta.RESPONSE_CODE_OK)
        {
            session = response.LoginSession;
            sessionIdProvider.Session = session;
            DecodeToken(session);
            apiDetails = response.LoginArea;
            user = response.LoginUser;
            api = GetApi(apiDetails.ApiDomain);

            systemConfig = await GetSystemConfig();

            return user;
        }

        switch (response.Meta.Code)
        {
            case Meta.RESPONSE_CODE_ACCOUNT_LOCKED: throw new AccountLockedException();
            case Meta.RESPONSE_CODE_INVALID_USERNAME: throw new InvalidUsernameException();
            case Meta.RESPONSE_CODE_INVALID_PASSWORD: throw new InvalidPasswordException();
            case Meta.RESPONSE_CODE_INVALID_VERIFICATION_CODE: throw new InvalidVerificationCodeException();
            case Meta.RESPONSE_CODE_MFA_ENABLED: throw new MFAEnabledException();
            case Meta.RESPONSE_CODE_INCORRECT_REGION: throw new InvalidRegionException(response.LoginArea.ApiDomain);
        }
        throw new LoginException($"Login failed, unknown response code [{response.Meta.Code}]");
    }

    public async Task<string?> GetAlarmImageBase64(Alarm alarm)
    {
        if (!string.IsNullOrEmpty(alarm.PicUrl))
        {
            return await GetAlarmImageBase64(alarm.PicUrl);
        }
        return null;
    }

    public async Task<string?> GetAlarmImageBase64(string url)
    {
        var stream = await DownloadAuthenticatedFile(url);
        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public async Task<IEnumerable<Camera>> GetCameras(CancellationToken stoppingToken = default)
    {
        var response = await api.GetPagedList("CLOUD, TIME_PLAN, CONNECTION, SWITCH,STATUS," +
                                                                    "WIFI, NODISTURB, KMS,P2P, TIME_PLAN," +
                                                                    "CHANNEL, VTM,DETECTOR, FEATURE, CUSTOM_TAG, " +
                                                                    "UPGRADE,VIDEO_QUALITY, QOS, PRODUCTS_INFO, FEATURE_INFO", stoppingToken);
        response.Meta.ThrowIfNotOk("Getting device list");
        var cameras = response.DeviceInfos
            .Select(async device => await ParseCamera(device, response))
            .Select(device => device.Result)
            .Where(device => device != null)
            .Cast<Camera>()
            .ToList();
 

        foreach (var c in cameras)
        {
            try
            {
                if (c.Online ?? false)
                {
                    await c.GetExtraInformation();
                }
            }
            catch
            {
                //Do nothing here but let loop continue in case a camera fails 
            }
        }
        return cameras;
    }

    private async Task<Camera?> ParseCamera(EzvizDeviceInfo deviceInfo, PagedListResponse response)
    {
        try
        {
            var device = new Camera(deviceInfo, response, this);
            if (SUPPORTED_DEVICE_CATEGORIES.Contains(device.DeviceInfo.DeviceCategory))
            {
                if (LogAllResponses)
                {
                    await Log(Guid.NewGuid(), deviceInfo, response);
                }
                return device;
            }            
        }
        catch (EzvizNetException ex)
        {
            await Log(ex.Id?? Guid.NewGuid(), deviceInfo, response);
            throw;
        }
        return null;
    }

    private async Task Log(Guid id, EzvizDeviceInfo deviceInfo, PagedListResponse response)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

#pragma warning disable IL2026
        var log = $"[[DeviceInfo=>\n\n{ JsonSerializer.Serialize(deviceInfo, options)}\n\n]] [[PagedResponse=>\n\n{ JsonSerializer.Serialize(response, options)}\n\n]]";
#pragma warning restore IL2026
        await requestLogger.Log(id, deviceInfo?.DeviceSerial, log);

    }

    private async Task Log(string? serialNumber, string name, object response)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

#pragma warning disable IL2026
        var log = $"[[{name}=>\n\n{JsonSerializer.Serialize(response, options)}]]";
#pragma warning restore IL2026
        await requestLogger.Log(name, serialNumber ?? "GLOBAL", log);

    }

    public async Task SetDefenceMode(DefenceMode mode)
    {
        var payload = new Dictionary<string, object>() {
            { "groupId", -1 },
            { "mode", (int)mode }
        };
        var response = await api.SetDefenceMode(payload);
        response.Meta.ThrowIfNotOk("Could not set Defence Mode");
    }

    public async Task<DefenceMode> GetDefenceMode()
    {
        var response = await api.GetDefenceMode();
        if (LogAllResponses)
        {
            await Log(null, "Get Defence Mode", response);
        }

        response.Meta.ThrowIfNotOk("Could not get Defence Mode");
        return EnumX.ToObject<DefenceMode>(int.Parse(response.Mode));
    }


    private HttpClient GetHttpClientWithAuth(string? baseUrl)
    {
        var client = new HttpClient(new SessionIdDelegatingHandler(sessionIdProvider));
        if (baseUrl != null)
        {
            client.BaseAddress = new Uri($"https://{baseUrl}");
        }
        return client;
    }

    private IEzvizApi GetApi(string baseUrl)
    {
        if (api != null)
        {
            return api;
        }
        return RestService.For<IEzvizApi>(GetHttpClientWithAuth(baseUrl));
    }

    private string GetPasswordHash(string plaintext)
    {
        using (var md5 = MD5.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(plaintext);
            var hashed = md5.ComputeHash(bytes);
            return BitConverter.ToString(hashed).Replace("-", string.Empty).ToLower();
        }
    }

    internal async Task GetNewSession()
    {
        await Login();
    }

    private void DecodeToken(LoginSession session)
    {
        var claimsAsBase64 = session.SessionId.Split(".")[1].Replace("-", "+").Replace("_", "/");
        while (claimsAsBase64.Length % 4 != 0)
        {
            claimsAsBase64 += "=";
        }
        string claimsAsJson = Encoding.UTF8.GetString(Convert.FromBase64String(claimsAsBase64));
#pragma warning disable IL2026
        var token = JsonSerializer.Deserialize<Token>(claimsAsJson);
#pragma warning restore IL2026
        var timestamp = token?.exp ?? 0;
        session.SessionExpiry = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
    }

    private async Task<SystemConfigInfo> GetSystemConfig()
    {
        //var response = await api.GetServiceUrls(await GetSessionId());
        var response = await api.GetServiceUrls();
        if (LogAllResponses)
        {
            await Log(null, "Get System Config", response);
        }

        response.Meta.ThrowIfNotOk("Could not get API service information");       
        return response.SystemConfigInfo;
    }

    internal async Task<ICollection<Algorithm>?> GetDetectionSensitivity(string? serialNumber)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
        var payload = new Dictionary<string, object>() { { "subSerial", serialNumber } };
        var response = await api.GetDetectionSensitivity(payload);
        if (response.ResultCode != "0")
        {
            return null;
        }

        if (LogAllResponses)
        {
            await Log(serialNumber, "Sensitivity", response);
        }

        return response.AlgorithmConfig.AlgorithmList;
    }

    internal async Task SetDetectionSensitivity(string? serialNumber, int type, DetectionSensitivityLevel level)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
        var response = await api.SetDetectionSensitivity(serialNumber, 1, type, (int)level);
        response.Meta.ThrowIfNotOk("Setting device sensitivity");
    }

    internal async Task SetAlarmSoundLevel(string? serialNumber, bool enable, AlarmSound level)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
        var payload = new Dictionary<string, object>() {
            { "enable", enable ? 1: 0 },
            { "soundType", (int)level },
            { "voiceId" , "0" },
            { "deviceSerial", serialNumber }
        };
        var response = await api.SetAlarmSoundLevel(serialNumber, payload);
        if (!response.IsSuccessStatusCode)
        {
            throw new EzvizNetException($"Unable to update the sound level. [{response.StatusCode}][{response.ReasonPhrase}]", response.Error);
        }
    }

    internal async Task<ICollection<Alarm>> GetAlarms(string? serialNumber, bool includeImage)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
        var query = new Dictionary<string, object>() {
            { "deviceSerials", serialNumber },
            { "queryType", -1 },
            { "limit", 10 },
            { "stype", -1 }
        };
        var response = await api.GetAlarmInformation(query);
        if (LogAllResponses)
        {
            await Log(serialNumber, "Alarms", response);
        }

        response.Meta.ThrowIfNotOk("Querying alarms");

        if (includeImage)
        {
            foreach (var alarm in response.Alarms)
            {
                alarm.DownloadedPicture = await GetAlarmImageBase64(alarm);
            }
        }

        return response.Alarms;
    }

    internal async Task ChangeSwitch(string? serialNumber, SwitchType @switch, bool enable)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
        var payload = new Dictionary<string, object>() {
            { "enable", enable ? 1: 0 },
            { "serial", serialNumber },
            { "channelNo" , "1" },
            { "type", (int)@switch }

        };
        var response = await api.ChangeSwitch(serialNumber, @switch, payload);
        response.Meta.ThrowIfNotOk($"Changing switch {@switch} to {enable}");
    }

    internal async Task SetCameraArmed(string? serialNumber, bool armed)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
        var payload = new Dictionary<string, object>() {
            { "type", "Global" },
            { "status" , armed ? "1" : "0" },
            { "actor", "V" }

        };
        var response = await api.ChangeCameraArmedStatus(serialNumber, 0, payload);
        response.Meta.ThrowIfNotOk($"Setting camera armed to {armed}");
    }

    internal async Task<AlarmDetectionMethod> GetAlarmDetectionMethod(string? serialNumber)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }

        var response = await api.GetDeviceConfig(serialNumber, 1, "Alarm_DetectHumanCar");
        if (LogAllResponses)
        {
            await Log(serialNumber, "AlarmDetectionMethod", response);
        }

        response.Meta.ThrowIfNotOk($"Getting alarm detection method");
#pragma warning disable IL2026
        var parsedResponse = JsonSerializer.Deserialize<IDictionary<string, JsonElement>>(response.ValueInfo);
#pragma warning restore IL2026
        if (parsedResponse == null)
        {
            throw new EzvizNetException($"Could not parse device config response {response.ValueInfo}");
        }
        return EnumX.ToObject<AlarmDetectionMethod>(parsedResponse["type"].GetInt32());
    }

    internal async Task<DisplayMode> GetImageDisplayMode(string? serialNumber)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }

        var response = await api.GetDeviceConfig(serialNumber, 1, "display_mode");
        if (LogAllResponses)
        {
            await Log(serialNumber, "ImageDisplayMode", response);
        }

        response.Meta.ThrowIfNotOk($"Getting image display method");

#pragma warning disable IL2026
        var parsedResponse = JsonSerializer.Deserialize<IDictionary<string, JsonElement>>(response.ValueInfo);
#pragma warning restore IL2026
        if (parsedResponse == null)
        {
            throw new EzvizNetException($"Could not parse device config response {response.ValueInfo}");
        }
        return EnumX.ToObject<DisplayMode>(parsedResponse["mode"].GetInt32());
    }

    internal async Task SetAlarmDetectionMethod(string? serialNumber, AlarmDetectionMethod method)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
#pragma warning disable IL2026
        var value = JsonSerializer.Serialize(new Dictionary<string, object>()
        {
            {"type", (int)method }
        });
#pragma warning restore IL2026

        var payload = new Dictionary<string, object>() {
            { "key", "Alarm_DetectHumanCar" },
            { "value", value}
        };
        var response = await api.SetDeviceConfig(serialNumber, 0, payload);
        response.Meta.ThrowIfNotOk($"Setting alarm detection method");
    }

    internal async Task SetImageDisplayMode(string? serialNumber, DisplayMode method)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
#pragma warning disable IL2026
        var value = JsonSerializer.Serialize(new Dictionary<string, object>()
        {
            {"mode", (int)method }
        });
#pragma warning restore IL2026
        var payload = new Dictionary<string, object>() {
            { "key", "display_mode" },
            { "value", value}
        };
        var response = await api.SetDeviceConfig(serialNumber, 0, payload);
        response.Meta.ThrowIfNotOk($"Setting display mode");
    }

    internal async Task SendAlarm(string? serialNumber, bool alarmOn)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }
        var response = await api.SendAlarm(serialNumber, 1, alarmOn ? 2 : 1);
        response.Meta.ThrowIfNotOk($"Sending Alarm");
    }

    internal async Task<Stream> DownloadAuthenticatedFile(string url)
    {
        var client = GetHttpClientWithAuth(null);
        return await client.GetStreamAsync(url);
    }

    public async Task EnablePushNotifications(IPushNotificationLogger logger, Action<Alarm> messageHandler)
    {
        if (user == null || session == null)
        {
            throw new EzvizNetException("Cannot register for push notifications before logging in");
        }
        pushManager = new PushNotificationManager(this, logger, messageHandler);
        await pushManager.Connect(user, session, systemConfig);
    }

    public async Task Shutdown()
    {
        if ( pushManager != null)
        {
            await pushManager.Shutdown();
        }        
    }

    public Task CheckPushConnection()
    {
        return pushManager.OpenPushNotificationStream();
    }


}

internal class Token
{
    public long exp { get; set; }
}
