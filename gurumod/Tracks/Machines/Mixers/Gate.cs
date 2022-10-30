// 
//  Gate.cs
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
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;

namespace gurumod.Machines
{
	[Serializable()]
	public class Gate : Processor
	{
		// input[0]: audio in
		// input[1]: gate min
		// input[2]: gate max
		
		[XmlElement("MinGateManual")] [JsonInclude]
		public double MinGateManual = 0.0;

		[XmlElement("MaxGateManual")] [JsonInclude]
		public double MaxGateManual = 1.0;
		
		public Gate ()
		{
		}

		public Gate(SerializationInfo info, StreamingContext ctxt)
		{
			base.Construct(info, ctxt);
			MinGateManual = (double)info.GetValue("MinGateManual", typeof(double));
			MaxGateManual = (double)info.GetValue("MaxGateManual", typeof(double));
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext ctxt)
		{
			base.GetObjectData (info, ctxt);
			info.AddValue("MinGateManual", MinGateManual);
			info.AddValue("MaxGateManual", MaxGateManual);
		}
		
		public override void Initialize ()
		{
			Inputs = new InputData[3];
			for(int ein = 0; ein < Inputs.Length; ein++)
			{
				Inputs[ein] = new InputData();
				Inputs[ein].SourceID = 0;
				Inputs[ein].SourceType = Machines.InputData.SourceTypeGenerator;
			}
			base.InputCount = 3;
		}
		
		public override void InitInputs ()
		{
			Inputs = new InputData[3];
			for(int ein = 0; ein < Inputs.Length; ein++)
			{
				Inputs[ein] = new InputData();
				Inputs[ein].SourceID = 0;
				Inputs[ein].SourceType = Machines.InputData.SourceTypeGenerator;
			}
		}

		public override string GTString ()
		{
			//	For each input of the processor:
				//		iiitttaaaa
				//			i: 3 digit source id
				//			t: 3 digit source type
				//			a: 4 digit amplitude (x.xx)
			string toret;

			toret = this.Inputs.Length.ToString("D3") + Processor.ProcTypeGate.ToString("D3");


			for(int ein = 0; ein < this.Inputs.Length; ein++)
			{
				string sid = this.Inputs[ein].SourceID.ToString("D3");
				string stype = this.Inputs[ein].SourceType.ToString("D3");
				string amp = this.Inputs[ein].Amplitude.ToString("0.00");

				toret = toret + sid + stype + amp;
			}


			//	For gates
				//		llllllllllhhhhhhhhhh
				//			l: 10 digit MinGateManual
				//			h: 10 digit MaxGateManual

			string minman = this.MinGateManual.ToString("0.00000000");
			string maxman = this.MaxGateManual.ToString("0.00000000");

			toret = toret + minman + maxman;
			return toret;
		}
		
		public override short[] Process (Dictionary<string, short[]> Signals)
		{
			short[] toret = Signals[Inputs[0].InputKey()];
			if(toret == null) { return null; }
			
			short[] mingate = new short[1];
			if(Inputs[1].InputKey() != "-1") { mingate = Signals[Inputs[1].InputKey()]; }
			short[] maxgate = new short[1];
			if(Inputs[2].InputKey() != "-1") { maxgate = Signals[Inputs[2].InputKey()]; }
			
			
			
			//Console.WriteLine("Len of toret {0}", toret.Length);
			
			for(int efrm = 0; efrm < toret.Length; efrm++)
			{
				//Console.WriteLine("Inputs.Length {0}, efrm {1}", Inputs.Length, efrm);
				/*if((Inputs[0].SourceID == -1 && Math.Abs(toret[efrm]) > MinGateManual)
				|| (Inputs[0].SourceID > -1 && Math.Abs(mingate[efrm]) < toret[efrm]))
				*/
				if(Inputs[1].SourceID > -1)
				{
					//Console.WriteLine("SourceID > -1 (its {0})", Inputs[1].SourceID);
					double absv = 0;
					try
					{
						
						absv = Math.Abs(toret[efrm]);
					}
					catch(Exception ex)
					{
						Console.WriteLine("Exception during gate processing (input 1) {0}, {1}", efrm, toret[efrm]);
						Console.WriteLine(ex.Message);
					}
					//Console.WriteLine("Absolute value of frame: {0}, mingatelen {1}", absv, mingate.Length);
					if(absv < mingate[efrm]) { toret[efrm] = (short)0; }
				}
				else
				{
					try
					{
						double absv = 0;
						if(toret[efrm] != 0) { absv = Math.Abs(toret[efrm]); }
						if(absv < (this.MinGateManual * short.MaxValue)) { toret[efrm] = (short)0; }
					}
					catch(Exception ex)
					{
						Console.WriteLine("Exception in gate processing");
						Console.WriteLine(ex.Message);
					}
				}
				
				if(Inputs[2].SourceID > -1)
				{
					double absv = 0;
					
					try
					{
						Math.Abs(toret[efrm]);
					}
					catch(Exception ex)
					{
						Console.WriteLine("Exception during gate processing (input 2) {0}, {1}", efrm, toret[efrm]);
						Console.WriteLine(ex.Message);
					}
					if(absv > maxgate[efrm]) { toret[efrm] = (short)0; }
				}
				else
				{
					double absv = Math.Abs(toret[efrm]);
					if(absv > (this.MaxGateManual * short.MaxValue)) { toret[efrm] = (short)0; }
				}
				
				/*if(Inputs[2].SourceID > -1)
				{
					double absv = Math.Abs(toret[efrm]);
					if(absv > maxgate[efrm]) { toret[efrm] = (short)0; }
				}*/
				/*else
				{
					double absv = Math.Abs(toret[efrm]);
					if(absv > this.MaxGateManual) { toret[efrm] = (short)0;}
				}*/
				
				
				/*if((Inputs[0].SourceID == -1 && Math.Abs(toret[efrm]) < MaxGateManual)
				|| (Inputs[0].SourceID > -1 && Math.Abs(maxgate[efrm]) < toret[efrm]))
				{
					//	do nothing
				}
				else
				{
					toret[efrm] = (short)0;
				}*/
			}
			
			return toret;
		}
	}
}

