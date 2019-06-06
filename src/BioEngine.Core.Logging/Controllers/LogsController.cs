using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using Serilog.Events;

namespace BioEngine.Core.Logging.Controllers
{
    [Route("logs")]
    [Authorize("logs")]
    public class LogsController : Controller
    {
        private readonly LoggingLevelSwitch _switch;

        public LogsController(LogLevelController levelController)
        {
            _switch = levelController.Switch;
        }

        private string SwitchLevel(LogEventLevel level)
        {
            var result = $"Current level: {_switch.MinimumLevel.ToString()}. New level: {level.ToString()}";
            _switch.MinimumLevel = level;
            return result;
        }

        [HttpGet("debug")]
        public IActionResult Debug()
        {
            return Ok(SwitchLevel(LogEventLevel.Debug));
        }

        [HttpGet("info")]
        public IActionResult Info()
        {
            return Ok(SwitchLevel(LogEventLevel.Information));
        }

        [HttpGet("error")]
        public IActionResult Error()
        {
            return Ok(SwitchLevel(LogEventLevel.Error));
        }
    }
}
