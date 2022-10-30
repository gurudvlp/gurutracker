// 
//  PatternChannel.cs
//  
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
// 
//  Copyright (c) 2012-2022 guru
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
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace gurumod
{
	[XmlRoot("PatternChannel")] 
	[Serializable()]
	public class PatternChannel : ISerializable
	{
		//	This class is a list of elements for one channel in a pattern.  It sequentially lists the things to
		//	do for this channel.
		
		[XmlElement("Elements")] [JsonInclude]
		public PatternElement[] Elements;

		[XmlIgnore()] [JsonIgnore] 
		private int _ElementCount = 0;

		[XmlIgnore()] [JsonInclude]
		public int CurrentSample  = -1;

		[XmlIgnore()] [JsonIgnore]
		private int Source = -1;

		[XmlIgnore()] [JsonIgnore]
		private int SourceState = -1;

		[XmlIgnore()] [JsonInclude]
		public float Volume = 1.0f;

		[XmlElement("ChannelID")] [JsonInclude]
		public int ChannelID = 0;

		[XmlIgnore()] public Queue<int> MachineSoundBuffers = new Queue<int>();
		
		public PatternChannel (int elementcnt)
		{
			_ElementCount = elementcnt;
			Elements = new PatternElement[elementcnt];
			for(int eel = 0; eel < elementcnt; eel++)
			{
				Elements[eel] = new PatternElement();
			}
		}
		
		public PatternChannel()
		{
			
		}

		public PatternChannel(SerializationInfo info, StreamingContext ctxt)
		{
			ChannelID = (int)info.GetValue("ChannelID", typeof(int));
			Elements = (PatternElement[])info.GetValue("Elements", typeof(PatternElement[]));
			_ElementCount = Elements.Length;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("ChannelID", ChannelID);
			info.AddValue("Elements", Elements);
		}
		
		[XmlElement("ElementCount")]
		public int ElementCount
		{
			get
			{
				return _ElementCount;
			}
			set
			{
				PatternElement[] tmpels = new PatternElement[value];
				
				for(int enel = 0; enel < value; enel++)
				{
					if(enel < _ElementCount)
					{
						tmpels[enel] = Elements[enel];
					}
					else
					{
						tmpels[enel] = new PatternElement();
					}
				}
				Elements = tmpels;
				
				_ElementCount = value;
			}
		}
		
		public void PlayElement(int elid)
		{
			//	Process and play an element from this channel.
			if(elid >= ElementCount || elid < 0) { return; }
			
			if(CurrentSample > -1 && Engine.TheTrack.Samples[CurrentSample].UseWaveMachine)
			{
				if(PlayElementFromMachine(elid)) { return; }
			}
			
			
			if(Elements[elid] == null) { return; }
			
			if(Elements[elid].Volume > -1) 
			{
				Volume = ((float)Elements[elid].Volume) / 100;
				if(Engine.TheTrack.ChannelMuted[this.ChannelID]) { Volume = 0.0f; }
				if(Source > -1)
				{
					AL.Source(Source, ALSourcef.Gain, Volume);
					
				}
			}
			
			if(Engine.TheTrack.ChannelMuted[this.ChannelID])
			{
				//Console.WriteLine ("Channel {0} is muted", this.ChannelID);
				Volume = 0.0f;
			}
			
			
			
			if(Elements[elid].Note > -1 && (Elements[elid].SampleID > -1 || CurrentSample > -1))
			{
				//	This element has a note and sample associated with it, so let's play that shiiiiot
				int usesample = 0;
				if(CurrentSample > -1) { usesample = CurrentSample; }
				if(Elements[elid].SampleID > -1) { usesample = Elements[elid].SampleID; CurrentSample = usesample; }
				if(Source > -1 && !Engine.TheTrack.Samples[usesample].UseWaveMachine)
				{
					AL.GetSource(Source, ALGetSourcei.SourceState, out SourceState);
					if((ALSourceState)SourceState == ALSourceState.Playing) { AL.SourceStop(Source); }
					
					AL.DeleteSource(Source);
				}
				
				if(Elements.Length > elid)
				{
					try
					{
						if(Engine.TheTrack.Samples[usesample].UseWaveMachine)
						{
							PlayElementFromMachine(elid);
							//AL.SourcePlay(Source);
						}
						else
						{
							Source = AL.GenSource();
							AL.Source(Source, ALSourcei.Buffer, Engine.TheTrack.Samples[usesample].buffer);
							AL.Source(Source, ALSourcef.Pitch, Elements[elid].GetSpeed());
							AL.Source(Source, ALSourcef.Gain, Volume);
							AL.SourcePlay(Source);
						}
					}
					catch(Exception ex)
					{
						Console.WriteLine("Exception in generating a sound to play and crap");
						Console.WriteLine(ex.Message);
					}
				}
			}
			else if(Elements[elid].Note == -2)
			{
				//	Note cut
				int usesample = 0;
				if(CurrentSample > -1) { usesample = CurrentSample; }
				if(Elements[elid].SampleID > -1) { usesample = Elements[elid].SampleID; CurrentSample = usesample; }
				if(Engine.TheTrack.Samples[usesample].UseWaveMachine)
				{
					/*AL.GetSource(Source, ALGetSourcei.SourceState, out SourceState);
					if((ALSourceState)SourceState == ALSourceState.Playing) { AL.SourceStop(Source); }
					
					AL.DeleteSource(Source);*/
					PlayElementFromMachine(elid);
				}
				if(Source > -1)
				{
					AL.GetSource(Source, ALGetSourcei.SourceState, out SourceState);
					if((ALSourceState)SourceState == ALSourceState.Playing) { AL.SourceStop(Source); }
					AL.DeleteSource(Source);
				}
			}
			
			/*int smpid = Elements[elid].SampleID;
			
			if(smpid < 0) { return; }
			if(Engine.TheTrack.Samples[smpid] == null) { return; }
			Engine.TheTrack.Samples[smpid].Play(Elements[elid].GetSpeed());
			*/
			//Console.WriteLine("Done playing pattern element");
		}
		
		public bool PlayElementFromMachine(int elementid)
		{
			//Console.WriteLine("Using WaveMachine");
			int note = -1;
			int octave = 5;
			
			
			
			if(Elements[elementid] != null)
			{
				if(Elements[elementid].Volume > -1) { this.Volume = Elements[elementid].Volume; }
				//Engine.TheTrack.Samples[CurrentSample].WaveMachine.Amplitude = this.Volume;
				if(Elements[elementid].SampleID > -1 && Elements[elementid].SampleID != CurrentSample && CurrentSample > -1)
				{
					return false;
				}
				
				note = Elements[elementid].Note;
				octave = Elements[elementid].Octave;
				
				if(Elements[elementid].SampleID > 0) { CurrentSample = Elements[elementid].SampleID; }
				//forcenewbuffer = true;
				
				if(note > -1) 
				{
					Engine.TheTrack.Samples[CurrentSample].WaveMachine.LastNote = note;
					Engine.TheTrack.Samples[CurrentSample].WaveMachine.LastOctave = octave;
				}
				//if(octave > -1) { Engine.TheTrack.Samples[Elements[elementid].SampleID].WaveMachine.LastOctave = octave; }
				
				//Console.WriteLine("PlayElementFromMachine: ElID {0}", elementid);
				//Console.WriteLine("PlayElementFromMachine: SampleID {0}", Elements[elementid].SampleID);
				
				if(note == -2)
				{
					Console.WriteLine("PlayMachineElement: Notecut triggered.");
					
					if(CurrentSample > -1 && Engine.TheTrack.Samples != null && Engine.TheTrack.Samples[CurrentSample] != null)
					{
						Engine.TheTrack.Samples[CurrentSample].WaveMachine.Running = false;
					}
				}
				else { Engine.TheTrack.Samples[CurrentSample].WaveMachine.Running = true; }
			}
			
			
			
			return true;
		}
		
		public void ProcessMachine()
		{
			//	If this channel currently has a running sound machine, then we
			//	need to process that mof to make sure that the output is continuous!
			if(AL.IsSource(Source)) { AL.Source(Source, ALSourcef.Gain, this.Volume); }
			
			if(this.CurrentSample < 0) { return; }
			
			if(Engine.TheTrack.Samples[CurrentSample] == null) { return; }
			if(!Engine.TheTrack.Samples[CurrentSample].UseWaveMachine) { return; }
			if(Engine.TheTrack.Samples[CurrentSample].WaveMachine == null) { return; }
			
			if(!Engine.TheTrack.Samples[CurrentSample].WaveMachine.Running)
			{
				//	If the machine isn't running, we need to clear all of the buffered data
				//Console.WriteLine("SoundMachine Not running, clearing sound buffers");
				AL.SourceStop(Source);
				if(MachineSoundBuffers.Count > 0)
				{
					AL.SourceUnqueueBuffers(Source, MachineSoundBuffers.Count);
					
					for(int emsb = 0; emsb < MachineSoundBuffers.Count; emsb++)
					{
						int dd = MachineSoundBuffers.Dequeue();
						AL.DeleteBuffer(dd);
					}
					MachineSoundBuffers.Clear();
					
				}
				
				return;
			}
			
			int note = Engine.TheTrack.Samples[CurrentSample].WaveMachine.LastNote;
			int octave = Engine.TheTrack.Samples[CurrentSample].WaveMachine.LastOctave;
			
			if(!AL.IsSource(Source)) { Source = AL.GenSource(); }
			AL.Source(Source, ALSourcef.Gain, this.Volume);
			
			int buffer = AL.GenBuffer();
			int buffersprocessed = 0;
			//bool forcenewbuffer = false;
		
			AL.GetSource(Source, ALGetSourcei.SourceState, out SourceState);
			AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out buffersprocessed);
		
			
			bool outputobtained = false;
			//Console.WriteLine("Buffers processed {0}  MSB: {1}", buffersprocessed, MachineSoundBuffers.Count);
			
			if(buffersprocessed > 0 || MachineSoundBuffers.Count < 2)
			{
				/*for(int ebuf = 0; ebuf < buffersprocessed; ebuf++)
				{
					if(MachineSoundBuffers.Count > 0)
					{
						int msbid = MachineSoundBuffers.Dequeue(); 
						AL.SourceUnqueueBuffer(Source);
						AL.DeleteBuffer(msbid);
					}
				}*/
				
				var segment = Engine.TheTrack.Samples[CurrentSample].WaveMachine.GetSignal(null, note, octave);
				
				if(segment == null) { outputobtained = false; }
				else
				{
					outputobtained = true;
					int samplerate = Engine.TheTrack.Samples[CurrentSample].WaveMachine.SampleRate;
					
					//	This was the original method for how to add sample
					//	data to OpenAL.  This does not appear to work with
					//	modern C# and will need to be revisited!!
					//
					System.Runtime.InteropServices.GCHandle pinnedArray = 
						System.Runtime.InteropServices.GCHandle.Alloc(
							segment, 
							System.Runtime.InteropServices.GCHandleType.Pinned
						);

					IntPtr pointer = pinnedArray.AddrOfPinnedObject();

					AL.BufferData(
						buffer, 
						Engine.TheTrack.Samples[CurrentSample].WaveMachine.Format, 
						pointer, 
						segment.Length * 2, 
						samplerate
					);
					
					
					AL.SourceQueueBuffer(Source, buffer);
					MachineSoundBuffers.Enqueue(buffer);

					pinnedArray.Free();
				}
				
				for(int ebuf = 0; ebuf < buffersprocessed; ebuf++)
				{
					if(MachineSoundBuffers.Count > 1)
					{
						int msbid = MachineSoundBuffers.Dequeue(); 
						AL.SourceUnqueueBuffer(Source);
						AL.DeleteBuffer(msbid);
					}
				}
			}
			
			if(outputobtained)
			{
				AL.Source(Source, ALSourcef.Gain, Volume);
				//Console.WriteLine("Enqueued source buffer");
				//AL.Source(Source, ALSourcei.Buffer, buffer);
				if((ALSourceState)SourceState != ALSourceState.Playing)
				{
					AL.SourcePlay(Source);
				}
			}
		}
	}
}

