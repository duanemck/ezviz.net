using ezviz.net;
using ezviz.net.domain.deviceInfo;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

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

var device = devices.First(d => d.SerialNumber == "G06307980");

var options = new JsonSerializerOptions()
{
    WriteIndented = true,
};
Console.WriteLine(JsonSerializer.Serialize(device, options));
Console.WriteLine(JsonSerializer.Serialize( await device.GetLastAlarm(), options));



Console.ReadLine();

