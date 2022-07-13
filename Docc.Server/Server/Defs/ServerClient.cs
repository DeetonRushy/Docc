using Docc.Common;

namespace Docc.Server;

public class ServerClient
{
    // add any client information that is
    // server specific.

    public string Name { get; init; }
    public Guid Id { get; init; }

    public static ServerClient From(SharedClient client)
    {
        return new()
        {
            Name = client.Name,
            Id = client.Id,
        };
    }
}
