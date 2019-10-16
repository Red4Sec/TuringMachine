using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Inputs
{
    public class TcpQueryFuzzingInput : FuzzingInputBase, IEquatable<TcpQueryFuzzingInput>
    {
        /// <summary>
        /// EndPoint
        /// </summary>
        public IPEndPoint EndPoint { get; set; }

        /// <summary>
        /// Request
        /// </summary>
        public byte[] Request { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TcpQueryFuzzingInput() : base("TcpRequest") { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="endPoint">EndPoint</param>
        /// <param name="request">Request</param>
        public TcpQueryFuzzingInput(IPEndPoint endPoint, byte[] request) : base("Tcp Query")
        {
            EndPoint = endPoint;
            Request = request;
        }

        /// <summary>
        /// Get Tcp output
        /// </summary>
        /// <returns>Tcp output</returns>
        public override byte[] GetStream()
        {
            var tcp = new TcpClient();
            tcp.Connect(EndPoint);

            var ret = tcp.GetStream();

            if (Request != null)
            {
                ret.Write(Request, 0, Request.Length);
            }

            using (var ms = new MemoryStream())
            {
                ret.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public bool Equals(TcpQueryFuzzingInput obj)
        {
            if (obj == null) return false;

            return base.Equals(obj)
                && obj.EndPoint.Equals(EndPoint)
                && ((obj.Request == null && Request == null) || Request.SequenceEqual(obj.Request));
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override bool Equals(object obj)
        {
            if (obj is TcpQueryFuzzingInput o)
            {
                return Equals(o);
            }

            return false;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override bool Equals(FuzzingInputBase obj)
        {
            if (obj is TcpQueryFuzzingInput o)
            {
                return Equals(o);
            }

            return false;
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = -1289872652;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EndPoint.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Request.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
