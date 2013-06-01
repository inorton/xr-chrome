using System;
using NUnit.Framework;
using XR.Chrome;

namespace XR.Chrome.Tests
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void TestLaunch ()
		{
			var cl = new ChromeLauncher() {
				Address = new Uri("http://m.bbc.co.uk"),
			};

			cl.Run();

		}
	}
}

