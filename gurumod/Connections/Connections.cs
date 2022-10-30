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

using gurumod.Logging;

namespace gurumod
{
	public class IncomingConnections
	{

		public string Protocol = "";
		public Interface TheInterface;
		
		bool isactive = false;
		
	  	byte[] message = new byte[4096];
	  	int bytesRead;
		string currentmsgin;

		private bool _clientStillConnected = true;
		public bool clientStillConnected 
		{
			get { return _clientStillConnected; }
			set { _clientStillConnected = value; }
		}

				
		//bool alreadypinged = false;
		long pingtimeforwebsite = 0;		
	
		int threadloops = 0;
		long lastmessagetime = Engine.TimeStamp();

		private int _keepAliveTime = 30;
		public int keepAliveTime 
		{
			get 
			{
				if(TheInterface != null) { return TheInterface.keepAliveTime; }
				return _keepAliveTime; 
			}

			set
			{
				if(TheInterface != null) { TheInterface.keepAliveTime = value; }
				else { _keepAliveTime = value; }
			}
		}
		
		int turnssincemessage = 0;
		
		int connectionid = -1;
		
		TcpClient tcpClient;
		NetworkStream clientStream;
		
		public string RemoteIP
		{
			get
			{
				if(tcpClient == null) { return ""; }
				return tcpClient.Client.RemoteEndPoint.ToString();
			}
		}
		
		static ASCIIEncoding encoder = new ASCIIEncoding();
		
		public IncomingConnections(int tconnectionid)
		{
			connectionid = tconnectionid;
		}
		
		//
		//	AddIncomingConnection will assign an incoming connection to an open
		//	connection object.  It will also tell that object what
		//	protocol is being represented so that it knows what interface
		//	to assign
		//
		public static void AddIncomingConnection(object client, string protocol)
		{
			for(int econnection = 0; econnection < Engine.MaxIncomingConnections; econnection++)
			{
				try
				{
					if(Engine.Connections[econnection].IsActive() == false)
					{
						//	This connection is open, so we should sign it up for the new
						//	connection being made to the btEngine
						
						if(Engine.Connections == null)
						{
							Log.lWarning("Engine connection list is null.", "Connections", "AddIncomingConnection");
						}
						
						if(Engine.Connections != null
						&& Engine.Connections[econnection] == null)
						{
							Log.lWarning("Connection object is null: " + econnection.ToString(), "Connections", "AddIncomingConnection");
						}

						Engine.Connections[econnection].Claim(client, protocol);
						return;
					}
				}
				catch(Exception ex)
				{
					Log.lWarning("Exception adding new connection.", "Connections", "AddIncomingConnection");
					Log.lWarning(ex.Message, "Connections", "AddIncomingConnection");
				}
			}
			
			//	No open connections were found.
			Log.lWarning("No open connection spots available.", "Connections", "AddIncomingConnection");
		}
		
		//	This method is called when a client attempts to connect,
		//	and the AddIncomingConnection thing verified there was a
		//	slot left.
		public void Claim(object client, string protocol)
		{
			tcpClient = new TcpClient();
			tcpClient = (TcpClient)client;
	 		clientStream = tcpClient.GetStream();
			
			isactive = true;
			
			clientStillConnected = true;
			lastmessagetime = Engine.TimeStamp();
			
			//	The protocol specifies what interface we need to attach to this
			//	connection object.  The interface takes care of the actual data
			//	parsing and manipulation
			this.Protocol = protocol.ToUpper();

			if(protocol.ToUpper() == "GTDBG") { TheInterface = new DebugInterface(); }
			else { TheInterface = new WebInterface(); }

			TheInterface.RemoteIP = this.RemoteIP;
		}
		
		public bool IsActive()
		{
			return isactive;
		}
		
		public bool Close()
		{
			
			tcpClient.Close();
			tcpClient = null;
			clientStream = null;
			isactive = false;
			turnssincemessage = 0;
			
			clientStillConnected = false;

			
			return true;
		}
		
