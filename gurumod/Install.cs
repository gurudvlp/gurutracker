// 
//  Install.cs
//  
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
// 
//  Copyright (c) 2012 Brian Murphy
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
using System;
using System.IO;
using System.Reflection;
using ICSharpCode.SharpZipLib.Tar;

namespace gurumod.Installer
{
	public class Installer
	{
		public static string[] LinuxDataFolders = new string[]
			{"/usr/share/" + Engine.EngineName,
			 "/usr/local/share/" + Engine.EngineName};
		public static string[] LinuxBinFolders = new string[]
			{"/usr/bin/",
			 "/usr/local/bin/"};
		
		private static string binfolder = "";
		public static string BinFolder
		{
			get
			{
				if(binfolder.EndsWith("/")) { return binfolder; } else { return binfolder + "/"; }
			}
			set
			{
				binfolder = value;
			}
		}
		
		private static string datafolder = "";
		public static string DataFolder
		{
			get
			{
				if(datafolder.EndsWith("/")) { return datafolder; } else { return datafolder + "/"; }
			}
			set
			{
				datafolder = value;
			}
		}
		
		public Installer ()
		{
		}
		
		public static bool IsInstalled()
		{
			//	Attempt to determine if gurutracker is already installed on this system.
			
			
			if(Environment.OSVersion.Platform == PlatformID.Unix)
			{
				//	A Unix/Unix like environment
				Console.WriteLine("PlatformID: Unix");
				bool binfexists = LinuxBinFolderExists();
				bool datafexists = LinuxDataFolderExists();
				
				if(!datafexists)
				{
					Console.WriteLine(Engine.EngineName + " does not appear to be installed.");
					//Console.WriteLine("Please run with the flag --install");
					
					return false;
					//Install("/usr/share/" + Engine.EngineName + "/", Installer.BinFolder); 
				}
				//if(System.IO.Directory.Exists)
				
			}
			else
			if(Environment.OSVersion.Platform == PlatformID.Win32NT
			|| Environment.OSVersion.Platform == PlatformID.Win32S
			|| Environment.OSVersion.Platform == PlatformID.Win32Windows)
			{
				//	The lovely Windows.
			}
			
			//string datafolder = Engine.PFP(Environment.SpecialFolder.CommonApplicationData + "/" + Engine.EngineName);
			//string binfolder = Engine.PFP(new Environment.SpecialFolder.ProgramFiles);
			//Environment.SpecialFolder binfolder = Environment.SpecialFolder.ProgramFiles;
			
			//Console.WriteLine("Install bin folder: {0} {1}", binfolder, datafolder);
			
			//Environment.Exit(0);
			
			return true;
		}
		
		public static bool Install()
		{
			Console.WriteLine(Engine.EngineName + " " + Engine.EngineVersion);
			Console.WriteLine("Copyright 2012 Brian Murphy");
			Console.WriteLine(" www.gurudigitalsolutions.com");
			Console.WriteLine("");
			Console.WriteLine("Attempting to install...");
			
			
			
			//if(!System.IO.Directory.Exists(Installer.DataFolder))
			//{
				//	Our shared data folder does not exist.  So, try to create
				//	it to put our data in.
				//Console.WriteLine("Data: {0}  :: Bin: {1}", Installer.DataFolder, Installer.BinFolder);
				try
				{
					Console.WriteLine("Data folder located at {0}", Installer.DataFolder);
					System.IO.Directory.CreateDirectory(Installer.DataFolder);
					InstallResources();
					
					
					Console.WriteLine("Installed resources, now exiting.");
					Environment.Exit(0);
				}
				catch(Exception ex)
				{
					Console.WriteLine("Installation failed.");
					Console.WriteLine(ex.Message);
					//Console.WriteLine(ex.StackTrace);
					
					if(ex.Message.IndexOf("Access to the path") == 0)
					{
						Console.WriteLine("You may need to run as root to install.");
					}
					
					Environment.Exit(0);
					

				}
			//}
			
			return true;
		}
		
