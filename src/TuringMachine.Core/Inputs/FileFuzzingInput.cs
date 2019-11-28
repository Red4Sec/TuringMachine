using System;
using System.IO;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Inputs
{
	public class FileFuzzingInput : FuzzingInputBase, IEquatable<FileFuzzingInput>
	{
		private byte[] _Data;
		private FileInfo _Info;

		/// <summary>
		/// Use Cache
		/// </summary>
		public bool UseCache { get; set; }

		/// <summary>
		/// Name
		/// </FileName>
		public string FileName
		{
			get => _Info?.FullName;
			set { _Info = new FileInfo(value); }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public FileFuzzingInput() : base("File") { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filename">File</param>
		public FileFuzzingInput(string filename) : base("File")
		{
			FileName = filename;
			UseCache = true;
		}

		/// <summary>
		/// Get file content
		/// </summary>
		/// <returns>File content</returns>
		public override byte[] GetStream()
		{
			if (UseCache && _Data != null)
			{
				return _Data;
			}

			_Info.Refresh();
			_Data = File.ReadAllBytes(FileName);
			return _Data;
		}

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>Return true if are equals</returns>
		public bool Equals(FileFuzzingInput obj)
		{
			if (obj == null) return false;

			return base.Equals(obj)
				&& obj.FileName.EqualWithNullCheck(FileName)
				&& obj.UseCache.EqualWithNullCheck(UseCache);
		}

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>Return true if are equals</returns>
		public override bool Equals(object obj)
		{
			if (obj is FileFuzzingInput o)
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
			if (obj is FileFuzzingInput o)
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
			var hashCode = 179978476;
			hashCode = hashCode * -1521134295 + base.GetHashCode();
			hashCode = hashCode * -1521134295 + UseCache.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + FileName.GetHashCodeWithNullCheck();
			return hashCode;
		}
	}
}
