// 
//  Sample.cs
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
using System.Diagnostics;
using System.Threading;
using System.IO;

using gurumod.Logging;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace gurumod
{
	[XmlRoot("Sample")]
	[Serializable()]
	public class Sample : ISerializable
	{
		[XmlElement("Name")] [JsonInclude]
		public string Name 
		{
			get { return _name; }
			set { _name = value; }
		}
		private string _name = "unnamed";
		
		[XmlElement("Artist")] [JsonInclude]
		public string Artist 
		{
			get { return _artist; }
			set { _artist = value; }
		}
		private string _artist = "Unknown Artist";

		[XmlElement("Year")] [JsonInclude]
		public int Year 
		{
			get { return _year; }
			set { _year = value; }
		}
		private int _year = 2022;

		[XmlElement("ID")] [JsonInclude]
		public int ID 
		{
			get { return _id; }
			set { _id = value; }
		}
		private int _id = 0;
		

		[XmlElement("Filename")] [JsonInclude]
		public string Filename 
		{
			get { return _filename; }
			set { _filename = value; }
		}
		private string _filename = "";
		
		[XmlIgnore()] [JsonIgnore]
		private long _bitRate = 0;
		
		[XmlIgnore()] [JsonInclude]
		public long BitRate 
		{
			get { return _bitRate; }
			set { _bitRate = value; }
		}

		[XmlIgnore()] [JsonIgnore]
		public byte[] SoundData;
		
		[XmlIgnore()] [JsonIgnore]
		public int buffer;

        [XmlIgnore()] [JsonIgnore]
		int source;

        [XmlIgnore()] [JsonIgnore]
		int state;

		[XmlIgnore()] [JsonIgnore]
		public static OpenTK.Audio.OpenAL.ALContext context;

		[XmlIgnore()] [JsonIgnore]
		public static OpenTK.Audio.OpenAL.ALDevice alDevice;

        [XmlIgnore()] [JsonInclude]
		public int channels;

		[XmlIgnore()] [JsonInclude]
		public int bits_per_sample;

		[XmlIgnore()] [JsonInclude]
		public int sample_rate;
		
		[XmlIgnore()] [JsonIgnore]
		public bool Loaded = false;

		[XmlElement("WaveGenerator")] [JsonInclude]
		public Generator WaveGenerator = new Generator();

		[XmlElement("UseWaveGenerator")] [JsonInclude]
		public bool UseWaveGenerator = false;

		[XmlElement("WaveMachine")] [JsonInclude]
		public Machine WaveMachine = new Machine();

		[XmlElement("UseWaveMachine")] [JsonInclude]
		public bool UseWaveMachine = false;
		
		public Sample ()
		{
			//	Check if a sound device has already been opened, and if a
			//	context for it already exists.  If not, we need to set that up
			//	now.
			if(Sample.context == OpenTK.Audio.OpenAL.ALContext.Null) 
			{
				Log.lInfo("Sound device not set up yet.  Doing so now...", "Sample", "Sample");
				Sample.alDevice = OpenTK.Audio.OpenAL.ALC.OpenDevice(null);
				
				if(Sample.alDevice == OpenTK.Audio.OpenAL.ALDevice.Null)
				{
					Log.lError("There was a problem opening the default device.", "Sample", "Sample");
				}
				
				//	Set up the audio context and assign it's device back to the
				//	original device.  I'm not entirely sure why we need to
				//	reassign the device, but it is shown in the documentation.
				Sample.context = OpenTK.Audio.OpenAL.ALC.CreateContext(alDevice, OpenTK.Audio.OpenAL.ALC.GetAttributeArray(alDevice));
				OpenTK.Audio.OpenAL.ALC.MakeContextCurrent(Sample.context);
			}
			
			buffer = AL.GenBuffer();
			source = AL.GenSource();
			
			//	Not sure how this is supposed to work exactly.  Maybe this is
			//	called when the object is deserialized from a saved state?
			if(Filename != null && Filename != "") 
			{
				LoadSample();
			}
		}

		//	Initialize a sample from a serialized version.
		public Sample(SerializationInfo info, StreamingContext ctxt)
		{
			Name = (string)info.GetValue("Name", typeof(string));
			Artist = (string)info.GetValue("Artist", typeof(string));
			Year = (int)info.GetValue("Year", typeof(int));
			ID = (int)info.GetValue("ID", typeof(int));
			Filename = (string)info.GetValue("Filename", typeof(string));
			BitRate = (long)info.GetValue("BitRate", typeof(long));
			SoundData = (byte[])info.GetValue("SoundData", typeof(byte[]));
			channels = (int)info.GetValue("channels", typeof(int));
			bits_per_sample = (int)info.GetValue("bits_per_sample", typeof(int));
			sample_rate = (int)info.GetValue("sample_rate", typeof(int));
			UseWaveGenerator = (bool)info.GetValue("UseWaveGenerator", typeof(bool));
			UseWaveMachine = (bool)info.GetValue("UseWaveMachine", typeof(bool));
			WaveMachine = (Machine)info.GetValue("WaveMachine", typeof(Machine));
			WaveGenerator = (Generator)info.GetValue("WaveGenerator", typeof(Generator));
		}


		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{

			info.AddValue("Name", Name);
			info.AddValue("Artist", Artist);
			info.AddValue("Year", Year);
			info.AddValue("ID", ID);
			info.AddValue("Filename", Filename);
			info.AddValue("BitRate", BitRate);
			info.AddValue("SoundData", SoundData);
			info.AddValue("channels", channels);
			info.AddValue("bits_per_sample", bits_per_sample);
			info.AddValue("sample_rate", sample_rate);
			info.AddValue("UseWaveGenerator", UseWaveGenerator);
			info.AddValue("UseWaveMachine", UseWaveMachine);
			info.AddValue("WaveMachine", WaveMachine);
			info.AddValue("WaveGenerator", WaveGenerator);
		}

		
		public void LoadSample()
		{
			if(this.Loaded) { return; }
			if(UseWaveGenerator)
			{
				ImportSample(WaveGenerator.Generate(), WaveGenerator.Format, WaveGenerator.SampleRate);
				this.Loaded = true;
			}
			else
			{
				if(this.Filename == null || this.Filename == "") { return; }
				
				LoadSample(this.Filename);
			}
		}
		
		public void LoadSample(string filename)
		{
			Log.lInfo("Loading sample: " + filename, "Sample", "LoadSample");
			if(this.Loaded) { return; }
			
			if(UseWaveGenerator)
			{
				ImportSample(WaveGenerator.Generate(), WaveGenerator.Format, WaveGenerator.SampleRate);
				this.Loaded = true;
			}
			else
			{
				try
				{
					if(this.Filename == null || this.Filename == "") { this.Filename = filename; }
					
					string fname = filename;
					this.SoundData = LoadWave(
						File.OpenRead(Engine.PFP(Engine.TheTrack.MyPath + "Samples/Audio/" + fname)), 
						out channels, 
						out bits_per_sample, 
						out sample_rate
					);
					
					//
					//	This next line originally loaded the sameple data
					//	to OpenAL.  Doesn't seem to work with modern C#
					//
					//	This will need to be revisited.
					//AL.BufferData(buffer, GetSoundFormat(channels, bits_per_sample), this.SoundData, this.SoundData.Length, sample_rate);

		            AL.Source(source, ALSourcei.Buffer, buffer);
				}
				catch(Exception ex)
				{
					Log.lWarning("Exception loading sample.", "Sample", "LoadSample");
					Log.lWarning(ex.Message, "Sample", "LoadSample");
				}
			}
            /*AL.SourcePlay(source);
			
			do
            {
                Thread.Sleep(250);
                Trace.Write(".");
                AL.GetSource(source, ALGetSourcei.SourceState, out state);
            }
            while ((ALSourceState)state == ALSourceState.Playing);

            Trace.WriteLine("");

            AL.SourceStop(source);
            AL.DeleteSource(source);
            AL.DeleteBuffer(buffer);
            */
			
			this.Loaded = true;
		}
		
		public void Play()
		{
			Play (1.0f);
			
			
		}
		
		public static void UnserializeSample(string trackpath, int sampleid)
		{
			if(Engine.TheTrack.Samples == null) { Engine.TheTrack.Samples = new Sample[Track.MaxSamples]; }
			
			XmlSerializer s = new XmlSerializer(typeof(Sample));
			TextReader tr = new StreamReader(Engine.PFP(trackpath));
			Engine.TheTrack.Samples[sampleid] = new Sample();
			Engine.TheTrack.Samples[sampleid] = (Sample)s.Deserialize(tr);
		}
		
		public void Play(float speed)
		{
			AL.GetSource(source, ALGetSourcei.SourceState, out state);
			if((ALSourceState)state == ALSourceState.Playing) { AL.SourceStop(source); }
			
			//Console.WriteLine("Speed of sample: {0}", speed);
			//AL.SpeedOfSound(speed * 1000);
			AL.Source(source, ALSourcef.Pitch, speed);
			//AL.DopplerFactor(speed * 50);
			AL.SourcePlay(source);
			
		}
		
		//	Loads a wave/riff audio file.
		//	This method was written about 10 years before this comment.  From
		//	the bit of research that I have been doing recently, I strongly
		//	believe that this may have come from Stack Overflow.
        public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported. 1");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported. 2 " + data_signature);

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
		
		public bool ImportSample(short[] audiodata, ALFormat format, int samplerate)
		{
			buffer = AL.GenBuffer();
			
			//	Import the sample data to OpenAL.  This does not work
			//	with modern C#.  It will need to be revisited!
			//
			//AL.BufferData(buffer, format, audiodata, audiodata.Length * 2, samplerate);
			this.sample_rate = samplerate;
			return true;
		}
		
		public void SaveSample(string TrackPath, int sampleid)
		{
			if(!TrackPath.EndsWith("/")) { TrackPath = TrackPath + "/"; }
			if(!Directory.Exists(TrackPath + "Samples"))
			{
				Directory.CreateDirectory(TrackPath + "Samples");
			}
			if(!Directory.Exists(TrackPath + "Samples/Meta"))
			{
				Directory.CreateDirectory(TrackPath + "Samples/Meta");
			}
			if(!Directory.Exists(TrackPath + "Samples/Audio"))
			{
				Directory.CreateDirectory(TrackPath + "Samples/Audio");
			}
			if(!Directory.Exists(TrackPath + "Samples/Generators"))
			{
				Directory.CreateDirectory(TrackPath + "Samples/Generators");
			}
			if(!Directory.Exists(TrackPath + "Samples/Processors"))
			{
				Directory.CreateDirectory(TrackPath + "Samples/Processors");
			}
			
			WaveMachine.ProcessorTypes = new string[WaveMachine.Processors.Length];
			for(int epr = 0; epr < Engine.TheTrack.Samples[sampleid].WaveMachine.Processors.Length; epr++)
			{
				if(WaveMachine.Processors[epr] != null)
				//if(WaveMachine.ProcessorTypes[epr] != null)
				{
					WaveMachine.ProcessorTypes[epr] = WaveMachine.Processors[epr].GetType().ToString();
					if(!Directory.Exists(TrackPath + "Samples/Processors/" + sampleid.ToString()))
					{
						Directory.CreateDirectory(TrackPath + "Samples/Processors/" + sampleid.ToString());
						
					}
					
					WaveMachine.Processors[epr].Save(TrackPath, sampleid, epr);
				}
				else
				{
					WaveMachine.ProcessorTypes = new string[1];
					WaveMachine.Processors = new gurumod.Machines.Processor[1];
					WaveMachine.Processors[0] = new gurumod.Machines.Mixer();
					WaveMachine.ProcessorTypes[0] = "gurumod.Machines.Mixer";
				}
			}
			
			WaveMachine.GeneratorTypes = new string[WaveMachine.Generators.Length];
			for(int eg = 0; eg < WaveMachine.Generators.Length; eg++)
			{
				if(WaveMachine.Generators[eg] != null)
				{
					WaveMachine.GeneratorTypes[eg] = WaveMachine.Generators[eg].GetType().ToString();
					if(!Directory.Exists(TrackPath + "Samples/Generators/" + sampleid.ToString()))
					{
						Directory.CreateDirectory(TrackPath + "Samples/Generators/" + sampleid.ToString());
					}
					
					WaveMachine.Generators[eg].Save(TrackPath, sampleid, eg);
				}
				else
				{
					WaveMachine.GeneratorTypes[eg] = "";
				}
			}
			/*if(WaveMachine.GeneratorTypes != null && eg < WaveMachine.GeneratorTypes.Length)
			{
				for(int eg = 0; eg < Engine.TheTrack.Samples[sampleid].WaveMachine.GeneratorTypes.Length; eg++)
				{
					/*tgentypes[eg]
					else if(WaveMachine.GeneratorTypes == null)
					{
						WaveMachine.GeneratorTypes = new string[1];
						WaveMachine.Generators = new gurumod.Machines.Generator[1];
						WaveMachine.Generators[0] = new gurumod.Machines.Osc();
						WaveMachine.GeneratorTypes[0] = "gurumod.Machines.Osc";
					}*/
					
					/*if(Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg] != null)
					{
						Directory.CreateDirectory(TrackPath + "Samples/Generators/" + eg.ToString());
						Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg].Save(TrackPath, sampleid, eg);
						//Engine.TheTrack.Samples[sampleid].WaveMachine.GeneratorTypes[eg] = Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg].GetType().ToString();
						tgentypes[eg] = Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg].GetType().ToString();
					}
				}
			}*/
			/*for(int eg = 0; eg < Engine.TheTrack.Samples[sampleid].WaveMachine.Generators.Length; eg++)
			{
				if(WaveMachine.GeneratorTypes != null && eg < WaveMachine.GeneratorTypes.Length)
				{
					
					tgentypes[eg] = WaveMachine.GeneratorTypes[eg]; 
				}
				else if(WaveMachine.GeneratorTypes == null)
				{
					WaveMachine.GeneratorTypes = new string[1];
					WaveMachine.Generators = new gurumod.Machines.Generator[1];
					WaveMachine.Generators[0] = new gurumod.Machines.Osc();
					WaveMachine.GeneratorTypes[0] = "gurumod.Machines.Osc";
				}
				
				if(Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg] != null)
				{
					Directory.CreateDirectory(TrackPath + "Samples/Generators/" + eg.ToString());
					Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg].Save(TrackPath, sampleid, eg);
					//Engine.TheTrack.Samples[sampleid].WaveMachine.GeneratorTypes[eg] = Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg].GetType().ToString();
					tgentypes[eg] = Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[eg].GetType().ToString();
				}
			}
			*/
			string filename = TrackPath + "Samples/Meta/" + sampleid.ToString() + ".xml";
			
			try
			{
				XmlSerializer s = new XmlSerializer(typeof(Sample));
				TextWriter w = new StreamWriter(Engine.PFP(filename));
				s.Serialize(w, this);
				w.Close();
			}
			catch(Exception ex)
			{
				Log.lWarning("There was an exception while saving the track's metadata.", "Sample", "SaveSample");
				Log.lWarning(ex.Message, "Sample", "SaveSample");
			}
			
			
			
			
		}
		
		public byte[] GTString()
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


			string tout = "";

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

			MemoryStream sampstr = new MemoryStream();
			StreamWriter sampwr = new StreamWriter(sampstr);
			sampwr.AutoFlush = true;

			string sampleid = this.ID.ToString("D5");
			string year = this.Year.ToString("D4");
			string brate = this.BitRate.ToString("D19");
			string channels = this.channels.ToString();
			string bitspersample = this.bits_per_sample.ToString("D10");
			string samplerate = this.sample_rate.ToString("D10");
			string wavegenerator = "0"; if(this.UseWaveGenerator) { wavegenerator = "1"; }
			string wavemachine = "0"; if(this.UseWaveMachine) { wavemachine = "1"; }
			string datalength = "0";
			if(this.SoundData != null) { datalength = this.SoundData.Length.ToString("D19"); }
			else { datalength = (0).ToString("D19"); }

			if(sampleid.Length > 5) { sampleid = sampleid.Substring(sampleid.Length - 5); }


			tout = sampleid + year + brate + channels + bitspersample + samplerate + wavegenerator + wavemachine + datalength;
			tout = tout + this.Name + "\0" + this.Artist + "\0" + this.Filename + "\0";
			//Console.WriteLine("Samp Header: {0}", tout);
			//	Sound data plugged in here

			string wavegenout = "";
			if(this.UseWaveGenerator) { wavegenout = WaveGenerator.GTString(); }

			byte[] wavemachout = new byte[0];
			if(this.UseWaveMachine) { wavemachout = WaveMachine.GTString(); }

			sampwr.Write(tout);
			sampwr.Flush();
			if(this.SoundData != null) { sampstr.Write(this.SoundData, 0, this.SoundData.Length); }
			if(this.UseWaveGenerator) { sampwr.Write(wavegenout); }
			if(this.UseWaveMachine) { sampstr.Write(wavemachout, 0, wavemachout.Length); }

			sampwr.Flush();
			sampstr.Flush();

			//Log.lInfo("Length of wavemachout: " + wavemachout.Length.ToString(), "Sample", "GTString");

			byte[] outtr = sampstr.ToArray();
			File.WriteAllBytes("/tmp/gt-sample-"+this.ID.ToString()+".sm", outtr);
			return outtr;

		}


	}
}

