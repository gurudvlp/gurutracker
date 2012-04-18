/*
 * 	btEngine Web Interface
 * 
 * 	This code is originally from my btEngine project.  This basically provided a simple web
 * 	interface for various projects I have worked on.  This was almost 100% copy & paste from
 * 	another project, as I have been doing for a couple years with it.  Some changes along the
 * 	way, to the point of me wishing I had always done source control with it :)
 * 
 * 	Copyright 2012 Brian Murphy
 * 	www.gurudigitalsolutions.com
 * 
 * 
 * 
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;


namespace gurumod
{

	[XmlRoot("WebConnection")]
	public class WebInterface : Interface
	{
		//
		//	This class is sweet.  It provides the http/web interface.
		//
		
		[XmlIgnore()] public static bool LogGetRequests = false;
		[XmlIgnore()] public static bool LogPostRequests = false;
		[XmlIgnore()] public static string LogRequestPath = Engine.PFP(gurumod.Engine.ConfigPath + "Interfaces/Web/log/");
		[XmlIgnore()] public static long LastRequestTime = 0;
		[XmlIgnore()] public static long RequestsThisSecond = 0;
		[XmlElement("HostName")] public string HostName = "localhost";
		[XmlElement("RedirectOtherHostNames")] public bool RedirectOtherHostNames = false;
		
		//[XmlIgnore()] public static string TemplateDir = gurumod.Engine.ConfigPath + "webinterface/templates";
		[XmlElement("IncomingHeaders")] public string[] IncomingHTTPHeaders;
		[XmlElement("OutgoingHeaders")] public string[] OutgoingHTTPHeaders;
		[XmlIgnore()] char[] linedelimiters = {'\n'};
		[XmlIgnore()] public string TheTemplate = "";
		[XmlIgnore()] public WebPages.WebPage PageObject;
		[XmlIgnore()] public bool WaitingOnContent = false;
		[XmlIgnore()] public int RemainingContentLength = 0;
		[XmlElement("IncomingContent")] public string IncomingContent = "";
		[XmlElement("HTTPMethod")] public string HTTPMethod = "GET";
		[XmlIgnore()] public Dictionary<string, string> SortedHeaders = new Dictionary<string, string>();
		[XmlIgnore()] public Dictionary<string, string> PostVars = new Dictionary<string, string>();
		[XmlIgnore()] public Dictionary<string, string> IncomingCookies = new Dictionary<string, string>();
		//[XmlIgnore()] public static string MainTemplate = gurumod.Engine.ConfigPath + "webinterface/templates/index.html";
		[XmlIgnore()] public static Category[] Categories = new Category[10];
		//[XmlIgnore()] public static WebPages.ShellCommand[] ShCommands = new WebPages.ShellCommand[50];
		[XmlIgnore()] public static bool CategoriesInitialized = false;
		[XmlIgnore()] public bool OutgoingHeadersSent = false;
		
		[XmlElement("UserID")] public long UserID = 0;
		//[XmlElement("CurrentUser")] public User CurrentUser = null;
		[XmlElement("DomainName")] public string DomainName = "127.0.0.1";
		
		[XmlIgnore()] WebPages.IncomingRequest IncomingHTTPRequest = new WebPages.IncomingRequest();
		//public string RemoteIP = "";
		
		public WebInterface ()
		{
			//Console.WriteLine("btEngine: WebInterface: Spawning new web interface.");
		}
		
		public bool LoadPageCategories()
		{
			//if(WebInterface.Categories != null) { WebInterface.Categories = null; }
			//WebInterface.Categories = new Category[10];
			
			bool onefound = false;
			for(int eCat = 0; eCat < WebInterface.Categories.Length; eCat++)
			{
				if(LoadPageCategory(eCat)) { onefound = true; };
			}
			
			if(!onefound)
			{
				WebInterface.Categories[0] = new Category("Admin", "admin", "gurumod.WebPages.Admin");
			}
			
			Console.WriteLine("btEngine: WebInterface: Page categories found.");
			
			return true;
		}
		
		public bool LoadPageCategory(int catid)
		{
			//Console.WriteLine("btEngine: WebInterface: Loading page categories.");
			if(catid < 0 || catid >= WebInterface.Categories.Length)
			{
				Console.WriteLine("Invalid id.  Try between 0 and " + WebInterface.Categories.Length.ToString());
				return false;
			}
			
			
			
			string xmlname = Engine.PFP(gurumod.Engine.ConfigPath + "Interfaces/Web/PageCat/" + catid.ToString() + ".xml");
			if(System.IO.File.Exists(xmlname))
			{
				XmlSerializer s = new XmlSerializer(typeof(Category));
				
				TextReader tr = new StreamReader(xmlname);
				WebInterface.Categories[catid] = new Category();
				WebInterface.Categories[catid] = (Category)s.Deserialize(tr);
		

				tr.Close();
			}
			else
			{
				Console.WriteLine("WebInterface: LoadPageCategory (" + catid.ToString() + ") not found.");
				return false;
			}
			
			return true;
		}
		
		public bool LoadShellCommands()
		{
			bool onefound = false;
			/*WebInterface.ShCommands = new gurumod.WebPages.ShellCommand[50];
			for(int eCat = 0; eCat < WebInterface.ShCommands.Length; eCat++)
			{
				if(LoadShellCommand(eCat)) { onefound = true; };
			}
			
			if(!onefound)
			{
				WebInterface.ShCommands[0] = new gurumod.WebPages.ShellCommand();
				WebInterface.ShCommands[0].Command = "espeak";
				WebInterface.ShCommands[0].ComName = "espeaker";
				WebPages.ShellCommand.SaveAll();
			}
			*/
			//Console.WriteLine("btEngine: WebInterface: Shell Commands found.");
			
			return true;
		}
		
		public bool LoadShellCommand(int catid)
		{
			//Console.WriteLine("btEngine: WebInterface: Loading Shell Commands.");
			/*if(catid < 0 || catid >= WebInterface.ShCommands.Length)
			{
				Console.WriteLine("Invalid id.  Try between 0 and " + WebInterface.ShCommands.Length.ToString());
				return false;
			}
			
			
			
			string xmlname = Engine.PFP(gurumod.Engine.ConfigPath + "Interfaces/Web/ShellCommands/" + catid.ToString() + ".xml");
			if(System.IO.File.Exists(xmlname))
			{
				XmlSerializer s = new XmlSerializer(typeof(WebPages.ShellCommand));
				
				TextReader tr = new StreamReader(xmlname);
				WebInterface.ShCommands[catid] = new WebPages.ShellCommand();
				WebInterface.ShCommands[catid] = (WebPages.ShellCommand)s.Deserialize(tr);
		

				tr.Close();
			}
			else
			{
				//Console.WriteLine("WebInterface: LoadPageCategory (" + catid.ToString() + ") not found.");
				return false;
			}
			*/
			return true;
		}
		
		public override bool TakeTurn()
		{
			//Console.WriteLine("WebInterface taking turn");
			if(PageObject == null)
			{
				Console.WriteLine("Starting web interface turn");
				
				//Console.WriteLine("btEngine: WebInterface: Initializing Web Interface.");
				if(!WebInterface.CategoriesInitialized)
				{
					//	Initialize the page categories since they have
					//	not been loaded yet.
					
					Console.WriteLine("WebInterface: Initializing Page Categories.");
					LoadPageCategories();
					
					//LoadShellCommands();
					CategoriesInitialized = true;
				}
				
				if(WaitingOnContent == false) { Console.WriteLine("Collecting headers..."); CollectHeaders(); }
	
				if(RemainingContentLength > 0) { Console.WriteLine("Collecting content..."); CollectContent(); }
				else 
				{
					Console.WriteLine("Parsing headers");
					if(IncomingHTTPRequest.Headers != null) { ParseHeaders(); }
					
				}
			}
			else
			{
				//	The page object has already been initialized, so we are waiting for output.
				Console.WriteLine("page Object taking turn...");
				PageObject.TakeTurn();
				
				if(PageObject.UseAsciiOutput)
				{
					
					
					if(PageObject.TerminateOnSend)
					{
						base.TerminateAfterSend = true;
						base.OutgoingBuffer = PageObject.OutgoingBuffer;
					}
					else
					{
						base.OutgoingBuffer = PageObject.OutgoingBuffer;
						base.TerminateAfterSend = false;
					}
					
					
					
					//Console.WriteLine("Yet another 'headers to send'");
					//Console.WriteLine(headerztosend);
				}
				else
				{
					if(PageObject.TerminateOnSend)
					{
						base.UseAsciiOutput = false;
						if(!this.OutgoingHeadersSent)
						{
							//base.OutgoingBuffer = headerztosend + "\nContent-Length: " + PageObject.OutgoingByteBuffer.Length.ToString() + "\n\n";
							base.OutgoingBuffer = PageObject.OutgoingHeaders + "\nContent-Length: " + PageObject.OutgoingByteBuffer.Length.ToString() + "\n\n";
							this.OutgoingHeadersSent = true;
						}
						base.OutgoingByteBuffer = PageObject.OutgoingByteBuffer;
						base.TerminateAfterSend = true;
					}
					else
					{
						base.UseAsciiOutput = false;
						//base.OutgoingBuffer = headerztosend + "\n\n";
						base.OutgoingBuffer = PageObject.OutgoingHeaders + "\n\n";
						base.OutgoingByteBuffer = PageObject.OutgoingByteBuffer;
						base.TerminateAfterSend = false;
					}
				}
				
				
			}
			
			return true;
		}
		
		public void CollectContent()
		{
			//Console.WriteLine("Collecting incoming content.");
			if(RemainingContentLength < 1) { return; }
			
			if(this.IncomingBuffer.Length >= this.RemainingContentLength)
			{
				this.IncomingContent = this.IncomingBuffer.Substring(0, this.RemainingContentLength);
				if(this.IncomingBuffer.Length > this.RemainingContentLength) { this.IncomingBuffer = this.IncomingBuffer.Remove(this.RemainingContentLength); }
				RemainingContentLength = 0;
				WaitingOnContent = false;
			}
		}
		
		public void CollectHeaders()
		{
			//Console.WriteLine("Collecting headers");
			//	This method will analyze the http input to parse the headers.
			base.IncomingBuffer = base.IncomingBuffer.Replace("\r\n\r\n", "\n\n");

			if(base.IncomingBuffer.IndexOf("\n\n") > -1
			   /*&& clientstillconnected == true*/)
			{				
				//	Hell yeah, this needs to do it's badassity and run the HTTP request
				
				string[] cline = {base.IncomingBuffer.Substring(0, base.IncomingBuffer.IndexOf("\n\n")), base.IncomingBuffer.Substring(base.IncomingBuffer.IndexOf("\n\n")+2)};
				//string[] cline = base.IncomingBuffer.Split("\n\n", 2 ,StringSplitOptions.None);
				base.IncomingBuffer = cline[1];
				IncomingHTTPRequest.Headers = cline[0].Split(linedelimiters);
				
				//	Now that the headers are known, we need to determine which page to
				//	actually load and show.  Of course, this part needs to wait if
				//	we are waiting for more content to arrive (like a POST form)
				string[] headvars;
				char[] hvsp = {':'};
				foreach(string ehead in this.IncomingHTTPRequest.Headers)
				{
					string rehead = ehead.Trim();
					if(rehead.IndexOf(":") > -1)
					{
						headvars = ehead.Split(hvsp);
						if(headvars[0].ToUpper().Trim() == "CONTENT-LENGTH")
						{
							this.WaitingOnContent = true;
							this.RemainingContentLength = Int32.Parse(headvars[1].Trim());
						}
					}
				}
			}
		}
		
		public static void ConnectionLog()
		{
			
		}
		
		public void ParseHeaders()
		{
			//
			//	This method is tasked with determining what the headers sent by the
			//	client mean, and what should be done about them.
			//
			char[] delmforlist = {'&'};
			char[] delmforvar = {'='};
			//Console.WriteLine("Parsing headers.");
			this.IncomingHTTPRequest.RemoteIP = this.RemoteIP;
			this.IncomingHTTPRequest.ParseHeaders();
			
			
			if(this.IncomingHTTPRequest.SortedHeaders.ContainsKey("CONTENT-LENGTH") && this.IncomingHTTPRequest.Method == "POST")
			{
				//	We need to parse the POST variables
				string[] varlist;
				
				
				
				varlist = this.IncomingContent.Split(delmforlist);
				
				foreach(string evarset in varlist)
				{
					if(evarset != null && evarset != "")
					{
						string[] splitvars = evarset.Split(delmforvar);
							
						if(splitvars.Length > 1)
						{
							splitvars[0] = UnUrlize(splitvars[0]).ToUpper();
							splitvars[1] = UnUrlize(splitvars[1]);
						
							if(IncomingHTTPRequest.PostVars.ContainsKey(splitvars[0])) { Console.WriteLine("btEngine: WebInterface: ParsePostVars: Already contains key for " + splitvars[0]); }
							else { IncomingHTTPRequest.PostVars.Add(splitvars[0], splitvars[1]); }
						}
					}
				}
			}
			
			if(TryLogin()) { /*CurrentUser.Save();*/ }
			
			//WebPage PageObject;
			Console.WriteLine("Init page object...");
			InitPageObject();
			//
			//	Determine what page to load
			//
			/*if(IncomingHTTPRequest.Parts[0].ToLower() == "logout")
			{
				this.UserID = -1;
				this.CurrentUser = new User();
			}*/
			
			
			string headerztosend = OutgoingHeaders;
			   
			if(PageObject.UseAsciiOutput)
			{
				string datmpl = Templateize();
				
				if(PageObject.TerminateOnSend)
				{
					base.OutgoingBuffer = headerztosend + "\nContent-Length: " + datmpl.Length.ToString() + "\n\n";
					base.TerminateAfterSend = true;
				}
				else
				{
					base.OutgoingBuffer = headerztosend + "\n\n";
					base.TerminateAfterSend = false;
				}
				
				base.OutgoingBuffer = base.OutgoingBuffer + datmpl;
				
				//Console.WriteLine("Yet another 'headers to send'");
				//Console.WriteLine(headerztosend);
			}
			else
			{
				if(PageObject.TerminateOnSend)
				{
					base.UseAsciiOutput = false;
					base.OutgoingBuffer = headerztosend + "\nContent-Length: " + PageObject.OutgoingByteBuffer.Length.ToString() + "\n\n";
					base.OutgoingByteBuffer = PageObject.OutgoingByteBuffer;
				}
				else
				{
					base.UseAsciiOutput = false;
					base.OutgoingBuffer = headerztosend + "\n\n";
					base.OutgoingByteBuffer = PageObject.OutgoingByteBuffer;
				}
			}
		
			
			base.IncomingBuffer = "";
			this.IncomingContent = "";
			//base.TerminateAfterSend = true;
		}
		
		public string OutgoingHeaders
		{
			get
			{
				string oh = "";
				if(PageObject != null)
				{
					oh = "HTTP/1.1 " + PageObject.StatusMessage + "\n" +
						"Server: " + Engine.EngineName + " " + Engine.EngineVersion + " (" + (Environment.OSVersion).VersionString + ")\n" +
						"Content-type: " + PageObject.ContentType + "\n" +
						"Connection: close";
					
					if(PageObject.OutgoingHeaders != null && PageObject.OutgoingHeaders != "")
					{
						oh = oh + "\n" + PageObject.OutgoingHeaders.Trim();
					}
					
					/*if(CurrentUser != null)
					{
						if(CurrentUser.SessionID != "")
						{
							oh = oh + "\nSet-Cookie: sid=" + CurrentUser.SessionID + "; expires=Fri, 20-Dec-2012 23:59:59 GMT; path=/; domain=." + this.DomainName;
						}
						else
						{
							oh = oh + "\nSet-Cookie: sid=; expires=Wed, 30-Nov-2011 02:02:02 GMT; path=/; domain=." + this.DomainName;
						}
					}
					*/
					return oh;
				}
			
			
				oh = "HTTP/1.1 " + gurumod.HTTPStatusCodes.NotImplemented.ToString() + " Not Implemented\n" +
					"Server: " + Engine.EngineName  + " " + Engine.EngineVersion + " (" + (Environment.OSVersion).VersionString + ")\n" +
					"Content-type: " + PageObject.ContentType + "\n" +
					"Connection: close";
				
				return oh;
			}
		}
		
		public void InitPageObject()
		{
			if(IncomingHTTPRequest.SortedHeaders.ContainsKey("HOST"))
			{
				//Console.WriteLine("--test: " + IncomingHTTPRequest.SortedHeaders["HOST"].ToUpper());
				//Console.WriteLine("--test: " + this.HostName.ToUpper());
				if(1 == 1 || IncomingHTTPRequest.SortedHeaders["HOST"].ToUpper() == this.HostName.ToUpper())
				{
					int catid = GetCatID(IncomingHTTPRequest.Parts[0]);

					if(catid < 0) { Console.WriteLine("CatID: {0}", catid); PageObject = new gurumod.WebPages.Home(); }
					else
					{
						Console.WriteLine("CatID: {0}", catid);
						try
						{
							
							PageObject = (WebPages.WebPage)Activator.CreateInstance(Type.GetType(Categories[catid].ObjectType));
							
						}
						catch(Exception ex)
						{
							Console.WriteLine("btEngine: WebInterface: Invalid type spec on creating a page.");
							Console.WriteLine(ex.Message);
							
							PageObject = new gurumod.WebPages.Home();
						}
					}
				}
				else
				{
					//	The hostname specified does not match up.
					if(RedirectOtherHostNames)
					{
						PageObject = new gurumod.WebPages.StatusCodes.MovedPermanently();
						PageObject.OutgoingHeaders = "Location: http://" + this.HostName + "/" + IncomingHTTPRequest.RequestLine;
						Console.WriteLine("btEngine: WebInterface: HostNameFilter: " + PageObject.OutgoingHeaders);
						Console.WriteLine("  && " + this.HostName + " :: " + IncomingHTTPRequest.SortedHeaders["HOST"]);
						
					}
					else
					{
						PageObject = new gurumod.WebPages.Home();
					}
				}
			}
			else
			{
				//	The request didn't specify a hostname, so let's
				//	redirect to the main hostname.
				PageObject = new gurumod.WebPages.StatusCodes.MovedPermanently();
				PageObject.OutgoingHeaders = "Location: http://" + this.HostName + "/" + IncomingHTTPRequest.RequestLine;
			}
			
			PageObject.UserID = UserID;
			//PageObject.CurrentUser = this.CurrentUser;
			PageObject.InHeaders = this.IncomingHTTPRequest.Headers;
			PageObject.RequestParts = IncomingHTTPRequest.Parts;
			PageObject.IncomingContent = this.IncomingContent;
			PageObject.PostVars = this.IncomingHTTPRequest.PostVars;
			PageObject.Run();
		}
		
		public static int GetCatID(string TypeString)
		{
			for(int eCat = 0; eCat < WebInterface.Categories.Length; eCat++)
			{
				if(WebInterface.Categories[eCat] != null)
				{
					if(WebInterface.Categories[eCat].Command.ToUpper() == TypeString.ToUpper())
					{
						return eCat;
					}
				}
			}
			
			return -1;
		}
		
		public static string UnUrlize(string what)
		{
			//Console.WriteLine("UnUrlizing");
			what = what.Replace("+", " ");
			string[] HexCodes =
			{ "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
			
			for(int leftcolumn = 0; leftcolumn < 16; leftcolumn++)
			{
				for(int rightcolumn = 0; rightcolumn < 16; rightcolumn++)
				{
					string thiscombo = HexCodes[leftcolumn] + HexCodes[rightcolumn];
					
					what = what.Replace("%" + thiscombo, ((char)Convert.ToInt32(thiscombo, 16)).ToString());
				}
			}
			
			//Console.WriteLine("UnUrlized");
			
			return what;
		}
		
		public string Templateize()
		{
			if(PageObject == null) { return ""; }
			if(PageObject.Template == "") { return PageObject.OutgoingBuffer; }
			
			PageObject.Template = PageObject.Template.Replace("[PAGECONTENT]", PageObject.OutgoingBuffer);
			PageObject.Template = PageObject.Template.Replace("[PAGETITLE]", PageObject.PageTitle);
			PageObject.Template = PageObject.Template.Replace("[ENGINENAME]", Engine.EngineName);
			PageObject.Template = PageObject.Template.Replace("[ENGINEVERSION]", Engine.EngineVersion);
			
			//WebPages.WebPage FWPart = new WebPages.Login();
			//FWPart.UserID = this.UserID;
			//FWPart.CurrentUser = this.CurrentUser;
			//FWPart.Run();
			//PageObject.Template = PageObject.Template.Replace("[FRAMEWORK_LOGINBOX]", FWPart.Template);
			
			/*if(UserID > -1)
			{
				string menufile = System.IO.File.ReadAllText("webinterface/templates/leftmenu.html");
				PageObject.Template = PageObject.Template.Replace("[LEFTMENU]", menufile);
			}
			else
			{
				PageObject.Template = PageObject.Template.Replace("[LEFTMENU]", "");
			}*/
			
			return PageObject.Template;
		}
		
		public void AssembleOutgoingHeaders()
		{
			if(PageObject == null) { return; }
			
		}
		
		
		
		public override bool Run(string commandstring)
		{
			return true;
		}
		
		/*private string RenderPage()
		{
			//	This method creates the HTML page
			//
			//	It needs to determine the proper headers to send, and the html
			
			string outhead = "HTTP/1.1 200 OK\n" +
				"Server: " + Engine.EngineName + " v" + Engine.EngineVersion + "\n" +
				"Content-type: text/html\n" +
				"Connection: close\n";
			
			string outbody = "";
			if(System.IO.File.Exists(WebInterface.TemplateDir + "/index.html"))
			{
				outbody = System.IO.File.ReadAllText(WebInterface.TemplateDir + "/index.html");
			}
			else
			{
				outbody = "Hell yeah dude :)";
			}
			
			outhead = outhead + "Content-length: " + outbody.Length.ToString() + "\n\n";
			return outhead + outbody;
		}*/
		
		public bool TryLogin()
		{
			/*if(this.UserID != -1)
			{
				Console.WriteLine("btEngine: WebInterface: User connected from " + this.RemoteIP + " and the UserID is: " + this.UserID.ToString());
				return false;
			}
			//this.CurrentUser = new User();
			
			//Console.WriteLine("TryingLogin");
			if(IncomingHTTPRequest.Cookies.ContainsKey("SID") && IncomingHTTPRequest.Cookies["SID"] != "")
			{
				//Console.WriteLine("A SessionID was sent along with the cookie: " + IncomingCookies["SID"] + " :: " + Engine.Users[0].SessionID);
				
				
				if(this.CurrentUser.LoadFromSID(IncomingHTTPRequest.Cookies["SID"]))
				{
					this.UserID = this.CurrentUser.ID;
					this.CurrentUser.LastActiveTime = Engine.TimeStamp();
					this.CurrentUser.IPAddress = base.RemoteIP;
					Console.WriteLine("btEngine: WebInterface: " + this.CurrentUser.DisplayName + " connected from " + this.RemoteIP);
					return true;
				}
				
			}
			else
			{/
				///Console.WriteLine("No sid found in cookie, here they are:");
				/*foreach(KeyValuePair<string, string> kvp in IncomingCookies)
				{
					Console.WriteLine(kvp.Key + " -- " + kvp.Value);
				}*/
				
			//}
			
			//Console.WriteLine("SID Check done, now on to post login");
			
			/*
			 * Uncomment these:::
			 * */
			/*if(IncomingHTTPRequest.Method == "POST")
			{
				if(IncomingHTTPRequest.PostVars.ContainsKey("USERNAME")
				&& IncomingHTTPRequest.PostVars.ContainsKey("PASSWORD"))
				{
					if(this.CurrentUser.AttemptLogin(IncomingHTTPRequest.PostVars["USERNAME"], IncomingHTTPRequest.PostVars["PASSWORD"]))
					{
						this.UserID = this.CurrentUser.ID;
						this.CurrentUser.LastActiveTime = Engine.TimeStamp();
						this.CurrentUser.LoggedIn = true;
						this.CurrentUser.SessionID = User.GenerateSessionID();
						this.CurrentUser.IPAddress = base.RemoteIP;
						Console.WriteLine("btEngine: WebInterface: " + this.CurrentUser.DisplayName + " logged in from " + this.RemoteIP);
					}
					else
					{
						Console.WriteLine("btEngine: WebInterface: User connected from " + this.RemoteIP + " but the un/pw didn't check out.");
						return false;
					}
					Console.WriteLine("btEngine: WebInterface: User connected from " + this.RemoteIP);
					return true;
				}
			}
			*/
			//	So far this user hasn't logged in at all, so maybe it's a bot
			//	So we need to check...
			
			string debugger = "";
			if(IncomingHTTPRequest.SortedHeaders.ContainsKey("USER-AGENT"))
			{
				debugger = debugger + "1";
				if(IncomingHTTPRequest.SortedHeaders["USER-AGENT"].IndexOf("Googlebot") > 0)
				{
					debugger = debugger + "2";
					if(IncomingHTTPRequest.RemoteIP.StartsWith("66.249."))
					{
						Console.WriteLine("btEngine: WebInterface: Googlebot connected from " + this.RemoteIP);
						return false;
					}
				}
				else if(IncomingHTTPRequest.SortedHeaders["USER-AGENT"].IndexOf("Baiduspider") > 0)
				{
					debugger = debugger + "2";
					if(IncomingHTTPRequest.RemoteIP.StartsWith("180.76"))
					{
						Console.WriteLine("btengine: WebInterface: Baiduspider connected from " + this.RemoteIP);
						return false;
					}
				}
			}
			
			Console.WriteLine("btEngine: WebInterface: User connected from " + this.RemoteIP + " but no action was really taken (" + debugger + ")");
			
			return true;
			//return false;
		}
	}
	
	public static class HTTPStatusCodes
	{
		public static int OK = 200;
		public static int NoContent = 204;
		public static int MovedPermanently = 301;
		public static int Found = 302;
		public static int NotModified = 304;
		public static int BadRequest = 400;
		public static int UnAuthorized = 401;
		public static int Forbidden = 403;
		public static int NotFound = 404;
		public static int LengthRequired = 411;
		public static int InternalServerError = 500;
		public static int NotImplemented = 501;
		public static int BadGateway = 502;
		public static int ServiceUnavailable = 503;
		public static int GatewayTimeout = 504;
		public static int BandwidthLimitExceeded = 509;
	}
}