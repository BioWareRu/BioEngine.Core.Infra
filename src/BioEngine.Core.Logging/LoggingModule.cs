using System;
using System.Text;
using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Core;
using Serilog.Events;

namespace BioEngine.Core.Logging
{
    public abstract class LoggingModule<T> : BaseBioEngineModule<T> where T : LoggingModuleConfig
    {
        private readonly LogLevelController _controller = new LogLevelController();

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            Console.OutputEncoding = Encoding.UTF8;
            base.ConfigureServices(services, configuration, environment);
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            var loggerConfiguration =
                new LoggerConfiguration().Enrich.FromLogContext();
            if (environment.IsDevelopment())
            {
                loggerConfiguration = ConfigureDev(loggerConfiguration);
                _controller.Switch.MinimumLevel = Config.DevLevel;
            }
            else
            {
                loggerConfiguration = ConfigureProd(loggerConfiguration, environment.ApplicationName);
                _controller.Switch.MinimumLevel = Config.ProdLevel;
            }

            loggerConfiguration.MinimumLevel.ControlledBy(_controller.Switch);
            Log.Logger = loggerConfiguration.CreateLogger();

            services.AddSingleton(_controller);
            services.AddSingleton(_ => (ILoggerFactory)new SerilogLoggerFactory());
        }

        protected virtual LoggerConfiguration ConfigureDev(LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration.WriteTo.Console();
        }

        protected abstract LoggerConfiguration ConfigureProd(LoggerConfiguration loggerConfiguration,
            string appName);
    }

    public abstract class LoggingModuleConfig
    {
        public LogEventLevel DevLevel { get; set; } = LogEventLevel.Debug;
        public LogEventLevel ProdLevel { get; set; } = LogEventLevel.Information;
    }

    public class LogLevelController
    {
        public LoggingLevelSwitch Switch { get; } = new LoggingLevelSwitch();
    }
}
