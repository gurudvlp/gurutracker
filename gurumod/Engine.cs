
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

using ICSharpCode;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;

namespace gurumod
{
	public class Launcher
	{
		public static Engine TheEngine;
		
		public static void Main(string[] args)
		{
			Engine.Configuration = new Config();
			Engine.Configuration.Initialize();
			Config.Load();
			
			//Console.WriteLine(System.Reflection.Assembly().Location);
			//Console.WriteLine(System.AppDomain.CurrentDomain.BaseDirectory);
			//Directory.SetCurrentDirectory(Engine.Configuration.SharedConfigPath + "bin");
			//Console.WriteLine(System.AppDomain.CurrentDomain.BaseDirectory);
			//System.AppDomain.CurrentDomain.BaseDirectory = Engine.PFP(Engine.Configuration.SharedConfigPath + "bin");
			
			if(!Installer.Installer.IsInstalled()
			|| (args.Length > 0 && args[0].ToLower() == "install"))
			{
				Installer.Installer.Install();
			}
			
			//Engine.ConfigPath = Installer.Installer.DataFolder;
			//Engine.ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Engine.EngineName) + "/";
			gurumod.Launcher.TheEngine = new Engine(args);
			
			if(!Directory.Exists(Engine.PFP(Engine.Configuration.SharedConfigPath)))
			{
				Console.WriteLine(Engine.EngineName + " " + Engine.EngineVersion);
				Console.WriteLine("Copyright 2012 - 2013 Brian Murphy");
				Console.WriteLine(" www.gurudigitalsolutions.com");
				Console.WriteLine(" ");
				Console.WriteLine(Engine.EngineName + " could not find the configuration directory.");
				Console.WriteLine("All of the config files need to be placed in");
				Console.WriteLine(Engine.PFP(Engine.ConfigPath));
				Console.WriteLine("for this to run properly.  Please put the files there and try again.");
				Environment.Exit(0);
			}
			
			/*Track trk = new Track();
			trk.Author = "Brian Murphy";
			trk.ChannelCount = 12;
			trk.Email = "gurudvlp@gmail.com";
			trk.Title = "betateztz";
			trk.Tempo = 142;
			trk.WebSite = "http://www.gurudigitalsolutions.com";
			trk.Year = 2012;
			trk.Samples = new Sample[Track.MaxSamples];
			for(int es = 0; es < Track.MaxSamples; es++)
			{
				trk.Samples[es] = new Sample();
			}
			trk.Patterns = new Pattern[Track.MaxPatterns];
			trk.Patterns[0] = new Pattern(12, 128);
			
			trk.Save();
			Environment.Exit(0);*/
			
			gurumod.Launcher.TheEngine.Initialize();
			
			Engine.Configuration.Save();
			//Environment.Exit(0);
			
			System.IO.Directory.SetCurrentDirectory(Engine.Configuration.SharedConfigPath + "bin");
			Console.WriteLine("Current Directory: {0}", Directory.GetCurrentDirectory());
			
			try
			{
				gurumod.Launcher.TheEngine.Run();
			}
			catch(Exception ex)
			{
				Console.WriteLine(Engine.EngineName + " has crashed!");
				Console.WriteLine("Additional info:");
				Console.WriteLine(ex.Message);
				Console.WriteLine("---------------------------");
				Console.WriteLine("Press any key to continue...");
				Console.ReadKey();
				Environment.Exit(0);
			}
		}
	}
	
	[XmlRoot("EngineState")]
	public class Engine
	{
		//	The part that makes gurutracker go.
		
		[XmlIgnore()] public static string EngineName = "gurutracker";
		[XmlIgnore()] public static string EngineVersion = "v0.13.1011";
		//[XmlIgnore()] public static string SharedPath = "/usr/share/" + Engine.EngineName + "/";
		//[XmlIgnore()] public static string ConfigPath = "/home/" + Environment.UserName + "/.gurutracker/";
		//[XmlIgnore()] public static string TracksPath = "/home/" + Environment.UserName + "/gurutracker/Tracks/";
		
