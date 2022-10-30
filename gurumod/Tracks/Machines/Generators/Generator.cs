// 
//  Generator.cs
//  
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
// 
//  Copyright (c) 2012-2022 Brian Murphy
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace gurumod.Machines
{
	[XmlRoot("SoundGenerator")]
	[Serializable()]
	public abstract class Generator : ISerializable
	{
		[XmlElement("Enabled")] [JsonInclude]
		public bool Enabled = false;
		
		[XmlElement("Frequency")] [JsonInclude]
		public double Frequency = 440;

		[XmlIgnore()] [JsonInclude] 
		public int FramesIntoSample = 0;

		[XmlElement("Amplitude")] [JsonInclude]
		public double Amplitude = 0.75;

		[XmlElement("SampleRate")] [JsonInclude]
		public int SampleRate = 44100;

		[XmlElement("GeneratorType")] [JsonInclude]
		public int GeneratorType = 0;

		[XmlElement("Format")] [JsonInclude]
		public ALFormat Format = ALFormat.Mono16;

		[XmlIgnore()] [JsonIgnore] public static int GeneratorTypeOscillator = 0;
		[XmlIgnore()] [JsonIgnore] public static int GeneratorTypeWavePlayer = 1;
		
		[XmlIgnore()] [JsonInclude]
		public int LastNote = 0;

		[XmlIgnore()] [JsonInclude]
		public int LastOctave = 5;


		public Generator ()
		{
		}

		public Generator(SerializationInfo info, StreamingContext ctxt)
		{
			Construct(info, ctxt);
		}

		public void Construct(SerializationInfo info, StreamingContext ctxt)
		{
			Enabled = (bool)info.GetValue("Enabled", typeof(bool));
			Frequency = (double)info.GetValue("Frequency", typeof(double));
			Amplitude = (double)info.GetValue("Amplitude", typeof(double));
			SampleRate = (int)info.GetValue("SampleRate", typeof(int));
			GeneratorType = (int)info.GetValue("GeneratorType", typeof(int));
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{

			info.AddValue("Enabled", Enabled);
			info.AddValue("Frequency", Frequency);
			info.AddValue("Amplitude", Amplitude);
			info.AddValue("SampleRate", SampleRate);
			info.AddValue("GeneratorType", GeneratorType);
		}
		
		public abstract short[] GetData(int frames, double frequency);
		public abstract short[] GetData(int frames, int note, int octave);
		
		public virtual void Save(string trackpath, int sampleid, int generatorid)
		{
			try
			{
				string filename = trackpath + "Samples/Generators";//" + sampleid.ToString();
				if(!Directory.Exists(filename)) { Directory.CreateDirectory(filename); }
				filename = filename + "/" + sampleid.ToString();
				if(!Directory.Exists(filename)) { Directory.CreateDirectory(filename); }
				
				filename = filename + "/" + generatorid.ToString() + ".xml";
				
				//XmlSerializer s = new XmlSerializer(typeof(Generator));
				XmlSerializer s = new XmlSerializer(this.GetType());
				TextWriter w = new StreamWriter(Engine.PFP(filename));
				s.Serialize(w, this);
				w.Close();
			}
			catch(Exception ex)
			{
				Console.WriteLine("There was an exception while saving the generator.");
				Console.WriteLine(ex.Message);
			}
		}

		public virtual byte[] GTString()
		{
			//	For each generator:
			//		tessssssfffffaaaa
			//			t: 1 digit generator type
			//			e: 1 digit enabled (0 or 1)
			//			s: 6 digit sample rate
			//			f: 5 digit frequency
			//			a: 4 digit amplitude (x.xx)

			MemoryStream ms = new MemoryStream();
			StreamWriter wr = new StreamWriter(ms);
			wr.AutoFlush = true;

			string gentype = this.GeneratorType.ToString();
			string enabled = "0"; if(this.Enabled) { enabled = "1"; }
			string samplerate = this.SampleRate.ToString("D6");
			string frequency = this.Frequency.ToString("0000.0");
			string amp = this.Amplitude.ToString("0.00");

			wr.Write(gentype + enabled + samplerate + frequency + amp);
			return ms.ToArray();
		}


		public virtual Dictionary<string, string> Details()
		{
			return new Dictionary<string, string>();
		}
	}
}

