using System.Text;

namespace Docc.Common;

public class Encryption
{
				/*
				 * The smaller the salt, more characters will be
				 * changed, but the difference is smaller.
				 * 
				 * The larger the salt, the less the values will change.
				 * But the values that do will be a large change.
				 */

				public static int Salt { get; set; } = 16;

				// for testing right now.

				/*
				 * The plan is to use something really simple.
				 * Seen as this is planned for just testing things
				 * locally, there isn't much need for encryption.
				 * 
				 * However, I'll implement this just for that learning
				 * aspect too. Might aswell try.
				 */

				public static byte[] Encrypt(Request req)
				{
								var json = req.Serialize();
								var bytes = Encoding.Default.GetBytes(json);

								for (int i = 0; i < bytes.Length; i++)
        {
												if ((bytes[i] + Salt) > 127)
																continue;
												bytes[i] = (byte)(bytes[i] + Salt);
        }

								return bytes;
				}

				public static Request Decrypt(byte[] bytes)
    {
								for (int i = 0; i < bytes.Length; i++)
        {
												if (bytes[i] == '\0')
																continue;
												if (((bytes[i] + Salt)) > 127)
																continue;
												bytes[i] = (byte)(bytes[i] - Salt);
        }

								if (!Request.TryDeserialize(Encoding.Default.GetString(bytes), out Request? req))
								{
												return new RequestBuilder()
																.WithResult(RequestResult.BadPacket)
																.WithLocation(Request.DefaultLocation)
																.Build();
        }

								return req!;
    }
}