		//	This void performs this connections turn of events.
		//	Originally this would have (...found this comment this way...)
		public void TakeTurn()
		{
			//	Immediately return if this connection isn't active.
			if(IsActive() == false) { return; }
			
			//	Immediately return if there is not an interface defined.
			if(TheInterface == null) { return; }
			
			//	Now this needs to check if there are any commands or anything.
			bytesRead = 0;
			threadloops++;
			turnssincemessage++;
			
			//	Close this connection if there has not been any activity in
			//	the allotted time frame.
			if(Engine.TimeStamp() - lastmessagetime > keepAliveTime)
			{
				Close();
				return;
			}
			
			//	This needs to receive any data that is incoming, and then send it to the
			//	interface to take care of it.  Afterwards, it needs to check any outgoing
			//	data from the interface and send it out.
			if(clientStream.DataAvailable == true)
			{
				threadloops = 0;
				
				try
				{
					bytesRead = clientStream.Read(message, 0, 4096);
					pingtimeforwebsite = Engine.TimeStamp();
					lastmessagetime = Engine.TimeStamp();						
				}
				catch(Exception ex)
				{
					//a socket error has occured
					Log.lWarning("Exception during socket read.", "Connections", "TakeTurn");
					Log.lWarning(ex.Message, "Connections", "TakeTurn");
					
					Close();
					return;
				}
	
				//	If no bytes were read that generally means that the client
				//	has disconnected from our server.
				if (bytesRead == 0)
				{
					Close();
					return;
				}
				
				//alreadypinged = false;
				
				currentmsgin = encoder.GetString(message, 0, bytesRead);

				TheInterface.ReceivedData(currentmsgin);
				turnssincemessage = 0;
			}
			
			TheInterface.TakeTurn();
			
			//	If the interface for this connection is using ascii output, and
			//	if there is data to send, then we need to send that data to
			//	the client.
			if(TheInterface.UseAsciiOutput && TheInterface.OutgoingBuffer != "")
			{
				try
				{
					clientStream.Write(
						encoder.GetBytes(TheInterface.OutgoingBuffer), 
						0, 
						encoder.GetBytes(TheInterface.OutgoingBuffer).Length
					);

					clientStream.Flush();
					TheInterface.OutgoingBuffer = "";
				}
				catch(Exception ex)
				{
					Log.lWarning("ASCII output failed.", "Connections", "TakeTurn");
					Log.lWarning(ex.Message, "Connections", "TakeTurn");
				}
						
				
			}
			else if(TheInterface.UseAsciiOutput == false)
			{
				try
				{
					if(TheInterface.OutgoingBuffer.Length > 0)
					{
						clientStream.Write(
							encoder.GetBytes(TheInterface.OutgoingBuffer), 
							0, 
							encoder.GetBytes(TheInterface.OutgoingBuffer).Length
						);
					}
				}
				catch(Exception ex)
				{
					Log.lWarning("Binary output failed to encode.", "Connections", "TakeTurn");
					Log.lWarning(ex.Message, "Connections", "TakeTurn");
				}

				try
				{
					//	First we need to make sure that a client is connected.
					if(!tcpClient.Connected)
					{
						//	no connection is present here
					}
					else
					{
						if(TheInterface.OutgoingByteBuffer != null 
						&& TheInterface.OutgoingByteBuffer.Length > 0)
						{
							clientStream.Write(
								TheInterface.OutgoingByteBuffer, 
								0, 
								TheInterface.OutgoingByteBuffer.Length
							);
						}
					}

				}
				catch(Exception ex)
				{
					if(ex.GetBaseException().Message != "The socket has been shut down")
					{
						Log.lWarning("Binary output failed during write.", "Connections", "TakeTurn");
						Log.lWarning(ex.Message, "Connections", "TakeTurn");
						Log.lWarning(ex.GetBaseException().Message, "Connections", "TakeTurn");
						
						//	This must have assumed only web interface connections.
						//	Maybe this will be useful in the future.
						//Console.WriteLine("\t{0}", ((WebInterface)TheInterface).PageObject.RequestParts[1]);
					}
				}

				clientStream.Flush();
					
				TheInterface.OutgoingBuffer = "";
				TheInterface.OutgoingByteBuffer = null;

			}
			
			if(TheInterface.TerminateAfterSend)
			{
				Close();	
			}
			
				
		}
		
		
	}
}