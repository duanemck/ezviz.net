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

foreach (var device in devices)
{
    var alarms = await device.GetAlarms();
    Console.WriteLine(device.SerialNumber);
    foreach (var alarm in alarms)
    {
        Console.WriteLine(alarm.AlarmMessage);
    }
}

Console.ReadLine();

