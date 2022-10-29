
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
	
	[XmlRoot("EngineState")]
	public class Engine
	{
		//	The part that makes gurutracker go.
		
		[XmlIgnore()] public static string EngineName = "gurutracker";
		[XmlIgnore()] public static string EngineVersion = "v0.22.1029";
	
		[XmlIgnore()] public static Config Configuration = new Config();
		
		[XmlIgnore()] public static Dictionary<string, string> CommandFlags = new Dictionary<string, string>();
		
		[XmlIgnore()] public bool KeepRunning = true;
		
		
		[XmlIgnore()] public static long totalcycletime = 0;
		[XmlIgnore()] public static long totalcycles = 0;


		public static  Listeners[] TcpListeners = new Listeners[2];
		
		public static int MaxIncomingConnections = 255;
		public static IncomingConnections[] Connections = new IncomingConnections[MaxIncomingConnections];
		
		public static Track TheTrack;
		
		public Engine()
		{
			
		}
		
		//	Initialize the gurutracker engine with the specified arguments.
		public Engine(string[] args)
		{
			
			if(args.Length > 0)
			{
				bool skipnext = false;
				for(int earg = 0; earg < args.Length; earg++)
				{
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
									Log.lWarning(
										"Port " + args[earg + 1] + "does not appear to be numeric", 
										"Engine", 
										"Engine"
									);
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
										Log.lWarning(
											"Port " + tmpport.ToString() + " is out of range.",
											"Engine", 
											"Engine"
										);
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
									Log.lWarning(
										"Could not find the weblogpath: " + wlp, 
										"Engine", 
										"Engine"
									);
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
									Log.lInfo("Reinstall successful.", "Engine", "Engine");
									Environment.Exit(0);
								}
								else
								{
									Log.lError("Reinstall failed.", "Engine", "Engine");
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
								Console.WriteLine("Copyright 2012 - 2022 Brian Murphy");
								Console.WriteLine(" www.gurutronik.com");
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
				Engine.CommandFlags.Add("-f", Engine.PFP(Configuration.TracksPath + "untitled.gt"));
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
		
		//	I'm not sure if this method is specifically needed or if this is
		//	some ancient redundancy...
		//	This method will write a line to the debug device if it is requested
		public void WriteLine(string linetowrite)
		{
			Log.lWarning("Call to Engine.WriteLine...", "Engine", "WriteLine");
			if(gurumod.Engine.Configuration.DisplayDebug) { Console.WriteLine(linetowrite); }
		}
		
		
		public static long TimeStamp() { return (long)UnixTimeStamp(); }
		public static double UnixTimeStamp() { return UnixTimeStamp(System.DateTime.UtcNow); }
		
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

		
		//	I'm honestly not sure why this has an MD5 hashing method.  The fact
		//	that the only argument is 'password' actually frightens me a bit.
		//	I have a feeling that it's not actually used anywhere...
		public static string MD5(string password)
		{
			Log.lWarning("Call to Engine.MD5", "Engine", "MD5");
			//System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
			
			System.Security.Cryptography.MD5 hasher = System.Security.Cryptography.MD5.Create();
			byte[] bs = System.Text.Encoding.UTF8.GetBytes(password);
			byte[] hash = hasher.ComputeHash(bs);

			System.Text.StringBuilder s = new System.Text.StringBuilder();
			foreach (byte b in bs)
			{
			   s.Append(b.ToString("x2").ToLower());
			}

			return s.ToString();
		}
		
		//	Initialize the state of the gurutracker engine.
		public void Initialize()
		{
			//	Initialize any listeners
			Engine.TcpListeners[0] = new Listeners(Engine.Configuration.WebListenPort, "Web Interface Listener", "HTTP");
			Engine.TcpListeners[1] = new Listeners(2000, "Debug Interface", "GTDBG");
			
			for(int econ = 0; econ < Engine.MaxIncomingConnections; econ++)
			{
				Engine.Connections[econ] = new IncomingConnections(econ);
			}
			
			//	Initialize the track.
			if(Engine.CommandFlags.ContainsKey("-f"))
			{
				/*if(System.IO.Directory.Exists(Engine.CommandFlags["-f"]))
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
				}*/
				if(System.IO.File.Exists(Engine.CommandFlags["-f"]))
				{
					Log.lInfo("Loading track: " + Engine.CommandFlags["-f"], "Engine", "Initialize");

					Engine.TheTrack = new Track();
					Engine.TheTrack.NewTrack();
					Track.Load(Engine.CommandFlags["-f"]);
				}
				else
				{
					Log.lWarning("Track not found: " + Engine.CommandFlags["-f"], "Engine", "Initialize");

					Engine.TheTrack = new Track();
					Engine.TheTrack.NewTrack();
					Engine.TheTrack.Save();
				}
			}
			else
			{
				Log.lInfo("Creating new track.", "Engine", "Initialize");

				Engine.TheTrack = new Track();
				Engine.TheTrack.NewTrack();
			}
			
			
			Engine.TheTrack.LoopEnabled = true;
			
		}
		
		//	This method is the main heart loop of this program.  It will run
		//	forever until it either crashes or is shut down.
		//
		//	This needs to handle all of the things, including:
		//		Check each interface for updates
		//		Check each object for needed operations
		public void Run()
		{
			Engine.TcpListeners[0].Listen();
			Engine.TcpListeners[1].Listen();
			
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
				
				//	Loop through each connected client.
				for(int econnection = 0; econnection < MaxIncomingConnections; econnection++)
				{
					//	Check if the connection is null.  If so we really can't
					//	do anything with it.
					if(Connections[econnection] != null)
					{
						try
						{
							//	Check if the client connection is still marked
							//	internally as active.  If so, we need to hand
							//	over control to it for processing.
							if(Connections[econnection].IsActive() == true)
							{
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

				//	Play nice with the underlying OS.  This will happily consume
				//	100% of the CPU if we don't do short sleeps.
				Thread.Sleep(1);
				
				
				//	Check if there are any actions that need to be completed
				//	each minute.
				if(lastminute != theminute)
				{
					/*
					 * 
					 * This would be the place to run a loop that checks any devices that
					 * need to be updated each minute.
					 * 
					 */
				}
				
				//	Hand over control to the track.  That is where playback is
				//	processed.
				Engine.TheTrack.TakeTurn();
				
				endtime = (TimeStamp() * 1000) + System.DateTime.Now.Millisecond;
				cycletime = endtime - starttime;
				totalcycletime = totalcycletime + cycletime;
				totalcycles++;
				
				Thread.Sleep(1);
			}
		}
		
		//	Return a platform-independent path and filename from what is inputted.
		//	IE, if "/home/foo/yo" is input, on a Windows machine it will return
		//	"\home\foo\yo", and "/home/foo/yo" on Linux machines.
		public static string PFP(string path)
		{
			return path.Replace("/", Path.DirectorySeparatorChar.ToString());
		}
		
		
	}
	
	
}
