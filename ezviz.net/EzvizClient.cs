using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using ezviz.net.exceptions;
using ezviz.net.util;
using Refit;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ezviz.net;
public class EzvizClient
{
    private const string DEFAULT_REGION = "apiieu.ezvizlife.com";
    private readonly string[] SUPPORTED_DEVICE_CATEGORIES = new[] { "COMMON", "IPC", "BatteryCamera", "BDoorBell", "XVR", "CatEye" };

    private readonly string username;
    private readonly string password;
    private readonly string region;

    private LoginArea? apiDetails;
    private LoginSession? session;
    private EzvizUser? user;
    private SystemConfigInfo systemConfig = null!;

    private SessionIdProvider sessionIdProvider = new SessionIdProvider();


    private IEzvizApi api;

    public EzvizClient(string username, string password) : this(username, password, null)
    {
    }

    public EzvizClient(string username, string password, string? region)
    {
        this.username = username;
        this.password = GetPasswordHash(password);
        this.region = region ?? DEFAULT_REGION;
        sessionIdProvider.Login = GetNewSession;
        api = GetApi(this.region);

    }

    public async Task<EzvizUser> Login()
    {
        var payload = new Dictionary<string, object>()
        {
            { "account", username },
            { "password", password },
            { "cuName", "SFRDIDEw" },
            { "msgType", "0" },
            { "feature_code", "1fc28fa018178a1cd1c091b13b2f9f02"}
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
            case Meta.RESPONSE_CODE_MFA_ENABLED: throw new MFAEnabledException();
            case Meta.RESPONSE_CODE_INCORRECT_REGION: throw new InvalidRegionException(response.LoginArea.ApiDomain);
        }
        throw new LoginException($"Login failed, unknown response code [{response.Meta.Code}]");
    }

    public async Task<string?> GetAlarmImageBase64(Alarm alarm)
    {
        if (!string.IsNullOrEmpty(alarm.PicUrl))
        {
            var stream = await DownloadAuthenticatedFile(alarm.PicUrl);
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }
        return null;
    }

    public async Task<IEnumerable<Camera>> GetCameras(CancellationToken stoppingToken = default)
    {
        var response = await api.GetPagedList("CLOUD, TIME_PLAN, CONNECTION, SWITCH,STATUS," +
                                                                    "WIFI, NODISTURB, KMS,P2P, TIME_PLAN," +
                                                                    "CHANNEL, VTM,DETECTOR, FEATURE, CUSTOM_TAG, " +
                                                                    "UPGRADE,VIDEO_QUALITY, QOS, PRODUCTS_INFO, FEATURE_INFO", stoppingToken);
        response.Meta.ThrowIfNotOk("Getting device list");
        var cameras = response.DeviceInfos
            .Select(device => new Camera(device, response, this))
            .Where(device => SUPPORTED_DEVICE_CATEGORIES.Contains(device.DeviceInfo.DeviceCategory))
            .Cast<Camera>();

        foreach (var c in cameras.Where(c => c.Online ?? false))
        {
            try
            {
                await c.GetExtraInformation();
            }
            catch
            {
                //Do nothing here but let loop continue in case a camera fails 
            }
        }
        return cameras;
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
}

internal class Token
{
    public long exp { get; set; }
}
