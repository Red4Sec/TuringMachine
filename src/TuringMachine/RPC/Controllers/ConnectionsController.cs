using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TuringMachine.Core.Fuzzers;
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

        [HttpGet("all")]
        public ActionResult<IEnumerable<FuzzerClientInfo>> GetAll()
        {
            return _server.Server.Connections.Values.Select(u => u.Source).ToArray();
        }

        [HttpGet("stat")]
        public ActionResult<FuzzerStat<FuzzerClientInfo>> GetStat(Guid id)
        {
            return _server.Server.Connections.Values.Where(u => u.Id == id).FirstOrDefault();
        }

        [HttpGet("stats")]
        public ActionResult<IEnumerable<FuzzerStat<FuzzerClientInfo>>> GetStats()
        {
            return _server.Server.Connections.Values.ToArray();
        }
    }
}
