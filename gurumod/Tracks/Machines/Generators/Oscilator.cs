using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace gurumod
{
	public class Oscilator
	{
		[XmlElement("Enabled")] public bool Enabled = false;
		[XmlElement("WaveType")] public int WaveType = Generator.TypeSine;
		[XmlElement("Frequency")] public double Frequency = 440;
		[XmlIgnore()] public int FramesIntoSample = 0;
		[XmlElement("Amplitude")] public double Amplitude = 0.75;
		[XmlElement("SampleRate")] public int SampleRate = 44100;
		[XmlElement("Format")] public ALFormat Format = ALFormat.Mono16;
		
		[XmlIgnore()] public int LastNote = 0;
		[XmlIgnore()] public int LastOctave = 5;
		
		[XmlIgnore()] public short AmpStep = (short)0;
		[XmlIgnore()] public int Cycle = 0;
		
		public Oscilator ()
		{
		}
		
		public short[] GetData(int frames, double frequency)
		{
			if(Enabled == false) { return null; }
			//Console.WriteLine("Oscillator.GetData:  Frequency (OG/Fixed): {0} {1}", Frequency, frequency);
			//double usefrequency = 0;
			
			if(WaveType == Generator.TypeSine) { return SineWave(frames, frequency); }
			else if(WaveType == Generator.TypeSquare) { return SquareWave(frames, frequency); }
			else if(WaveType == Generator.TypeTriangle) { return TriangleWave(frames, frequency); }
			else if(WaveType == Generator.TypeSawtooth) { return SawtoothWave(frames, frequency); }
			else
			{
				Console.WriteLine("Wave type {0} not recognized.", WaveType);
				return null;
			}
		}
		
		public short[] GetData(int frames, int note, int octave)
		{
			//if(note > -1 && octave > -1) { Console.WriteLine("Machine: GetData note {0} octave {1}", note, octave); }
			if(!Enabled) { return null; }
			double freq = this.Frequency;
			
			if(note < 0) { note = LastNote; } else { LastNote = note; }
			if(octave < 0) { octave = LastOctave; } else { LastOctave = octave; }
			
			if(octave == 4) { freq = freq / 2; }
			else if(octave == 6) { freq = freq * 2; }
			
			double noteoff = (double)((double)(freq / 7) * (double)note);
			
			//Console.WriteLine("Getting wave data: note {0} octave {1} freq {2} noteoff {3}", note, octave, freq, noteoff);
			//freq = freq + noteoff;
			
			return GetData(frames, freq + noteoff);
		}
		
		public short[] SineWave(int frames, double frequency)
		{
			short[] toret = new short[frames];
			//Console.WriteLine("SineWave: Frequency {0} {1}", frequency, ((1f / frequency) * this.Frequency));
			
			int wavelen = SamplesPerWavelength(frequency);
			double sinsin = Math.Sin(2 * Math.PI * frequency);
			
			//Console.WriteLine("SineWave: wavelen, sinsin: {0} {1}", wavelen, sinsin);
			
			for(int i = 0; i < frames; i++)
			{
				//short)(short.MaxValue * Math.Sin((2 * Math.PI * frequency) / samplerate * i))
				//toret[i] = (short)(short.MaxValue * Math.Sin ((2 * Math.PI * ((1f / frequency) * this.Frequency)) / this.SampleRate * (i + this.FramesIntoSample)));
				//toret[i] = (short)(short.MaxValue * Math.Sin((2 * Math.PI * frequency)) / this.SampleRate * (i + this.FramesIntoSample));
				//toret[i] = (short)(short.MaxValue * Math.Sin((2 * Math.PI * frequency)) / this.SampleRate * (i));
				
				toret[i] = (short)(short.MaxValue * Math.Sin((2 * Math.PI * frequency) / SampleRate * i));
				//Console.WriteLine("SineWave: {0}", toret[i]);
				//toret[i] = (short)Math.Floor(toret[i] * Amplitude);
			}
			
			this.FramesIntoSample = this.FramesIntoSample + frames;
			return toret;
		}
		
		public short[] SquareWave(int frames, double frequency)
		{
			//Console.WriteLine("SquareWave generating {0} frames at {1}", frames, frequency);
			short[] toret = new short[frames];
			//double usefrequency = this.Frequency + (this.Frequency * frequency);
			//double usefrequency = this.Frequency * frequency;
			
			int wavelen = SamplesPerWavelength(frequency);
			//Console.WriteLine("wavelen (real): {0}, (ogfreq): {1}", wavelen, this.Frequency);
			if(FramesIntoSample >= wavelen) { FramesIntoSample = 0; }
			bool athigh = true;
			for (int i = 0; i < frames; i++)
			{   
				
				if(this.FramesIntoSample >= (double)(wavelen / 2)) { athigh = false; }
				else { athigh = true; }
				
				short tamp = 0;
				if(athigh) { tamp = short.MaxValue; }
				else { tamp = short.MinValue; }
				
				toret[i] = (short)(tamp * this.Amplitude);
				//short ttmp = (short)(short.MaxValue * Math.Sin((2 * Math.PI * ((1f / frequency) * this.Frequency)) / this.SampleRate * (i + this.FramesIntoSample)));
				//toret[i] = (short)(Math.Floor((Math.Sign(ttmp) * (short)ttmp) * Amplitude));
			    //Console.WriteLine("Square wave frame ampl: {0} {1} {2} {3} :: tamp {4}", toret[i], this.FramesIntoSample, wavelen, athigh, tamp);
				this.FramesIntoSample++;
				//if(this.FramesIntoSample == 100) { Console.WriteLine("Frame value {0} SPW {1}", toret[i], wavelen); }
				if(this.FramesIntoSample >= wavelen) { this.FramesIntoSample = 0; }
			}
			
			//this.FramesIntoSample = this.FramesIntoSample + frames;
			return toret;
		}
		
		public short[] TriangleWave(int frames, double frequency)
		{
			//Console.WriteLine("Retrieving {0} frames at {1} frequency.", frames, frequency);
			short[] toret = new short[frames];
			
			int wavelen = SamplesPerWavelength(frequency);
			double some = (double)((double)wavelen) / 2.0;
			double scope = (double)(short.MaxValue * 2);
			// In 'some' frames, the signal needs to move 'scope units'
			int estep = (int)Math.Ceiling(scope / some);
			
			for(int eframe = 0; eframe < frames; eframe++)
			{
				if(Cycle == 0)
				{
					AmpStep = (short)(AmpStep + (short)estep);
					if(AmpStep >= short.MaxValue) { Cycle = 1; }
				}
				else
				{
					AmpStep = (short)(AmpStep - (short)estep);
					if(AmpStep <= short.MinValue) { Cycle = 0; }
				}
				
				toret[eframe] = AmpStep;
			}
			
			/*int framesinsegment = Convert.ToInt32((double)this.SampleRate / (double)wavelen);
			//int estep = short.MaxValue / (framesinsegment / 4);
			
			Console.WriteLine("wavelen {0}   fis {1}   estep {2}", wavelen, framesinsegment, estep);
			for(int eframe = 0; eframe < frames; eframe++)
			{
				
				
				//if(thisampl > short.MaxValue) { thisampl = short.MaxValue; }
				
				if(eframe > framesinsegment - 1 && eframe < framesinsegment * 2)
				{
					Cycle = 1;
					this.FramesIntoSample = 0;
				}
				else if(eframe > (framesinsegment * 2) - 1 && eframe < framesinsegment * 3)
				{
					Cycle = 2;
					this.FramesIntoSample = 0;
				}
				else if(eframe > (framesinsegment * 3) - 1)
				{
					Cycle = 3;
					this.FramesIntoSample = 0;
				}
				else
				{
					Cycle = 0;
					this.FramesIntoSample = 0;
				}
				
				int thisampl = this.FramesIntoSample * estep;
				//Console.WriteLine("TriangleWave: Thisampl: {0}", thisampl);
				if(Cycle == 1) { thisampl = short.MaxValue - thisampl; }
				else if(Cycle == 2) { thisampl = thisampl * -1; }
				else if(Cycle == 3) { thisampl = short.MinValue + thisampl; }
				
				toret[eframe] = (short)(thisampl/* * this.Amplitude*///);
				//Console.WriteLine("TriangleWave: Frame: {0}", toret[eframe]);
				/*this.FramesIntoSample++;
				if(this.FramesIntoSample == wavelen) { Cycle = 0; this.FramesIntoSample = 0; }*/
			//}
			
			
			return toret;
		}
		
		public short[] SawtoothWave(int frames, double frequency)
		{
			short[] toret = new short[frames];
			
			int wavelen = SamplesPerWavelength(frequency);
			//int framesinsegment = Convert.ToInt32((double)this.SampleRate / (double)wavelen);
			int estep = (short.MaxValue + (-1 * short.MinValue) + 1) / (wavelen);
			
			int loops = 0;
			//Console.WriteLine("Entering sawtooth generation loop");
			for(int eframe = 0; eframe < frames; eframe++)
			{
				
				
				//if(thisampl > short.MaxValue) { thisampl = short.MaxValue; }
				
				int thisampl = short.MaxValue - this.FramesIntoSample * estep;
				/*if(FramesIntoSample >= (wavelen / 2))
				{
					//	Originally 0 was short.Max or something
					//thisampl = 0 - ((this.FramesIntoSample * estep) - short.MaxValue);
					
				}
				else
				{*/
					//thisampl = short.MaxValue - (this.FramesIntoSample * estep);
				//}
				
				toret[eframe] = (short)(thisampl * this.Amplitude);
				
				this.FramesIntoSample++;
				loops++;
				//if(loops >= 1) { Console.WriteLine("Loop 100 frame: {0} {1} {2} :: estep: {3}: shortmax, min {4} {5}", toret[eframe], thisampl, this.Amplitude, estep, short.MaxValue, short.MinValue); loops = 0; }
				if(this.FramesIntoSample >= wavelen)
				{
					//Console.WriteLine("FramesIntoSample >= wavelen ({0} >= {1})", this.FramesIntoSample, wavelen);
					this.FramesIntoSample = 0; 
				}
			}
			
			
			return toret;
		}
		
		public int SamplesPerWavelength()
		{
			return SamplesPerWavelength(Frequency);
		}
		
		public int SamplesPerWavelength(double frequency)
		{
			if(frequency == 0) { return 0; }
			if(this.SampleRate < 1) { return 0; }
			//Console.WriteLine("SamplesPerWaveLength: {0} {1} {2}", this.SampleRate, this.Frequency, (this.SampleRate / this.Frequency));
			return Convert.ToInt32((double)(double)this.SampleRate / (frequency));
		}
	}
}

