using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using ezviz.net.exceptions;
using Refit;
using System.Security.Cryptography;
using System.Text;

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
    private SystemConfigInfo systemConfig;

    private IEzvizApi? api;

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
        using (var md5 = HashAlgorithm.Create("MD5"))
        {
            var bytes = Encoding.UTF8.GetBytes(plaintext);
            var hashed = md5.ComputeHash(bytes);
            return BitConverter.ToString(hashed).Replace("-", string.Empty).ToLower();
        }
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



    public async Task<IEnumerable<Camera>> GetCameras()
    {
        var response = await api.GetPagedList(session.SessionId, "CLOUD, TIME_PLAN, CONNECTION, SWITCH,STATUS," +
                                                                    "WIFI, NODISTURB, KMS,P2P, TIME_PLAN," +
                                                                    "CHANNEL, VTM,DETECTOR, FEATURE, CUSTOM_TAG, " +
                                                                    "UPGRADE,VIDEO_QUALITY, QOS, PRODUCTS_INFO, FEATURE_INFO");
        response.Meta.ThrowIfNotOk("Getting device list");
        return response.DeviceInfos
            .Select(device => new Camera(device, response))
            .Where(device => SUPPORTED_DEVICE_CATEGORIES.Contains(device.DeviceInfo.DeviceCategory))
            .Cast<Camera>();
    }

    private async Task<SystemConfigInfo> GetSystemConfig()
    {
        var response = await api.GetServiceUrls(session.SessionId);
        response.Meta.ThrowIfNotOk("Could not get API service information");
        return response.SystemConfigInfo;
    }

}
