using ezviz.net.domain;
using Refit;

namespace ezviz.net;
public class EzvizClient : IDisposable
{
    private const string DEFAULT_REGION = "apiieu.ezvizlife.com";

    private readonly string username;
    private readonly string password;
    private readonly string region;

    public EzvizClient(string username, string password) : this(username, password, null)
    {
        this.username = username;
        this.password = password;
        this.region = region ?? DEFAULT_REGION;
    }

    public EzvizClient(string username, string password, string? region)
    {
        this.username = username;
        this.password = password;
        this.region = region ?? DEFAULT_REGION;
    }

    public void Dispose()
    {
     
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
        var response = await azviewApi.Login(payload);
        return response.LoginUser;
    }
}
