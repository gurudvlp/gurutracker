//
//  WaveMachineLoader.cs
//
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
//
//  Copyright (c) 2014-2022 Brian Murphy
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
using System.IO;
using System.Text;

namespace gurumod.Serializers.GT0001
{
	public class WaveMachineLoader
	{
		public WaveMachineLoader ()
		{
		}

		public static bool Load(BinaryReader reader, int sampid)
		{
			//	If the wave machine flag is 1, then:
			//		ssssssfffffgggppp
			//			s: 6 digit sample rate
			//			f: 5 digit frequency
			//			g: 3 digit number of generators
			//			p: 3 digit number of processors



			int samplerate = 0;
			double frequency = 440;
			int nogens = 0;
			int noprocs = 0;

			if(!GrabInt(reader, out samplerate, 6)) { Console.WriteLine("WaveMachine for {0} header null at sample rate", sampid); return false; }
			if(!GrabDouble(reader, out frequency, 5)) { Console.WriteLine("WaveMachine for {0} header null at frequency", sampid); return false; }
			if(!GrabInt(reader, out nogens, 3)) { Console.WriteLine("WaveMachine for {0} header null at number of generators", sampid); return false; }
			if(!GrabInt(reader, out noprocs, 3)) { Console.WriteLine("WaveMachine for {0} header null at number of processors", sampid); return false; }

			Engine.TheTrack.Samples[sampid].sample_rate = samplerate;
			if(nogens > 0) { Engine.TheTrack.Samples[sampid].WaveMachine.Generators = new gurumod.Machines.Generator[nogens]; }
			if(noprocs > 0) { Engine.TheTrack.Samples[sampid].WaveMachine.Processors = new gurumod.Machines.Processor[noprocs]; }

			//	For each generator:
			//		tessssssfffffaaaa
			//			t: 1 digit generator type
			//			e: 1 digit enabled (0 or 1)
			//			s: 6 digit sample rate
			//			f: 5 digit frequency
			//			a: 4 digit amplitude (x.xx)

			for(int egen = 0; egen < nogens; egen++)
			{
				if(!GetWaveMachineGenerator(reader, sampid, egen)) { Console.WriteLine("WaveMachine failed to load generators."); return false; }
			}


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

				if(!GrabInt(reader, out procid, 3)) { Console.WriteLine("Processor header procid corrupt."); return false; }
				if(!GrabInt(reader, out noinputs, 3)) { Console.WriteLine("Processor header noinputs corrupt."); return false; }
				if(!GrabInt(reader, out proctype, 3)) { Console.WriteLine("Processor header proctype corrupt."); return false; }
			

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

						if(!GrabInt(reader, out sid, 3)) { Console.WriteLine("Input data corrupt at sid during {0}", ein); return false; }
						if(!GrabInt(reader, out stype, 3)) { Console.WriteLine("Input data corrupt at sid during {0}", ein); return false; }
						if(!GrabDouble(reader, out ampl, 4)) { Console.WriteLine("Input data corrupt at sid during {0}", ein); return false; }


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


		public static bool GetWaveMachineGenerator(BinaryReader reader, int sampid, int egen)
		{

		
				int gentype = 0;
				int genenabled = 0;
				int gensamplerate = 0;
				double genfrequency = 0;
				double genamplitude = 0;

				if(!GrabInt(reader, out gentype, 1)) { Console.WriteLine("WaveMachineGen {0} invalid on gentype.", egen); return false; }
				if(!GrabInt(reader, out genenabled, 1)) { Console.WriteLine("WaveMachineGen {0} invalid on gentenabled.", egen); return false; }
				if(!GrabInt(reader, out gensamplerate, 6)) { Console.WriteLine("WaveMachineGen {0} invalid on gensamplerate.", egen); return false; }
				if(!GrabDouble(reader, out genfrequency, 5)) { Console.WriteLine("WaveMachineGen {0} invalid on genfreq.", egen); return false; }
				if(!GrabDouble(reader, out genamplitude, 4)) { Console.WriteLine("WaveMachineGen {0} invalid on genamp.", egen); return false; }
			

				if(gentype == 0)	//oscillator
				{
					//	For generator type 0 (oscillator)
					//		w
					//			w: 1 digit wave type
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen] = new Machines.Osc();
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].Amplitude = genamplitude;
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].GeneratorType = gentype;
					if(genenabled == 1) { Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].Enabled = true; }
					else { Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].Enabled = false; }
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].SampleRate = gensamplerate;
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].Frequency = genfrequency;

					GrabInt(reader, out ((Machines.Osc)Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen]).WaveType, 1);
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

					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen] = new Machines.WavFile();
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].Amplitude = genamplitude;
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].GeneratorType = gentype;
					if(genenabled == 1) { Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].Enabled = true; }
					else { Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].Enabled = false; }
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].SampleRate = gensamplerate;
					Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen].Frequency = (double)genfrequency;

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
						return false;
					}

					((Machines.WavFile)Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen]).Channels = nochans;
					((Machines.WavFile)Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen]).SampleRate = wsamplerate;
					((Machines.WavFile)Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen]).BitRate = (int)wbitrate;
					((Machines.WavFile)Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen]).AudioData = new short[waudiolen];
					((Machines.WavFile)Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen]).Filename = wfilename;

					for(long esh = 0; esh < waudiolen; esh++)
					{
						byte bone = reader.ReadByte();
						byte btwo = reader.ReadByte();

						((Machines.WavFile)Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen]).AudioData[esh] = ((Machines.WavFile)Engine.TheTrack.Samples[sampid].WaveMachine.Generators[egen]).ToShort(bone, btwo);
					}
				}




			return true;
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

		public static string GrabString(BinaryReader reader, int strlen)
		{
			byte[] inb = new byte[1];
			//bool keepgoin = true;
			string toret = "";

			inb = reader.ReadBytes (strlen);

			if(inb.Length < strlen) { Console.WriteLine("GrabString failed."); return ""; }
			toret = Encoding.UTF8.GetString(inb);


			return toret;
		}

		public static byte[] GrabBytes(BinaryReader reader, int strlen)
		{
			byte[] inb = new byte[1];
			//string toret = "";

			inb = reader.ReadBytes (strlen);
			return inb;
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
	}
}

