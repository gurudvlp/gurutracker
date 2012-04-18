// 
//  Generator.cs
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
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace gurumod.Machines
{
	[XmlRoot("SoundGenerator")]
	public abstract class Generator
	{
		[XmlElement("Enabled")] public bool Enabled = false;
		//[XmlElement("WaveType")] public int WaveType = Generator.TypeSine;
		[XmlElement("Frequency")] public double Frequency = 440;
		[XmlIgnore()] public int FramesIntoSample = 0;
		[XmlElement("Amplitude")] public double Amplitude = 0.75;
		[XmlElement("SampleRate")] public int SampleRate = 44100;
		[XmlElement("Format")] public ALFormat Format = ALFormat.Mono16;
		
		[XmlIgnore()] public int LastNote = 0;
		[XmlIgnore()] public int LastOctave = 5;
		public Generator ()
		{
		}
		
		public abstract short[] GetData(int frames, double frequency);
		public abstract short[] GetData(int frames, int note, int octave);
	}
}

