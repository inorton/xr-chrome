using System;
using System.Net;
using System.Web;
using NUnit.Framework;
using XR.Framework;

namespace XR.Framework.Tests
{
	public class XRContext
	{
		public string Now { get { return DateTime.Now.ToString ("s"); } }
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
		public void ServeStaticFiles ()
		{
			server.AddStaticPath (Helpers.OsPathJoin ("static", "wwwroot"));
			server.BeginListen ();

			var wc = new WebClient ();
			var index = wc.DownloadString ("http://localhost:4000/index.html");

			Assert.IsNotNull (index);

		}

		[Test]
		public void ServeXRIFiles ()
		{
			server.AddXRIPath<XRContext> ("templates", "/about.html");
			server.BeginListen ();
			var wc = new WebClient ();
			var xi = wc.DownloadString ("http://localhost:4000/about.html");
			Assert.IsNotNull (xi);
		}
	}
}

