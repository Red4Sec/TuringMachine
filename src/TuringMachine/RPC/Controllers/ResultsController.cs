using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Logs;

namespace TuringMachine.RPC.Controllers
{
	[ApiController, Route("results")]
	public class ResultsController : ControllerBase
	{
		/// <summary>
		/// Server
		/// </summary>
		private readonly RpcServer _server;

		/// <summary>
		/// Controller
		/// </summary>
		public ResultsController(RpcServer server)
		{
			_server = server;
		}

		[HttpGet("count")]
		public ActionResult<int> Count()
		{
			return _server.Server.Logs.Count;
		}

		[HttpGet("all")]
		public ActionResult<IEnumerable<FuzzerLog>> GetAll(int index = -1, int count = -1)
		{
			return _server.Server.Logs.Values.ToArray(index, count);
		}
	}
}
