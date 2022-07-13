using static Docc.Common.StaticHelpers;
using Docc.Common;
using Docc.Server;
using System.Text;

Environment.SetEnvironmentVariable("App-Version", "v0.0.4-dev.1");
Environment.SetEnvironmentVariable("App-Agent", $"Docc {Environment.GetEnvironmentVariable("App-Version")}");

DirectoryListingManager manager = new();
ServerConnection connection = new();

// The 'Location' could be viewed as (in raw form)
// '/api/private/index?Key=Value'
// The .WithArguments method basically makes this
// usage pointless, but any raw request will look that
// way.
// 
// NOTE: Any request that is returning data, primarily
//       server functions, should return to location '/'.
//       This doesn't really matter a whole lot, but it makes
//       it more explicit that you're not meaning to send this
//       anywhere in particular.
//       It's simply a response, with a focus on the content.

manager.MapGet("/errors/args-error", (args, sender) =>
{
    var page = args["fromPage"];
    var argument = args["inQuestion"];

    return new RequestBuilder()
        .WithLocation(Request.DefaultLocation)
        .AddContent($"'{page}' is missing argument '{argument}'")
        .AddContent($"try '{page}?{argument}=[value]' instead.")
        .WithResult(NotFound())
        .Build();
});

manager.MapGet("/api/v2/raw-version", (args, sender) =>
{
    string version;

    try
    {
        version = File.ReadAllText("version.txt");
    }
    catch (FileNotFoundException fileNotFound)
    {
        return new RequestBuilder()
            .WithLocation(Request.DefaultLocation)
            .WithResult(RequestResult.FileNotFound)
            .AddContent(fileNotFound.Message)
            .Build();
    }

    var response = new RequestBuilder()
        .WithLocation(Request.DefaultLocation)
        .WithResult(Okay())
        .AddContent(version)
        .Build();

    return response;
});
manager.MapGet("/api/v2/users", (args, _) =>
{
    var rb = new RequestBuilder()
        .WithLocation(Request.DefaultLocation)
        .AddContent("Deeton")
        .AddContent("Johnny")
        .WithResult(Okay());

    return rb.Build();
});
manager.MapGet("/api/v2/user/id", (args, _) =>
{
    if (!args.ContainsKey("uuid"))
    {
        return manager.CallMappedLocal("/errors/args-error", new()
        {
            // /errors/args-error?inQuestion="uuid"&fromPage="/api/v2/user/id"
            { "inQuestion", "uuid" },
            { "fromPage", "/api/v2/user/id" }
        });
    }

    var rb = new RequestBuilder()
        .WithLocation(Request.DefaultLocation)
        .AddContent(Guid.NewGuid().ToString())
        .WithResult(Okay());

    return rb.Build();
});

connection.OnMessage = (req, client) =>
{
    connection._logger.Log($"received request for '{req.Location}'");

    var response = manager.CallMappedLocal(req.Location, req.Arguments);
    client.SendRequest(response);
};

Console.ReadLine();