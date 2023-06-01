using System.Text;

namespace Speed.Engine.Logging;

public class ConsoleLogger : TextWriter, ILogger
{
    public override Encoding Encoding => Encoding.UTF8;

    private readonly StreamWriter ConsoleOut;

    public ConsoleLogger()
    {
        ConsoleOut = new StreamWriter(Console.OpenStandardOutput())
        {
            AutoFlush = true
        };

        Console.OutputEncoding = Encoding;
    }

    public void Error(string message)
    {
        throw new NotImplementedException();
    }

    public void Error(string message, Exception exception)
    {
        throw new NotImplementedException();
    }

    public void Fatal(string message)
    {
        throw new NotImplementedException();
    }

    public void Fatal(string message, Exception exception)
    {
        throw new NotImplementedException();
    }

    public void Info(string message)
    {
        WritePrefix("INFO", ConsoleColor.White, ConsoleColor.Cyan);
        ConsoleOut.WriteLine(message);
    }

    public void Warn(string message)
    {
        WritePrefix("WARN", ConsoleColor.White, ConsoleColor.DarkYellow);
        ConsoleOut.WriteLine(message);
    }

    public override void Write(string? value)
    {
        ConsoleOut.Write(value);
    }

    public override void WriteLine(string? value)
    {
        Info(value ?? string.Empty);
    }

    // ========================== Console Colors

    private ConsoleColor? ForegroundColor;
    private ConsoleColor? BackgroundColor;

    private void RememberColors()
    {
        ForegroundColor = Console.ForegroundColor;
        BackgroundColor = Console.BackgroundColor;
    }

    private void RestoreColors()
    {
        SetColors(ForegroundColor, BackgroundColor);
        ForegroundColor = null;
        BackgroundColor = null;
    }

    private static void SetColors(ConsoleColor? foreground, ConsoleColor? background)
    {
        if (foreground is not null) Console.ForegroundColor = (ConsoleColor)foreground;
        if (background is not null) Console.BackgroundColor = (ConsoleColor)background;
    }

    // ========================== Console Prefix

    private void WritePrefix(string type, ConsoleColor? foreground, ConsoleColor? background)
    {
        DateTime now = DateTime.Now;

        RememberColors();
        ConsoleOut.Write($"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}] [");
        SetColors(foreground, background);
        ConsoleOut.Write(type);
        RestoreColors();
        ConsoleOut.Write("]: ");
    }

    private void WritePrefixModule(string type, string module, ConsoleColor? foreground, ConsoleColor? background)
    {
        DateTime now = DateTime.Now;

        RememberColors();
        ConsoleOut.Write($"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}] [");
        SetColors(foreground, background);
        ConsoleOut.Write(type);
        RestoreColors();
        ConsoleOut.Write("] [" + module + "]: ");
    }
}
