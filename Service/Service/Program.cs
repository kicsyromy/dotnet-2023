using CommandLine;
using Mapster.Service.Endpoints;

namespace Mapster.Service;

public class Options
{
    [Option('i', "input", Required = true, HelpText = "Path to map data binary file")]
    public string? MapDataPath { get; set; }
}

internal static class Program
{
    // Set up Main as an async method
    private static async Task Main(string[] args)
    {
        Options? arguments = null;
        var argParseResult = Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
        {
            arguments = options;
        });

        if (argParseResult.Errors.Any())
        {
            Environment.Exit(-1);
        }

        // Set up a builder to help register our endpoint handlers
        var appBuilder = WebApplication.CreateBuilder();

        // Register TileEndpoint as a singleton instance but instantiate it explicitly to force data loading
        var tileEndpoint = new TileEndpoint(arguments!.MapDataPath!);
        appBuilder.Services.AddSingleton(_ => tileEndpoint);

        // Create the application instance
        var app = appBuilder.Build();
        // Set up to serve on localhost's unpriviledged 8080 port
        app.Urls.Add("http://localhost:8080");

        TileEndpoint.Register(app);

        await app.RunAsync();
    }
}
