using ezviz.net.domain;
using ezviz.net.exceptions;
using Refit;
using System.Security.Cryptography;
using System.Text;

namespace ezviz.net;
public class EzvizClient 
{
    private const string DEFAULT_REGION = "apiieu.ezvizlife.com";
    
    private const int RESPONSE_CODE_OK = 200;
        
    private const int RESPONSE_CODE_INCORRECT_REGION = 1100;
    private const int RESPONSE_CODE_INVALID_USERNAME = 1013;
    private const int RESPONSE_CODE_INVALID_PASSWORD = 1014;
    private const int RESPONSE_CODE_ACCOUNT_LOCKED = 1015;
    private const int RESPONSE_CODE_MFA_ENABLED = 6002;
    
    private readonly string username;
    private readonly string password;
    private readonly string region;
    
    private LoginArea? apiDetails;
    private LoginSession? session;
    private EzvizUser? user;
    private SystemConfigInfo systemConfig;

    public EzvizClient(string username, string password) : this(username, password, null)
    {                
    }

    public EzvizClient(string username, string password, string? region)
    {
        this.username = username;
        this.password = GetPasswordHash(password);
        this.region = region ?? DEFAULT_REGION;
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

        if (response.Meta.Code == RESPONSE_CODE_OK)
        {
            session = response.LoginSession;
            apiDetails = response.LoginArea;
            user = response.LoginUser;

            systemConfig = await GetSystemConfig();

            return user;
        }

        switch (response.Meta.Code)
        {
            case RESPONSE_CODE_ACCOUNT_LOCKED: throw new AccountLockedException();
            case RESPONSE_CODE_INVALID_USERNAME: throw new InvalidUsernameException();
            case RESPONSE_CODE_INVALID_PASSWORD: throw new InvalidPasswordException();
            case RESPONSE_CODE_MFA_ENABLED: throw new MFAEnabledException();
            case RESPONSE_CODE_INCORRECT_REGION: throw new InvalidRegionException(response.LoginArea.ApiDomain);                
        }
        throw new LoginException($"Login failed, unknown response code [{response.Meta.Code}]");
        
    }

    private async Task<SystemConfigInfo> GetSystemConfig()
    {
        var azviewApi = RestService.For<IEzvizApi>($"https://{apiDetails.ApiDomain}");
        var response = await azviewApi.GetServiceUrls(session.SessionId);

        if (response.Meta.Code != RESPONSE_CODE_OK)
        {
            throw new ServiceConfigException();
        }
        return response.SystemConfigInfo;
    }

}
