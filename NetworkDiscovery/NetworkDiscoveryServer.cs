using System;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NetworkDiscovery
{
    /// <summary>
    /// NetworkDiscoveryServer class that receives data from the multicast group network
    /// IMPORTANT Windows firewall must be open on UDP port 5040
    /// </summary>
    public class NetworkDiscoveryServer : IDisposable
    {
        /// <summary>
        /// ListeningCond is a variable for enabling/disabling NetworkDiscoveryServer's listening     
        /// </summary>
        public delegate void EventCast(NetworkDiscoveryMessage cC);
        /// <summary>
        /// MessageReceived it is event which will be called by the delegate of the class subscribed to it   
        /// </summary>
        public event EventCast MessageReceived;
        private const Int32 DEFAULT_SERVICE_PORT = 5040;
        private Int32 _servicePort;
        private IPAddress _multicastGropuHost;
        private UdpClient _udpServer;
        private IPEndPoint _remoteEndPoint;
        //Dispose flag
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                //disposition all unconrolling resources
            }
            _disposed = true; //помечаем флаг что метод Dispose уже был вызван
        }

        private String GetCheckedValue(NameValueCollection values, String key, Dictionary<String, String> defaultConfigValues)
        {
            if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(values[key]))
            {
                return values[key];
            }
            else
            {
                if (!String.IsNullOrEmpty(key) && !values.AllKeys.Contains(key))
                {
                    String defValueOutput;
                    if (defaultConfigValues.TryGetValue(key, out defValueOutput))
                    {
                        Debug.WriteLine($"SERVER: Warning, key <{key}> is not defined in App.config and will be initialized with default value {defaultConfigValues[key]}");

                        return defValueOutput;
                    }
                    else
                    {
                        Debug.WriteLine($"SERVER: Error, specific key <{key}> is not defined in App.config");
                    }
                }
                return null;
            }
        }

        private void Init()
        {
            var appConfig = ConfigurationManager.GetSection("main") as NameValueCollection;
            var defaultConfigValues = new Dictionary<String, String>
                                        {
                                            {"Port", DEFAULT_SERVICE_PORT.ToString()},
                                            {"multicastGropuHost", "0.0.0.0"},
                                        };
            if (!Int32.TryParse(GetCheckedValue(appConfig, "Port", defaultConfigValues), out _servicePort))
                _servicePort = DEFAULT_SERVICE_PORT;

            IPAddress.TryParse(GetCheckedValue(appConfig, "multicastGropuHost", defaultConfigValues), out _multicastGropuHost);

            ///Уточнить IPAddress.Any или _multicastGropuHost
            ///_remoteEndPoint = new IPEndPoint(_multicastGropuHost, _servicePort);
            _remoteEndPoint = new IPEndPoint(IPAddress.Any, _servicePort);
            if (_remoteEndPoint == null)
                Debug.WriteLine($"SERVER: Error, failed on initialization <_endPoint>");

            _udpServer = new UdpClient(_remoteEndPoint);
            if (_udpServer == null)
                Debug.WriteLine($"SERVER: Error, failed on initialization <_udpServer>");

            _udpServer.JoinMulticastGroup(_multicastGropuHost);
        }

        private T XmlDeserializeFromBytes<T>(Byte[] data) where T: class, new()
        {
            if (data == null || data.Length == 0)
            {
                throw new InvalidOperationException();
            }

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

                using (XmlReader xmlReader = XmlReader.Create(memoryStream))
                {
                    return (T)xmlSerializer.Deserialize(xmlReader);
                }
            }
        }

        private async Task ReceiveData()
        {
            var datagramReceived = await _udpServer.ReceiveAsync();
            var receiveData = XmlDeserializeFromBytes<NetworkDiscoveryMessage>(datagramReceived.Buffer);
            MessageReceived(receiveData);
            //string message = Encoding.ASCII.GetString(datagramReceived.Buffer, 3, datagramReceived.Buffer.Length - 3);
            //Console.WriteLine($"SERVER: Received  message ({message}) from {_remoteEndPoint.Address} port {_remoteEndPoint.Port}");
        }

        /// <summary>
        /// Listening for activity on all network interfaces
        /// </summary>
        public void Listening()
        {
            Init();

            Task.Run(async () =>
            {
                while (!_disposed)
                {
                    await ReceiveData();
                    Debug.WriteLine("Server listening...");
                }
            });
        }
    }
}
