// 
//  Track.cs
//  
//  Author:
//       guru <${AuthorEmail}>
// 
//  Copyright (c) 2012 - 2014 guru
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
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace gurumod
{
	[XmlRoot("Track")]
	public class Track
	{
		//	This class represents a track, or a full composed song/module
		[XmlIgnore()] public static int MaxSamples
		{
			get
			{
				return Engine.Configuration.MaxSamples;
			}
			set
			{
				Engine.Configuration.MaxSamples = value;
			}
		}// = 32;
		[XmlIgnore()] public static int MaxPatterns
		{
			get
			{
				return Engine.Configuration.MaxPatterns;
			}
			set
			{
				Engine.Configuration.MaxPatterns = value;
			}
		}// = 32;
		
		
		//[XmlElement("Samples")] public Sample[] Samples;
		[XmlIgnore()] public Sample[] Samples;
		[XmlElement("Author")] public string Author = "";
		[XmlElement("Title")] public string Title = "Untitled";
		[XmlElement("Tempo")] public int Tempo = 180;
		[XmlElement("Year")] public int Year = 2012;
		[XmlElement("ChannelCount")] public int ChannelCount = 12;
		[XmlElement("ChannelMuted")] public bool[] ChannelMuted;
		[XmlIgnore()] public int[] PatternSequence;
		//[XmlElement("Patterns")] public Pattern[] Patterns;// = new Pattern[Track.MaxPatterns];
		[XmlIgnore()] public Pattern[] Patterns;
		[XmlElement("DefaultPatternLength")] int DefaultPatternLength = 128;
		
		[XmlElement("Genre")] public string Genre = "";
		[XmlElement("WebSite")] public string WebSite = "";
		[XmlElement("Email")] public string Email = "";
		[XmlElement("Comments")] public string Comments = "";
		[XmlElement("GTVersion")] public string GTVersion = "0001";
		
		[XmlIgnore()] public long LastTurn = 0;
		[XmlIgnore()] public int CurrentPattern = 0;
		[XmlIgnore()] public int CurrentRow = 0;
		[XmlIgnore()] public bool PlayerEnabled = false;
		[XmlIgnore()] public bool LoopEnabled = false;
		[XmlIgnore()] public bool LoopPattern = true;
		[XmlIgnore()] public string MyPath = "";
		
		
		public Track ()
		{
			//Console.WriteLine("Constructing Track");
			if(MyPath == "")
			{
				if(Engine.CommandFlags.ContainsKey("-f"))
				{
					MyPath = Engine.CommandFlags["-f"];
				}
				else
				{
					MyPath = Engine.Configuration.TracksPath + "Untitled";
				}
				
				
			}
			
			if(!MyPath.EndsWith("/")) { MyPath = MyPath + "/"; }
			Console.WriteLine("Track() MyPath {0}", MyPath);
		}
		
		public void NewTrack()
		{
			Console.WriteLine("Generating an empty track.");
			Patterns = new Pattern[Engine.Configuration.MaxPatterns];
			Patterns[0] = new Pattern(ChannelCount, this.DefaultPatternLength);
			Samples = new Sample[Engine.Configuration.MaxSamples];
			
			for(int es = 0; es < Samples.Length; es++)
			{
				Samples[es] = new Sample();
			}

			ChannelMuted = new bool[ChannelCount];
			for(int ec = 0; ec < ChannelCount; ec++)
			{
				ChannelMuted[ec] = false;
			}

			Console.WriteLine("Empty track generated.");
			Console.WriteLine("End of newTrack()");
		}
		
		public void TakeTurn()
		{
			if(Samples == null) { Console.WriteLine("Samples[] is null"); }
			else
			{
				for(int es = 0; es < Track.MaxSamples; es++)
				{
					if(Samples[es] == null) { Console.WriteLine("Samples[{0}] is null", es); }
					else { Samples[es].LoadSample(); }
				}
			}
			
			if(Patterns == null)
			{
				Console.WriteLine("Patterns has come up null :(");
				/*Patterns = new Pattern[Track.MaxPatterns];
				Pattern.Load(0);
				Save();*/
			}
			
			if(ChannelMuted == null)
			{
				ChannelMuted = new bool[ChannelCount];
				for(int ech = 0; ech < ChannelCount; ech++)
				{
					ChannelMuted[ech] = false;
				}
			}
			
			if(PlayerEnabled)
			{
				double rowtime = (double)(60f / ((float)Tempo) / 8f);
				double rowoffset = rowtime * 0.8;
				
				if(((float)(Environment.TickCount - LastTurn)) / 1000f >= rowtime/*(60f / ((float)Tempo) / 8f)*/)
				{
					//Console.WriteLine("Playing row" + CurrentRow.ToString());
					//Console.WriteLine("Row " + CurrentRow.ToString() + ": " + Environment.TickCount.ToString() + " " + LastTurn.ToString() + " " + Tempo.ToString());
					Patterns[this.CurrentPattern].PlayRow(CurrentRow);
					CurrentRow++;
					
					//   OG
					LastTurn = Environment.TickCount;
					//if(LastTurn == 0) { LastTurn = Environment.TickCount; }
					//else { LastTurn = (long)(Environment.TickCount + (rowtime - rowoffset)); }
				}
				
				Patterns[this.CurrentPattern].ProcessMachines();
				
				if(CurrentRow >= Patterns[CurrentPattern].RowCount)
				{
					if(this.LoopEnabled && this.LoopPattern)
					{
						CurrentRow = 0;
					}
					else
					{
						int nextpattern = NextPattern(CurrentPattern);
						if(nextpattern > 0)
						{
							CurrentPattern = nextpattern;
							CurrentRow = 0;
						}
						else
						{
							if(LoopEnabled)
							{
								CurrentPattern = 0;
								CurrentRow = 0;
							}
							else
							{
								CurrentPattern = 0;
								CurrentRow = 0;
								PlayerEnabled = false;
							}
						}
						
					}
				}
			}


		}
		
		public void EnablePlayer()
		{
			PlayerEnabled = true;
			LastTurn = Environment.TickCount;
		}
		
		public void Save()
		{
			Console.WriteLine("Saving track as {0}", Engine.CommandFlags["-f"]);
			if(this.SaveAsGT())
			{
				Console.WriteLine("Save successful.");
			}
			else
			{
				Console.WriteLine("Save failed.");
			}
		}
		


		public bool SaveAsGT()
		{
			//	Save the Track in the native format.
			//
			//	First a header needs to be generated, followed by the other elements of the track.
			//	The format will be:
			//		gt-0001-pppppssssscccyyyytttllll
			//			p: 5 digit number of patterns
			//			s: 5 digit number of samples
			//			c: 3 digit number of channels
			//			y: 4 digit year
			//			t: 3 digit tempo
			//			l: 4 digit default pattern length
			//	Followed by:
			//		author<chr 0>title<chr 0>genre<chr 0>website<chr 0>email<chr 0>comments<chr 0>
			//		m
			//			m: muted channels.  repeats for however many channels there are, and is either 0 or 1
			//		ppppp...<chr 0>
			//			p: 5 digit pattern id, looped for pattern sequence
			//
			//	So from that, we have the header information.  Next comes the pattern information.
			//		rrrr...ccc...onnvvvsssiiiii...
			//			r: 4 digit number of rows in pattern
			//			c: 3 digit channel number/id
			//			o: 1 digit octave
			//			n: 2 digit note
			//			v: 3 digit volume
			//			s: 3 digitl special value
			//			i: 5 digit sample id
			//
			//	Now for sample data.  It gets a little more tricky.
			//		iiiiiyyyybbbbbbbbbbbbbbbbbbbcppppppppppssssssssssgmddddddddddddddddddd
			//			i: 5 digit sample id
			//			y: 4 digit year
			//			b: 19 digit bit rate
			//			c: 1 digit number of channels
			//			p: 10 digit bits per sample
			//			s: 10 digit sample rate
			//			g: 1 digit for using a wave generator (0 or 1)
			//			m: 1 digit for using a wave machine (0 or 1)
			//			d: 19 digit length of sound data
			//		n<chr 0>a<chr 0>f<chr 0>
			//			n: name of sample
			//			a: artist
			//			f: filename of sample
			//		sounddata as bytes
			//
			//	If the wave generator flag is 1, then:
			//		tfffffssssssllll
			//			t: sound type
			//			f: 5 digit frequency
			//			s: 6 digit sample rate
			//			l: 4 digit length in seconds
			//
			//	If the wave machine flag is 1, then:
			//		ssssssfffffgggppp
			//			s: 6 digit sample rate
			//			f: 5 digit frequency
			//			g: 3 digit number of generators
			//			p: 3 digit number of processors
			//	For each generator:
			//		tessssssfffffaaaa
			//			t: 1 digit generator type
			//			e: 1 digit enabled (0 or 1)
			//			s: 6 digit sample rate
			//			f: 5 digit frequency
			//			a: 4 digit amplitude (x.xx)
			//	For generator type 0 (oscillator)
			//		w
			//			w: 1 digit wave type
			//	For generator type 1 (wave player)
			//		cssssssbbbbbbbbbblllllllllllllllllllf<chr 0>
			//			c: 1 digit channel count
			//			s: 6 digit sample rate
			//			b: 10 digit bit rate
			//			l: 19 digit length of audio data
			//			f: filename
			//		list of 2-byte shorts of audio data
			//	For each processor:
			//		nnniiittt
			//			n: 3 digit processor id
			//			i: 3 digit number of inputs
			//			t: 3 digit processor type
			//	For each input of the processor:
			//		iiitttaaaa
			//			i: 3 digit source id
			//			t: 3 digit source type
			//			a: 4 digit amplitude (x.xx)
			//	For mixers
			//		m
			//			m: 1 digit combine method
			//	For envelopes:
			//		aaaaaaaaaabbbbddddddddddeeeessssssssssrrrrrrrrrr
			//			a: 10 digit attack
			//			b: 4 digit attack amplitude (x.xx)
			//			d: 10 digit decay
			//			e: 4 digit decay amplitude (x.xx)
			//			s: 10 digit sustain
			//			r: 10 digit release
			//	For gates
			//		llllllllllhhhhhhhhhh
			//			l: 10 digit MinGateManual
			//			h: 10 digit MaxGateManual
			//	For reverb
			//		wwwwwwwwwwdddddddddd
			//			w: 10 digit delay
			//			d: 10 digit decay

			MemoryStream gtstream = new MemoryStream();
			StreamWriter gtwriter = new StreamWriter(gtstream);

			gtwriter.AutoFlush = true;

			string tout = "gt-" + this.GTVersion + "-";
			int rpats = 0;
			for(int ep = 0; ep < this.Patterns.Length; ep++)
			{
				if(this.Patterns[ep] != null) { rpats++; }
			}
			string numpatterns = rpats.ToString("D5");
			int rsamps = 0;
			for(int es = 0; es < this.Samples.Length; es++)
			{
				if(this.Samples[es] != null) { rsamps++; }
			}
			string numsamples = rsamps.ToString("D5");
			string numchannels = this.ChannelCount.ToString("D3");
			string year = this.Year.ToString("D4");
			string tempo = this.Tempo.ToString("D3");
			string defpatrows = this.DefaultPatternLength.ToString("D4");

			tout = tout + numpatterns + numsamples + numchannels + year + tempo + defpatrows;
			tout = tout + this.Author + "\0";
			tout = tout + this.Title + "\0";
			tout = tout + this.Genre + "\0";
			tout = tout + this.WebSite + "\0";
			tout = tout + this.Email + "\0";
			tout = tout + this.Comments + "\0";

			for(int ech = 0; ech < this.ChannelCount; ech++)
			{
				if(this.ChannelMuted.Length > ech)
				{

					if(this.ChannelMuted[ech]) { tout = tout + "1"; }
					else { tout = tout + "0"; }

				}
				else
				{
					//	For some reason the ChannelMuted array isn't as large as the
					//	number of channels that exist.
					tout = tout + "0";
				}
			}

			//	Determine pattern sequence <<<<<< DO THIS !!!!!!
			tout = tout + "00000\0";
			gtwriter.Write(tout);

			for(int epat = 0; epat < this.Patterns.Length; epat++)
			{
				if(this.Patterns[epat] != null)
				{
					gtwriter.Write(this.Patterns[epat].RowCount.ToString("D4"));
					if(this.Patterns[epat] != null)
					{
						for(int ech = 0; ech < ChannelCount; ech++)
						{
							string tmpcol = this.Patterns[epat].ChannelGTString(ech);
							gtwriter.Write(tmpcol);

							//File.WriteAllText("/tmp/gtcol"+ech.ToString()+".txt", tmpcol);
							//gtwriter.Write(this.Patterns[epat].GTString());
						}
					}
				}
			}

			//gtwriter.Write(tout);

			for(int esam = 0; esam < this.Samples.Length; esam++)
			{
				if(this.Samples[esam] != null)
				{
					gtstream.Write(this.Samples[esam].GTString(), 0, this.Samples[esam].GTString().Length);
					//break;
				}

			}



			File.WriteAllBytes(Engine.CommandFlags["-f"], gtstream.ToArray());

			return true;
		}
		
		public Stream Serialize()
		{
			Stream toret = new MemoryStream();
			
			try
			{
				XmlSerializer s = new XmlSerializer(typeof(Track));
				s.Serialize(toret, this);
			}
			catch(Exception ex)
			{
				Console.WriteLine("Exceptiong while serializing the track.");
				Console.WriteLine(ex.Message);
				return null;
			}
			
			return toret;
		}

		public static bool LoadGT(string filename)
		{
			if(!File.Exists(filename)) { return false; }

			FileStream lf = File.OpenRead(filename);
			BinaryReader reader = new BinaryReader(lf);

			reader.BaseStream.Position = 0;
			byte[] rbuf = reader.ReadBytes (8);

			if(rbuf.Length < 8) { Console.WriteLine("File is too small to be valid."); return false; }
			string headtag = Encoding.UTF8.GetString(rbuf);

			if(headtag.Substring(0, 3) != "gt-" || headtag.Substring(7) != "-")
			{
				Console.WriteLine("File had an invalid header.");
				return false;
			}

			string version = headtag.Substring(3, 4);
			if(version != "0001") { Console.WriteLine("Version mismatch."); return false; }

			Serializers.LoadGT_0001.Load(reader);
			if(Engine.TheTrack == null)
			{
				Console.WriteLine("Track deserializer returned null.");
				return false;
			}
			return true;
		}


		
		public static void Load(string trackpath)
		{
			//Track.LoadGT("/tmp/test.gt");
			if(!Track.LoadGT(trackpath))
			{
				Console.WriteLine("Failed to load track {0}", trackpath);
				Environment.Exit(0);
			}
			else
			{
				Console.WriteLine("Loaded track {0}", trackpath);
			}



		}
		
		public int AddPattern()
		{
			
			
			Pattern[] tpatz = new Pattern[Track.MaxPatterns];
			
			int firstnull = -1;
			for(int eogp = 0; eogp < this.Patterns.Length; eogp++)
			{
				if(Patterns[eogp] != null)
				{
					tpatz[eogp] = Patterns[eogp];
				}
				else
				{
					tpatz[eogp] = null;
					if(firstnull == -1) { firstnull = eogp; }
				}
			}
			
			if(firstnull == -1) { firstnull = Patterns.Length; }
			
			tpatz[firstnull] = new Pattern(this.ChannelCount, this.DefaultPatternLength);
			Patterns = tpatz;
			
			return firstnull;
			
		}
		
		public int AddPattern(int cloneid)
		{
			int newid = AddPattern();
			string trackfile = Engine.CommandFlags["-f"];
			if(!trackfile.EndsWith("/")) { trackfile = trackfile + "/"; }
			
			Patterns[cloneid].Save(Engine.PFP(trackfile), cloneid);
			Pattern.Load(newid, trackfile, cloneid);
			//Pattern.Load(newid, Engine.PFP(Engine.ConfigPath + "tmp/" +cloneid.ToString() + ".xml"));
			//System.IO.File.Delete(Engine.PFP(Engine.ConfigPath + "tmp/" + cloneid.ToString() + ".xml"));
			return newid;
		}
		
		public int NextPattern(int frompattern)
		{
			if(frompattern == Patterns.Length - 1) { return 0; }
			
			for(int epat = frompattern + 1; epat < Patterns.Length; epat++)
			{
				if(Patterns[epat] != null) { return epat; }
			}
			
			return 0;
		}
		

	}
}

