// 
//  Mixer.cs
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
	//
	//	This is a basic, no frills mixer.  It is meant to be more of a template of how to create additional
	//	effect processors.
	//
	
	[XmlRoot("Mixer")]
	public class Mixer : Processor
	{
		[XmlElement("CombineMethod")] public int CombineMethod = gurumod.Machines.Mixer.CombineMethodAdd;
		
		[XmlIgnore()] public static int CombineMethodAdd = 0;
		[XmlIgnore()] public static int CombineMethodSubtract = 1;
		[XmlIgnore()] public static int CombineMethodMultiply = 2;
		[XmlIgnore()] public static int CombineMethodDivide = 3;
		//[XmlElement("Inputs")] public InputData[] Inputs;
		//[XmlElement("InputCount")] public int InputCount = 4;
		//	The inputs in this class:
		//		0 - Audio Input A
		//		1 - Audio Input B
		
		//[XmlIgnore()] public static int MaxInputs = 32;

	
		
		public Mixer ()
		{
			
		}

		public override string GTString ()
		{
			//	For each input of the processor:
				//		iiitttaaaa
				//			i: 3 digit source id
				//			t: 3 digit source type
				//			a: 4 digit amplitude (x.xx)
			string toret;

			toret = this.Inputs.Length.ToString("D3") + Processor.ProcTypeMixer.ToString("D3");


			for(int ein = 0; ein < this.Inputs.Length; ein++)
			{
				string sid = this.Inputs[ein].SourceID.ToString("D3");
				string stype = this.Inputs[ein].SourceType.ToString("D3");
				string amp = this.Inputs[ein].Amplitude.ToString("0.00");

				toret = toret + sid + stype + amp;
			}

			//	For mixers
				//		m
				//			m: 1 digit combine method

			toret = toret + this.CombineMethod.ToString();
			return toret;
		}
		
		public override void Initialize()
		{
			Console.WriteLine("Initializing Mixer Inputs");
			InputCount = 2;
			Inputs = new InputData[InputCount];
			for(int ein = 0; ein < InputCount; ein++)
			{
				Inputs[ein] = new InputData();
			}
			
			Inputs[0].SourceID = 0;
			Inputs[1].SourceID = 0;
			Inputs[0].SourceType = Machines.InputData.SourceTypeGenerator;
			Inputs[1].SourceType = Machines.InputData.SourceTypeGenerator;
		}
		
		public override short[] Process (Dictionary<string, short[]> Signals)
		{
			short[] SourceA = new short[1];//Signals[MxSettings.SourceAID];
			short[] SourceB = new short[1];//Signals[MxSettings.SourceBID];
			
			if(!Signals.ContainsKey(Inputs[0].InputKey()))
			{
				Console.WriteLine("Signals does not have key {0}. Count: {1}", Inputs[0].InputKey(), Signals.Count);
				
				return null;
			}
			if(!Signals.ContainsKey(Inputs[1].InputKey()))
			{
				Console.WriteLine("Signals does not have key {0}. Count: {1}", Inputs[1].InputKey(), Signals.Count);
				return null;
			}
			
			SourceA = Signals[Inputs[0].InputKey()];
			SourceB = Signals[Inputs[1].InputKey()];
			
			if(SourceA == null && SourceB == null) { Console.WriteLine("While processing the mixer, both sources are null."); return null; }
			
			if(SourceA == null) { return SourceB; }
			if(SourceB == null) { return SourceA; }

			double SourceAAmp = Inputs[0].Amplitude;
			double SourceBAmp = Inputs[1].Amplitude;
			
			short[] toret = new short[SourceA.Length];
			for(int esamp = 0; esamp < SourceA.Length; esamp++)
			{
				SourceA[esamp] = (short)(Math.Round(SourceA[esamp] * SourceAAmp));
				SourceB[esamp] = (short)(Math.Round(SourceB[esamp] * SourceBAmp));

				if(CombineMethod == CombineMethodAdd)
				{
					toret[esamp] = (short)((SourceA[esamp] + SourceB[esamp])/* * this.Amplitude*/);
				
				}
				else if(CombineMethod == CombineMethodSubtract)
				{
					toret[esamp] = (short)((SourceA[esamp] - SourceB[esamp])/* * this.Amplitude*/);
					
				}
				else if(CombineMethod == CombineMethodDivide)
				{
					short usea = SourceA[esamp];
					short useb = SourceB[esamp];
					if(useb == 0) { useb = 1; }
					toret[esamp] = (short)(usea / useb);
				}
				else if(CombineMethod == CombineMethodMultiply)
				{
					toret[esamp] = (short)(SourceA[esamp] * SourceB[esamp]);
				}
				
				
			}
			
			//if(MixMethod == CombineMethodAdd) { Console.WriteLine("MixAudio: Method was add."); }
			//if(MixMethod == CombineMethodSubtract) { Console.WriteLine("MixAudio: Method was subtract."); }
			
			return toret;
					                       
			//******************************************************************	                       
			//short[] MaxGateControl = new short[1];
			//short[] MinGateControl = new short[1];
			
			//Console.WriteLine("SourceTypeA {0} B {1}", MxSettings.SourceTypeA, MxSettings.SourceTypeB);
			/*if(MxSettings.SourceTypeA == MixerSettings.SourceTypeOscillator) { SourceA = Signals[MxSettings.SourceAID]; }
			else { Console.WriteLine("SourceA is a Mixer"); SourceA = MixAudio(Mixers[MxSettings.SourceAID]); }
			
			if(MxSettings.SourceTypeB == MixerSettings.SourceTypeOscillator) { SourceB = Signals[MxSettings.SourceBID]; }
			else { Console.WriteLine("SourceB is a Mixer"); SourceB = MixAudio(Mixers[MxSettings.SourceBID]); }
			
			if(MxSettings.MaxGateControlSource > -1) { MaxGateControl = Signals[MxSettings.MaxGateControlSource]; }
			if(MxSettings.MinGateControlSource > -1) { MinGateControl = Signals[MxSettings.MinGateControlSource]; }
			
			if(SourceA == null && SourceB == null)
			{
				Console.WriteLine("Attemping to MixAudio, but both sources are null!");
				return null;
			}
			
			if(SourceA == null) { Console.WriteLine("MixAudio: SourceA is null => returning SourceB"); return SourceB; }
			if(SourceB == null) { Console.WriteLine("MixAudio: SourceB is null => returning SourceA"); return SourceA; }
			
			//Console.WriteLine("Mixing 2 Sources of Audio");
			short[] toret = new short[SourceA.Length];
			for(int esamp = 0; esamp < SourceA.Length; esamp++)
			{
				if(CombineMethod == CombineMethodAdd)
				{
					toret[esamp] = (short)((SourceA[esamp] + SourceB[esamp])/* * this.Amplitude*///);
			/*		if(esamp > 0)
					{
						if(toret[esamp] < 20000 && toret[esamp] > -20000)
						{
							//toret[esamp] = 0;
							//toret[esamp] = (short)((toret[esamp - 1] * 4) + toret[esamp]);
						}
						//toret[esamp] = (short)((toret[esamp - 1] * 4) + toret[esamp]);
					}
				}
				else if(CombineMethod == CombineMethodSubtract)
				{
					toret[esamp] = (short)((SourceA[esamp] - SourceB[esamp])/* * this.Amplitude*///);
			/*		if(esamp > 0)
					{
						if(toret[esamp] < 20000 && toret[esamp] > -20000)
						{
							//toret[esamp] = 0;
							//toret[esamp] = (short)((toret[esamp - 1] * 4) + toret[esamp]);
						}
						//toret[esamp] = (short)((toret[esamp - 1] * 4) + toret[esamp]);
					}
				}
				else if(CombineMethod == CombineMethodDivide)
				{
					short usea = SourceA[esamp];
					short useb = SourceB[esamp];
					if(useb == 0) { useb = 1; }
					toret[esamp] = (short)(usea / useb);
				}
				else if(CombineMethod == CombineMethodMultiply)
				{
					toret[esamp] = (short)(SourceA[esamp] * SourceB[esamp]);
				}
				
				try
				{
					//	Gate functions
					double usemin = MxSettings.GateMin;
					double usemax = MxSettings.GateMax;
					
					if(MxSettings.MaxGateControlSource > -1)
					{
						usemax = (double)(Math.Abs((double)MaxGateControl[esamp]) / (double)short.MaxValue);
						//Console.WriteLine("Gate: Min {0} Max {1}", usemin, usemax);
					}
					if(MxSettings.MinGateControlSource > -1)
					{
						usemin = (double)(Math.Abs((double)MinGateControl[esamp]) / (double)short.MaxValue);
					}
					
					
					
					if(Math.Abs(toret[esamp]) < (short.MaxValue * usemin) || Math.Abs(toret[esamp]) > (short.MaxValue * usemax))
					{
						toret[esamp] = 0;
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine("Exception while applying gate.");
					Console.WriteLine(ex.Message);
				}
			}
			
			//if(MixMethod == CombineMethodAdd) { Console.WriteLine("MixAudio: Method was add."); }
			//if(MixMethod == CombineMethodSubtract) { Console.WriteLine("MixAudio: Method was subtract."); }
			*/
			//return new short[1];// toret;
		}
		
		
		public void SetInputSource(int inputid, int sourceid, int sourcetype)
		{
			if(inputid < 0 || inputid >= InputCount) { return; }
			if(Inputs == null) { Inputs = new InputData[InputCount]; }
			if(Inputs[inputid] == null) { Inputs[inputid] = new InputData(); }
			
			Inputs[inputid].SourceID = sourceid;
			Inputs[inputid].SourceType = sourcetype;
		}
	}
}

