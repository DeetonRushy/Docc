using Docc.Common;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;

namespace Docc.Client;

internal class ClientConnection
{
    private readonly ILogger _logger;

    protected Socket Socket { get; }

    public IPAddress Address { get; }
    public IPHostEntry Entry { get; } = Dns.GetHostEntry("localhost");
    public IPEndPoint ServerEndpoint { get; }

    public ClientConnection(SharedClient client)
    {
        _logger = new ClientConsoleLogger();

        // at this point, if the local user is banned, they are gone.

        Address = Entry.AddressList[0];
        ServerEndpoint = new IPEndPoint(Address, 25755);
        Socket = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        int attempts = 0;

        while (attempts <= 5)
        {
            try
            {
                Socket.Connect(ServerEndpoint);
            }
            catch
            {
                attempts++;
                continue;
            }

            break;
        }

        if (attempts == 5)
        {
            _logger.Log("failed to connect to the server.");
            Exit(0);
        }

        // We're connected, instantly send the client info over.

        var rb = new RequestBuilder()
            .WithLocation("/")
            .AddContent(JsonConvert.SerializeObject(client));

        Socket.SendEncrypted(rb.Build(), _logger);

        var status = Socket.ReceiveEncrypted(_logger);

        if (status is null)
        {
            // tf?
            _logger.Log("server failed to respond with handshake.");
            Exit(0);
            // so IDE's can see that beyond this point status will not
            // be null.
            return;
        }

        if (status.Result != RequestResult.OK)
        {
            _logger.Log($"server rejected the connection with reason: {Translation.From(status.Result).Conversion}");
            Exit(-1);
        }

        // If it's an Okay(), we need to make sure the server sent
        // our Guid back.

        var guid = status.Content.FirstOrDefault();

        if (guid is null)
        {
            _logger.Log("server failed to provide valid handshake.");
            Environment.Exit(-1);
            return;
        }

        if (!Guid.TryParse(guid, out var id))
        {
            _logger.Log("server failed to provide valid handshake.");
            Environment.Exit(-1);
            return;
        }

        if (client.Id != id)
        {
            _logger.Log("server failed to provide valid handshake.");
        }

        _logger.Log("connected to server!");
    }

    public Request? MakeRequest(Request query)
    {
        _logger.Log($"making request to '{query.Location}'");

        Socket.SendEncrypted(query, _logger);
        var res = Socket.ReceiveEncrypted();
        _logger.Log($"received response '{res?.Location}' ({res?.Result})");
        return res;
    }
}
