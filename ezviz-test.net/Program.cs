using ezviz.net;
using ezviz.net.domain.deviceInfo;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddUserSecrets<Program>()
    .Build();

var username = config["username"];
var password = config["password"];

if (username == null || password == null)
{
    throw new ArgumentException("Please specify username and password");
}

var client = new EzvizClient(username, password);
var result = await client.Login();

var devices = await client.GetCameras();

var device = devices.First(d => d.SerialNumber == "");
//await device.ToggleAudio(true);

var switches = device.Switches;


foreach (var @switch in switches)
{
    Console.WriteLine($"{@switch.Type} ==> {@switch.Enable}");
}

Console.ReadLine();

