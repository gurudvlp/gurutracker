
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Data;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace gurumod
{

	[XmlRoot("Multicaster")]
	public class Multicaster
	{
		public string MulticastAddress = "224.0.4.21";
		public int MulticastPort = 4200;
		public Socket ReceivingSocket;
		
		public Multicaster()
		{
			
		}
		
		public Multicaster(string ip, int port)
		{
			this.MulticastAddress = ip;
			this.MulticastPort = port;
			
			
		}
		
		public bool Connect()
		{
			ReceivingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPAddress ipp = IPAddress.Parse(this.MulticastAddress);
			this.ReceivingSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ipp));
			this.ReceivingSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
			this.ReceivingSocket.Connect(new IPEndPoint(IPAddress.Parse(this.MulticastAddress), this.MulticastPort));
			
			return true;
		}
	}
}
