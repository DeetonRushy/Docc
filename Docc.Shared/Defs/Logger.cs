namespace Docc.Common;

public interface ILogger
{
    public string Target { get; }

    void Log(string message);
    void Log(string message, params object[] args);
}

public class ServerConsoleLogger : ILogger
{
    public string Target => "server";
    public static string Tab
        => new(' ', 5);

    public void Log(string message)
    {
        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} {Target}{Tab}{message}");
    }

    public void Log(string message, params object[] args)
    {
        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} {Target}\t{message}", args);

    }
}

public class ClientConsoleLogger : ILogger
{
    public string Target => "client";
    public static string Tab
        => new(' ', 5);

    public void Log(string message)
    {
        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} {Target}{Tab}{message}");
    }

    public void Log(string message, params object[] args)
    {
        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} {Target}\t{message}", args);

    }
}
