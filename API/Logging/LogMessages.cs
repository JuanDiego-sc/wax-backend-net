namespace API.Logging;

public static partial class LogMessages
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "----- Sending query: {QueryName} {Query} -----")]
    public static partial void SendingQuery(this ILogger logger, string queryName, object query);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "----- Sending command: {CommandName} {Command} -----")]
    public static partial void SendingCommand(this ILogger logger, string commandName, object command);
}