		public static bool Install(string datafolder, string binfolder)
		{
			Installer.BinFolder = binfolder;
			Installer.DataFolder = datafolder;
			return Installer.Install();
		}
		
		public static bool InstallResources()
		{
			
			ExtractTar("Interfaces.tar", Installer.DataFolder);
			ExtractTar("Samples.tar", Installer.DataFolder);
			//ExtractTar("version.txt", Installer.DataFolder);
			ExtractTar("Bin.tar", Installer.DataFolder);
			ExtractTar("Tracks.tar", Installer.datafolder);


			
			//Console.WriteLine("Initializing Configuration, in {0}", Environment.CurrentDirectory);
			//Console.WriteLine(System.AppDomain.CurrentDomain.FriendlyName)
			string exename = System.AppDomain.CurrentDomain.FriendlyName;
			string exeloc = Environment.CurrentDirectory;
			//Console.WriteLine("exeloc, name {0} {1}", exeloc, exename);
			
			
			File.Copy(Engine.PFP(exeloc + "/" + exename), Engine.PFP(Engine.Configuration.SharedConfigPath + "bin/gurutracker.exe"), true);


			if(Environment.OSVersion.Platform == PlatformID.Unix)
			{
				//	Create a link from /usr/bin/gurutracker to /usr/share/gurutracker/bin/Debug/gurutracker.exe
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.EnableRaisingEvents=false; 
				proc.StartInfo.FileName = "ln";
				proc.StartInfo.Arguments = "-s " + Installer.DataFolder + "bin/gurutracker " + Installer.BinFolder + "gurutracker";
				proc.Start();
				proc.WaitForExit();
				
				proc = new System.Diagnostics.Process();
				proc.EnableRaisingEvents = false;
				proc.StartInfo.FileName = "chmod";
				proc.StartInfo.Arguments = "ugo+x " + Installer.DataFolder + "bin/Debug/gurutracker*";
				proc.Start();
				proc.WaitForExit();
			}
			
			
			return true;
		}
		
		public static bool ExtractTar(string filename, string destination)
		{
			
			Assembly _assembly = Assembly.GetExecutingAssembly();
			//Console.WriteLine(_assembly.GetFile)
			
			Stream inStream = _assembly.GetManifestResourceStream("gurumod.Resources." + filename);
			/*string[] mrns = _assembly.GetManifestResourceNames();
			
			for(int en = 0; en < mrns.Length; en++)
			{
				Console.WriteLine("mrns {0}", mrns[en]);
			}
			if(inStream == null) { Console.WriteLine("inStream is null :( gurumod.Resources.{0}", filename); }*/
			TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream);
			Console.WriteLine("Extracting {0} to {1}", filename, destination);
			tarArchive.ExtractContents(destination);
			tarArchive.CloseArchive();
			
			//Stream gZipStream = ICSharpCode.SharpZipLib.GZip.GZipInputStream(inStream);
			
			
			
			inStream.Close();
			
			return true;
		}
		
		private static bool LinuxBinFolderExists()
		{
			for(int elbf = 0; elbf < LinuxBinFolders.Length; elbf++)
			{
				if(LinuxBinFolders[elbf] != null)
				{
					if(System.IO.Directory.Exists(LinuxBinFolders[elbf]))
					{
						Installer.BinFolder = LinuxBinFolders[elbf];
						return true;
					}
				}
			}
			
			Installer.BinFolder = "/usr/bin/";
			return false;
		}
		
		private static bool LinuxDataFolderExists()
		{
			for(int elbf = 0; elbf < LinuxDataFolders.Length; elbf++)
			{
				if(LinuxDataFolders[elbf] != null && System.IO.Directory.Exists(LinuxDataFolders[elbf]))
				{
					Installer.DataFolder = LinuxDataFolders[elbf];
					return true;
				}
			}
			
			Installer.DataFolder = "/usr/share/" + Engine.EngineName + "/";
			return false;
		}
	}
}