		[XmlIgnore()] public static Config Configuration = new Config();
		
		[XmlIgnore()] public static Dictionary<string, string> CommandFlags = new Dictionary<string, string>();
		
		[XmlIgnore()] public bool KeepRunning = true;
		//[XmlElement("DisplayDebug")] public static bool DisplayDebug = true;
		
		
		[XmlIgnore()] public static long totalcycletime = 0;
		[XmlIgnore()] public static long totalcycles = 0;

		public static  Listeners[] TcpListeners = new Listeners[1];
		
		public static int MaxIncomingConnections = 255;
		public static IncomingConnections[] Connections = new IncomingConnections[MaxIncomingConnections];
		//public static int WebListenPort = 6789;
		
		public static Track TheTrack;
		
		public Engine()
		{
			
		}
		
		public Engine(string[] args)
		{
			
			if(args.Length > 0)
			{
				bool skipnext = false;
				for(int earg = 0; earg < args.Length; earg++)
				{
					//Console.WriteLine("Argument {0}: {1}", earg, args[earg]);
					if(!skipnext)
					{
						if(args.Length > earg + 1)
						{
							if(args[earg].ToLower() == "--trackfile"
							|| args[earg].ToLower() == "-f")
							{
								Engine.CommandFlags.Add("-f", args[earg + 1]);
								skipnext = true;
							}
							else
							if(args[earg].ToLower() == "--tempo")
							{
								Engine.CommandFlags.Add("--tempo", args[earg + 1]);
								skipnext = true;
							}
							else
							if(args[earg].ToLower() == "--port"
							|| args[earg].ToLower() == "-p")
							{
								
								int tmpport = 6789;
								if(!Int32.TryParse(args[earg + 1], out tmpport))
								{
									Console.WriteLine("Port {0} does not appear numeric.", args[earg + 1]);
								}
								else
								{
									if(tmpport > 0 && tmpport < 65536)
									{
										Engine.CommandFlags.Add("--port", args[earg + 1]);
										Engine.Configuration.WebListenPort = tmpport;
										skipnext = true;
									}
									else
									{
										Console.WriteLine("Port {0} is out of range.", tmpport);
										skipnext = true;
									}
									
								}
							}
							else
							if(args[earg].ToLower() == "--weblogpath")
							{
								string wlp = args[earg + 1];
								if(Directory.Exists(wlp))
								{
									Engine.CommandFlags.Add("--weblogpath", wlp);
									WebInterface.LogRequestPath = wlp;
								}
								else
								{
									Console.WriteLine("Could not find the weblogpath {0}", wlp);
								}
								
								skipnext = true;
							}
							else
							if(args[earg].ToLower() == "--webtemplatedir")
							{
								string wtd = args[earg + 1];
								if(Directory.Exists(wtd))
								{
									if(!wtd.EndsWith("/")) { wtd = wtd + "/"; }
									Engine.CommandFlags.Add("--webtemplatedir", wtd);
									Configuration.WebTemplateDir = wtd;
								}
							}
							else
							if(args[earg].ToLower() == "--install"
							|| args[earg].ToLower() == "-install"
							|| args[earg].ToLower() == "install")
							{
								Installer.Installer.IsInstalled();
								if(Installer.Installer.Install())
								{
									Console.WriteLine("Reinstall successful.");
									Environment.Exit(0);
								}
								else
								{
									Console.WriteLine("Reinstall failed.");
									Environment.Exit(0);
								}
							}
						}
						else
						{
							//	This is either the last or only argument passed to the program.
							if(args[earg].ToLower() == "-h"
							|| args[earg].ToLower() == "--help"
							|| args[earg].ToLower() == "-help")
							{
								Console.WriteLine(Engine.EngineName + " " + Engine.EngineVersion);
								Console.WriteLine("Copyright 2012 Brian Murphy");
								Console.WriteLine(" www.gurudigitalsolutions.com");
								Console.WriteLine(" ");
								Console.WriteLine("USAGE:  gurutracker.exe [FLAGS]");
								Console.WriteLine("FLAGS:");
								Console.WriteLine("\t-f <filename>\t\t\tSpecify the name of the track file.");
								Console.WriteLine("\t\t\t\t\tOn your system, this defaults to:");// (ConfigPath)/track.xml");
								Console.WriteLine("\t\t\t\t\t{0}track.xml", Engine.PFP(Engine.ConfigPath));
								Console.WriteLine("\t--install\t\t\tInstall gurutracker for all users on your system.");
								Console.WriteLine("\t-p\t\t\t\tSpecify the port to listen on for web connections.");
								Console.WriteLine("\t--logget\t\t\tLog all incoming GET web requests.");
								Console.WriteLine("\t--logpost\t\t\tLog all incoming POST web requests.");
								Console.WriteLine("\t--weblogpath <path>\t\tPath to log incoming web requests.");
								Environment.Exit(0);
							}
							else if(args[earg].ToLower() == "--logget")
							{
								Engine.CommandFlags.Add("--logget", true.ToString());
								WebInterface.LogGetRequests = true;
							}
							else if(args[earg].ToLower() == "--logpost")
							{
								Engine.CommandFlags.Add("--logpost", true.ToString());
								WebInterface.LogPostRequests = true;
							}
						}
					}
					else
					{
						skipnext = false;
					}
				}
			}
			
			if(!Engine.CommandFlags.ContainsKey("-f"))
			{
				Engine.CommandFlags.Add("-f", Engine.PFP(Configuration.TracksPath + "untitled/"));
			}
		}
		
