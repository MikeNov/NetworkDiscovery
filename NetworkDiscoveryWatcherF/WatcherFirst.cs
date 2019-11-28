using NetworkDiscovery;
using System.Threading;
using System;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace Execute
{
    public class WatcherFirst
    {
        static void Main(string[] args)
        {
            NetworkDiscoveryClient nDC = new NetworkDiscoveryClient();
            nDC.Init();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            var localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
            NetworkDiscoveryMessage nDM = new NetworkDiscoveryMessage("Tom", localIp, Int32.Parse("5040"));
            Int32 nDMIndex = 0;
            while (true)
            {
                nDM.Index = Interlocked.Increment(ref nDMIndex);
                nDC.Sending(nDM);
                Thread.Sleep(300);
            }
        }
    }
}
