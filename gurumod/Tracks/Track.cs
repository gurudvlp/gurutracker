// 
//  Track.cs
//  
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
// 
//  Copyright (c) 2012 - 2022 Brian Murphy
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

using gurumod.Logging;

namespace gurumod
{
	[XmlRoot("Track")]
	[Serializable()]
	public class Track : ISerializable
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

		private int _PatternCount = 0;
		[XmlIgnore()] public int PatternCount
		{
			get
			{
				return _PatternCount;
			}
			set
			{
				_PatternCount = value;
			}
		}

		[XmlIgnore()] public Sample[] Samples;
		[XmlElement("Author")] public string Author = "";
		[XmlElement("Title")] public string Title = "Untitled";
		[XmlElement("Tempo")] public int Tempo = 180;
		[XmlElement("Year")] public int Year = 2014;
		[XmlElement("ChannelCount")] public int ChannelCount = 12;
		[XmlElement("ChannelMuted")] public bool[] ChannelMuted;
		[XmlIgnore()] public int[] PatternSequence;
		[XmlIgnore()] public Pattern[] Patterns;
		[XmlElement("DefaultPatternLength")] public int DefaultPatternLength = 128;
		
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
			if(MyPath == "")
			{
				if(Engine.CommandFlags.ContainsKey("-f")) { MyPath = Engine.CommandFlags["-f"]; }
				else { MyPath = Engine.Configuration.TracksPath + "Untitled"; }
			}
			
