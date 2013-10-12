// 
//  InputData.cs
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

namespace gurumod.Machines
{
	
	public class InputData
	{
		[XmlElement("SourceID")] public int SourceID = -1;
		[XmlElement("SourceType")] public int SourceType = MixerSettings.SourceTypeOscillator;
		[XmlElement("Amplitude")] public double Amplitude = 1.0;
		
		[XmlIgnore()] public static int SourceTypeGenerator = 0;
		[XmlIgnore()] public static int SourceTypeProcessor = 1;
		
		public InputData ()
		{
		}
		
		public string InputKey()
		{
			if(SourceType == gurumod.Machines.InputData.SourceTypeGenerator)
			{
				return SourceID.ToString();
			}
			
			if(SourceType == Machines.InputData.SourceTypeProcessor)
			{
				return "proc" + SourceID.ToString();
			}
			
			return SourceID.ToString();
		}
	}
}

