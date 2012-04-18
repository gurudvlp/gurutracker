// 
//  Pattern.cs
//  
//  Author:
//       guru <${AuthorEmail}>
// 
//  Copyright (c) 2012 guru
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
using System.Text;
using System.IO;

namespace gurumod
{
	[XmlRoot("Pattern")]
	public class Pattern
	{
		//	One pattern of the track
		[XmlElement("Channels")] public PatternChannel[] Channels;
		
		[XmlIgnore()] private int rowcnt = 0;
		
		[XmlElement("RowCount")] public int RowCount
		{
			get
			{
				return rowcnt;
			}
			set
			{
				rowcnt = value;
			}
		}
		
		[XmlIgnore()] private int channelcnt = 0;
		[XmlElement("ChannelCount")] public int ChannelCount
		{
			get
			{
				return channelcnt;
			}
			set
			{
				PatternChannel[] tchan = new PatternChannel[value];
				for(int ech = 0; ech < value; ech++)
				{
					if(ech < channelcnt)
					{
						tchan[ech] = Channels[ech];
					}
					else
					{
						tchan[ech] = new PatternChannel(RowCount);
					}
				}
				Channels = tchan;
				
				channelcnt = value;
			}
		}
		
		public Pattern()
		{
			
		}
		
		public Pattern (int channels, int length)
		{
			this.RowCount = length;
			this.ChannelCount = channels;
		}
		
		public void PlayRow(int row)
		{
			//	Play a row of noise from this pattern.
			if(row > this.RowCount || row < 0) { return; }
			
			for(int ech = 0; ech < this.ChannelCount; ech++)
			{
				
				if(Channels[ech] != null)
				{
					//Console.WriteLine("Playing channel {0}", ech);
					Channels[ech].ChannelID = ech;
					Channels[ech].PlayElement(row);
				}
			}
			
		}
		
		public void ProcessMachines()
		{
			for(int ech = 0; ech < this.ChannelCount; ech++)
			{
				if(Channels[ech] != null)
				{
					Channels[ech].ProcessMachine();
				}
			}
		}
		
		public void Save()
		{
			//Save(Engine.PFP(Engine.ConfigPath + "pattern.xml"));
		}
		
		public void Save(string TrackName, int patternid)
		{
			Console.WriteLine("Saving Pattern {0} {1}", patternid, TrackName);
			if(!Directory.Exists(TrackName + "Patterns"))
			{
				Directory.CreateDirectory(TrackName + "Patterns");
			}
			
			string filename = TrackName + "Patterns/" + patternid.ToString() + ".xml";
			try
			{
				XmlSerializer s = new XmlSerializer( typeof(Pattern) );
				TextWriter w = new StreamWriter(Engine.PFP(filename));
				s.Serialize( w, this );
				w.Close();
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error saving pattern");
				Console.WriteLine(ex.Message);
				
				
			}
		}
		
		public static void Load(int patternid, string TrackPath)
		{
			Load(patternid, TrackPath, patternid);
		}
		
		public static void Load(int patternid, string TrackPath, int cloneid)
		{
			Console.WriteLine("Loading pattern {0} {1}", patternid, TrackPath);
			XmlSerializer s = new XmlSerializer(typeof(Pattern));
			
			string filename = TrackPath + "Patterns/" + cloneid.ToString() + ".xml";
			
			TextReader tr = new StreamReader(filename);
			if(Engine.TheTrack == null) { Console.WriteLine ("TheTrack is null!"); }
			if(Engine.TheTrack.Patterns == null) { Console.WriteLine("TheTrack.Patterns is null!"); }
			Engine.TheTrack.Patterns[patternid] = new Pattern();
			Engine.TheTrack.Patterns[patternid] = (Pattern)s.Deserialize(tr);
	

			tr.Close();
		}
	}
}

