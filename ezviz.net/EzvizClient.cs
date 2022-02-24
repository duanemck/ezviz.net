using ezviz.net.domain;
using Refit;
using System.Security.Cryptography;
using System.Text;

namespace ezviz.net;
public class EzvizClient 
{
    private const string DEFAULT_REGION = "apiieu.ezvizlife.com";

    private readonly string username;
    private readonly string password;
    private readonly string region;

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
        var response = await azviewApi.Login(payload);
        return response.LoginUser;
    }

}
