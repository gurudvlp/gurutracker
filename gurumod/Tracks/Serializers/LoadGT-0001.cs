//
//  LoadGT-0001.cs
//
//  Author:
//       guru <${AuthorEmail}>
//
//  Copyright (c) 2014 guru
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
using System.Text;
using System.IO;

namespace gurumod.Serializers
{
	public class LoadGT_0001
	{



		public LoadGT_0001 ()
		{
		}


		public static bool Load(BinaryReader reader)
		{

			if(!GetTrackHeader(reader)) { Console.WriteLine("Track header could not be read."); return false; }
			if(!GetMutedChannels(reader)) { Console.WriteLine("Muted channel list could not be read."); return false; }
			if(!GetPatternSequence(reader)) { Console.WriteLine("Pattern sequence could not be read."); return false; }
			if(!GetPatternData(reader)) { Console.WriteLine("Pattern data could not be read."); return false; }
			if(!GetSampleData(reader)) { Console.WriteLine("Sample data could not be read."); return false; }





			//GrabSamples(reader, nosamps);
			if(Engine.TheTrack.Samples == null)
			{
				Console.WriteLine("No samples were loaded.");
				Engine.TheTrack.Samples = new Sample[Engine.Configuration.MaxSamples];
				for(int es = 0; es < Engine.TheTrack.Samples.Length; es++)
				{
					Engine.TheTrack.Samples[es] = new Sample();
				}
			}
			return true;
		}

	
	

	/*	public static bool GrabSamples(BinaryReader reader, int numsamples)
		{
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

			Engine.TheTrack.Samples = new Sample[Engine.Configuration.MaxSamples];
			for(int es = 0; es < Engine.Configuration.MaxSamples; es++) { Engine.TheTrack.Samples[es] = new Sample(); }

			for(int esamp = 0; esamp < numsamples; esamp++)
			{
				int sampid = -1;
				int year = 0;
				long bitrate = 44100;
				int nochans = 1;
				long bitspersample = 2;
				long samplerate = 2;
				int wavegen = 0;
				int wavemach = 0;
				long sounddatalen = 6;
				string sampname = "";
				string artist = "";
				string sampfilename = "";

				if(!GrabInt(reader, out sampid, 5))
				{
					Console.WriteLine("Sample header for id corrupt"); return false;
				}

				if(!GrabInt(reader, out year, 4))
				{
					Console.WriteLine("Sample header for year corrupt"); return false;
				}

				if(!GrabLong(reader, out bitrate, 19)
				   || !GrabInt(reader, out nochans, 1)
				   || !GrabLong(reader, out bitspersample, 10)
				   || !GrabLong(reader, out samplerate, 10)
				   || !GrabInt(reader, out wavegen, 1)
				   || !GrabInt(reader, out wavemach, 1)
				   || !GrabLong(reader, out sounddatalen, 19))
				{
					Console.WriteLine("Sample header {0} is corrupt.", esamp);
					Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
					                  sampid, year, bitrate, nochans, bitspersample, samplerate, wavegen, wavemach, sounddatalen);
					return false;
				}

				if(sampid >= 0)
				{
					Engine.TheTrack.Samples[sampid] = new Sample();
					Engine.TheTrack.Samples[sampid].ID = sampid;
					Engine.TheTrack.Samples[sampid].Year = year;
					Engine.TheTrack.Samples[sampid].BitRate = bitrate;
					Engine.TheTrack.Samples[sampid].channels = nochans;
					Engine.TheTrack.Samples[sampid].bits_per_sample = (int)bitspersample;
					Engine.TheTrack.Samples[sampid].sample_rate = (int)samplerate;

					if(wavegen == 1) { Engine.TheTrack.Samples[sampid].UseWaveGenerator = true; }
					if(wavemach == 1) { Engine.TheTrack.Samples[sampid].UseWaveMachine = true; }

					sampname = GrabString(reader);
					artist = GrabString(reader);
					sampfilename = GrabString(reader);

					Engine.TheTrack.Samples[sampid].Name = sampname;
					Engine.TheTrack.Samples[sampid].Artist = artist;
					Engine.TheTrack.Samples[sampid].Filename = sampfilename;

					if(sounddatalen > 0)
					{
						Engine.TheTrack.Samples[sampid].SoundData = new byte[sounddatalen];
						Engine.TheTrack.Samples[sampid].SoundData = reader.ReadBytes ((int)sounddatalen);
					}

					if(wavegen == 1)
					{
						Engine.TheTrack.Samples[sampid].WaveGenerator = GrabWaveGenerator(reader);
					}

					if(wavemach == 1)
					{
						Engine.TheTrack.Samples[sampid].WaveMachine = GrabWaveMachine(reader, sampid);
					}
				}

			}

			return true;
		}
*/
		public static gurumod.Generator GrabWaveGenerator(BinaryReader reader)
		{
			//	If the wave generator flag is 1, then:
			//		tfffffssssssllll
			//			t: sound type
			//			f: 5 digit frequency
			//			s: 6 digit sample rate
			//			l: 4 digit length in seconds

			gurumod.Generator togen = new Generator();

			int soundtype = 0;
			int frequency = 0;
			int samplerate = 0;
			int samplelen = 0;

			if(!GrabInt(reader, out soundtype, 1)
			   || !GrabInt(reader, out frequency, 5)
			   || !GrabInt(reader, out samplerate, 6)
			   || !GrabInt(reader, out samplelen, 4))
			{
				Console.WriteLine("Loading sample type of wave generator failed.");
				return null;
			}

			togen.WaveType = soundtype;
			togen.Frequency = (double)frequency;
			togen.SampleRate = samplerate;
			togen.Length = samplelen;

			return togen;

		}

