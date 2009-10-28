
using System;
using NUnit.Framework;
using NUtilities.Web;

namespace NUtilities.Test
{

	[TestFixture()]
	public class WebHttpClientTest
	{

		[Test()]
		public void GetTest()
		{
			WebHttpClient client = new WebHttpClient(@"http://localhost/");
			string friendsString = client.GetMessage(@"");
			Assert.IsNotEmpty(friendsString);
		}
	}
}
