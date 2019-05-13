using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

namespace BioEngine.Core.Logging.Loki
{
    public class LokiLoggingModule : LoggingModule<LokiLoggingConfig>
    {
        protected override void CheckConfig()
        {
            if (string.IsNullOrEmpty(Config.Url))
            {
                throw new ArgumentException("Loki url is not set");
            }
        }

        protected override LoggerConfiguration ConfigureProd(LoggerConfiguration loggerConfiguration,
            string appName)
        {
            LokiCredentials credentials;
            if (!string.IsNullOrEmpty(Config.Login))
            {
                credentials = new BasicAuthCredentials(Config.Url, Config.Login, Config.Password);
            }
            else
            {
                credentials = new NoAuthCredentials(Config.Url);
            }

            loggerConfiguration =
                loggerConfiguration.WriteTo.LokiHttp(credentials, new LogLabelProvider(appName));
            return loggerConfiguration;
        }
    }

    public class LogLabelProvider : ILogLabelProvider
    {
        private readonly string _appName;


        public LogLabelProvider(string appName)
        {
            _appName = appName;
        }

        public IList<LokiLabel> GetLabels()
        {
            return new List<LokiLabel> {new LokiLabel("app", _appName)};
        }
    }

    public class LokiLoggingConfig : LoggingModuleConfig
    {
        public string Url { get; }
        public string Login { get; }
        public string Password { get; }

        public LokiLoggingConfig(string url, string login = "", string password = "")
        {
            Url = url;
            Login = login;
            Password = password;
        }
    }
}
