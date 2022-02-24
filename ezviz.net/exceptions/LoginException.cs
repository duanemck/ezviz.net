
namespace ezviz.net.exceptions;
public class LoginException : Exception
{
    public LoginException(string message) : base(message)
    {

    }

    public LoginException(string message, Exception inner) : base(message, inner)
    {

    }
}

public class InvalidUsernameException : LoginException
{
    public InvalidUsernameException() : base("API reports: Invalid Username")
    {

    }
}

public class InvalidPasswordException : LoginException
{
    public InvalidPasswordException() : base("API reports: Invalid Password")
    {

    }
}

public class InvalidRegionException : LoginException
{
    public InvalidRegionException(string correctRegion) : base($"API reports: Invalid Region, should be [{correctRegion}]")
    {

    }
}

public class AccountLockedException : LoginException
{
    public AccountLockedException() : base("API reports: Account Locked")
    {

    }
}

public class MFAEnabledException : LoginException
{
    public MFAEnabledException() : base("MFA Enabled. This is not currently supported by this library.")
    {

    }
}