		public static string ConfigPath
		{
			get
			{
				if(Configuration == null || Configuration.SharedConfigPath == null)
				{
					string toret = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
					
					
					if(!toret.EndsWith("/")) { toret = toret + "/"; }
					toret = toret + "gurutracker/";
					return toret;
				}
				
				return Configuration.SharedConfigPath;
			}
			set
			{
				Configuration.SharedConfigPath = value;
			}
		}
		
		public void WriteLine(string linetowrite)
		{
			//	This method will write a line to the debug device if it is requested
			if(gurumod.Engine.Configuration.DisplayDebug) { Console.WriteLine(linetowrite); }
		}
		
		
		public static long TimeStamp()
		{

			
			return (long)UnixTimeStamp();
		}
		
		public static double UnixTimeStamp()
		{
		    //TimeSpan unix_time = (System.DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
		    //return unix_time.TotalSeconds;
			return UnixTimeStamp(System.DateTime.UtcNow);
		}
		
		public static double UnixTimeStamp(System.DateTime datetime)
		{
			TimeSpan unix_time = (datetime - new DateTime(1970, 1, 1, 0, 0, 0));
			return unix_time.TotalSeconds;
		}

		public static DateTime UnixTimeStampToDateTime(long unixTimeStamp )
		{
		    // Unix timestamp is seconds past epoch
		    System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0);
		    dtDateTime = dtDateTime.AddSeconds((double)unixTimeStamp ).ToLocalTime();
		    return dtDateTime;
		}

		
		public static string MD5(string password)
		{
			System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] bs = System.Text.Encoding.UTF8.GetBytes(password);
			bs = x.ComputeHash(bs);
			System.Text.StringBuilder s = new System.Text.StringBuilder();
			foreach (byte b in bs)
			{
			   s.Append(b.ToString("x2").ToLower());
			}
			return s.ToString();
		}
		
		public void Initialize()
		{
			//	This method needs to set up the engine.  This includes creating listening devices for
			//	the various interfaces.
			//
			//	As of now, only http connections will be allowed, so we only need one listener.
			
			Engine.TcpListeners[0] = new Listeners(Engine.Configuration.WebListenPort, "Web Interface Listener", "HTTP");
			
			
			
			for(int econ = 0; econ < Engine.MaxIncomingConnections; econ++)
			{
				Engine.Connections[econ] = new IncomingConnections(econ);
			}
			
			//	Initialize the track.
			if(Engine.CommandFlags.ContainsKey("-f"))
			{
				if(System.IO.Directory.Exists(Engine.CommandFlags["-f"]))
				{
					Console.WriteLine("Loading track from specified directory. {0}", Engine.CommandFlags["-f"]);
					Engine.TheTrack = new Track();
					Console.WriteLine("Loading still...");
					Engine.TheTrack.NewTrack();
					Console.WriteLine("And..");
					Track.Load(Engine.CommandFlags["-f"]);
					Console.WriteLine("Finally...");
					Console.WriteLine("A test of saving the .gt format...");

					//TheTrack.SaveAsGT();
				}
				else
				{
					Console.WriteLine("Track name specified, but the file was not found.");
					Console.WriteLine(Engine.CommandFlags["-f"]);
					Engine.TheTrack = new Track();
					Engine.TheTrack.NewTrack();
					Engine.TheTrack.Save();
				}
			}
			else
			{
				Console.WriteLine("No track specified, so using a blank one.");
				Engine.TheTrack = new Track();
				Engine.TheTrack.NewTrack();
			}
			
			
			Engine.TheTrack.LoopEnabled = true;
			/*
			 * Old Track Initialization code:
			 * 
			//Engine.TheTrack = new Track();
			Track.Load(Engine.ConfigPath + "track.xml");
			//Engine.TheTrack.EnablePlayer();
			Engine.TheTrack.LoopEnabled = true;
			//Engine.TheTrack.Save();
			*/
			
		}
		
		public void Run()
		{
			//	This method needs to maintain the main loop of the program.
			//	This needs to check each interface for updates,
			//	Check each object for needed operations
			Engine.TcpListeners[0].Listen();
			
			int caught = 0;

			long starttime = (TimeStamp() * 1000) + System.DateTime.Now.Millisecond;
			long endtime = 0;
			long cycletime = 0;
			
			int theminute = 0;
			int lastminute = 0;
			
			
			
			while(this.KeepRunning)
			{
				lastminute = theminute;
				theminute = System.DateTime.Now.Minute;
				
				starttime = (TimeStamp() * 1000) + System.DateTime.Now.Millisecond;
				for(int econnection = 0; econnection < MaxIncomingConnections; econnection++)
				{
					//string tconnectiontype = "";
					
					if(Connections[econnection] != null)
					{
						try
						{
							if(Connections[econnection].IsActive() == true)
							{
								//	This connection is currently active, so we should hand action
								//	over to this object.
								//
								Connections[econnection].TakeTurn();
							}
						}
						catch(Exception ex)
						{
							//	crap-poopit
							caught++;
							Console.WriteLine("Connection Failure::Connection:" + econnection.ToString());
							Console.WriteLine("Exception Message:");
							Console.WriteLine(ex.Message);
							Console.WriteLine("-----");
							Console.WriteLine(ex.StackTrace);
							Console.WriteLine("-----");
							Console.WriteLine("Inner Exception:");
							Console.WriteLine((ex.InnerException).Message);
							Console.WriteLine("-----");
							Console.WriteLine("Base Exception:");
							Console.WriteLine((ex.GetBaseException()).Message);
							
							
							Environment.Exit(0);
							if(caught == 100)
							{
								//Environment.Exit(0);
							}
						}
					}
					
					
				}
				Thread.Sleep(1);
				
				
				//	And to check if any servers have special shit to do
				if(lastminute != theminute)
				{
					/*
					 * 
					 * This would be the place to run a loop that checks any devices that
					 * need to be updated each minute.
					 * 
					 */
				}
				
				Engine.TheTrack.TakeTurn();
				
				endtime = (TimeStamp() * 1000) + System.DateTime.Now.Millisecond;
				cycletime = endtime - starttime;
				totalcycletime = totalcycletime + cycletime;
				totalcycles++;
				
				Thread.Sleep(1);
			}
		}
		
		public static string PFP(string path)
		{
			//	Return a platform-independent path and filename from what is inputted.
			//	IE, if "/home/foo/yo" is input, on a Windows machine it will return
			//	"\home\foo\yo", and "/home/foo/yo" on Linux machines.
			
			return path.Replace("/", Path.DirectorySeparatorChar.ToString());
		}
		
		
	}
	
	
}
