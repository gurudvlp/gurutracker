
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace gurumod.WebPages
{

	[XmlRoot("IncomingHTTPRequest")]
	public class IncomingRequest
	{
		[XmlElement("RemoteIP")] public string RemoteIP = "";
		[XmlElement("RemotePort")] public int RemotePort = 0;
		[XmlElement("Method")] public string Method = "GET";
		[XmlElement("UserAgent")] public string UserAgent = "";
		[XmlElement("TimeStamp")] public long TimeStamp = Engine.TimeStamp();
		[XmlElement("UserID")] public long UserID = -1;
		[XmlElement("Headers")] public string[] Headers;
		[XmlIgnore()] public Dictionary<string, string> SortedHeaders = new Dictionary<string, string>();
		[XmlIgnore()] public Dictionary<string, string> Cookies = new Dictionary<string, string>();
		[XmlIgnore()] public Dictionary<string, string> PostVars = new Dictionary<string, string>();
		
		[XmlElement("RequestLine")] public string RequestLine = "";
		[XmlElement("RequestParts")] public string[] Parts;
		
		public IncomingRequest ()
		{
		}
		
		public bool ParseHeaders()
		{
			if(this.Headers == null) { Console.WriteLine("btEngine: WebInterface: ParseHeaders: Headers are null!"); return false; }
			
			int spaceloc = this.Headers[0].IndexOf(" ");
			if(spaceloc < 2) { Console.WriteLine("btEngine: WebInterface: ParseHeaders: Invalid {[METHOD]} [URI] [VERSION]"); return false; }
			
			this.Method = this.Headers[0].Substring(0, spaceloc).ToUpper();
			
			
			if(WebInterface.LastRequestTime == Engine.TimeStamp())
			{
				WebInterface.RequestsThisSecond++;
			}
			else
			{
				WebInterface.LastRequestTime = Engine.TimeStamp();
				WebInterface.RequestsThisSecond = 0;
			}
			
			if(WebInterface.LogGetRequests && this.Method == "GET")
			{
				string fname = WebInterface.LogRequestPath + Engine.TimeStamp().ToString() + "-" + WebInterface.RequestsThisSecond.ToString() + "-get.txt";
				//string fullheaders = "";
				//foreach(string ehead in this.IncomingHTTPHeaders) { fullheaders = fullheaders + ehead.Trim() + "\n"; }
				//System.IO.File.Create(fname);
				System.IO.File.WriteAllLines(fname, this.Headers);
			}
			
			if(WebInterface.LogPostRequests && this.Method == "POST")
			{
				string fname = WebInterface.LogRequestPath + Engine.TimeStamp().ToString() + "-" + WebInterface.RequestsThisSecond.ToString() + "-post.txt";
				//System.IO.File.Create(fname);
				System.IO.File.WriteAllLines(fname, this.Headers);
			}
			
			SortHeaders();
			
			RequestLine = this.Headers[0];
			RequestLine = RequestLine.Replace(this.Method, "");
			RequestLine = RequestLine.Replace("HTTP/1.1", "");
			RequestLine = RequestLine.Replace("HTTP/1.0", "");
			//RequestLine = (RequestLine.Trim()).ToUpper();
			RequestLine = RequestLine.Trim();
			RequestLine = RequestLine.Remove(0, 1);
			
			char[] rldelm = {'/'};
			Parts = RequestLine.Split(rldelm);
			
			if(this.Method == "POST" && this.SortedHeaders.ContainsKey("CONTENT-LENGTH")) { ParsePostVars(); }
			
			return true;
		}
		
		public bool SortHeaders()
		{
			char[] delm = {':'};
			char[] delmforlist = {'&'};
			char[] delmforvar = {'='};
			char[] delmforcookie = {';'};
			
			foreach(string ehead in this.Headers)
			{
				//Console.WriteLine("Inc. Head Loop: " + ehead);
				string[] headerprts;
				if(ehead.IndexOf(":") > -1)
				{
					headerprts = ehead.Split(delm);
					
					//
					//	Cookies?
					//
					
					if(headerprts[0].ToUpper().Trim() == "COOKIE") { ParseCookie(headerprts[1]); }					
					else if(SortedHeaders.ContainsKey(headerprts[0].Trim().ToUpper()))
					{
						Console.WriteLine("Header key already present: {0}", headerprts);
					}
					else 
					{ 
						this.SortedHeaders.Add(headerprts[0].Trim().ToUpper(), headerprts[1].Trim());
					}
				}
			}
			
			return true;
		}
		
		private bool ParseCookie(string cookiedata)
		{
			char[] delmforvar = {'='};
			string[] splitvars = cookiedata.Split(delmforvar);
						
			if(splitvars.Length > 1)
			{
				splitvars[0] = WebInterface.UnUrlize(splitvars[0]).ToUpper().Trim();
				splitvars[1] = WebInterface.UnUrlize(splitvars[1]).Trim();
			
				if(!Cookies.ContainsKey(splitvars[0]))
				{
					//Console.WriteLine("Cookie already present. " + splitvars[0] + "  -  " + splitvars[1]);
					Cookies.Add(splitvars[0], splitvars[1]);
				}
			}
			else
			{
				return false;
			}
			
			return true;
		}
		
		private bool ParsePostVars()
		{
			
			
			return true;
		}
	}
}
