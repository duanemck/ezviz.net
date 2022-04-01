using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz_cli;

static internal class AllOptions
{
    public static Option<string> UsernameOption = new Option<string>("--username", "Username for ezviz API") { IsRequired = true };

    public static Option<string> PasswordOption = new Option<string>("--password", "Password for ezviz API") { IsRequired = true };

    public static Option<string> SerialOption = new Option<string>("--serial", "Serial number of the camera") { IsRequired = true };

    public static Option<bool> JsonOption = new Option<bool>("--json", "Output result as JSON");
}

