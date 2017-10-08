using System.Net;
using System.Text;

namespace TimusLib
{
    public static class WebHelper
    {
        public static string DownloadPage(string adress)
        {
            var client = new WebClient {Encoding = Encoding.UTF8};
            return client.DownloadString(adress);
        }
    }
}