using System.Net;
using System.Net.Sockets;

using Newtonsoft.Json;

using Docc.Common;

namespace Docc.Server;

/*
 * TODO:
 * 
 * Make the encrypted communications not block the client
 * from connecting.
 */

internal class ServerConnection
{
    public readonly ILogger _logger;

    public string Version { get; }
    public string AppName { get; }

    public Dictionary<ServerClient, Socket> Connections { get; }

    protected Socket Socket { get; }
    protected Thread Listener { get; }
    protected Thread MessageAcceptor { get; }
    protected Thread Validator { get; }
    protected bool Running { get; } = true;

    public IPAddress Address { get; }
    public IPEndPoint Endpoint { get; }                                     // for now
    public IPHostEntry Host { get; set; } = Dns.GetHostEntry("localhost");

    public ServerConnection()
    {
        _logger = new ServerConsoleLogger();

        // start the server on ServerSocket.

        Version = Environment.GetEnvironmentVariable("App-Version") ?? "(no version)";
        AppName = Environment.GetEnvironmentVariable("App-Agent") ?? "Docc";

        Connections = new Dictionary<ServerClient, Socket>();

        // TODO: replace all console.writeline's with 
        // a better logging system.
        _logger.Log($"starting server. ({AppName})");

        Address = Host.AddressList[0];
        Endpoint = new IPEndPoint(Address, 25755);
        Socket = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket.Bind(Endpoint);
        Socket.Listen(10);

        Listener = new(() =>
        {
            _logger.Log("began accepting clients...");

            while (Running)
            {
                var connection = Socket.Accept();

                // TODO: authenticate the user

                Request? firstPacket = connection.ReceiveEncrypted(_logger);
                SharedClient? info;

                try
                {
                    var contents = firstPacket?.Content.FirstOrDefault();

                    if (contents is null)
                    {
                        // client didn't send the first packet, which we expected.
                        // chances are it's not our client or they're lagging.
                        _logger?.Log($"client contents were null.");

                        var rejection = new RequestBuilder()
                            .WithLocation(Request.DefaultLocation)
                            .WithResult(RequestResult.BadPacket)
                            .AddContent("Request data was null.");

                        connection.SendEncrypted(rejection.Build());

                        connection.Close();
                        continue;
                    }

                    info = JsonConvert.DeserializeObject<SharedClient>(contents);
                }
                catch (JsonException)
                {
                    // we failed to deserialize the first packet. Just reject the
                    // connection.
                    
                    // sending random stuff instantly grants you a fuck off
                    _logger.Log("failed to deserialize request contents.");

                    var rb = new RequestBuilder()
                        .WithLocation(Request.DefaultLocation)
                        .WithResult(RequestResult.BadPacket)
                        .AddContent("failed to authenticate.");

                    connection.SendEncrypted(rb.Build());
                    connection.Close();
                    continue;
                }

                if (info is not SharedClient user)
                {
                    _logger.Log("client sent invalid ServerClient object");
                    connection.Close();
                    continue;
                }

                Connections.Add(ServerClient.From(user), connection);

                _logger.Log($"accepted client '{user.Name}'");

                var accepted = new RequestBuilder()
                    .WithLocation("/")
                    .AddContent($"{user.Id}")
                    .WithResult(RequestResult.OK)
                    .Build();

                connection.SendEncrypted(accepted);
            }
        });
        MessageAcceptor = new(() =>
        {
            while (Running)
            {
                SpinWait.SpinUntil(() => Connections.Any(x => x.Value.Available > 0));
                var clientsWhoSentMessages = Connections.Where(x => x.Value.Available > 0);
                _logger.Log($"accepting {clientsWhoSentMessages.Count()} requests...");

                List<(Request?, Socket)> sentMessages = new();

                foreach (var client in clientsWhoSentMessages)
                {
                    var message = client.Value.ReceiveEncrypted();
                    sentMessages.Add((message, client.Value));
                }

                foreach (var sentMessage in sentMessages)
                {
                    OnMessage(sentMessage.Item1 ?? Request.Default, sentMessage.Item2);
                }
            }
        });
        Validator = new(() =>
        {
            while (true)
            {
                ValidateUsers();
                // not ideal, but does its job for the time being.
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        });

        Listener.Start();
        MessageAcceptor.Start();
        Validator.Start();
    }

    public Action<Request, Socket> OnMessage { get; set; }
        = delegate (Request req, Socket client)
        {

        };

    // iterate all users & remove any who's connection
    // has dropped.
    private void ValidateUsers()
    {
        List<Socket> schedule = new();

        foreach (var (user, conn) in Connections)
        {
            if (!conn.Connected)
            {
                schedule.Add(conn);
                _logger.Log($"client '{user.Name}' has disconnected.");
            }
        }

        schedule.ForEach(conn => conn.Close());
    }
}
