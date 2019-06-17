using System;
using Serilog;
using Serilog.Sinks.Graylog;

namespace BioEngine.Core.Logging.Graylog
{
    public class GraylogLoggingModule : LoggingModule<GraglogModuleConfig>
    {
        protected override void CheckConfig()
        {
            if (string.IsNullOrEmpty(Config.Host))
            {
                throw new ArgumentException("Graylog host is not set");
            }

            if (Config.Port == 0)
            {
                throw new ArgumentException("Graylog port is not set");
            }
        }

        protected override LoggerConfiguration ConfigureProd(LoggerConfiguration loggerConfiguration,
            string appName)
        {
            var facility = appName;
            if (!string.IsNullOrEmpty(Config.Facility))
            {
                facility = Config.Facility;
            }

            loggerConfiguration = loggerConfiguration
                .WriteTo.Graylog(new GraylogSinkOptions
                {
                    HostnameOrAddress = Config.Host, Port = Config.Port, Facility = facility
                });

            return loggerConfiguration;
        }
    }

    public class GraglogModuleConfig : LoggingModuleConfig
    {
        public string Facility { get; }
        public string Host { get; }
        public int Port { get; }

        public GraglogModuleConfig(string host, int port, string facility)
        {
            Host = host;
            Port = port;
            Facility = facility;
        }
    }
}
