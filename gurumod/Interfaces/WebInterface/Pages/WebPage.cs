using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace gurumod.WebPages
{
	
	
	public abstract class WebPage
	{
		//	This class provides the basis for each page that will be used in the
		//	WebInterface.  This outlines the required functions, but each individual
		//	page will obviously add their own.
		
		[XmlIgnore()] public string PageTitle = "btEngine";
		[XmlIgnore()] public string[] InHeaders;
		[XmlIgnore()] public string[] RequestParts;
		[XmlIgnore()] public string OutgoingBuffer = "";
		[XmlIgnore()] public byte[] OutgoingByteBuffer;
		[XmlIgnore()] public string OutgoingHeaders = "";
		[XmlElement("UseAsciiOutput")] public bool UseAsciiOutput = true;
		[XmlIgnore()] public string Template = "[PAGECONTENT]";
		[XmlIgnore()] public string IncomingContent = "";
		[XmlIgnore()] public Dictionary<string, string> PostVars;
		[XmlIgnore()] public long UserID = -1;
		//[XmlIgnore()] public User CurrentUser = null;
		[XmlElement("MimeType")] public string ContentType = "text/html";
		[XmlIgnore()] public string StatusMessage = "200 OK";
		[XmlIgnore()] public bool TerminateOnSend = true;
		[XmlElement("Stream")] public bool Stream = false;
		
		public WebPage()
		{
		}
		
		public abstract bool Run();
		
		public virtual bool TakeTurn()
		{
			return true;
		}
	}
}