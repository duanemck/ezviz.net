using ezviz.net;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddUserSecrets<Program>()
    .Build();

var username = config["username"];
var password = config["password"];

var client = new EzvizClient(username, password);
var result = await client.Login();

Console.WriteLine(result.Username);
