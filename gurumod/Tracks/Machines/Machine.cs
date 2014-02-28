using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace gurumod
{
	
	public class Machine
	{
		[XmlIgnore()] public int FramesIntoSample = 0;
		[XmlElement()] public int SampleRate = 44100;
		[XmlElement()] public double Frequency = 440;
		[XmlElement()] public double Amplitude = 0.75;
		[XmlElement()] public int WaveType = Generator.TypeSine;
		[XmlElement()] public ALFormat Format = ALFormat.Mono16;
		//[XmlElement()] public Oscilator[] Oscs = new Oscilator[3];
		//[XmlElement()] public MixerSettings[] Mixers = new MixerSettings[2];
		
		[XmlIgnore()] public Machines.Generator[] Generators;
		[XmlIgnore()] public Machines.Processor[] Processors;
		[XmlElement("GeneratorTypes")] public string[] GeneratorTypes = new string[128];
		[XmlElement("ProcessorTypes")] public string[] ProcessorTypes = new string[128];
		
		[XmlElement("MaxGenerators")] public int MaxGenerators = 32;
		[XmlElement("MaxProcessors")] public int MaxProcessors = 32;

		[XmlIgnore()] public static ASCIIEncoding encoder = new ASCIIEncoding();

		[XmlIgnore()] private int _lastnote = -2;
		[XmlIgnore()] private bool _newnote = false;
		[XmlIgnore()] public int LastNote 
		{
			get
			{
				return _lastnote;
			}
			set
			{
				_newnote = true;
				_lastnote = value;
			}
		}

		[XmlIgnore()] public bool IsNoteNew
		{
			get
			{
				if(_newnote) { _newnote = false; return true; }
				return false;
			}
			set
			{
				_newnote = value;
			}
		}


			//= -2;
		[XmlIgnore()] public int LastOctave = 5;
		[XmlIgnore()] public bool Running = false;
		//[XmlIgnore()] public short[,] Signals;
		[XmlIgnore()] public Dictionary<string, short[]> Signals = new Dictionary<string, short[]>();
		
		public Machine ()
		{
			
			
			Generators = new Machines.Generator[this.MaxGenerators];
			Processors = new Machines.Processor[this.MaxProcessors];
			
			Generators[0] = new Machines.Osc();
			Processors[0] = new Machines.Mixer();
		}

		public byte[] GTString()
		{
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

			string samplerate = this.SampleRate.ToString("D6");
			string frequency = this.Frequency.ToString("0000.0");
			string nogens = this.Generators.Length.ToString("D3");
			string noprocs = this.Processors.Length.ToString("D3");

			MemoryStream genstream = new MemoryStream();
			StreamWriter gw = new StreamWriter(genstream);

			for(int egen = 0; egen < this.Generators.Length; egen++)
			{
				if(this.Generators[egen] != null)
				{
					byte[] gendata = this.Generators[egen].GTString();
					byte[] genbytes = this.Generators[egen].GTString();

					genstream.Write(gendata, 0, gendata.Length);
					if(genbytes != null) { genstream.Write(genbytes, 0, genbytes.Length); }
				}
				else
				{
					nogens = (Int32.Parse(nogens) - 1).ToString("D3");
				}
			}

			MemoryStream procstream = new MemoryStream();
			StreamWriter pw = new StreamWriter(procstream);

			for(int eproc = 0; eproc < this.Processors.Length; eproc++)
			{
				string procid = eproc.ToString("D3");
				string procdata = this.Processors[eproc].GTString();

				pw.Write(procid + procdata);
			}

			MemoryStream gtstream = new MemoryStream();
			StreamWriter gtwr = new StreamWriter(gtstream);

			gtwr.Write(samplerate + frequency + nogens + noprocs);
			gtstream.Write (genstream.ToArray(), 0, genstream.ToArray().Length);
			gtstream.Write(procstream.ToArray(), 0, procstream.ToArray().Length);

			return gtstream.ToArray();
		}
		
		public short[] GetSignal(PatternElement element)
		{
			if(element == null) { return GetSignal(element, -1, 5); }
			
			return GetSignal(element, element.Note, element.Octave);
		}
		
		public short[] GetSignal(PatternElement element, int note, int octave)
		{
			//	Return the sound data
			//	We need to determine how long one eighth of a beat is so that
			//	we can determine the number of frames in 1/8 beat
			
			/*if(Oscs == null)
			{
				Oscs = new Oscilator[3];
				
				for(int eos = 0; eos < Oscs.Length; eos++)
				{
					if(Oscs[eos] == null)
					{
						Oscs[eos] = new Oscilator();
						if(eos == 0) { Oscs[eos].Enabled = true; }
					}
				}
			}
			
			if(Mixers == null)
			{
				Mixers = new MixerSettings[2];
				
				for(int emx = 0; emx < Mixers.Length; emx++)
				{
					if(Mixers[emx] == null)
					{
						Mixers[emx] = new MixerSettings();
					}
				}
			}*/
			
			if(Generators == null)
			{
				InitGenerators();
			}
			
			if(Processors == null)
			{
				InitProcessors();
			}
			
			if(!Running)
			{
				//if(note > -1)
				if(note > -1 && element != null)
				{
					Console.WriteLine("Machine: Not running, but current note is set.");
					Running = true;
				}
			}
			else if(Running && note == -2)
			{
				Console.WriteLine("Machine: Running, but current note is ===");
				Running = false;
			}
			
			if(!Running) { Console.WriteLine("Machine: Not running, so exiting."); return null; }
			//Console.WriteLine("Machine: Running, and now parsing data for audio output. Note: {0}", note);
			
			float freq = ((1f / 12f) * note);
			if(octave == 4) { freq = 0.5f + (freq * 0.5f); }
			if(octave == 5) { freq =  1.0f + freq; }
			if(octave == 6) { freq =  2.0f + (freq * 2.0f); }

			//float freq = (float)Engine.Configuration.NoteFreq[octave][note];
			
			//
			//	Each Row is 1/8 beat
			//	beat = row * 8
			//	time per beat = 60 / bpm
			//  tpb / 8 = row time
			double bps = ((double)(Engine.TheTrack.Tempo / 60f));
			double timeperrow = (double)((1f / bps) / 8f);
			
			//timeperrow = ((double)(60f / Engine.TheTrack.Tempo) / 8f);
			
			//int framecnt = (int)(Math.Ceiling((timeperrow * this.SampleRate) / bps));
			//int framecnt = (int)(Math.Ceiling(((timeperrow * this.SampleRate) / bps) / 2));
			int framecnt = (int)(Math.Ceiling(((double)this.SampleRate / freq)));
			//int playlength = Math.Ceiling(((double)(60f / ((float)Engine.TheTrack.Tempo) / 8f)) * this.SampleRate);
			
			//Console.WriteLine("GetSignal {0} {1} {2}", bps, timeperrow, framecnt);
			
			if(element != null)
			{
				if(element.Volume > -1)
				{
					//Console.WriteLine("Changing volume for Machine");
					this.Amplitude = (double)((double)element.Volume / 100f);
				}
				
			}
			
		
			
			//
			//	Now based on the mixers, we need to combine the oscillator
			//	sound data into one stream.
			//
			
			short[] toret = null;// = new short[oscone.Length];
			
			
			Signals = new Dictionary<string, short[]>();
			
			
			Console.WriteLine("Adding signals from generators.");
			for(int egen = 0; egen < Generators.Length; egen++)
			{
				if(Generators[egen] != null)
				{
					Signals.Add(egen.ToString(), Generators[egen].GetData(framecnt, note, octave));
				}
			}
			
			Console.WriteLine("Creating output from processor chain.");
			bool newnote = this.IsNoteNew;
			for(int ep = 0; ep < this.Processors.Length; ep++)
			{
				if(this.Processors[ep] != null) { this.Processors[ep].IsNoteNew = newnote; }
			}
			toret = Process(0);
			
			
			
			return toret;
		
		}
		
		public void LoadGenerators(string trackpath, int sampleid)
		{
			Console.WriteLine("Attempting to load generators");
			if(!Directory.Exists(Engine.PFP(trackpath))) { return; }
			if(!Directory.Exists(Engine.PFP(trackpath + "Samples/Generators/" + sampleid.ToString()))) { return; }
			
			//GeneratorTypes = new string[128];
			if(GeneratorTypes == null) { return; }
			Generators = new Machines.Generator[GeneratorTypes.Length];
			
			for(int eg = 0; eg < Generators.Length; eg++)
			{
				if(File.Exists(Engine.PFP(trackpath + "Samples/Generators/" + sampleid.ToString() + "/" + eg.ToString() + ".xml"))
				&& GeneratorTypes[eg] != null && GeneratorTypes[eg] != "")
				{
					//Machines.Processor inp;
					//Type tt = Type.GetType(ProcessorTypes[eg]);
					Type t = Type.GetType(GeneratorTypes[eg]);
					XmlSerializer s = new XmlSerializer(t);
					TextReader tr = new StreamReader(Engine.PFP(trackpath + "Samples/Generators/" + sampleid.ToString() + "/" + eg.ToString() + ".xml"));
					//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg] = new Sample();
					//inp = new Machines.Processor();
					//inp = (Machines.Processor)s.Deserialize(tr);
					Console.WriteLine("Trying to spawn a generator.");
					//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg] = new gurumod.Machines.Mixer();
					
					//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg] = (Machines.Processor)Activator.CreateInstance(t, new Object[]{s.Deserialize(tr)});
					Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg] = (Machines.Generator)Activator.CreateInstance(t, true);
					Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg] = (Machines.Generator)s.Deserialize(tr);
					//Activator.CreateInstance()
					
					//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg]
				}
			}
		}
		
		public void LoadProcessors(string trackpath, int sampleid)
		{
			Console.WriteLine("Attempting to load processors");
			if(!Directory.Exists(Engine.PFP(trackpath))) { return; }
			if(!Directory.Exists(Engine.PFP(trackpath + "Samples/Processors/" + sampleid.ToString()))) { return; }
			
			//GeneratorTypes = new string[128];
			if(ProcessorTypes == null) { return; }
			Processors = new Machines.Processor[ProcessorTypes.Length];
			
			for(int eg = 0; eg < Processors.Length; eg++)
			{
				if(File.Exists(Engine.PFP(trackpath + "Samples/Processors/" + sampleid.ToString() + "/" + eg.ToString() + ".xml"))
				&& ProcessorTypes[eg] != null && ProcessorTypes[eg] != "")
				{
					//Machines.Processor inp;
					//Type tt = Type.GetType(ProcessorTypes[eg]);
					Type t = Type.GetType(ProcessorTypes[eg]);
					XmlSerializer s = new XmlSerializer(t);
					TextReader tr = new StreamReader(Engine.PFP(trackpath + "Samples/Processors/" + sampleid.ToString() + "/" + eg.ToString() + ".xml"));
					//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg] = new Sample();
					//inp = new Machines.Processor();
					//inp = (Machines.Processor)s.Deserialize(tr);
					Console.WriteLine("Trying to spawn a processor.");
					//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg] = new gurumod.Machines.Mixer();
					
					//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg] = (Machines.Processor)Activator.CreateInstance(t, new Object[]{s.Deserialize(tr)});
					Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg] = (Machines.Processor)Activator.CreateInstance(t, true);
					Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg] = (Machines.Processor)s.Deserialize(tr);
					//Activator.CreateInstance()
					
					//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[eg]
				}
			}
		}
		
		/*public void InitOscillators()
		{
			Oscs = new Oscilator[3];
			Oscs[0] = new Oscilator();
			Oscs[1] = new Oscilator();
			Oscs[2] = new Oscilator();
			
			Oscs[0].Enabled = true;
			Oscs[0].WaveType = Generator.TypeSine;
		}
		
		public void InitMixers()
		{
			Mixers = new MixerSettings[2];
				
			for(int emx = 0; emx < Mixers.Length; emx++)
			{
				if(Mixers[emx] == null)
				{
					Mixers[emx] = new MixerSettings();
				}
			}
		}*/
		
		public void InitGenerators()
		{
			Generators = new Machines.Generator[1];
			Generators[0] = new Machines.Osc();
			Generators[0].Enabled = true;
			
		}
		
		public void InitProcessors()
		{
			Processors = new Machines.Processor[1];
			Processors[0] = new Machines.Mixer();
			Processors[0].Initialize();
			//Processors[0].InitInputs();
		}
		
		//public short[] MixAudio(short[] SourceA, short[] SourceB, MixerSettings MxSettings)
		/*public short[] MixAudio(MixerSettings MxSettings)
		{
			short[] SourceA = new short[1];//this.Signals[MxSettings.SourceAID];
			short[] SourceB = new short[1];//this.Signals[MxSettings.SourceBID];
			short[] MaxGateControl = new short[1];
			short[] MinGateControl = new short[1];
			
			//Console.WriteLine("SourceTypeA {0} B {1}", MxSettings.SourceTypeA, MxSettings.SourceTypeB);
			if(MxSettings.SourceTypeA == MixerSettings.SourceTypeOscillator) { SourceA = this.Signals[MxSettings.SourceAID.ToString()]; }
			else { Console.WriteLine("SourceA is a Mixer"); SourceA = MixAudio(Mixers[MxSettings.SourceAID]); }
			
			if(MxSettings.SourceTypeB == MixerSettings.SourceTypeOscillator) { SourceB = this.Signals[MxSettings.SourceBID.ToString()]; }
			else { Console.WriteLine("SourceB is a Mixer"); SourceB = MixAudio(Mixers[MxSettings.SourceBID]); }
			
			if(MxSettings.MaxGateControlSource > -1) { MaxGateControl = this.Signals[MxSettings.MaxGateControlSource.ToString()]; }
			if(MxSettings.MinGateControlSource > -1) { MinGateControl = this.Signals[MxSettings.MinGateControlSource.ToString()]; }
			
			if(SourceA == null && SourceB == null)
			{
				Console.WriteLine("Attemping to MixAudio, but both sources are null!");
				return null;
			}
			
			if(SourceA == null) { Console.WriteLine("MixAudio: SourceA is null => returning SourceB"); return SourceB; }
			if(SourceB == null) { Console.WriteLine("MixAudio: SourceB is null => returning SourceA"); return SourceA; }
			
			//Console.WriteLine("Mixing 2 Sources of Audio");
			short[] toret = new short[SourceA.Length];
			for(int esamp = 0; esamp < SourceA.Length; esamp++)
			{
				if(MxSettings.MixMethod == MixerSettings.MixMethodAdd)
				{
					toret[esamp] = (short)((SourceA[esamp] + SourceB[esamp])/* * this.Amplitude*///);
				/*	if(esamp > 0)
					{
						if(toret[esamp] < 20000 && toret[esamp] > -20000)
						{
							//toret[esamp] = 0;
							//toret[esamp] = (short)((toret[esamp - 1] * 4) + toret[esamp]);
						}
						//toret[esamp] = (short)((toret[esamp - 1] * 4) + toret[esamp]);
					}
				}
				else if(MxSettings.MixMethod == MixerSettings.MixMethodSubtract)
				{
					//toret[esamp] = (short)((SourceA[esamp] - SourceB[esamp])/* * this.Amplitude*//*);
					if(SourceA[esamp] > SourceB[esamp]) { toret[esamp] = (short)(SourceA[esamp] - SourceB[esamp]); }
					else { toret[esamp] = (short)(SourceB[esamp] - SourceA[esamp]); }
				}
				else if(MxSettings.MixMethod == MixerSettings.MixMethodDivide)
				{
					short usea = SourceA[esamp];
					short useb = SourceB[esamp];
					if(useb == 0) { useb = 1; }
					toret[esamp] = (short)(usea / useb);
				}
				else if(MxSettings.MixMethod == MixerSettings.MixMethodMultiply)
				{
					toret[esamp] = (short)(SourceA[esamp] * SourceB[esamp]);
				}
				
				// y[i] = y[i-1] + α * (x[i] - y[i-1])
				/*if(esamp > 1)
				{
					toret[esamp] = (short)(toret[esamp - 1] + (toret[esamp] / (toret[esamp - 1] + 1)) * (toret[esamp] - toret[esamp - 1]));
					//toret[esamp] = (short)(toret[esamp] + (toret[esamp - 10] / 2));
					//toret[esamp] = (short)((toret[esamp] + toret[esamp - 1]) / 2);
				}*/
				
				
				/*
				try
				{
					//	Gate functions
					double usemin = MxSettings.GateMin;
					double usemax = MxSettings.GateMax;
					
					if(MxSettings.MaxGateControlSource > -1)
					{
						usemax = (double)(Math.Abs((double)MaxGateControl[esamp]) / (double)short.MaxValue);
						//Console.WriteLine("Gate: Min {0} Max {1}", usemin, usemax);
					}
					if(MxSettings.MinGateControlSource > -1)
					{
						usemin = (double)(Math.Abs((double)MinGateControl[esamp]) / (double)short.MaxValue);
					}
					
					
					
					if(Math.Abs(toret[esamp]) < (short.MaxValue * usemin) || Math.Abs(toret[esamp]) > (short.MaxValue * usemax))
					{
						toret[esamp] = 0;
					}//else { toret[esamp] = (short)(toret[esamp] * -1); }
					//if(toret[esamp] > 0) { toret[esamp] = (short)0; }
				}
				catch(Exception ex)
				{
					Console.WriteLine("Exception while applying gate.");
					Console.WriteLine(ex.Message);
				}
			}
			
			//if(MixMethod == MixerSettings.MixMethodAdd) { Console.WriteLine("MixAudio: Method was add."); }
			//if(MixMethod == MixerSettings.MixMethodSubtract) { Console.WriteLine("MixAudio: Method was subtract."); }
			
			return toret;
		}
		*/
	
		public short[] Process(int processorid)
		{
			Console.WriteLine("Processing id: {0}", processorid);
			
			try
			{
				if(Processors == null)
				{
					Console.WriteLine("Processors is null!");
					return null;
				}
				
				if(Processors[processorid] == null)
				{
					Console.WriteLine("Processor {0} is null :(", processorid);
					return null;
				}
				
				if(Processors[processorid].Inputs == null)
				{
					Console.WriteLine("Processor {0} Inputs is null!", processorid);
					Processors[processorid].Initialize();
					return null;
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Exception during Process");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				Environment.Exit(0);
			}
			for(int ein = 0; ein < Processors[processorid].Inputs.Length; ein++)
			{
				Console.WriteLine("Processing {0} input {1}", processorid, ein);
				if(Processors[processorid].Inputs[ein].SourceType == Machines.InputData.SourceTypeProcessor
			    && Processors[processorid].Inputs[ein].SourceID > -1)
				{
					//Console.Write("yeah");
					if(!Signals.ContainsKey("proc" + Processors[processorid].Inputs[ein].SourceID.ToString()))
					{
						try
						{
							int nproc = Processors[processorid].Inputs[ein].SourceID;
							
							Console.WriteLine("Adding signal {0}", "proc" + nproc.ToString());
							
							Signals.Add("proc" + nproc.ToString(), Process(nproc));
							
						}
						catch(Exception ex)
						{
							Console.WriteLine("Exception while processing a processor.");
							Console.WriteLine(ex.Message);
							Console.WriteLine(ex.StackTrace);
							Environment.Exit(0);
						}
					}
				}
			}
			
			return Processors[processorid].Process(Signals);
		}

		public int NextGeneratorID()
		{
			for(int eg = 0; eg < this.Generators.Length; eg++)
			{
				if(this.Generators[eg] == null) { return eg; }
			}

			return -1;
		}

		public int NextProcessorID()
		{
			for(int eg = 0; eg < this.Processors.Length; eg++)
			{
				if(this.Processors[eg] == null) { return eg; }
			}

			return -1;
		}


	}
}

