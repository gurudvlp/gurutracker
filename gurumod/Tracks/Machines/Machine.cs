using System;
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
		[XmlElement()] public Oscilator[] Oscs = new Oscilator[3];
		[XmlElement()] public MixerSettings[] Mixers = new MixerSettings[2];
		
		[XmlIgnore()] public Machines.Generator[] Generators;
		[XmlIgnore()] public Machines.Processor[] Processors;
		
		[XmlElement("MaxGenerators")] public int MaxGenerators = 32;
		[XmlElement("MaxProcessors")] public int MaxProcessors = 32;
		
		[XmlIgnore()] public int LastNote = -2;
		[XmlIgnore()] public int LastOctave = 5;
		[XmlIgnore()] public bool Running = false;
		//[XmlIgnore()] public short[,] Signals;
		[XmlIgnore()] public Dictionary<string, short[]> Signals = new Dictionary<string, short[]>();
		
		public Machine ()
		{
			Oscs[0] = new Oscilator();
			Oscs[0].Enabled = true;
			Oscs[0].WaveType = Generator.TypeSine;
			
			Oscs[1] = new Oscilator();
			Oscs[2] = new Oscilator();
			
			Mixers = new MixerSettings[2];
			Mixers[0] = new MixerSettings();
			Mixers[1] = new MixerSettings();
			
			Generators = new Machines.Generator[this.MaxGenerators];
			Processors = new Machines.Processor[this.MaxProcessors];
			
			Generators[0] = new Machines.Osc();
			Processors[0] = new Machines.Mixer();
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
			
			if(Oscs == null)
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
			
			float freq = ((1f / 7f) * note);
			if(octave == 4) { freq = 0.5f + (freq * 0.5f); }
			if(octave == 5) { freq =  1.0f + freq; }
			if(octave == 6) { freq =  2.0f + (freq * 2.0f); }
			
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
			
			//short[] oscone = Oscs[0].GetData(framecnt, (double)freq);
			//short[] osctwo = Oscs[1].GetData(framecnt, (double)freq);
			//short[] oscthr = Oscs[2].GetData(framecnt, (double)freq);
			
			//if(osctwo == null && oscthr == null) { return oscone; }
			
			//
			//	Now based on the mixers, we need to combine the oscillator
			//	sound data into one stream.
			//
			
			short[] toret = null;// = new short[oscone.Length];
			
			//Signals = new short[Oscs.Length, framecnt];
			Signals = new Dictionary<string, short[]>();
			for(int eosc = 0; eosc < Oscs.Length; eosc++)
			{
				Signals.Add(eosc.ToString(), Oscs[eosc].GetData(framecnt, note, octave));
				//Signals[eosc] = Oscs[eosc].GetData(framecnt, note, octave);
			}
			//toret = MixAudio(Mixers[0]);
			toret = Process(0);
			
			//if(Mixers[0].SourceTypeA == MixerSettings.SourceTypeOscillator)
			//{
				
				
				/*toret = MixAudio(Oscs[Mixers[0].SourceAID].GetData(framecnt, note, octave),
				                 Oscs[Mixers[0].SourceBID].GetData(framecnt, note, octave),
				                 Mixers[0]);
				*/
			//	if(toret == null) { Console.WriteLine("Mixer settings found, but returning a null stream."); }
			//}
			//else
			//{
				//Console.WriteLine("No mixer settings were found.");
			//}
			
			//short[] toret = new short[oscone.Length];
			/*for(int i = 0; i < oscone.Length; i++)
			{
				toret[i] =(short)(oscone[i] + osctwo[i]);
			}
			*/
			
			return toret;
			//if(WaveType == Generator.TypeSine) { return SineWave (framecnt, (double)freq); }
			//if(WaveType == Generator.TypeSquare) { return SquareWave(framecnt, (double)freq); }
			//return null;
		}
		
		
		
		/*public static short[] SineWave(double frequency, int seconds, int samplerate)
		{
			short[] toret;
	 
			int frames = seconds * samplerate;
			toret = new short[frames];
			for (int i = 0; i < frames; i++)
			{
				toret[i] = (short)(short.MaxValue * Math.Sin((2 * Math.PI * frequency) / samplerate * i));
			}
			
			return toret;
		}*/
		
		public void InitOscillators()
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
		}
		
		//public short[] MixAudio(short[] SourceA, short[] SourceB, MixerSettings MxSettings)
		public short[] MixAudio(MixerSettings MxSettings)
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
					toret[esamp] = (short)((SourceA[esamp] + SourceB[esamp])/* * this.Amplitude*/);
					if(esamp > 0)
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
					//toret[esamp] = (short)((SourceA[esamp] - SourceB[esamp])/* * this.Amplitude*/);
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
		
		public short[] Process(int processorid)
		{
			for(int ein = 0; ein < Processors.Length; ein++)
			{
				if(Processors[processorid].Inputs[ein].SourceType == Machines.InputData.SourceTypeProcessor)
				{
					if(!Signals.ContainsKey("proc" + Processors[processorid].Inputs[ein].SourceID.ToString()))
					{
						int nproc = Processors[processorid].Inputs[ein].SourceID;
						Signals.Add("proc" + nproc.ToString(), Process(nproc));
					}
				}
			}
			
			return Processors[processorid].Process(Signals);
		}
	}
}

