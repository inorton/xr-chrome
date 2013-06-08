using System;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using NUnit.Framework;
using XR.Framework;

namespace XR.Framework.Tests
{
	public class TestContext : RequestContextBase
	{
		public string Now { get { return DateTime.Now.ToString ("s"); } }
		public string Message { get { return "Moose!"; } }
	}

	[TestFixture]
	public class Backend
	{

		BackendServer server;

		[SetUp]
		public void Init ()
		{
			if (server != null) { 
				server.StopServer ();
			}
			server = new BackendServer () { Port = 4000 };
		}

		[Test]
		[Repeat(10)]
		public void ServeStaticFiles ()
		{
			server.AddStaticPath (Helpers.OsPathJoin ("static", "wwwroot"));
			server.BeginListen ();

			var wc = new WebClient ();
			var index = wc.DownloadString ("http://localhost:4000/index.html");

			Assert.IsNotNull (index);

		}

		[Test]
		[Repeat(10)]
		public void ServeXRIFiles ()
		{
			server.AddDynamicPath<TestContext> ("templates", "/about.html", new Regex ("^/about.html$"));
			server.BeginListen ();
			var wc = new WebClient ();
			var xi = wc.DownloadString ("http://localhost:4000/about.html");
			Assert.IsNotNull (xi);
			Assert.IsTrue (xi.Contains ("\"Moose!\""));
		}
	}
}

