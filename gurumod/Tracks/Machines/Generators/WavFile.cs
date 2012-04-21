// 
//  WavFile.cs
//  
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
// 
//  Copyright (c) 2012 Brian Murphy
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

namespace gurumod.Machines
{
	public class WavFile : gurumod.Machines.Generator
	{
		[XmlElement("Filename")] public string Filename = "";
		[XmlElement("SampleRate")] public int SampleRate = 44100;
		[XmlElement("Channels")] public int Channels = 1;
		[XmlElement("BitRate")] public int BitRate = 0;
		
		[XmlIgnore()] public short[] AudioData;
		
		public WavFile ()
		{
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
				
				if(AudioData == null) { return null; }
			}
			
			short[] toret = new short[frames];
			
			int cframe = base.FramesIntoSample;
			for(int eafr = 0; eafr < frames; eafr += 2)
			{
				toret[eafr] = AudioData[cframe];
				toret[eafr + 1] = AudioData[cframe];
				cframe++;
				if(cframe >= AudioData.Length) { cframe = 0; }
			}
			/*for(int eafr = 0; eafr < frames; eafr++)
			{
				toret[eafr] = AudioData[cframe];
				cframe++;
				if(cframe >= AudioData.Length) { cframe = 0; }
			}*/
			
			base.FramesIntoSample = cframe;
			//Console.WriteLine("Oscillator.GetData:  Frequency (OG/Fixed): {0} {1}", Frequency, frequency);
			//double usefrequency = 0;
			
			/*if(WaveType == gurumod.Generator.TypeSine) { return SineWave(frames, frequency); }
			else if(WaveType == gurumod.Generator.TypeSquare) { return SquareWave(frames, frequency); }
			else if(WaveType == gurumod.Generator.TypeTriangle) { return TriangleWave(frames, frequency); }
			else if(WaveType == gurumod.Generator.TypeSawtooth) { return SawtoothWave(frames, frequency); }
			else
			{
				Console.WriteLine("Wave type {0} not recognized.", WaveType);
				return null;
			}*/
			return toret;
		}
		
		public override short[] GetData(int frames, int note, int octave)
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
		
		public void LoadFile()
		{
			string trackpath = Engine.TheTrack.MyPath;
			if(File.Exists(Engine.PFP(trackpath + "Samples/Audio/" + Filename)))
			{
				FileStream wavst = File.OpenRead(Engine.PFP(trackpath + "Samples/Audio/" + Filename));
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
						short ad = (short)0;
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
							
							if(tt == null) { keepon = false; }
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
				/*for(int efrm = 0; efrm < AudioData.Length; efrm++)
				{
					AudioData[efrm] = (short)reader.ReadInt16();
				}*/

                //return reader.ReadBytes((int)reader.BaseStream.Length);
			}
		}
	}
}

