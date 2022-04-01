using ezviz.net;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace ezviz_cli.Commands;

internal abstract class AuthenticatedCommand : Command
{
    public AuthenticatedCommand(string name, string description) : base(name,description)
    {
        AddOption(AllOptions.UsernameOption);
        AddOption(AllOptions.PasswordOption);        
    }    
}
