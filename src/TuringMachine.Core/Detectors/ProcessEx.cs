using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace TuringMachine.Core.Detectors
{
	public class ProcessEx : IDisposable
	{
		private ManualResetEvent _exited;
		private Process _process;
		private volatile StringBuilder _output = new StringBuilder();

		/// <summary>
		/// Process Id
		/// </summary>
		public int ProcessId { get; }

		/// <summary>
		/// Exit time out
		/// </summary>
		public TimeSpan ExitTimeOut { get; set; } = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Is exited ?
		/// </summary>
		public bool HasExited
		{
			get => _exited.WaitOne() || (_process != null && _process.HasExited);
		}

		/// <summary>
		/// Exit Code
		/// </summary>
		public int ExitCode { get; private set; } = 0;

		/// <summary>
		/// Process output
		/// </summary>
		public string Output => _output.ToString();

		/// <summary>
		/// StartInfo
		/// </summary>
		internal protected ProcessStartInfoEx StartInfo { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pi">Start info</param>
		public ProcessEx(ProcessStartInfoEx pi)
		{
			StartInfo = pi;
			_exited = new ManualResetEvent(false);

			_process = new Process
			{
				StartInfo = pi.GetProcessStartInfo(),
				EnableRaisingEvents = true
			};
			_process.OutputDataReceived += OutputDataReceived;
			_process.ErrorDataReceived += OutputDataReceived;
			_process.Exited += OnProcess_Exited;

			_process.Start();
			ProcessId = _process.Id;

			if (pi.RedirectStandardError)
			{
				_process.BeginErrorReadLine();
			}
			if (pi.RedirectStandardOutput)
			{
				_process.BeginOutputReadLine();
			}
		}

		private void OnProcess_Exited(object sender, EventArgs e)
		{
			if (_process != null)
			{
				ExitCode = _process.ExitCode;
			}

			_exited?.Set();
		}

		private void OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Data))
			{
				_output.Append(e.Data);
			}
		}

		/// <summary>
		/// Wait for exit
		/// </summary>
		/// <param name="timeOut">TimeOut</param>
		/// <returns>True if is exited</returns>
		public bool WaitForExit(TimeSpan timeOut) => _exited.WaitOne(timeOut);

		/// <summary>
		/// Wait for exit
		/// </summary>
		/// <returns>True if is exited</returns>
		public bool WaitForExit() => _exited.WaitOne();

		/// <summary>
		/// Wait unit output
		/// </summary>
		/// <param name="cad">String</param>
		/// <param name="timeout">Timeouts</param>
		/// <returns>Return true if was found</returns>
		public bool WaitUntilOutput(string cad, TimeSpan timeout)
		{
			var time = DateTime.UtcNow;

			while
				(
				!_output.ToString().Contains(cad) &&
				(DateTime.UtcNow - time) < timeout
				)
			{
				Thread.Sleep(1);
			}

			return _output.ToString().Contains(cad);
		}

		/// <summary>
		/// Free resources
		/// </summary>
		public void Dispose()
		{
			if (_process != null)
			{
				_process.Dispose();
				_process = null;
			}

			if (_exited != null)
			{
				_exited.Dispose();
				_exited = null;
			}
		}

		/// <summary>
		/// Kill process
		/// </summary>
		public void KillProcess()
		{
			try { _process?.Kill(); } catch { }
		}
	}
}
