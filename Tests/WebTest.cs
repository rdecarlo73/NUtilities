using System;
using NUtilities.Web;
using NUnit.Framework;

namespace NUtilities.Test
{
  [TestFixture]
  public class WebTest
  {
    [Test]
    public void GetMessageTest()
    {
      string result = String.Empty;
      WebHttpClient webClient = new WebHttpClient("http://twitter.com");
      result = webClient.GetMessage("/statuses/public_timeline.xml");
      Assert.IsNotEmpty(result);
    }
  }
}
