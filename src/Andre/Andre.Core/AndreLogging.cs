using Microsoft.Extensions.Logging;

namespace Andre.Core
{
    public class AndreLogging
    {
        /// <summary>
        /// The logger factory used to create loggers for this library.
        /// Replaceable to allow integration with host application's logging system.
        /// </summary>
        public static ILoggerFactory LoggerFactory { get; set; }

        static AndreLogging()
        {
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });
        }
        
        public static ILogger For<T>() => LoggerFactory.CreateLogger<T>();
        public static ILogger For<T>(T _) => For<T>();
        public static ILogger For(Type t) => LoggerFactory.CreateLogger(t);
        
    }
}