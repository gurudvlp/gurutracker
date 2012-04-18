// 
//  Processor.cs
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

namespace gurumod.Machines
{
	[XmlRoot("Processor")]
	public abstract class Processor
	{
		[XmlElement("Inputs")] public InputData[] Inputs;
		[XmlElement("InputCount")] public int InputCount = 4;
		//	The inputs in this class:
		//		0 - Audio Input A
		//		1 - Audio Input B
		//		2 - Gate Control Low
		//		3 - Gate Control High
		
		[XmlIgnore()] public static int MaxInputs = 32;
		
		
		public Processor ()
		{
		}
		
		public abstract short[] Process(Dictionary<string, short[]> Signals);
	}
}

