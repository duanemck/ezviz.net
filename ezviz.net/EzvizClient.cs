using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using ezviz.net.exceptions;
using ezviz.net.util;
using Refit;
using System.Security.Claims;
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

    private IEzvizApi api;

    public EzvizClient(string username, string password) : this(username, password, null)
    {
    }

    public EzvizClient(string username, string password, string? region)
    {
        this.username = username;
        this.password = GetPasswordHash(password);
        this.region = region ?? DEFAULT_REGION;
        api = RestService.For<IEzvizApi>($"https://{this.region}");
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

    private async Task<string> GetSessionId()
    {

        if (session == null)
        {
            throw new EzvizNetException("Not logged in to Ezviz API");
        }
        if (session.SessionExpiry <= DateTime.UtcNow - TimeSpan.FromMinutes(10))
        {
            await Login();
        }
        return session.SessionId;

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
        var azviewApi = RestService.For<IEzvizApi>($"https://{region}");

        LoginResponse response;
        try
        {
            response = await azviewApi.Login(payload);
        }
        catch (Exception ex)
        {
            throw new LoginException("Error logging in", ex);
        }

        if (response.Meta.Code == Meta.RESPONSE_CODE_OK)
        {
            session = response.LoginSession;
            DecodeToken(session);
            apiDetails = response.LoginArea;
            user = response.LoginUser;
            api = RestService.For<IEzvizApi>($"https://{apiDetails.ApiDomain}");

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

    void DecodeToken(LoginSession session)
    {
        var claimsAsBase64 = session.SessionId.Split(".")[1].Replace("-","+").Replace("_", "/");
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

    public async Task<IEnumerable<Camera>> GetCameras(CancellationToken stoppingToken)
    {
        var response = await api.GetPagedList(await GetSessionId(), "CLOUD, TIME_PLAN, CONNECTION, SWITCH,STATUS," +
                                                                    "WIFI, NODISTURB, KMS,P2P, TIME_PLAN," +
                                                                    "CHANNEL, VTM,DETECTOR, FEATURE, CUSTOM_TAG, " +
                                                                    "UPGRADE,VIDEO_QUALITY, QOS, PRODUCTS_INFO, FEATURE_INFO", stoppingToken);
        response.Meta.ThrowIfNotOk("Getting device list");
        return response.DeviceInfos
            .Select(device => new Camera(device, response, this))
            .Where(device => SUPPORTED_DEVICE_CATEGORIES.Contains(device.DeviceInfo.DeviceCategory))
            .Cast<Camera>();
    }

    public async Task SetDefenceMode(DefenceMode mode)
    {
        var payload = new Dictionary<string, object>() {
            { "groupId", -1 },
            { "mode", (int)mode }
        };
        var response = await api.SetDefenceMode(await GetSessionId(), payload);
        response.Meta.ThrowIfNotOk("Could not set Defence Mode");
    }

    public async Task<DefenceMode> GetDefenceMode()
    {
        var response = await api.GetDefenceMode(await GetSessionId());
        response.Meta.ThrowIfNotOk("Could not get Defence Mode");
        return EnumX.ToObject<DefenceMode>(int.Parse(response.Mode));
    }

    private async Task<SystemConfigInfo> GetSystemConfig()
    {
        var response = await api.GetServiceUrls(await GetSessionId());
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
        var response = await api.GetDetectionSensitivity(await GetSessionId(), payload);
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
        var response = await api.SetDetectionSensitivity(await GetSessionId(), serialNumber, 1, type, (int)level);
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
        var response = await api.SetAlarmSoundLevel(await GetSessionId(), serialNumber, payload);
        if (!response.IsSuccessStatusCode)
        {
            throw new EzvizNetException($"Unable to update the sound level. [{response.StatusCode}][{response.ReasonPhrase}]", response.Error);
        }
    }

    internal async Task<ICollection<Alarm>> GetAlarms(string? serialNumber)
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
        var response = await api.GetAlarmInformation(await GetSessionId(), query);
        response.Meta.ThrowIfNotOk("Querying alarms");

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
        var response = await api.ChangeSwitch(await GetSessionId(), serialNumber, @switch, payload);
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
        var response = await api.ChangeCameraArmedStatus(await GetSessionId(), serialNumber, 0, payload);
        response.Meta.ThrowIfNotOk($"Setting camera armed to {armed}");
    }

    internal async Task<AlarmDetectionMethod> GetAlarmDetectionMethod(string? serialNumber)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }

        var response = await api.GetDeviceConfig(await GetSessionId(), serialNumber, 1, "Alarm_DetectHumanCar");
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

        var response = await api.GetDeviceConfig(await GetSessionId(), serialNumber, 1, "display_mode");
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
        var response = await api.SetDeviceConfig(await GetSessionId(), serialNumber, 0, payload);
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
        var response = await api.SetDeviceConfig(await GetSessionId(), serialNumber, 0, payload);
        response.Meta.ThrowIfNotOk($"Setting display mode");
    }

    public async Task SetChannelWhistle(string? serialNumber)
    {
        if (serialNumber == null)
        {
            throw new ArgumentNullException(nameof(serialNumber));
        }

        var payload = new Dictionary<string, object>() {
            {
                "channelWhistleList" , new Whistle[]{ new Whistle{
                                                Channel = 1,
                                                DeviceSerial = serialNumber,
                                                Duration = 10,
                                                Status = 1,
                                                Volume =10
                                       } }
             }
        };
        var response = await api.SetChannelWhistle(await GetSessionId(), serialNumber, payload);
        response.Meta.ThrowIfNotOk($"Setting display mode");
    }



}

class Whistle
{
    public int Channel { get; set; }
    public string DeviceSerial { get; set; }
    public int Duration { get; set; }
    public int Status { get; set; }
    public int Volume { get; set; }
}

public class Token
{
    public long exp { get; set; }
}
