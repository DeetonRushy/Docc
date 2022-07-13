using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using Newtonsoft.Json;

namespace Docc.Common;

public static class StaticHelpers
{
    public static RequestResult NotFound()
        => RequestResult.ContentNotFound;
    public static RequestResult Okay()
        => RequestResult.OK;

    public static void Exit(int code)
        => Environment.Exit(code);
}

public static class SocketExtensions
{
    public static Request? Receive(this Socket socket)
    {
        SpinWait.SpinUntil(() => socket.Available != 0);

        byte[] buffer = new byte[1024];
        try
        {
            socket.Receive(buffer);
        }
        catch(SocketException socketException)
        {
            return new RequestBuilder()
                .WithLocation("/internal_error")
                .AddContent(socketException.Message)
                .WithResult(RequestResult.SockedDied)
                .Build();
        }

        var contents = Encoding.Default.GetString(buffer);

        if (!Request.TryDeserialize(contents, out Request? result))
        {
            return null;
        }

        return result;
    }
    public static async Task<Request?> ReceiveAsync(this Socket socket)
    {
        byte[] buffer = new byte[1024];
        _ = await socket.ReceiveAsync(buffer, SocketFlags.None);

        var contents = Encoding.Default.GetString(buffer);

        if (!Request.TryDeserialize(contents, out Request? result))
        {
            return null;
        }

        return result;
    }

    public static void SendRequest(this Socket sock, Request req, ILogger? logger = null)
    {
        var converted = JsonConvert.SerializeObject(req);
        var bytes = Encoding.Default.GetBytes(converted);

        try
        {
            sock.Send(bytes);
        }
        catch (SocketException ex)
        {
            logger?.Log($"failed to send request. ({ex.Message})");
        }
    }

    public static void SendEncrypted(this Socket sock, Request req, ILogger? logger = null)
    {
        SendRequest(sock, req, logger);
    }

    public static Request? ReceiveEncrypted(this Socket sock, ILogger? logger = null)
    {
        return Receive(sock);
    }
}
