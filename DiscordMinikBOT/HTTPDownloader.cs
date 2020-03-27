using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Threading;
using System.IO;

namespace DiscordMinikBOT
{
    public static class HTTPDownloader
    {
        public static void DownloadFile(string address, Action<string> downloadFinishedFunc, string dest = "")
        {
            Thread t = new Thread(() => _DownloadFile(address, downloadFinishedFunc, dest));
            t.Start();
        }

        static void _DownloadFile(string address, Action<string> finishFunc, string dest)
        {
            WebClient client = new WebClient();
            string data = client.DownloadString(address);
            if(dest.Length > 0)
            {
                File.WriteAllText(dest, data);
            }

            finishFunc(data);
        }
    }
}
