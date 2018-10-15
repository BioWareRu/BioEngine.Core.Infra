using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using Serilog.Events;

namespace BioEngine.Core.Infra.Controllers
{
    [Route("logs")]
    [Authorize]
    public class LogsController : Controller
    {
        private readonly LoggingLevelSwitch _switch;

        public LogsController(LogLevelController levelController)
        {
            _switch = levelController.Switch;
        }

        private string SwitchLevel(LogEventLevel level)
        {
            var result = $"Current level: {_switch.MinimumLevel}. New level: {level}";
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