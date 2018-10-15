using System;
using System.Text;
using BioEngine.Core.Modules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core;

namespace BioEngine.Core.Infra
{
    public class InfraModule : BioEngineModule<InfraModuleConfig>
    {
        public override void ConfigureHostBuilder(IWebHostBuilder hostBuilder)
        {
            AddLogging(hostBuilder);
        }

        private void AddLogging(IWebHostBuilder hostBuilder)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var controller = new LogLevelController();
            hostBuilder.ConfigureServices((context, services) =>
            {
                var isGraylogDisabled =
                    !string.IsNullOrEmpty(context.Configuration["BRC_GRAYLOG_DISABLED"]) &&
                    context.Configuration["BRC_GRAYLOG_DISABLED"] == "true";
                var loggerConfiguration =
                    new LoggerConfiguration().Enrich.FromLogContext();
                if (context.HostingEnvironment.IsDevelopment() || isGraylogDisabled)
                {
                    loggerConfiguration = loggerConfiguration
                        .WriteTo.LiterateConsole();
                    controller.Switch.MinimumLevel = Config.DevLevel;
                }
                else
                {
                    var facility = context.HostingEnvironment.ApplicationName;
                    if (!string.IsNullOrEmpty(context.Configuration["BRC_GRAYLOG_FACILITY"]))
                    {
                        facility = context.Configuration["BRC_GRAYLOG_FACILITY"];
                    }

                    loggerConfiguration = loggerConfiguration
                        .WriteTo.Graylog(new GraylogSinkOptions
                        {
                            HostnameOrAddress = context.Configuration["BRC_GRAYLOG_HOST"],
                            Port = int.Parse(context.Configuration["BRC_GRAYLOG_PORT"]),
                            Facility = facility
                        });
                    controller.Switch.MinimumLevel = Config.ProdLevel;
                }

                loggerConfiguration.MinimumLevel.ControlledBy(controller.Switch);
                Log.Logger = loggerConfiguration.CreateLogger();
            });
            hostBuilder.UseSerilog();
            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(controller);
                services.AddMvc().AddApplicationPart(typeof(WebHostBuilderExtensions).Assembly);
            });
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