		public static bool GrabWaveMachine(BinaryReader reader, int sampid)
		{
			//	If the wave machine flag is 1, then:
			//		ssssssfffffgggppp
			//			s: 6 digit sample rate
			//			f: 5 digit frequency
			//			g: 3 digit number of generators
			//			p: 3 digit number of processors

			if(!Serializers.GT0001.WaveMachineLoader.Load(reader, sampid))
			{
				Console.WriteLine("Failed to launch wave machine loader.");
				return false;
			}

			int noprocs = Engine.TheTrack.Samples[sampid].WaveMachine.Processors.Length;

			//	For each processor:
			//		nnniiittt
			//			n: 3 digit processor id
			//			i: 3 digit number of inputs
			//			t: 3 digit processor type
			Engine.TheTrack.Samples[sampid].WaveMachine.Processors = new gurumod.Machines.Processor[Engine.TheTrack.Samples[sampid].WaveMachine.MaxProcessors];

			for(int eproc = 0 ; eproc < noprocs; eproc++)
			{
				int procid = 0;
				int noinputs = 0;
				int proctype = 0;

				if(!GrabInt(reader, out procid, 3)
				   || !GrabInt(reader, out noinputs, 3)
				   || !GrabInt(reader, out proctype, 3))
				{
					Console.WriteLine("Processor header corrupt while loading wave machine");
					return false;
				}

				//if(procid == -1) { procid = eproc; }

				//	For each input of the processor:
				//		iiitttaaaa
				//			i: 3 digit source id
				//			t: 3 digit source type
				//			a: 4 digit amplitude (x.xx)
				Machines.InputData[] tinputs = new Machines.InputData[noinputs];

				if(noinputs > 0)
				{
				
					for(int ein = 0; ein < noinputs; ein++)
					{
						int sid = 0;
						int stype = 0;
						double ampl = 0;

						if(!GrabInt(reader, out sid, 3)
						   || !GrabInt(reader, out stype, 3)
						   || !GrabDouble(reader, out ampl, 4))
						{
							Console.WriteLine("Input data corrupt while loading");
							return false;
						}

						tinputs[ein] = new gurumod.Machines.InputData();
						tinputs[ein].Amplitude = ampl;
						tinputs[ein].SourceType = stype;
						tinputs[ein].SourceID = sid;
					}


				}

				if(procid > 0 && procid < Engine.TheTrack.Samples[sampid].WaveMachine.Processors.Length)
				{


					if(proctype == Machines.Processor.ProcTypeMixer)
					{
						//	For mixers
						//		m
						//			m: 1 digit combine method
						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid] = new gurumod.Machines.Mixer();
						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid].ProcessorType = proctype;

						int combmeth = 0;
						if(!GrabInt(reader, out combmeth, 1))
						{
							Console.WriteLine("Load mixer failed");
							return false;
						}

						((gurumod.Machines.Mixer)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).CombineMethod = combmeth;
					}
					else if(proctype == Machines.Processor.ProcTypeEnvelope)
					{
						//	For envelopes:
						//		aaaaaaaaaabbbbddddddddddeeeessssssssssrrrrrrrrrr
						//			a: 10 digit attack
						//			b: 4 digit attack amplitude (x.xx)
						//			d: 10 digit decay
						//			e: 4 digit decay amplitude (x.xx)
						//			s: 10 digit sustain
						//			r: 10 digit release

						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid] = new gurumod.Machines.Envelope();
						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid].ProcessorType = proctype;

