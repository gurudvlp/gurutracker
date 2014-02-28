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
using System.IO;

namespace gurumod.Machines
{
	[XmlRoot("Processor")]
	public abstract class Processor
	{
		[XmlElement("Inputs")] public InputData[] Inputs;
		[XmlElement("InputCount")] public int InputCount = 4;

		[XmlElement("ProcessorType")] public int ProcessorType = 0;
		//	The inputs in this class:
		//		0 - Audio Input A
		//		1 - Audio Input B
		//		2 - Gate Control Low
		//		3 - Gate Control High
		
		[XmlIgnore()] public static int MaxInputs = 32;
		[XmlIgnore()] public bool IsNoteNew = false;

		[XmlIgnore()] public static int ProcTypeMixer = 0;
		[XmlIgnore()] public static int ProcTypeGate = 1;
		[XmlIgnore()] public static int ProcTypeEnvelope = 2;
		[XmlIgnore()] public static int ProcTypeReverb = 3;
		
		public Processor ()
		{
		}
		
		public virtual void InitInputs()
		{
			Inputs = new InputData[InputCount];
			for(int ein = 0; ein < InputCount; ein++)
			{
				Inputs[ein] = new InputData();
			}
		}
		
		public abstract short[] Process(Dictionary<string, short[]> Signals);
		public abstract void Initialize();

		public abstract string GTString();

		public virtual void Save(string trackpath, int sampleid, int processorid)
		{
			Console.WriteLine("Saving Processor {0} on sample {1}", processorid, sampleid);
			
			try
			{
				string filename = trackpath + "Samples/Processors"; ///" + sampleid.ToString();
				if(!Directory.Exists(filename)) { Directory.CreateDirectory(filename); }
				filename = filename + "/" + sampleid.ToString();
				
				if(!Directory.Exists(filename)) { Directory.CreateDirectory(filename); }
				
				filename = filename + "/" + processorid.ToString() + ".xml";
				Type mytype = this.GetType();
				//XmlSerializer s = new XmlSerializer(typeof(Processor));
				XmlSerializer s = new XmlSerializer(mytype);
				TextWriter w = new StreamWriter(Engine.PFP(filename));
				s.Serialize(w, this);
				w.Close();
			}
			catch(Exception ex)
			{
				Console.WriteLine("There was an exception while saving the processor.");
				Console.WriteLine(ex.Message);
			}
		}
	}
}

