using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace gurumod
{
	
	public class Generator
	{
		[XmlIgnore()] public static int TypeSine = 0;
		[XmlIgnore()] public static int TypeSilence = 4;
		[XmlIgnore()] public static int TypeSquare = 1;
		[XmlIgnore()] public static int TypeTriangle = 2;
		[XmlIgnore()] public static int TypeSawtooth = 3;
		
		[XmlElement("Type")] public int WaveType = 0;
		[XmlElement("Frequency")] public double Frequency = 440;
		[XmlElement("SampleRate")] public int SampleRate = 44100;
		[XmlElement("Length")] public int Length = 3;
		[XmlElement("ALFormat")] public ALFormat Format = ALFormat.Mono16;
		
		public Generator ()
		{
		}

		public string GTString()
		{
			//	If the wave generator flag is 1, then:
			//		tfffffssssssllll
			//			t: sound type
			//			f: 5 digit frequency
			//			s: 6 digit sample rate
			//			l: 4 digit length in seconds
			string toret = "";
			string wavetype = this.WaveType.ToString();
			string frequency = this.Frequency.ToString("D5");
			string samplerate = this.SampleRate.ToString("D6");
			string len = this.Length.ToString("D4");

			toret = wavetype + frequency + samplerate + len;

			return toret;

		}
		
		
		public short[] Generate()
		{
			if(WaveType == Generator.TypeSine) { return Generator.SineWave(this.Frequency, this.Length, this.SampleRate); }
			else if(WaveType == Generator.TypeSquare) { return Generator.SquareWave(this.Frequency, this.Length, this.SampleRate); }
			else { return null; }
		}
		
		public static short[] SineWave(double frequency, int seconds, int samplerate)
		{
			short[] toret;
	 
			int frames = seconds * samplerate;
			toret = new short[frames];
			for (int i = 0; i < frames; i++)
			{
				toret[i] = (short)(short.MaxValue * Math.Sin((2 * Math.PI * frequency) / samplerate * i));
			}
			
			return toret;
		}
		
		public static short[] SquareWave(double frequency, int seconds, int samplerate)
		{
			short[] toret;
			int frames = seconds * samplerate;
			toret = new short[frames];
			
			for (int i = 0; i < frames; i++)
			{    
				short ttmp = (short)(short.MaxValue * Math.Sin((2 * Math.PI * frequency) / samplerate * i));
				toret[i] = (short)(Math.Sign(ttmp) * (short)ttmp);
			    
			}
			
			return toret;
		}
	}
}

