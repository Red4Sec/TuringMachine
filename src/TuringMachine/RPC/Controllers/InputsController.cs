using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;
using TuringMachine.Core.Logs;

namespace TuringMachine.RPC.Controllers
{
	[ApiController, Route("inputs")]
	public class InputsController : ControllerBase
	{
		/// <summary>
		/// Server
		/// </summary>
		private readonly RpcServer _server;

		/// <summary>
		/// Controller
		/// </summary>
		public InputsController(RpcServer server)
		{
			_server = server;
		}

		[HttpPost("add")]
		public ActionResult<bool> Add([FromBody]FuzzingInputBase entry)
		{
			_server.Server.Inputs.Add(entry.Id, new FuzzerStat<FuzzingInputBase>(entry));
			_server.Server.UpdateInputs();
			return true;
		}

		[HttpDelete("remove")]
		public ActionResult<bool> Remove(Guid id)
		{
			if (_server.Server.Inputs.Remove(id))
			{
				_server.Server.UpdateInputs();
				return true;
			}
			return false;
		}

		[HttpGet("count")]
		public ActionResult<int> Count()
		{
			return _server.Server.Inputs.Count;
		}

		[HttpGet("all")]
		public ActionResult<IEnumerable<FuzzingInputBase>> GetAll(int index = -1, int count = -1)
		{
			return _server.Server.Inputs.Values.Select(u => u.Source).ToArray(index, count);
		}

		[HttpGet("stat")]
		public ActionResult<FuzzerStat<FuzzingInputBase>> GetStat(Guid id)
		{
			return _server.Server.Inputs.Values.Where(u => u.Id == id).FirstOrDefault();
		}

		[HttpGet("stats")]
		public ActionResult<IEnumerable<FuzzerStat<FuzzingInputBase>>> GetStats(int index = -1, int count = -1)
		{
			return _server.Server.Inputs.Values.ToArray(index, count);
		}
	}
}
