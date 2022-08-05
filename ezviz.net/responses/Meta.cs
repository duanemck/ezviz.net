using ezviz.net.exceptions;

namespace ezviz.net.domain;

internal class Meta
{
    public const int RESPONSE_CODE_OK = 200;

    public const int RESPONSE_CODE_INCORRECT_REGION = 1100;
    public const int RESPONSE_CODE_INVALID_VERIFICATION_CODE = 1012;
    public const int RESPONSE_CODE_INVALID_USERNAME = 1013;
    public const int RESPONSE_CODE_INVALID_PASSWORD = 1014;
    public const int RESPONSE_CODE_ACCOUNT_LOCKED = 1015;
    public const int RESPONSE_CODE_DEVICE_EXCEPTION = 2004;
    public const int RESPONSE_CODE_MFA_ENABLED = 6002;

    public int Code { get; set; }
    public string Message { get; set; } = null!;
    public object MoreInfo { get; set; } = null!;

    public void ThrowIfNotOk(string message) 
    {
        if (Code == 2003)
        {
            throw new EzvizNetException($"Device is offline, could not update");
        }
        if (Code != RESPONSE_CODE_OK)
        {
            throw new EzvizNetException($"Something went wrong connecting to the Ezviz Api [{message}]");
        }
    }
}
