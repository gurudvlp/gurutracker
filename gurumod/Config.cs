// 
//  Config.cs
//  
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
// 
//  Copyright (c) 2012 - 2014 Brian Murphy
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
using System.Collections;
using System.Collections.Generic;
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

		[XmlIgnore()] public Dictionary<int, double>[] NoteFreq = new Dictionary<int, double>[8];
		
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


			if(NoteFreq == null) { InitNoteFrequencies(); }

		}

		public void InitNoteFrequencies()
		{
			for(int etn = 0; etn < 8; etn++)
			{
				NoteFreq[etn] = new Dictionary<int, double>();
			}

			NoteFreq[0].Add(0, 16.35);
			NoteFreq[0].Add(1, 17.32);
			NoteFreq[0].Add(2, 18.35);
			NoteFreq[0].Add(3, 19.45);
			NoteFreq[0].Add(4, 20.6);
			NoteFreq[0].Add(5,  21.83);
			NoteFreq[0].Add(6,  23.12);
			NoteFreq[0].Add(7, 24.5);
			NoteFreq[0].Add(8, 25.96);
			NoteFreq[0].Add(9, 27.5);
			NoteFreq[0].Add(10, 29.14);
			NoteFreq[0].Add(11, 30.87);

			NoteFreq[1].Add(0, 32.7);
			NoteFreq[1].Add(1, 34.65);
			NoteFreq[1].Add(2, 36.71);
			NoteFreq[1].Add(3, 38.89);
			NoteFreq[1].Add(4, 41.20);
			NoteFreq[1].Add(5, 43.65);
			NoteFreq[1].Add(6, 46.25);
			NoteFreq[1].Add(7, 49.0);
			NoteFreq[1].Add(8, 51.91);
			NoteFreq[1].Add(9, 55.0);
			NoteFreq[1].Add(10, 58.27);
			NoteFreq[1].Add(11, 61.74);
			
			NoteFreq[2].Add(0, 65.41);
			NoteFreq[2].Add(1, 69.30);
			NoteFreq[2].Add(2, 73.42);
			NoteFreq[2].Add(3, 77.78);
			NoteFreq[2].Add(4, 82.41);
			NoteFreq[2].Add(5, 87.31);
			NoteFreq[2].Add(6, 92.5);
			NoteFreq[2].Add(7, 98.0);
			NoteFreq[2].Add(8, 103.83);
			NoteFreq[2].Add(9, 110.0);
			NoteFreq[2].Add(10, 116.54);
			NoteFreq[2].Add(11, 123.47);

			NoteFreq[3].Add(0, 130.81);
			NoteFreq[3].Add(1, 138.59);
			NoteFreq[3].Add(2, 146.83);
			NoteFreq[3].Add(3, 155.56);
			NoteFreq[3].Add(4, 164.81);
			NoteFreq[3].Add(5, 174.61);
			NoteFreq[3].Add(6, 185.0);
			NoteFreq[3].Add(7, 196.0);
			NoteFreq[3].Add(8, 207.65);
			NoteFreq[3].Add(9, 220.0);
			NoteFreq[3].Add(10, 233.08);
			NoteFreq[3].Add(11, 246.94);

			NoteFreq[4].Add(0, 261.63);
			NoteFreq[4].Add(1, 277.18);
			NoteFreq[4].Add(2, 293.66);
			NoteFreq[4].Add(3, 311.13);
			NoteFreq[4].Add(4, 329.63);
			NoteFreq[4].Add(5, 349.23);
			NoteFreq[4].Add(6, 369.99);
			NoteFreq[4].Add(7, 392.00);
			NoteFreq[4].Add(8, 415.30);
			NoteFreq[4].Add(9, 440.0);
			NoteFreq[4].Add(10, 466.16);
			NoteFreq[4].Add(11, 493.88);

			NoteFreq[5].Add(0, 523.25);
			NoteFreq[5].Add(1, 554.37);
			NoteFreq[5].Add(2, 587.33);
			NoteFreq[5].Add(3, 622.25);
			NoteFreq[5].Add(4, 659.26);
			NoteFreq[5].Add(5, 698.46);
			NoteFreq[5].Add(6, 739.99);
			NoteFreq[5].Add(7, 783.99);
			NoteFreq[5].Add(8, 830.61);
			NoteFreq[5].Add(9, 880.0);
			NoteFreq[5].Add(10, 932.33);
			NoteFreq[5].Add(11, 987.77);

			NoteFreq[6].Add(0, 1046.5);
			NoteFreq[6].Add(1, 1108.73);
			NoteFreq[6].Add(2, 1174.66);
			NoteFreq[6].Add(3, 1244.51);
			NoteFreq[6].Add(4, 1318.51);
			NoteFreq[6].Add(5, 1396.91);
			NoteFreq[6].Add(6, 1479.98);
			NoteFreq[6].Add(7, 1567.98);
			NoteFreq[6].Add(8, 1661.22);
			NoteFreq[6].Add(9, 1760.0);
			NoteFreq[6].Add(10, 1864.66);
			NoteFreq[6].Add(11, 1975.53);

			NoteFreq[7].Add(0, 2093.0);
			NoteFreq[7].Add(1, 2217.46);
			NoteFreq[7].Add(2, 2349.32);
			NoteFreq[7].Add(3, 2489.02);
			NoteFreq[7].Add(4, 2637.02);
			NoteFreq[7].Add(5, 2793.83);
			NoteFreq[7].Add(6, 2959.96);
			NoteFreq[7].Add(7, 3135.96);
			NoteFreq[7].Add(8, 3322.44);
			NoteFreq[7].Add(9, 3520.0);
			NoteFreq[7].Add(10, 3729.31);
			NoteFreq[7].Add(11, 3951.07);
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


			Engine.Configuration.InitNoteFrequencies();

		}
		
	}
}

