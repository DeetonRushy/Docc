using Newtonsoft.Json;

namespace Docc.Common;

/*
 *  Docc.Common.Request
 *  
 *  This class represents a network request/packet.
 */

[Serializable]
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Request
{
    // (example-client): /users/create
    // (example-server): /client?key=1
    [JsonProperty(PropertyName = "location")]
    public string Location { get; set; } = string.Empty;

    // The arguments following the request
    // example: /users/create?apikey="10292023"
    [JsonProperty(PropertyName = "arguments")]
    public Dictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();

    // The result provided by the sender.
    [JsonProperty(PropertyName = "result")]
    public RequestResult Result { get; set; } = RequestResult.GenericError;

    // The application name the sender is. Ex: "Docc v2"
    [JsonProperty(PropertyName = "user-agent")]
    public string UserAgent { get; set; } = "Unknown";

    [JsonProperty(PropertyName = "content")]
    public List<string> Content { get; set; } = new();

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    public static bool TryDeserialize(string content, out Request? req)
    {
        Request? jsonResult = null;

        try
        {
            jsonResult = JsonConvert.DeserializeObject<Request>(content);
        }
        catch (JsonException)
        {
            req = null;
            return false;
        }

        req = jsonResult;
        return true;
    }

    public override string ToString()
    {
        return Serialize();
    }

    public static string DefaultLocation
        => "/";

    public static Request Default
        => new RequestBuilder()
            .WithLocation("/")
            .WithResult(RequestResult.BadPacket)
            .Build();
}

// Just makes things look easier on the eyes when making a client/server.
//
/*
 * RequestBuilder rb = new()
 *      .WithLocation("/api/v3/version")
 *      .WithArguments(new() { {"userid", "232"} })
 *      .WithResult(RequestResult.OK)
 *      .WithAgent(UserAgent)
 *      .WithContent($"v{Environment.GetEnvironmentVariable("version")}");
 *      
 * var request = rb.Build();
 */

public class RequestBuilder
{
    private string _location;
    private Dictionary<string, string> _arguments = new();
    private RequestResult _result;
    private string _userAgent = Environment.GetEnvironmentVariable("App-Agent")!;
    private List<string> _content = new();

    public Request Build()
    {
        if (_content is null)
            _content = new List<string>();
        if (_userAgent is null)
            _userAgent = "unknown";
        if (_arguments is null)
            _arguments = new();
        if (_location is null)
            throw new ArgumentException("attempted to build a request with no location.");

        return new()
        {
            Arguments = _arguments,
            Location = _location,
            UserAgent = _userAgent,
            Result = _result,
            Content = _content
        };
    }

    public RequestBuilder WithLocation  (string location)
    {
        _location = location;
        return this;
    }
    public RequestBuilder WithArguments (Dictionary<string, string> arguments)
    {
        _arguments = arguments;
        return this;
    }
    public RequestBuilder WithArgument  (string Key, string Value)
    {
        _arguments.TryAdd(Key, Value);
        return this;
    }
    public RequestBuilder WithResult    (RequestResult result)
    {
        this._result = result;
        return this;
    }
    public RequestBuilder WithAgent     (string agent)
    {
        this._userAgent = agent;
        return this;
    }
    public RequestBuilder WithContent   (List<string> contents)
    {
        _content = contents;
        return this;
    }
    public RequestBuilder WithContent   (string contents, char delim)
    {
        _content = contents.Split(delim).ToList();
        return this;
    }
    public RequestBuilder AddContent    (string contents)
    {
        _content.Add(contents);
        return this;
    }
}
