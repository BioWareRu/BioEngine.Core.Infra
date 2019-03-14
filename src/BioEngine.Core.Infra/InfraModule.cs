using System;
using System.Text;
using BioEngine.Core.Modules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Graylog;

namespace BioEngine.Core.Infra
{
    public class InfraModule : BioEngineModule<InfraModuleConfig>
    {
        private readonly LogLevelController _controller = new LogLevelController();

        public override void ConfigureHostBuilder(IWebHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddHttpContextAccessor();
                services.AddMemoryCache();
            });
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment environment)
        {
            Console.OutputEncoding = Encoding.UTF8;
            base.ConfigureServices(services, configuration, environment);

            var isGraylogDisabled =
                !string.IsNullOrEmpty(configuration["BRC_GRAYLOG_DISABLED"]) &&
                configuration["BRC_GRAYLOG_DISABLED"] == "true";
            var loggerConfiguration =
                new LoggerConfiguration().Enrich.FromLogContext();
            if (environment.IsDevelopment() || isGraylogDisabled)
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Console();
                _controller.Switch.MinimumLevel = Config.DevLevel;
            }
            else
            {
                var facility = environment.ApplicationName;
                if (!string.IsNullOrEmpty(configuration["BRC_GRAYLOG_FACILITY"]))
                {
                    facility = configuration["BRC_GRAYLOG_FACILITY"];
                }

                loggerConfiguration = loggerConfiguration
                    .WriteTo.Graylog(new GraylogSinkOptions
                    {
                        HostnameOrAddress = configuration["BRC_GRAYLOG_HOST"],
                        Port = int.Parse(configuration["BRC_GRAYLOG_PORT"]),
                        Facility = facility
                    });
                _controller.Switch.MinimumLevel = Config.ProdLevel;
            }
            loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);

            loggerConfiguration.MinimumLevel.ControlledBy(_controller.Switch);
            Log.Logger = loggerConfiguration.CreateLogger();

            services.AddSingleton(_controller);
            services.AddMvc().AddApplicationPart(typeof(WebHostBuilderExtensions).Assembly);
            services.AddSingleton(_ => (ILoggerFactory)new SerilogLoggerFactory());
        }
    }

    public class InfraModuleConfig
    {
        public LogEventLevel DevLevel { get; set; } = LogEventLevel.Debug;
        public LogEventLevel ProdLevel { get; set; } = LogEventLevel.Error;
    }

    public class LogLevelController
    {
        public LoggingLevelSwitch Switch { get; } = new LoggingLevelSwitch();
    }
}
