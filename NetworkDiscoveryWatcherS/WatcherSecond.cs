using NetworkDiscovery;
using System.Threading;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Execute
{
    public class WatcherSecond
    {
        static void Main(string[] args)
        {
            ///Server
            NetworkDiscoveryServer nDS = new NetworkDiscoveryServer();
            nDS.MessageReceived += NDS_MessageReceived;
            nDS.Listening();

            Console.ReadKey();
        }

        private static void NDS_MessageReceived(NetworkDiscoveryMessage nDM)
        {
            if (nDM != null)
            {
                Console.WriteLine($"NOTIFIER: <server> message received {nDM}, {nDM.Index}, {nDM.Name}, {nDM.Ip}, {nDM.Port}");
            }
        }
    }
}
