using Microsoft.Extensions.Logging;

namespace Core;

public static class Logging
{
    private static ILoggerFactory? _factory;
    public static ILoggerFactory Factory => _factory ??= LoggerFactory.Create(builder =>
    {
        builder.SetMinimumLevel(LogLevel.Debug);
        builder.AddSimpleConsole();
        builder.AddDebug();
    });

    public static void SetFactory(ILoggerFactory factory) => _factory = factory;
}
