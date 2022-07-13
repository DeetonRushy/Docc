
namespace Docc.Common;

/*
 * This isn't the reponse.
 * It's an answer to: is there a reponse?
 * 
 * It should also answer: Why isn't there a reponse?
 */

public enum RequestResult : ushort
{
    /*
     * This signifies that the request returned something and succeeded.
     * It must be zero due to the fact that in json form, it makes it
     * easier to use 'if (Json.Result != 0) { Handle() }' patterns.
     */
    OK = 0,

    /*
     * Anything beyond this point must be broad
     * descriptions on why a request failed.
     * 
     * Do not add anything for no reason, only add new results
     * when they are 100% required. Meaning no other result already
     * explains it.
     */


    /*
     * Used as the default within Request.
     * If this result is ever received it means the request wasn't initialized
     * correctly.
     */
    GenericError = 100,

    /*
     * Used whenever a packet is malformed.
     */
    BadPacket = 101,

    /*
     * Occurs when the primary socket dies.
     */
    SockedDied = 102,

    /*
     * Occurs when the receiver cannot find a required file.
     */
    FileNotFound = 201,

    /*
     * Just a 404 error. This tells the receiver the specified content
     * was not found.
     */
    ContentNotFound = 404
}

public class Translation
{
    private static readonly Dictionary
        <RequestResult, string>
        CommonTranslations = new()
        {
            // NOTE: Ok is a special case, due to the fact that the result 'OK'
            // should not be shown to the user. The content should.
            { RequestResult.OK, string.Empty },
            { RequestResult.GenericError, "There was an internal error." },
            { RequestResult.ContentNotFound, "The specified endpoint could not be found." },
            { RequestResult.SockedDied, "The remote applicant disconnected." },
            { RequestResult.FileNotFound, "The remote applicant failed to find a required file."},
            { RequestResult.BadPacket, "One or more packets were malformed or invalid." }
        };

    public RequestResult Original { get; init; }
    public string Conversion { get; init; } = string.Empty;

    public static Translation From(RequestResult res)
    {
        if (!CommonTranslations.TryGetValue(res, out string? trans))
        {
            throw new NotImplementedException($"RequestResult::{res}: this translation has not been implemented in Docc.Common.Translation.CommonTranslations");
        }

        return new Translation { Original = res, Conversion = trans! };
    }
}