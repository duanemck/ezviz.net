using ezviz_cli.Commands;
using System.CommandLine;

var rootCommand = new RootCommand("ezviz.net CLI");

rootCommand.AddCommand(new GetAllCamerasCommand());
rootCommand.AddCommand(new GetCameraDetailCommand());

return await rootCommand.InvokeAsync(args);