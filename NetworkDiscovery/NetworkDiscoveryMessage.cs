using System;
using System.Net;
using System.Threading;
using System.Xml.Serialization;

namespace NetworkDiscovery
{
    /// <summary>
    /// CommonClass is a test data for transmitting between client and server
    /// </summary>
    [Serializable, XmlRoot("CommonClass", Namespace = "")]
    public class NetworkDiscoveryMessage
    {
        [XmlElement(ElementName = "Name")]
        public String Name { get; set; }

        [XmlElement(ElementName = "Ip")]
        public String Ip { get; set; }

        [XmlElement(ElementName = "Port")]
        public Int32 Port { get; set; }

        [XmlElement(ElementName = "Index")]
        public Int32 Index { get; set; }

        public NetworkDiscoveryMessage()
        {
        }

        public NetworkDiscoveryMessage(String name, String localIp, Int32 port) : this()
        {
            Name = name;
            Ip = localIp;
            Port = port;
        }
    }
}
