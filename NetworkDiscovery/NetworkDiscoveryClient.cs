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
using System.Diagnostics;
using System.Threading.Tasks;

namespace NetworkDiscovery
{
    /// <summary>
    /// NetworkDiscoveryClient class that transmits data into the multicast group network
    /// </summary>
    public class NetworkDiscoveryClient : IDisposable
    {
        private const Int32 DEFAULT_SERVICE_PORT = 5040;
        private Int32 _servicePort;
        private IPAddress _multicastGropuHost;
        private UdpClient _udpClient;
        private IPEndPoint _endPoint;
        //Dispose flag
        private bool _disposed = false;
        /// <summary>
        /// NetworkDiscoveryClient dispose
        /// </summary>
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
            _disposed = true;
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
                        Debug.WriteLine($"CLIENT: Warning, key <{key}> is not defined in App.config and will be initialized with default value {defaultConfigValues[key]}");
                        return defValueOutput;
                    }
                    else
                    {
                        Debug.WriteLine($"CLIENT: Error, specific key <{key}> is not defined in App.config");
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// NetworkDiscoveryClient initialization
        /// </summary>
        public void Init()
        {
            var appConfig = ConfigurationManager.GetSection("main") as NameValueCollection;
            var defaultConfigValues = new Dictionary<String, String>
                                            {
                                                {"port", DEFAULT_SERVICE_PORT.ToString()},
                                                {"multicastGropuHost", "0.0.0.0"},
                                            };
            if (!Int32.TryParse(GetCheckedValue(appConfig, "port", defaultConfigValues), out _servicePort))
                _servicePort = DEFAULT_SERVICE_PORT;

            IPAddress.TryParse(GetCheckedValue(appConfig, "multicastGropuHost", defaultConfigValues), out _multicastGropuHost);

            _udpClient = new UdpClient();
            _udpClient.JoinMulticastGroup(_multicastGropuHost);

            _endPoint = new IPEndPoint(_multicastGropuHost, _servicePort);
            if (_endPoint == null)
                Debug.WriteLine($"CLIENT: Error, failed on initialization <_endPoint>");

        }

        private Byte[] XmlSerializeToByte<T>(T transmittedData)
        {
            if (transmittedData == null)
                return null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlSerializer formatter = new XmlSerializer(transmittedData.GetType());
                
                var settings = new XmlWriterSettings();

                settings.NewLineOnAttributes = true;
                settings.Indent = false;
                settings.OmitXmlDeclaration = true;

                var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.Position = 0;
                memoryStream.Flush();
                formatter.Serialize(XmlWriter.Create(memoryStream, settings), transmittedData, emptyNamespaces);
                memoryStream.Close();

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// NetworkDiscoveryClient sending data to MulticastGroup network
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void Sending<T>(T data)
        {
            Byte[] transmittedData = XmlSerializeToByte(data);

            if (transmittedData != null)
            {
                _udpClient.Send(transmittedData, transmittedData.Length, _endPoint);
            }
        }
    }
}
