using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TuringMachine.Core.Fuzzers;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Logs;

namespace TuringMachine.RPC.Controllers
{
	[ApiController, Route("connections")]
	public class ConnectionsController : ControllerBase
	{
		/// <summary>
		/// Server
		/// </summary>
		private readonly RpcServer _server;

		/// <summary>
		/// Controller
		/// </summary>
		public ConnectionsController(RpcServer server)
		{
			_server = server;
		}

		[HttpGet("count")]
		public ActionResult<int> Count()
		{
			return _server.Server.Connections.Count;
		}

		[HttpGet("all")]
		public ActionResult<IEnumerable<FuzzerClientInfo>> GetAll(int index = -1, int count = -1)
		{
			return _server.Server.Connections.Values.Select(u => u.Source).ToArray(index, count);
		}

		[HttpGet("stat")]
		public ActionResult<FuzzerStat<FuzzerClientInfo>> GetStat(Guid id)
		{
			return _server.Server.Connections.Values.Where(u => u.Id == id).FirstOrDefault();
		}

		[HttpGet("stats")]
		public ActionResult<IEnumerable<FuzzerStat<FuzzerClientInfo>>> GetStats(int index = -1, int count = -1)
		{
			return _server.Server.Connections.Values.ToArray(index, count);
		}
	}
}
