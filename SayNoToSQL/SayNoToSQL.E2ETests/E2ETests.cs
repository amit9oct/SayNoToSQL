using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace SayNoToSQL.E2ETests
{
    [TestClass]
    public class E2ETests
    {
        [TestMethod]
        public void TestStart()
        {
            using (var web = new WebClient())
            {
                string response = web.DownloadString(@"http://localhost:25575/");
                if(string.IsNullOrEmpty(response))
                {
                    throw new System.Exception("No Response Received");
                }
            }
        }

        [TestMethod]
        public void TestUpload()
        {
            using (var web = new WebClient())
            {
                string response = web.DownloadString(@"http://localhost:25575/FileUpload/");
                if (string.IsNullOrEmpty(response))
                {
                    throw new System.Exception("No Response Received");
                }
            }
        }
    }
}
