// 
//  PatternElement.cs
//  
//  Author:
//       guru <${AuthorEmail}>
// 
//  Copyright (c) 2012 guru
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

namespace gurumod
{
	[XmlRoot("PatternElement")]
	public class PatternElement
	{
		//	Each element in a pattern.  This controls volume, what sample, whether to play or stop, etc.
		[XmlElement("Octave")] public int Octave = 5;
		[XmlElement("Note")] public int Note = -1;
		[XmlElement("Volume")] public int Volume = -1;
		[XmlElement("Special")] public int SpecialControl = -1;
		[XmlElement("SampleID")] public int SampleID = -1;
		
		public PatternElement ()
		{
		}
		
		[XmlIgnore()]
		public string NoteString
		{
			get
			{
				if(Note == 0)
				{
					return "C-" + Octave.ToString();
				}
				else if(Note == 1) { return "D-" + Octave.ToString(); }
				else if(Note == 2) { return "E-" + Octave.ToString(); }
				else if(Note == 3) { return "F-" + Octave.ToString(); }
				else if(Note == 4) { return "G-" + Octave.ToString(); }
				else if(Note == 5) { return "A-" + Octave.ToString(); }
				else if(Note == 6) { return "B-" + Octave.ToString(); }
				else if(Note == -1) { return "---"; }
				else if(Note == -2) { return "==="; }
				else
				{
					return "?-?";
				}
			}
		}
		
		[XmlIgnore()]
		public string VolumeString
		{
			get
			{
				if(Volume < 0) { return "---"; }
				else 
				{
					if(Volume < 10) { return "v0" + Volume.ToString(); }
					return "v" + Volume.ToString();
				}
			}
		}
		
		[XmlIgnore()]
		public string SampleIDString
		{
			get
			{
				if(SampleID < 0) { return "--"; }
				else 
				{
					if(SampleID < 10) { return "0" + SampleID.ToString(); }
					return SampleID.ToString();
				}
			}
		}
		
		
		public float GetSpeed()
		{
			float freq = (1f / 7f) * Note;
			if(this.Octave == 4) { return 0.5f + (freq * 0.5f); }
			if(this.Octave == 5) { return 1.0f + freq; }
			if(this.Octave == 6) { return 2.0f + (freq * 2.0f); }
			
			return 1.0f;
		}
	}
}

