namespace Speed.Engine.Logging;

internal interface ILogger
{
    public void Info(string message);
    public void Warn(string message);
    public void Error(string message);
    public void Error(string message, Exception exception);
    public void Fatal(string message);
    public void Fatal(string message, Exception exception);
}
