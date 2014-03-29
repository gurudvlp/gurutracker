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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;

namespace gurumod
{
	[XmlRoot("Pattern")]
	[Serializable()]
	public class Pattern : ISerializable
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

		public Pattern(SerializationInfo info, StreamingContext ctxt)
		{
			Channels = (PatternChannel[])info.GetValue("Channels", typeof(PatternChannel[]));
			channelcnt = (int)info.GetValue("ChannelCount", typeof(int));
			RowCount = (int)info.GetValue("RowCount", typeof(int));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("RowCount", RowCount);
			info.AddValue("Channels", Channels);
			info.AddValue("ChannelCount", Channels.Length);
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

		public string GTString()
		{
			//		rrrr...ccc...onnvvvsssiiiii...
			//			r: 4 digit number of rows in pattern
			//			c: 3 digit channel number/id
			//			o: 1 digit octave
			//			n: 2 digit note
			//			v: 3 digit volume
			//			s: 3 digitl special value
			//			i: 5 digit sample id
			string tout = this.RowCount.ToString("D4");

			for(int echan = 0; echan < this.Channels.Length; echan++)
			{


				//Console.WriteLine(tout);
				//	Environment.Exit(0);
			}
			Console.WriteLine("Length of tout: {0}", tout.Length);

			return tout;
		}

		public string ChannelGTString(int channel)
		{
			string tout = channel.ToString("D3");


			for(int erow = 0; erow < this.RowCount; erow++)
			{
				if(this.Channels[channel].Elements[erow] != null)
				{

					string octave = this.Channels[channel].Elements[erow].Octave.ToString();
					string note = this.Channels[channel].Elements[erow].Note.ToString("D2");
					string volume = this.Channels[channel].Elements[erow].Volume.ToString("D3");
					string special = this.Channels[channel].Elements[erow].SpecialControl.ToString("D3");
					string sampleid = this.Channels[channel].Elements[erow].SampleID.ToString("D5");
					string telement = "";

					if(octave.Length > 1) { octave = octave.Substring(octave.Length - 1); }
					if(note.Length > 2) { note = "-" + note.Substring(note.Length - 1); }
					if(volume.Length > 3) { volume = "-" + volume.Substring(volume.Length - 2); }
					if(special.Length > 3) { special = "-" + special.Substring(special.Length - 2); }
					if(sampleid.Length > 5) { sampleid = "-" + sampleid.Substring(sampleid.Length - 4); }

					//Console.WriteLine("Channel {0} Row {1}\t {2}{3}{4}{5}{6}", channel, erow, octave, note, volume, special, sampleid);

					telement = octave + note + volume + special + sampleid;
					tout = tout + telement;
				}
				else
				{
					tout = tout + "5-1-01-01-0001";
				}
				//Console.WriteLine("octave {0}\nnote {1}\nvolume {2}\nspecial {3}\nsid {4}\n{5}\n\n", octave, note, volume, special, sampleid, telement);

			}

			return tout;
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

