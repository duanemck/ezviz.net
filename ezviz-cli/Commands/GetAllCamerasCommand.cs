using Alba.CsConsoleFormat;
using ezviz.net;
using System.CommandLine;
using System.CommandLine.Invocation;
using static System.ConsoleColor;

namespace ezviz_cli.Commands;

internal class GetAllCamerasCommand : AuthenticatedCommand
{
    public GetAllCamerasCommand() : base("cameras", "Fetch name and serial of all cameras linked to this ezviz account")
    {
        this.SetHandler(async (string username, string password) => await Handle(username, password), AllOptions.UsernameOption, AllOptions.PasswordOption);
    }

    private LineThickness headerThickness = new LineThickness(LineWidth.Double, LineWidth.Single);

    public async Task Handle(string username, string password)
    {
        var client = new EzvizClient(username, password);
        await client.Login();
        var cameras = await client.GetCameras();


        var doc = new Document(
            new Grid
            {
                Color = Gray,
                Columns = { GridLength.Auto, GridLength.Auto, GridLength.Auto },
                Children =
                {
                    new Cell("Name") {Stroke = headerThickness},
                    new Cell("Serial") {Stroke = headerThickness},
                    new Cell("Armed") {Stroke=headerThickness},
                    cameras.Select(c => new Cell[]
                    {
                        new Cell(c.Name), new Cell(c.SerialNumber), new Cell(c.Armed)
                    })
                }
            }
        );

        ConsoleRenderer.RenderDocument(doc);
    }
}
