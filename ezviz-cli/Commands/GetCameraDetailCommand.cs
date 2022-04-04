using Alba.CsConsoleFormat;
using Alba.CsConsoleFormat.Fluent;
using ezviz.net;
using ezviz.net.domain;
using System.CommandLine;
using System.Text;
using System.Text.Json;
using static System.ConsoleColor;

namespace ezviz_cli.Commands;

internal class GetCameraDetailCommand : AuthenticatedCommand
{
    private readonly JsonSerializerOptions jsonSerializationOptions = new JsonSerializerOptions()
    {
        WriteIndented = true
    };

    public GetCameraDetailCommand() : base("camera-detail", "Fetch status and config of specified camera")
    {
        AddOption(AllOptions.SerialOption);
        AddOption(AllOptions.JsonOption);

        this.SetHandler(async (string username, string password, string serial, bool json) => await Handle(username, password, serial, json),
            AllOptions.UsernameOption,
            AllOptions.PasswordOption,
            AllOptions.SerialOption,
            AllOptions.JsonOption);
    }

    public async Task Handle(string username, string password, string serial, bool json)
    {
        var client = new EzvizClient(username, password);
        await client.Login();
        var cameras = await client.GetCameras();
        var camera = cameras.FirstOrDefault(c => c.SerialNumber == serial);

        if (camera == null)
        {
            Console.Error.WriteLine($"No camera with serial {serial} found");
            return;
        }

        if (json)
        {
#pragma warning disable IL2026
            Console.WriteLine(JsonSerializer.Serialize(camera, jsonSerializationOptions));
#pragma warning restore IL2026
        }
        else
        {
            PrintCameraDetails(camera);
        }
    }

    private LineThickness headerThickness = new LineThickness(LineWidth.Double, LineWidth.Single);

    private void PrintCameraDetails(Camera camera)
    {
        Console.OutputEncoding = Encoding.UTF8;
        //var delim = "|";
        //var builder = new StringBuilder();
        //builder.Append("Name=").Append(camera.Name).Append(delim);
        //builder.Append("Serial=").Append(camera.SerialNumber).Append(delim);
        //builder.Append("Armed=").Append(camera.Armed).Append(delim);
        //builder.Append("DetectionMethod=").Append(camera.AlarmDetectionMethod).Append(delim);
        //builder.Append("ScheduleEnabled=").Append(camera.AlarmScheduleEnabled).Append(delim);


        var doc = new Document(
            new Span("Name") ,
                    new Span("Serial"),
            new Grid
            {
                Color = Gray,
                Columns = {GridLength.Auto, GridLength.Auto, GridLength.Auto},
                Children =
                {
                    
                    new Cell("Armed") {Stroke=headerThickness},
                    new Cell(camera.Name), new Cell(camera.SerialNumber), new Cell(camera.Armed)
                }
            }
            );
        
        ConsoleRenderer.RenderDocument(doc);

        //Colors.WriteLine(camera.Name.Green(), camera.SerialNumber);
    }



}
