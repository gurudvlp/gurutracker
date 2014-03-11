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


			//LoadHeader(reader, out thetrack);
			int nopats = 0;
			int nosamps = 0;
			int nochans = 0;
			int year = 2014;
			int tempo = 145;
			int patlen = 128;

			if(!GrabInt(reader, out nopats, 5)
			|| !GrabInt(reader, out nosamps, 5)
			|| !GrabInt(reader, out nochans, 3)
			   || !GrabInt(reader, out year, 4)
			   || !GrabInt(reader, out tempo, 3)
			   || !GrabInt(reader, out patlen, 4))
			{
				Console.WriteLine("Track header is corrupted.");
				return false;
			}
			Engine.TheTrack.Year = year;
			Engine.TheTrack.Tempo = tempo;
			Engine.TheTrack.ChannelCount = nochans;


			Console.WriteLine("Patterns: {0}\nSamples: {1}\nChannels: {2}\nYear: {3}\nTempo: {4}\nPattern Len: {5}", nopats, nosamps, nochans, year, tempo, patlen);

			string author = GrabString(reader);
			string title = GrabString(reader);
			string genre = GrabString(reader);
			string website = GrabString(reader);
			string email = GrabString(reader);
			string comments = GrabString(reader);

			Console.WriteLine("{0} {1} {2} {3} {4} {5}", author, title, genre, website, email, comments);
			Engine.TheTrack.Author = author;
			Engine.TheTrack.Title = title;
			Engine.TheTrack.Genre = genre;
			Engine.TheTrack.WebSite = website;
			Engine.TheTrack.Email = email;
			Engine.TheTrack.Comments = comments;


			Engine.TheTrack.ChannelMuted = new bool[nochans];
			for(int ec = 0; ec < nochans; ec++)
			{
				int yesno = 0;
				if(!GrabInt(reader, out yesno, 1))
				{
					Console.WriteLine("Corruption while loading channel mute data.");
					return false;
				}

				if(yesno == 0) { Engine.TheTrack.ChannelMuted[ec] = false; }
				else { Engine.TheTrack.ChannelMuted[ec] = true; }
			}


			string patternlist = GrabString(reader);

			if(patternlist == "") { Engine.TheTrack.PatternSequence = new int[1]; }
			else
			{
				Engine.TheTrack.PatternSequence = new int[patternlist.Length / 5];
				for(int ep = 0; ep < patternlist.Length / 5; ep++)
				{
					Engine.TheTrack.PatternSequence[ep] = Int32.Parse(patternlist.Substring(ep * 5, 5));
				}
			}


			Engine.TheTrack.Patterns = new Pattern[nopats];

			for(int ep = 0; ep < nopats; ep++)
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
				Engine.TheTrack.Patterns[ep].Channels = new PatternChannel[nochans];
				Engine.TheTrack.Patterns[ep].ChannelCount = nochans;
				Engine.TheTrack.Patterns[ep].RowCount = tpatternrows;

				for(int ech = 0; ech < nochans; ech++)
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

						if(note != -1
						   || sampleid > -1
						   || specialcontrol > -1
						   || volume > -1)
						{
							Console.WriteLine("{0}-{1} {2} {3} {4}", note, octave, sampleid, specialcontrol, volume);
						}
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



			GrabSamples(reader, nosamps);
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

	

		public static bool GrabSamples(BinaryReader reader, int numsamples)
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
			for(int es = 0; es < numsamples; es++) { Engine.TheTrack.Samples[es] = new Sample(); }

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

				if(sampid < 0) { sampid = esamp; }

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
					Engine.TheTrack.Samples[sampid].WaveMachine = GrabWaveMachine(reader);
				}

			}

			return true;
		}

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

		public static gurumod.Machine GrabWaveMachine(BinaryReader reader)
		{
			//	If the wave machine flag is 1, then:
			//		ssssssfffffgggppp
			//			s: 6 digit sample rate
			//			f: 5 digit frequency
			//			g: 3 digit number of generators
			//			p: 3 digit number of processors

			gurumod.Machine inmachine = new Machine();

			int samplerate = 0;
			double frequency = 440;
			int nogens = 0;
			int noprocs = 0;

			if(!GrabInt(reader, out samplerate, 6)
			   || !GrabDouble(reader, out frequency, 5)
			   || !GrabInt(reader, out nogens, 3)
			   || !GrabInt(reader, out noprocs, 3))
			{
				Console.WriteLine("Failed to load wave machine header. {0} {1} {2} {3}", samplerate, frequency, nogens, noprocs);
				return null;
			}

			//	For each generator:
			//		tessssssfffffaaaa
			//			t: 1 digit generator type
			//			e: 1 digit enabled (0 or 1)
			//			s: 6 digit sample rate
			//			f: 5 digit frequency
			//			a: 4 digit amplitude (x.xx)

			inmachine.Generators = new gurumod.Machines.Generator[inmachine.MaxGenerators];

			for(int egen = 0; egen < nogens; egen++)
			{
				int gentype = 0;
				int genenabled = 0;
				int gensamplerate = 0;
				double genfrequency = 0;
				double genamplitude = 0;

				if(!GrabInt(reader, out gentype, 1)
				   || !GrabInt(reader, out genenabled, 1)
				   || !GrabInt(reader, out gensamplerate, 6)
				   || !GrabDouble(reader, out genfrequency, 5)
				   || !GrabDouble(reader, out genamplitude, 4))
				{
					Console.WriteLine("Failed to load generator header {0} in wave machine.", egen);
					Console.WriteLine("{0} {1} {2} {3} {4}", gentype, genenabled, gensamplerate, genfrequency, genamplitude);
					return null;
				}

				if(gentype == 0)	//oscillator
				{
					//	For generator type 0 (oscillator)
					//		w
					//			w: 1 digit wave type
					inmachine.Generators[egen] = new Machines.Osc();
					inmachine.Generators[egen].Amplitude = genamplitude;
					inmachine.Generators[egen].GeneratorType = gentype;
					if(genenabled == 1) { inmachine.Generators[egen].Enabled = true; }
					else { inmachine.Generators[egen].Enabled = false; }
					inmachine.Generators[egen].SampleRate = gensamplerate;
					inmachine.Generators[egen].Frequency = genfrequency;

					GrabInt(reader, out ((Machines.Osc)inmachine.Generators[egen]).WaveType, 1);
				}
				else if(gentype == 1)	//wave player
				{
					//	For generator type 1 (wave player)
					//		cssssssbbbbbbbbbblllllllllllllllllllf<chr 0>
					//			c: 1 digit channel count
					//			s: 6 digit sample rate
					//			b: 10 digit bit rate
					//			l: 19 digit length of audio data
					//			f: filename
					//		list of 2-byte shorts of audio data

					inmachine.Generators[egen] = new Machines.WavFile();
					inmachine.Generators[egen].Amplitude = genamplitude;
					inmachine.Generators[egen].GeneratorType = gentype;
					if(genenabled == 1) { inmachine.Generators[egen].Enabled = true; }
					else { inmachine.Generators[egen].Enabled = false; }
					inmachine.Generators[egen].SampleRate = gensamplerate;
					inmachine.Generators[egen].Frequency = (double)genfrequency;

					int nochans = 0;
					int wsamplerate = 0;
					long wbitrate = 0;
					long waudiolen = 0;
					string wfilename = "";

					if(!GrabInt(reader, out nochans, 1)
					   || !GrabInt(reader, out wsamplerate, 6)
					   || !GrabLong(reader, out wbitrate, 10)
					   || !GrabLong(reader, out waudiolen, 19))
					{
						Console.WriteLine("Loading wave player in wave machine failed.");
						return null;
					}

					((Machines.WavFile)inmachine.Generators[egen]).Channels = nochans;
					((Machines.WavFile)inmachine.Generators[egen]).SampleRate = wsamplerate;
					((Machines.WavFile)inmachine.Generators[egen]).BitRate = (int)wbitrate;
					((Machines.WavFile)inmachine.Generators[egen]).AudioData = new short[waudiolen];
					((Machines.WavFile)inmachine.Generators[egen]).Filename = wfilename;

					for(long esh = 0; esh < waudiolen; esh++)
					{
						byte bone = reader.ReadByte();
						byte btwo = reader.ReadByte();

						((Machines.WavFile)inmachine.Generators[egen]).AudioData[esh] = ((Machines.WavFile)inmachine.Generators[egen]).ToShort(bone, btwo);
					}
				}
			}


			//	For each processor:
			//		nnniiittt
			//			n: 3 digit processor id
			//			i: 3 digit number of inputs
			//			t: 3 digit processor type
			inmachine.Processors = new gurumod.Machines.Processor[inmachine.MaxProcessors];

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
					return null;
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
							return null;
						}

						tinputs[ein] = new gurumod.Machines.InputData();
						tinputs[ein].Amplitude = ampl;
						tinputs[ein].SourceType = stype;
						tinputs[ein].SourceID = sid;
					}


				}

				if(procid > 0 && procid < inmachine.Processors.Length)
				{


					if(proctype == Machines.Processor.ProcTypeMixer)
					{
						//	For mixers
						//		m
						//			m: 1 digit combine method
						inmachine.Processors[procid] = new gurumod.Machines.Mixer();
						inmachine.Processors[procid].ProcessorType = proctype;

						int combmeth = 0;
						if(!GrabInt(reader, out combmeth, 1))
						{
							Console.WriteLine("Load mixer failed");
							return null;
						}

						((gurumod.Machines.Mixer)inmachine.Processors[procid]).CombineMethod = combmeth;
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

						inmachine.Processors[procid] = new gurumod.Machines.Envelope();
						inmachine.Processors[procid].ProcessorType = proctype;

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
							return null;
						}

						((gurumod.Machines.Envelope)inmachine.Processors[procid]).Attack = attack;
						((gurumod.Machines.Envelope)inmachine.Processors[procid]).AttackAmp = attackamp;
						((gurumod.Machines.Envelope)inmachine.Processors[procid]).Decay = decay;
						((gurumod.Machines.Envelope)inmachine.Processors[procid]).DecayAmp = decayamp;
						((gurumod.Machines.Envelope)inmachine.Processors[procid]).Sustain = sustain;
						((gurumod.Machines.Envelope)inmachine.Processors[procid]).Release = release;
					}
					else if(proctype == gurumod.Machines.Processor.ProcTypeGate)
					{
						//	For gates
						//		llllllllllhhhhhhhhhh
						//			l: 10 digit MinGateManual
						//			h: 10 digit MaxGateManual

						inmachine.Processors[procid] = new gurumod.Machines.Gate();
						inmachine.Processors[procid].ProcessorType = proctype;

						double min = 0;
						double max = 0;

						if(!GrabDouble(reader, out min, 10)
						   || !GrabDouble(reader, out max, 10))
						{
							Console.WriteLine("Error reading gate values");
							return null;
						}

						((gurumod.Machines.Gate)inmachine.Processors[procid]).MinGateManual = min;
						((gurumod.Machines.Gate)inmachine.Processors[procid]).MaxGateManual = max;
					}
					else if(proctype == gurumod.Machines.Processor.ProcTypeReverb)
					{

						//	For reverb
						//		wwwwwwwwwwdddddddddd
						//			w: 10 digit delay
						//			d: 10 digit decay
						inmachine.Processors[procid] = new gurumod.Machines.Reverb();
						inmachine.Processors[procid].ProcessorType = proctype;

						double min = 0;
						double max = 0;

						if(!GrabDouble(reader, out min, 10)
						   || !GrabDouble(reader, out max, 10))
						{
							Console.WriteLine("Error reading gate values");
							return null;
						}

						((gurumod.Machines.Reverb)inmachine.Processors[procid]).Delay = min;
						((gurumod.Machines.Reverb)inmachine.Processors[procid]).Decay = max;
					}

					if(procid >= inmachine.Processors.Length)
					{
						//	??
					}
					else
					{

						inmachine.Processors[procid].InputCount = tinputs.Length;
						inmachine.Processors[procid].Inputs = tinputs;
					}
				}

			}


		

		

			return inmachine;
		}

		public static bool GrabInt(BinaryReader reader, out int intval, int length)
		{
			intval = 0;
			byte[] buf = new byte[length];
			buf = reader.ReadBytes (length);

			if(buf.Length < length) { Console.WriteLine("Data not long enough to retrieve what was asked for."); return false; }

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
			byte[] buf = new byte[length];
			buf = reader.ReadBytes (length);

			if(buf.Length < length) { Console.WriteLine("Data not long enough to retrieve what was asked for."); return false; }

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

		public static bool GrabDouble(BinaryReader reader, out double doubval, int length)
		{
			doubval = 0;
			byte[] buf = new byte[length];
			buf = reader.ReadBytes (length);

			if(buf.Length < length) { Console.WriteLine("Data not long enough to retrieve what was asked for."); return false; }

			string tnm = Encoding.UTF8.GetString(buf);
			if(!Double.TryParse(tnm, out doubval))
			{
				Console.WriteLine("Data was not a double. {0}", tnm);
				return false;
			}

			return true;
		}
	}
}

