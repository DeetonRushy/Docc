global using static Docc.Common.StaticHelpers;
using Docc.Common;
using Docc.Client;

ClientConnection connection = new ClientConnection(new Docc.Common.SharedClient()
{
    Name = "Deeton",
    Id = Guid.NewGuid()
});

// will basically request the "/api/v2/raw-version" endpoint every time you click
// connection.MakeRequest() could be used for any registered endpoint on the server
while (true)
{
    var vrq = new RequestBuilder()
        .WithLocation("/api/v2/raw-version")
        .Build();

    var version = connection.MakeRequest(vrq);

    Console.WriteLine(version + "\n\nPress any key to redo request...");

    Console.ReadKey();
}
