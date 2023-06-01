using ezviz.net;
using ezviz_cli;
using ezviz_cli.Commands;
using System.CommandLine;

var rootCommand = new RootCommand("ezviz.net CLI");

IEzvizClient client = new EzvizClient(new DefaultRequestResponseLogger());
//TODO: Fix
//rootCommand.AddCommand(new GetAllCamerasCommand());
rootCommand.AddCommand(new GetCameraDetailCommand(client));

return await rootCommand.InvokeAsync(args);