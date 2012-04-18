
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
