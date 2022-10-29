

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
			
			//Engine.ConfigPath = Installer.Installer.DataFolder;
			//Engine.ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Engine.EngineName) + "/";
			gurumod.Launcher.TheEngine = new Engine(args);
			
			if(!Directory.Exists(Engine.PFP(Engine.Configuration.SharedConfigPath)))
			{
				Console.WriteLine(Engine.EngineName + " " + Engine.EngineVersion);
				Console.WriteLine("Copyright 2012 - 2014 Brian Murphy");
				Console.WriteLine(" www.gurutronik.com");
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
			trk.Year = 2014;
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
}