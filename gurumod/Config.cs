// 
//  Config.cs
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
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace gurumod
{
	[XmlRoot("Config")]
	public class Config
	{
		[XmlElement("TracksPath")] public string TracksPath = "";
		[XmlElement("SharedSamples")] public string SharedSamples = Engine.ConfigPath + "samples/";
		[XmlElement("PersonalSamples")] public string PersonalSamples = "";
		[XmlElement("DisplayDebug")] public bool DisplayDebug = true;
		[XmlElement("WebListenPort")] public int WebListenPort = 6789;
		[XmlElement("MaxSamples")] public int MaxSamples = 32;
		[XmlElement("MaxPatterns")] public int MaxPatterns = 32;
		[XmlElement("PersonalConfigPath")] public string PersonalConfigPath = "";
		[XmlElement("SharedConfig")] public string SharedConfigPath = "";
		[XmlElement("WebTemplateDir")] public string WebTemplateDir = "";
		
		public Config ()
		{
		}
		
		public void Initialize()
		{
						
			PersonalConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			if(!PersonalConfigPath.EndsWith("/")) { PersonalConfigPath = PersonalConfigPath + "/"; }
			PersonalConfigPath = PersonalConfigPath + "gurutracker/";
			if(!Directory.Exists(Engine.PFP(PersonalConfigPath))) { Directory.CreateDirectory(Engine.PFP(PersonalConfigPath)); }
			
			SharedConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			if(!SharedConfigPath.EndsWith("/")) { SharedConfigPath = SharedConfigPath + "/"; }
			SharedConfigPath = SharedConfigPath + "gurutracker/";
			
			SharedSamples = SharedConfigPath + "samples/";
			
			PersonalSamples = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			if(!PersonalSamples.EndsWith("/")) { PersonalSamples = PersonalSamples + "/"; }
			if(!Directory.Exists(Engine.PFP(PersonalSamples + "gurutracker"))) { Directory.CreateDirectory(Engine.PFP(PersonalSamples + "gurutracker")); }
			if(!Directory.Exists(Engine.PFP(PersonalSamples + "gurutracker/Samples"))) { Directory.CreateDirectory(Engine.PFP(PersonalSamples + "gurutracker/Samples")); }
			PersonalSamples = PersonalSamples + "gurutracker/Samples/";
			
			
			TracksPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			if(!TracksPath.EndsWith("/")) { TracksPath = TracksPath + "/"; }
			TracksPath = TracksPath + "gurutracker/Tracks/";
			if(!Directory.Exists(TracksPath)) { Directory.CreateDirectory(Engine.PFP(TracksPath)); }
			
			WebTemplateDir = SharedConfigPath + "Interfaces/Web/templates/";
			
			//Directory.SetCurrentDirectory(this.SharedConfigPath + "bin");
		}
		
		public void Save()
		{
			string confpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			if(!confpath.EndsWith("/")) { confpath = confpath + "/gurutracker/config.xml"; }
			
			XmlSerializer s = new XmlSerializer(typeof(Config));
			TextWriter w = new StreamWriter(Engine.PFP(confpath));
			s.Serialize(w, this);
		}
		
		public static void Load()
		{
			string conffile = "";
			if(!File.Exists(Engine.PFP(Engine.Configuration.PersonalConfigPath + "config.xml")))
			{
				//	The user running gurutracker does not have their own config file, so we look for
				//	a system wide one.
				
				if(File.Exists(Engine.PFP(Engine.Configuration.SharedConfigPath + "config.xml")))
				{
					//	There *is* a global configuration file to use.
					conffile = Engine.Configuration.SharedConfigPath + "config.xml";
				}
			}
			else
			{
				conffile = Engine.Configuration.PersonalConfigPath + "config.xml";
			}
			
			if(conffile == "")
			{
				Console.WriteLine("Could not find a configuration file.  Using defaults.");
				return;
			}
			
			XmlSerializer s = new XmlSerializer(typeof(Config));
			TextReader tr = new StreamReader(Engine.PFP(conffile));
			Engine.Configuration = new Config();
			Engine.Configuration = (Config)s.Deserialize(tr);
			
		}
		
	}
}

