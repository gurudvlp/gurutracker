// 
//  WavFile.cs
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
using System.IO;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;

using gurumod.Logging;

namespace gurumod.Machines
{
	[Serializable()]
	public class WavFile : gurumod.Machines.Generator
	{
		[XmlElement("Filename")] [JsonInclude]
		public string Filename = "";

		[XmlElement("Channels")] [JsonInclude]
		public int Channels = 1;

		[XmlElement("BitRate")] [JsonInclude]
		public int BitRate = 0;
		
		[XmlIgnore()] [JsonIgnore]
		public short[] AudioData;
		
		public WavFile ()
		{
			base.GeneratorType = gurumod.Machines.Generator.GeneratorTypeWavePlayer;
		}

		public WavFile(SerializationInfo info, StreamingContext ctxt)
		{
			base.Construct(info, ctxt);
			Filename = (string)info.GetValue("Filename", typeof(string));
			Channels = (int)info.GetValue("Channels", typeof(int));
			BitRate = (int)info.GetValue("BitRate", typeof(int));
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext ctxt)
		{
			base.GetObjectData (info, ctxt);
			info.AddValue("Filename", Filename);
			info.AddValue("Channels", Channels);
			info.AddValue("BitRate", BitRate);
		}
	

		public override byte[] GTString()
		{
			//	For generator type 1 (wave player)
			//		cssssssbbbbbbbbbblllllllllllllllllllf<chr 0>
			//			c: 1 digit channel count
			//			s: 6 digit sample rate
			//			b: 10 digit bit rate
			//			l: 19 digit length of audio data
			//			f: filename
			//		list of 2-byte shorts of audio data

			MemoryStream ms = new MemoryStream();
			StreamWriter wr = new StreamWriter(ms);

			ms.Write( base.GTString (), 0, base.GTString().Length);
			string toret = "";

			string channels = this.Channels.ToString();
			string samplerate = this.SampleRate.ToString("D6");
			string bitrate = this.BitRate.ToString("D10");
			string audiolen = this.AudioData.Length.ToString("D19");

			toret = toret + channels + samplerate + bitrate + audiolen;
			wr.Write(toret);

			if(this.AudioData == null || this.AudioData.Length == 0) { return ms.ToArray(); }


			byte[] audiobytes = new byte[this.AudioData.Length * 2];

			for(int eaud = 0; eaud < this.AudioData.Length; eaud++)
			{
				byte b1 = new byte();
				byte b2 = new byte();

				this.FromShort(this.AudioData[eaud], out b1, out b2);
				audiobytes[eaud * 2] = b1;
				audiobytes[(eaud * 2) + 1] = b2;
			}

			ms.Write(audiobytes, 0, audiobytes.Length);

			return ms.ToArray();
		}

		public short ToShort(short byte1, short byte2)
		{
		    return (short)((byte2 << 8) + byte1);
		}

		public void FromShort(short number, out byte byte1, out byte byte2)
		{
		    byte2 = (byte)(number >> 8);
		    byte1 = (byte)(number & 255);
		}
		
		//
		//	When getting data (ie GetData), the number
		//	of frames requested is based off of a 44100
		//	sample rate...  or i would like it to be :/
		//
		public override short[] GetData(int frames, double frequency)
		{
			if(Enabled == false) { return null; }
			if(AudioData == null) 
			{
				this.LoadFile();
				
				if(AudioData == null)
				{
					Console.WriteLine("Attempted to load AudioData from WavFile, but it's still null.");
					return null; }
			}
			
			short[] toret = new short[frames];
			
			int cframe = base.FramesIntoSample;
			try
			{
				for(int eafr = 0; eafr < frames; eafr += 2)
				{
					if(cframe >= AudioData.Length) { cframe = 0; }
					try { toret[eafr] = AudioData[cframe]; }
					catch(Exception blah)
					{
						Log.lWarning("Exception copying audio data.", "WavFile", "GetData");
						Log.lWarning(blah.Message);
						/*Console.WriteLine("Exception in WavFile.GetData -- copying audiodata");
						Console.WriteLine("eafr: {0}", eafr);
						Console.WriteLine("cframe: {0}", cframe);
						Console.WriteLine("AD Len: {0}", AudioData.Length);
						Console.WriteLine("tr Len: {0}", toret.Length);*/
					}

					try { if(toret.Length > eafr + 1) { toret[eafr + 1] = AudioData[cframe]; } }
					catch(Exception blah)
					{
						Log.lWarning("Exception copying audio data (2)", "WavFile", "GetData");
						Log.lError(blah.Message, "WavFile", "GetData");
						/*Console.WriteLine("Exception in WavFile.GetData -- copying audiodata (2)");
						Console.WriteLine("eafr: {0}", eafr);
						Console.WriteLine("cframe: {0}", cframe);
						Console.WriteLine("AD Len: {0}", AudioData.Length);
						Console.WriteLine("tr Len: {0}", toret.Length);
						*/
					}
					cframe++;

				}
			}
			catch(Exception ex)
			{
				Log.lWarning("Exception in GetData", "WavFile", "GetData");
				Log.lError(ex.Message, "WavFile", "GetData");
				/*Console.WriteLine("Exception in WavFile.GetData.");
				Console.WriteLine(ex.Message);
				Console.WriteLine("--------------------------------------------");
				Console.WriteLine("cframe: {0}", cframe);
				Console.WriteLine("frames: {0}", frames);
				Console.WriteLine("AudioDataLen: {0}", AudioData.Length);*/
			}

			
			base.FramesIntoSample = cframe;
			return toret;
		}
		
