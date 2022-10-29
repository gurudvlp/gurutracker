using OpenTK.Audio;

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
			
			//	Before launching the full program we will check if the user
			//	is simply trying to run a sound check.
			if(args.Length > 0
			&& (args[0].ToLower() == "soundtest"
				|| args[0].ToLower() == "--soundtest"
				|| args[0].ToLower() == "--diag"))
			{
				Launcher.soundTest(args);
				Environment.Exit(0);
			}

			if(!Installer.Installer.IsInstalled())
			{
				if(args.Length > 0 && 
				   (args[0].ToLower() == "install"
				 	|| args[0].ToLower() == "--install"
				 	|| args[0].ToLower() == "-install"))
				{

					Installer.Installer.Install();
				}
				else
				{
					Console.WriteLine("Please run with --install flag");
					Environment.Exit(0);
				}
			}
			else
			{
				Environment.CurrentDirectory = "/usr/share/gurutracker/bin";
			}
			
			gurumod.Launcher.TheEngine = new Engine(args);
			
			if(!Directory.Exists(Engine.PFP(Engine.Configuration.SharedConfigPath)))
			{
				Console.WriteLine(Engine.EngineName + " " + Engine.EngineVersion);
				Console.WriteLine("Copyright 2012 - 2022 Brian Murphy");
				Console.WriteLine(" www.gurutronik.com");
				Console.WriteLine(" ");
				Console.WriteLine(Engine.EngineName + " could not find the configuration directory.");
				Console.WriteLine("All of the config files need to be placed in");
				Console.WriteLine(Engine.PFP(Engine.ConfigPath));
				Console.WriteLine("for this to run properly.  Please put the files there and try again.");
				Environment.Exit(0);
			}
			
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

		//	Perform some checks of the audio systems.  This is useful for debugging.
		public static void soundTest(string[] args)
		{
			Console.WriteLine("Running sound diagnostics...");

			//	Enumerate available devices.
			string devices = OpenTK.Audio.OpenAL.ALC.GetString(
				OpenTK.Audio.OpenAL.ALDevice.Null, 
				OpenTK.Audio.OpenAL.AlcGetString.DeviceSpecifier
			);
			
			Console.WriteLine("Available devices: {0}", devices);

			return;
		}
	}
}