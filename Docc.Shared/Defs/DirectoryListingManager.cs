using System.Net.Sockets;

namespace Docc.Common;

/*
 * 
 */

public delegate Request ContentCallback(Dictionary<string, string> args, SharedSenderInfo info);

public record DirectoryListing(string Location, ContentCallback Responder);

public class DirectoryListingManager
{
    protected List<DirectoryListing> ActiveListings { get; set; }
        = new List<DirectoryListing>();

    public void MapGet(string Location, ContentCallback callback)
    {
        ActiveListings.Add(new DirectoryListing(Location, callback));
    }
    public void MapSet(string Location, ContentCallback callback)
    {
        ActiveListings.Add(new DirectoryListing(Location, callback));
    }

    /// <summary>
    /// automatically serves <paramref name="info"/> to the specified <paramref name="location"/>
    /// </summary>
    /// <param name="location">The mapped callback to search for</param>
    /// <param name="info">The sender information, the person to serve <paramref name="location"/> to.</param>
    public void CallMappedOnline(string location, Dictionary<string, string> args, SharedSenderInfo info)
    {
        if (!ActiveListings.Any(ActiveListings.Contains))
        {
            info.Serve(new Request { Location = "/404" });
            return;
        }

        var response = ActiveListings
            .Where(x => x.Location == location)
            .First()
            .Responder
            .Invoke(args, info);

        info.Serve(response);
    }

    public Request CallMappedLocal(string location, Dictionary<string, string> args)
    {
        if (!ActiveListings.Any(x => x.Location == location))
        {
            return new Request { Location = "/404" };
        }

        var response = ActiveListings
            .Where(x => x.Location == location)
            .First()
            .Responder
            .Invoke(args, null!);

        return response;
    }
}