		public override short[] GetData(int frames, int note, int octave)
		{
			//if(note > -1 && octave > -1) { Console.WriteLine("Machine: GetData note {0} octave {1}", note, octave); }
			if(!Enabled) { return null; }
			double freq = this.Frequency;

			double nf = Engine.Configuration.NoteFreq[octave][note];
			double nmul = this.Frequency / Engine.Configuration.NoteFreq[4][9];

			return GetData(frames, nf * nmul);
		}

		public override Dictionary<string, string> Details()
		{
			Dictionary<string, string> toret = new Dictionary<string, string>();
			toret.Add("filename", this.Filename);
			toret.Add("samplerate", this.SampleRate.ToString());
			toret.Add("bitrate", this.BitRate.ToString());
			toret.Add("channels", this.Channels.ToString());

			return toret;
		}
		
		public void LoadFile()
		{
			string trackpath = Engine.TheTrack.MyPath;
			if(File.Exists(Engine.PFP("/usr/share/gurutracker/samples/" + Filename)))
			{
				FileStream wavst = File.OpenRead(Engine.PFP("/usr/share/gurutracker/samples/" + Filename));
				BinaryReader reader = new BinaryReader(wavst);
				
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
				
				this.SampleRate = sample_rate;
				this.Channels = num_channels;
				this.BitRate = bits_per_sample;
				
                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported. 2 " + data_signature);

                int data_chunk_size = reader.ReadInt32();
				
				if(num_channels == 1 && bits_per_sample == 8) { base.Format = ALFormat.Mono8; }
				else if(num_channels == 1 && bits_per_sample == 16) { base.Format = ALFormat.Mono16; }
				else if(num_channels == 2 && bits_per_sample == 8) { base.Format = ALFormat.Stereo8; }
				else if(num_channels == 2 && bits_per_sample == 16) { base.Format = ALFormat.Stereo16; }
				else { throw new NotSupportedException("Wave file not support because of channels/bit rate"); }
				
                base.SampleRate = sample_rate;
                
				AudioData = new short[reader.BaseStream.Length];
				
				bool keepon = true;
				int pos = 0;
				
				try
				{
					while(keepon)
					{
						short tt = (short)0;
						byte[] tin = new byte[2];
						
						for(int echan = 0; echan < num_channels; echan++)
						{
							if(base.Format == ALFormat.Mono16) 
							{
								//tt = (short)reader.Read();
								//ad = (short)reader.Read();
								tin = reader.ReadBytes(2);
								
								short smm = BitConverter.ToInt16(tin, 0);
								if(smm != 0)
									smm = Convert.ToInt16((~smm | 1));
								
								tt = smm;
							}
							else
							{
								tt = (short)reader.Read(); 
								
							}
							
							//if(tt == null) { keepon = false; }
							if(tt == -1) { keepon = false; }
							else
							{
								AudioData[pos] = (short)tt;
							}
							pos++;
						}
				}
				}
				catch(Exception ex)
				{
					Console.WriteLine("Exception while loading wave file.");
					Console.WriteLine(ex.Message);
				}

			}
		}
	}
}

