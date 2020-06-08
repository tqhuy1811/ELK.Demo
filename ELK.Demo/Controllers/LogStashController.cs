using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ELK.Demo.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class LogStashController : ControllerBase
	{

		private readonly ILogger<LogStashController> _logger;

		public LogStashController(ILogger<LogStashController> logger)
		{
			_logger = logger;
		}
		[HttpPost]
		public IActionResult Index([FromBody] Object data)
		{
			_logger.LogInformation(data.ToString());
			return Ok();
		}
	}
}