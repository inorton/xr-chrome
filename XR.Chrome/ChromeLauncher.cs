using System;
using System.Diagnostics;

namespace XR.Chrome
{
	public class ChromeLauncher
	{
		public Uri Address { get; set; }

		public string AdditionalArgs { get; set ; }

		static string GetChromeExe ()
		{
			return "google-chrome";
		}

		public void Run ()
		{
			var args = String.Format ("--app='{0}'", Address);
			if ( !string.IsNullOrEmpty(AdditionalArgs) )
				args += " " + AdditionalArgs;

			var pi = new ProcessStartInfo() {
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = GetChromeExe(),
				Arguments =  args
			};
			var p = Process.Start( pi );

			p.WaitForExit();
		}
	}
}

