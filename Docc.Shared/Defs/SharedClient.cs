using System.Net.Sockets;
using System.Text;

using Newtonsoft.Json;

namespace Docc.Common;

[Serializable]
public class SharedClient
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; init; } = string.Empty;

    [JsonProperty(PropertyName = "uuid")]
    public Guid Id { get; init; }

}

public class SharedSenderInfo
{
    public SharedClient Sender { get; init; }
    public Socket? Socket { get; init; }

    public void Serve(Request content)
    {
        Socket.Send(Encoding.Default.GetBytes(content.Serialize()));
        // do not care about a response here, will be handled elsewhere.
    }
}