						double attack = 0;
						double attackamp = 0;
						double decay = 0;
						double decayamp = 0;
						double sustain = 0;
						double release = 0;

						if(!GrabDouble(reader, out attack, 10)
						   || !GrabDouble(reader, out attackamp, 4)
						   || !GrabDouble(reader, out decay, 10)
						   || !GrabDouble(reader, out decayamp, 4)
						   || !GrabDouble(reader, out sustain, 10)
						   || !GrabDouble(reader, out release, 10))
						{
							Console.WriteLine("Error loading envelope.");
							return false;
						}

						((gurumod.Machines.Envelope)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).Attack = attack;
						((gurumod.Machines.Envelope)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).AttackAmp = attackamp;
						((gurumod.Machines.Envelope)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).Decay = decay;
						((gurumod.Machines.Envelope)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).DecayAmp = decayamp;
						((gurumod.Machines.Envelope)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).Sustain = sustain;
						((gurumod.Machines.Envelope)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).Release = release;
					}
					else if(proctype == gurumod.Machines.Processor.ProcTypeGate)
					{
						//	For gates
						//		llllllllllhhhhhhhhhh
						//			l: 10 digit MinGateManual
						//			h: 10 digit MaxGateManual

						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid] = new gurumod.Machines.Gate();
						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid].ProcessorType = proctype;

						double min = 0;
						double max = 0;

						if(!GrabDouble(reader, out min, 10)
						   || !GrabDouble(reader, out max, 10))
						{
							Console.WriteLine("Error reading gate values");
							return false;
						}

						((gurumod.Machines.Gate)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).MinGateManual = min;
						((gurumod.Machines.Gate)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).MaxGateManual = max;
					}
					else if(proctype == gurumod.Machines.Processor.ProcTypeReverb)
					{

						//	For reverb
						//		wwwwwwwwwwdddddddddd
						//			w: 10 digit delay
						//			d: 10 digit decay
						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid] = new gurumod.Machines.Reverb();
						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid].ProcessorType = proctype;

						double min = 0;
						double max = 0;

						if(!GrabDouble(reader, out min, 10)
						   || !GrabDouble(reader, out max, 10))
						{
							Console.WriteLine("Error reading gate values");
							return false;
						}

						((gurumod.Machines.Reverb)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).Delay = min;
						((gurumod.Machines.Reverb)Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid]).Decay = max;
					}

					if(procid >= Engine.TheTrack.Samples[sampid].WaveMachine.Processors.Length)
					{
						//	??
					}
					else
					{

						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid].InputCount = tinputs.Length;
						Engine.TheTrack.Samples[sampid].WaveMachine.Processors[procid].Inputs = tinputs;
					}
				}

			}


		

		

			return true;
		}



		public static bool GrabInt(BinaryReader reader, out int intval, int length)
		{
			intval = 0;
			byte[] buf = GrabFully(reader, length);
		

			//if(buf.Length < length) { Console.WriteLine("Data not long enough to retrieve what was asked for."); return false; }

			string tnm = Encoding.UTF8.GetString(buf);
			if(!Int32.TryParse(tnm, out intval))
			{
				Console.WriteLine("Data was not an integer. {0}", tnm);
				intval = -1;
				return false;
			}

			return true;
		}

		public static bool GrabLong(BinaryReader reader, out long intval, int length)
		{
			intval = 0;
			byte[] buf = GrabFully(reader, length);

			//if(buf.Length < length) { Console.WriteLine("Data not long enough to retrieve what was asked for."); return false; }

			string tnm = Encoding.UTF8.GetString(buf);
			if(!Int64.TryParse(tnm, out intval))
			{
				Console.WriteLine("Data was not a long. {0}", tnm);
				return false;
			}

			return true;
		}

		public static string GrabString(BinaryReader reader)
		{
			byte[] inb = new byte[1];
			bool keepgoin = true;
			string toret = "";

			while(keepgoin)
			{
				inb = reader.ReadBytes(1);
				if(inb.Length < 1) { keepgoin = false; }
				else
				{
					if(Encoding.UTF8.GetString(inb) == "\0") { keepgoin = false; }
					else { toret = toret + Encoding.UTF8.GetString(inb); }

				}

			}

			return toret;
		}

		public static string GrabString(BinaryReader reader, int strlen)
		{
			byte[] buf = GrabFully(reader, strlen);

			string toret = Encoding.UTF8.GetString(buf);


			return toret;
		}

		public static byte[] GrabBytes(BinaryReader reader, int strlen)
		{
			byte[] buf = GrabFully(reader, strlen);


			return buf;
		}

		public static bool bGrabDouble(byte[] barray, out double doubval, int start, int length)
		{
			string toret = Encoding.UTF8.GetString(barray, start, length);

			if(!Double.TryParse(toret, out doubval)) { Console.WriteLine("Data in bGrabDouble {0} {1} didn't parse.", start, length); return false; }

			return true;
		}

		public static bool GrabDouble(BinaryReader reader, out double doubval, int length)
		{
			doubval = 0;
			byte[] buf = GrabFully(reader, length);



			//buf = reader.ReadBytes (length);

			//if(buf.Length < length) { Console.WriteLine("Data not long enough to retrieve what was asked for."); return false; }

			string tnm = Encoding.UTF8.GetString(buf);
			if(!Double.TryParse(tnm, out doubval))
			{
				Console.WriteLine("Data was not a double. {0}", tnm);
				return false;
			}

			return true;
		}

		public static bool bGrabInt(byte[] barray, out int intval, int start, int length)
		{
			string toret = Encoding.UTF8.GetString(barray, start, length);
			if(!Int32.TryParse(toret, out intval))
			{
				Console.WriteLine("Failed to bGrabInt at {0} {1}.", start, length);
				return false;
			}



			return true;
		}

		public static byte[] GrabFully(BinaryReader reader, int len)
		{
			byte[] buf = new byte[len];

			if (len < 1)
		    {
		        len = 1;
		    }
		    
		    
		    long read=0;
		    
		    int chunk;
		    while ( (chunk = reader.Read(buf, (int)read, buf.Length-(int)read)) > 0)
		    {
		        read += chunk;
		        
		        // If we've reached the end of our buffer, check to see if there's
		        // any more information
		        if (read == buf.Length)
		        {
		            int nextByte = reader.ReadByte();
		            
		            // End of stream? If so, we're done
		            if (nextByte==-1)
		            {
		                return buf;
		            }
		            
		            // Nope. Resize the buffer, put in the byte we've just
		            // read, and continue
		            byte[] newBuffer = new byte[buf.Length*2];
		            Array.Copy(buf, newBuffer, buf.Length);
		            newBuffer[read]=(byte)nextByte;
		            buf = newBuffer;
		            read++;
		        }
		    }
		    // Buffer is now too big. Shrink it.
		    byte[] ret = new byte[read];
		    Array.Copy(buf, ret, read);
		    return ret;
		}

		public static bool GetTrackHeader(BinaryReader reader)
		{
		

			int nopats = 0;
			int nosamps = 0;
			int nochans = 0;
			int year = 2014;
			int tempo = 145;
			int patlen = 128;


			if(!GrabInt(reader, out nopats, 5)) { Console.WriteLine("Track header corrupt on nopats."); return false; }
			if(!GrabInt(reader, out nosamps, 5)) { Console.WriteLine("Track header corrupt on nosamps."); return false; }
			if(!GrabInt(reader, out nochans, 3)) { Console.WriteLine("Track header corrupt on nochans."); return false; }
			if(!GrabInt(reader, out year, 4)) { Console.WriteLine("Track header corrupt on year."); return false; }
			if(!GrabInt(reader, out tempo, 3)) { Console.WriteLine("Track header corrupt on tempo."); return false; }
			if(!GrabInt(reader, out patlen, 4)) { Console.WriteLine("Track header is corrupted on patlen."); return false; }

			Engine.TheTrack.Year = year;
			Engine.TheTrack.Tempo = tempo;
			Engine.TheTrack.ChannelCount = nochans;
			Engine.TheTrack.DefaultPatternLength = patlen;
			Engine.TheTrack.PatternCount = nopats;

			string author = GrabString(reader);
			string title = GrabString(reader);
			string genre = GrabString(reader);
			string website = GrabString(reader);
			string email = GrabString(reader);
			string comments = GrabString(reader);

			//Console.WriteLine("{0} {1} {2} {3} {4} {5}", author, title, genre, website, email, comments);
			Engine.TheTrack.Author = author;
			Engine.TheTrack.Title = title;
			Engine.TheTrack.Genre = genre;
			Engine.TheTrack.WebSite = website;
			Engine.TheTrack.Email = email;
			Engine.TheTrack.Comments = comments;

			Engine.TheTrack.Samples = new Sample[nosamps];
			for(int es = 0; es < nosamps; es++)
			{
				Engine.TheTrack.Samples[es] = new Sample();
			}

			return true;
		}


		public static bool GetMutedChannels(BinaryReader reader)
		{
			byte[] inmu = new byte[1];
			inmu = reader.ReadBytes (Engine.TheTrack.ChannelCount);

			for(int ec = 0; ec < Engine.TheTrack.ChannelCount; ec++)
			{
				int tmpchnid = 0;
				if(!bGrabInt(inmu, out tmpchnid, 0 * ec, 1))
				{
					Console.WriteLine("Failed to load muted channels list.");
					return false;
				}

				if(tmpchnid == 0) { Engine.TheTrack.ChannelMuted[ec] = true; }
				else { Engine.TheTrack.ChannelMuted[ec] = false; }
			}
			return true;
		}

		public static bool GetPatternSequence(BinaryReader reader)
		{
			string patternlist = GrabString(reader);

			if(patternlist == "") { Engine.TheTrack.PatternSequence = new int[1]; }
			else
			{
				Engine.TheTrack.PatternSequence = new int[patternlist.Length / 5];
				int ptrcnt = 0;
				for(int ep = 0; ep < patternlist.Length; ep = ep + 5)
				{
					string ptno = patternlist.Substring(ep, 5);
					Engine.TheTrack.PatternSequence[ptrcnt] = Int32.Parse(ptno);
					ptrcnt++;
				}
			}

			return true;
		}

		public static bool GetPatternData(BinaryReader reader)
		{
			Engine.TheTrack.Patterns = new Pattern[Engine.TheTrack.PatternCount];

			for(int ep = 0; ep < Engine.TheTrack.PatternCount; ep++)
			{
				//		rrrr...ccc...onnvvvsssiiiii...
				//			r: 4 digit number of rows in pattern
				//			c: 3 digit channel number/id
				//			o: 1 digit octave
				//			n: 2 digit note
				//			v: 3 digit volume
				//			s: 3 digitl special value
				//			i: 5 digit sample id
				Console.WriteLine("Loading pattern {0}", ep);

				int tpatternrows = 0;
				if(!GrabInt(reader, out tpatternrows, 4))
				{
					Console.WriteLine("Failed to grab the number of rows for the current pattern.");
					return false;
				}
				Engine.TheTrack.Patterns[ep] = new Pattern();
				Engine.TheTrack.Patterns[ep].Channels = new PatternChannel[Engine.TheTrack.ChannelCount];
				Engine.TheTrack.Patterns[ep].ChannelCount = Engine.TheTrack.ChannelCount;
				Engine.TheTrack.Patterns[ep].RowCount = tpatternrows;

				for(int ech = 0; ech < Engine.TheTrack.ChannelCount; ech++)
				{
					int tchid = 0;
					if(!GrabInt(reader, out tchid, 3))
					{
						Console.WriteLine("Failed to grab channel ID");
						return false;
					}


					Engine.TheTrack.Patterns[ep].Channels[tchid] = new PatternChannel(tpatternrows);
					Engine.TheTrack.Patterns[ep].Channels[tchid].ChannelID = tchid;
					Engine.TheTrack.Patterns[ep].Channels[tchid].ElementCount = tpatternrows;



					for(int erow = 0; erow < tpatternrows; erow++)
					{
						//Console.WriteLine("{0} {1} {2}", ep, ech, erow);
						int octave = 5;
						int note = -1;
						int volume = -1;
						int specialcontrol = -1;
						int sampleid = 0;

						if(!GrabInt(reader, out octave, 1)) { Console.WriteLine("Deserializer failed to parse octave."); return false; }
						if(!GrabInt(reader, out note, 2)) { Console.WriteLine("Deserializer failed to parse note."); return false; }
						if(!GrabInt(reader, out volume, 3)) { Console.WriteLine("Deserializer failed to parse volume."); return false; }
						if(!GrabInt(reader, out specialcontrol, 3)) { Console.WriteLine("Deserializer failed to parse specialcontrol."); return false; }
						if(!GrabInt(reader, out sampleid, 5)) { Console.WriteLine("Deserializer failed to parse sampleid."); return false; }

						Engine.TheTrack.Patterns[ep].Channels[tchid].Elements[erow].Octave = octave;
						Engine.TheTrack.Patterns[ep].Channels[tchid].Elements[erow].Note = note;
						Engine.TheTrack.Patterns[ep].Channels[tchid].Elements[erow].Volume = volume;
						Engine.TheTrack.Patterns[ep].Channels[tchid].Elements[erow].SpecialControl = specialcontrol;
						Engine.TheTrack.Patterns[ep].Channels[tchid].Elements[erow].SampleID = sampleid;


					}
				}
			}


			if(Engine.TheTrack.Patterns == null)
			{
				Console.WriteLine("Pattern data is null after loading.");
			}
			else
			{
				Console.WriteLine("Loaded {0} patterns", Engine.TheTrack.Patterns.Length);

			}	

			return true;
		}


		public static bool GetSampleData(BinaryReader reader)
		{
			for(int es = 0; es < Engine.TheTrack.Samples.Length; es++)
			{
				if(!GetSampleHeader(reader, es)) { Console.WriteLine("Sample header could not be loaded."); return false; }
				if(!GetSampleDetails (reader, es)) { Console.WriteLine("Sample data could not be loaded for {0}.", es); return false; }

				if(Engine.TheTrack.Samples[es].UseWaveGenerator)
				{
					Console.WriteLine("Sample {0} uses a WaveGenerator");
				}
				else if(Engine.TheTrack.Samples[es].UseWaveMachine)
				{
					Serializers.GT0001.WaveMachineLoader.Load(reader, es);
				}
				else
				{
					Engine.TheTrack.Samples[es].SoundData = reader.ReadBytes (Engine.TheTrack.Samples[es].SoundData.Length);
				}
			}

			return true;
		}


		public static bool GetSampleHeader(BinaryReader reader, int sampid)
		{
			int ssid = 0;
			int year = 0;
			long bitrate = 0;
			int channels = 0;
			int bitspersample = 0;
			int samplerate = 0;
			int usewavegen = 0;
			int usewavemach = 0;
			long sounddatalen = 0;

			if(!GrabInt(reader, out ssid, 5)) { Console.WriteLine("Sample Header {0} corrupt at id"); return false; }
			if(!GrabInt(reader, out year, 4)) { Console.WriteLine("Sample Header {0} corrupt at year"); return false; }
			if(!GrabLong(reader, out bitrate, 19)) { Console.WriteLine("Sample Header {0} corrupt at bitrate"); return false; }
			if(!GrabInt(reader, out channels, 1)) { Console.WriteLine("Sample Header {0} corrupt at channels"); return false; }
			if(!GrabInt(reader, out bitspersample, 10)) { Console.WriteLine("Sample Header {0} corrupt at bitspersample"); return false; }
			if(!GrabInt(reader, out samplerate, 10)) {  Console.WriteLine("Sample Header {0} corrupt at samplerate"); return false; }
			if(!GrabInt(reader, out usewavegen, 1)) { Console.WriteLine("Sample Header {0} corrupt at usewavegen"); return false; }
			if(!GrabInt(reader, out usewavemach, 1)) { Console.WriteLine("Sample Header {0} corrupt at usewavemachine"); return false; }
			if(!GrabLong(reader, out sounddatalen, 19)) { Console.WriteLine("Sample Header {0} corrupt at sounddatalen"); return false; }

			Engine.TheTrack.Samples[sampid].ID = ssid;
			Engine.TheTrack.Samples[sampid].Year = year;
			Engine.TheTrack.Samples[sampid].BitRate = bitrate;
			Engine.TheTrack.Samples[sampid].channels = channels;
			Engine.TheTrack.Samples[sampid].bits_per_sample = bitspersample;
			Engine.TheTrack.Samples[sampid].sample_rate = samplerate;
			if(usewavegen == 1) { Engine.TheTrack.Samples[sampid].UseWaveGenerator = true; } else { Engine.TheTrack.Samples[sampid].UseWaveGenerator = false; }
			if(usewavemach == 1) { Engine.TheTrack.Samples[sampid].UseWaveMachine = true; } else { Engine.TheTrack.Samples[sampid].UseWaveMachine = false; }

			if(sounddatalen > 0) { Engine.TheTrack.Samples[sampid].SoundData = new byte[sounddatalen]; }

			return true;
		}

		public static bool GetSampleDetails(BinaryReader reader, int sampid)
		{
			Engine.TheTrack.Samples[sampid].Name = GrabString(reader);
			Engine.TheTrack.Samples[sampid].Artist = GrabString(reader);
			Engine.TheTrack.Samples[sampid].Filename = GrabString(reader);

			return true;
		}
	}
}