			if(!MyPath.EndsWith("/")) { MyPath = MyPath + "/"; }
			Console.WriteLine("Track() MyPath {0}", MyPath);
		}

		public Track(SerializationInfo info, StreamingContext ctxt)
		{
			Author = (string)info.GetValue("Author", typeof(string));
			Title = (string)info.GetValue("Title", typeof(string));
			Tempo = (int)info.GetValue("Tempo", typeof(int));
			Year = (int)info.GetValue("Year", typeof(int));
			ChannelCount = (int)info.GetValue("ChannelCount", typeof(int));
			ChannelMuted = (bool[])info.GetValue("ChannelMuted", typeof(bool[]));
			DefaultPatternLength = (int)info.GetValue("DefaultPatternLength", typeof(int));
			Genre = (string)info.GetValue("Genre", typeof(string));
			WebSite = (string)info.GetValue("WebSite", typeof(string));
			Email = (string)info.GetValue("Email", typeof(string));
			Comments = (string)info.GetValue("Comments", typeof(string));
			PatternSequence = (int[])info.GetValue("PatternSequence", typeof(int[]));

			//Samples = new Sample[32];
			//for(int es = 0; es < 32; es++) { Samples[es] = new Sample(); }
			Samples = (Sample[])info.GetValue("Samples", typeof(Sample[]));
			Patterns = (Pattern[])info.GetValue("Patterns", typeof(Pattern[]));

			if(Patterns == null)
			{
				Patterns = new Pattern[32];
				Patterns[0] = new Pattern(12, 128);
			}

			if(Samples == null)
			{
				Samples = new Sample[32];
				for(int es = 0; es < Samples.Length; es++)
				{
					Samples[es] = new Sample();
					Samples[es].ID = es;
				}
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("Author", Author);
			info.AddValue("Title", Title);
			info.AddValue("Tempo", Tempo);
			info.AddValue("Year", Year);
			info.AddValue("ChannelCount", ChannelCount);
			info.AddValue("ChannelMuted", ChannelMuted);
			info.AddValue("DefaultPatternLength", DefaultPatternLength);
			info.AddValue("Genre", Genre);
			info.AddValue("WebSite", WebSite);
			info.AddValue("Email", Email);
			info.AddValue("Comments", Comments);
			info.AddValue("PatternSequence", PatternSequence);
			info.AddValue("Samples", Samples);
			info.AddValue("Patterns", Patterns);
		}
		
		//	Generate a brand new, empty track.
		public void NewTrack()
		{
			Log.lInfo("Generating an empty track.", "Track", "NewTrack");

			Patterns = new Pattern[Engine.Configuration.MaxPatterns];
			Patterns[0] = new Pattern(ChannelCount, this.DefaultPatternLength);
			Samples = new Sample[Engine.Configuration.MaxSamples];
			
			for(int es = 0; es < Samples.Length; es++)
			{
				Samples[es] = new Sample();
			}

			//	Set all channels as not muted by default.
			ChannelMuted = new bool[ChannelCount];
			for(int ec = 0; ec < ChannelCount; ec++)
			{
				ChannelMuted[ec] = false;
			}

		}
		
		//	TakeTurn() is where the actually processing for this track happens.
		//	It is called pretty much as frequently as possible from the Engine.
		public void TakeTurn()
		{
			if(Samples == null) { Log.lWarning("Samples[] is null!", "Track", "TakeTurn"); }
			else
			{
				//	Loop through each sample and make sure it's loaded.
				//	Sample.LoadSample will return immediately if the sample has
				//	already been loaded, so this is not much of a time penalty.
				for(int es = 0; es < Track.MaxSamples; es++)
				{
					if(Samples[es] == null) { Log.lWarning("Sample " + es.ToString() + " is null!", "Track", "TakeTurn"); }
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
			
			//	Process playing the track if it is enabled (ie Play was pressed)
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
			Log.lInfo("Saving track as: " + Engine.CommandFlags["-f"], "Track", "Save");

			if(this.SaveAsGT()) { Log.lInfo("Save successful.", "Track", "Save"); }
			else { Log.lInfo("Save failed.", "Track", "Save"); }
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
			/*
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
					byte[] sampstr = this.Samples[esam].GTString();
					gtstream.Write(sampstr, 0, sampstr.Length);

					File.WriteAllBytes("/tmp/gt-smp-"+esam.ToString()+".txt", sampstr);
					//break;
				}

			}


			gtstream.Flush();
			File.WriteAllBytes(Engine.CommandFlags["-f"], gtstream.ToArray());
			*/
			//
			//	Testing binary serialization.
			FileStream fs = File.Create(Engine.CommandFlags["-f"]);
			BinaryFormatter formatter = new BinaryFormatter();
			StreamWriter gtwriter = new StreamWriter(fs);

			gtwriter.Write("gt-" + this.GTVersion + "-");
			gtwriter.Flush();

			formatter.Serialize(fs, this);
			fs.Close();

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
				Log.lWarning("Exception while serializing the track.", "Track", "Serialize");
				Log.lWarning(ex.Message, "Track", "Serialize");
				
				return null;
			}
			
			return toret;
		}

		public static bool LoadGT(string filename)
		{
			/*if(!File.Exists(filename)) { return false; }

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
			}*/

			if(!File.Exists(filename)) { return false; }

			FileStream fs = File.Open(filename, FileMode.Open);
			BinaryFormatter bform = new BinaryFormatter();

			BinaryReader reader = new BinaryReader(fs);
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

			try	{ Engine.TheTrack = (Track)bform.Deserialize(fs); }
			catch(Exception ex)
			{
				Console.WriteLine("Exception while deserializing track.");
				Console.WriteLine(ex.Message);
			}
			fs.Close();

			for(int es = 0; es < Engine.TheTrack.Samples.Length; es++)
			{
				if(Engine.TheTrack.Samples[es] != null && Engine.TheTrack.Samples[es].WaveMachine != null)
				{
					Console.WriteLine("Sample: {0}, ProcLen {1}  ProcTypeLen {2}", es,
					                  Engine.TheTrack.Samples[es].WaveMachine.Processors.Length,
					                  Engine.TheTrack.Samples[es].WaveMachine.ProcessorTypes.Length);
				}
			}
			/*
			for(int ep = 0; ep < 5; ep++)
			{
				bool isnull = false;
				if(Engine.TheTrack.Samples[ep] == null) { isnull = true; } else { isnull = false; }
				Console.WriteLine("Samples {0} Null: {1}", ep, isnull);

				if(Engine.TheTrack.Samples[ep] != null)
				{
					Console.WriteLine("\tSoundData Null: {0}", (Engine.TheTrack.Samples[ep].SoundData == null));
					Console.WriteLine("\tUseWaveGenerator: {0}", Engine.TheTrack.Samples[ep].UseWaveGenerator);
					Console.WriteLine("\tUseWaveMachine: {0}", Engine.TheTrack.Samples[ep].UseWaveMachine);

					if(Engine.TheTrack.Samples[ep].UseWaveMachine)
					{
						Console.WriteLine("\t\tWaveMachine Null: {0}", (Engine.TheTrack.Samples[ep].WaveMachine == null));

						Console.WriteLine("\t\tProcessors Null: {0}",  (Engine.TheTrack.Samples[ep].WaveMachine.Processors == null));
						Console.WriteLine("\t\tGenerators Null: {0}",  (Engine.TheTrack.Samples[ep].WaveMachine.Generators == null));

						for(int ex = 0; ex < Engine.TheTrack.Samples[ep].WaveMachine.Processors.Length; ex++)
						{
							Console.WriteLine("\t\t\tProcessor {0} Null: {1}", ex, (Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex] == null));
							if(Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex] != null)
							{
								Console.WriteLine("\t\t\t\tInputs Null: {0}", (Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].Inputs == null));
								Console.WriteLine("\t\t\t\tInput Count: {0}", (Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].InputCount));
								Console.WriteLine("\t\t\t\tProcessor Type: {0}", Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].ProcessorType);

								if(Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].Inputs != null)
								{
									for(int einp = 0; einp < Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].Inputs.Length; einp++)
									{
										Console.WriteLine("\t\t\t\t\tInput {0} Null: {1}", einp, (Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].Inputs[einp] == null));
										if(Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].Inputs[einp] != null)
										{
											Console.WriteLine("\t\t\t\t\t\tSourceID: {0}", Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].Inputs[einp].SourceID);
											Console.WriteLine("\t\t\t\t\t\tSourceType: {0}", Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].Inputs[einp].SourceType);
											Console.WriteLine("\t\t\t\t\t\tAmplitude: {0}", Engine.TheTrack.Samples[ep].WaveMachine.Processors[ex].Inputs[einp].Amplitude);
										}
									}
								}
							}
						}
					}
				}
			}

			Environment.Exit(0);*/
			return true;
		}


		//	Load a track that is already saved to disk.
		public static void Load(string trackpath)
		{
			if(!Track.LoadGT(trackpath)) { Log.lError("Failed to load track: " + trackpath, "Track", "Load"); }
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

