
using System;

namespace gurumod
{


	public abstract class Interface
	{
		public string IncomingBuffer = "";
		public string OutgoingBuffer = "";
		public bool TerminateAfterSend = false;
		public bool UseAsciiOutput = true;
		public byte[] OutgoingByteBuffer;
		public string RemoteIP = "";

		//	Set a default interface keep alive time of 30 seconds.  This means
		//	that after 30 seconds of not activity the connection will be closed.
		private int _keepAliveTime = 30;
		public int keepAliveTime 
		{
			get { return _keepAliveTime; }
			set { _keepAliveTime = value; }
		}
		
		public Interface ()
		{
		}
		
		public void ReceivedData(string datain)
		{
			if(datain == null) { return; }
			IncomingBuffer = IncomingBuffer + datain;
		}
		
		public abstract bool TakeTurn();
		
		public abstract bool Run(string commandstring);
	}
}
