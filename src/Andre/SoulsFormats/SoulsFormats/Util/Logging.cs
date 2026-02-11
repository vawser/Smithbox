using Microsoft.Extensions.Logging;

namespace SoulsFormats.Util
{
    public class Logging
    {
        /// <summary>
        /// The logger factory used to create loggers for this library.
        /// Replaceable to allow integration with host application's logging system.
        /// </summary>
        public static ILoggerFactory LoggerFactory { get; set; }

        static Logging()
        {
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });
        }
        
        static ILogger For<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}