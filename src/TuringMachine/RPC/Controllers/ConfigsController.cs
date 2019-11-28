using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TuringMachine.Core.Interfaces;
using TuringMachine.Core.Logs;

namespace TuringMachine.RPC.Controllers
{
	[ApiController, Route("configs")]
	public class ConfigsController : ControllerBase
	{
		/// <summary>
		/// Server
		/// </summary>
		private readonly RpcServer _server;

		/// <summary>
		/// Controller
		/// </summary>
		public ConfigsController(RpcServer server)
		{
			_server = server;
		}

		[HttpPost("add")]
		public ActionResult<bool> Add([FromBody]FuzzingConfigBase entry)
		{
			_server.Server.Configurations.Add(entry.Id, new FuzzerStat<FuzzingConfigBase>(entry));
			_server.Server.UpdateConfigurations();
			return true;
		}

		[HttpDelete("remove")]
		public ActionResult<bool> Remove(Guid id)
		{
			if (_server.Server.Configurations.Remove(id))
			{
				_server.Server.UpdateConfigurations();
				return true;
			}
			return false;
		}

		[HttpGet("all")]
		public ActionResult<IEnumerable<FuzzingConfigBase>> GetAll()
		{
			return _server.Server.Configurations.Values.Select(u => u.Source).ToArray();
		}

		[HttpGet("stat")]
		public ActionResult<FuzzerStat<FuzzingConfigBase>> GetStat(Guid id)
		{
			return _server.Server.Configurations.Values.Where(u => u.Id == id).FirstOrDefault();
		}

		[HttpGet("stats")]
		public ActionResult<IEnumerable<FuzzerStat<FuzzingConfigBase>>> GetStats()
		{
			return _server.Server.Configurations.Values.ToArray();
		}
	}
}
