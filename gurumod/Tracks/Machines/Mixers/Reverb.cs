//
//  Reverb.cs
//
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
//
//  Copyright (c) 2013 Brian Murphy
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
	public class Reverb : Processor
	{
		// input[0]: audio in
		// input[1]: gate min
		// input[2]: gate max
		
		//[XmlElement("MinGateSource")] public int MinGateSource = -1;
		//[XmlElement("MaxGateSource")] public int MaxGateSource = -1;
		//[XmlElement("MinGateSourceType")] public int MinGateSourceType = -1;
		//[XmlElement("MaxGateSourceType")] public int MaxGateSourceType = -1;
		[XmlElement("Delay")] public double Delay = 10000;
		[XmlElement("Decay")] public double Decay = 0.3;
		[XmlIgnore()] public Queue<short[]> VerbBuffer;
		[XmlIgnore()] public long FramesElapsed = 0;
		[XmlIgnore()] public short[] LeftOver;

		
		public Reverb ()
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

			toret = this.Inputs.Length.ToString("D3") + Processor.ProcTypeReverb.ToString("D3");


			for(int ein = 0; ein < this.Inputs.Length; ein++)
			{
				string sid = this.Inputs[ein].SourceID.ToString("D3");
				string stype = this.Inputs[ein].SourceType.ToString("D3");
				string amp = this.Inputs[ein].Amplitude.ToString("0.00");

				toret = toret + sid + stype + amp;
			}

			//	For reverb
				//		wwwwwwwwwwdddddddddd
				//			w: 10 digit delay
				//			d: 10 digit decay

			string delay = this.Delay.ToString("0000000.00");
			string decay = this.Decay.ToString("0000000.00");

			toret = toret + delay + decay;

			return toret;
		}
		
		public override void Initialize ()
		{
			Inputs = new InputData[1];
			for(int ein = 0; ein < Inputs.Length; ein++)
			{
				Inputs[ein] = new InputData();
				Inputs[ein].SourceID = 0;
				Inputs[ein].SourceType = Machines.InputData.SourceTypeGenerator;
			}
			base.InputCount = 1;

			VerbBuffer = new Queue<short[]>();
		}
		
		public override void InitInputs ()
		{
			Inputs = new InputData[1];
			for(int ein = 0; ein < Inputs.Length; ein++)
			{
				Inputs[ein] = new InputData();
				Inputs[ein].SourceID = 0;
				Inputs[ein].SourceType = Machines.InputData.SourceTypeGenerator;
			}
		}
		
		public override short[] Process (Dictionary<string, short[]> Signals)
		{
			short[] toret = Signals[Inputs[0].InputKey()];
			if(toret == null) { return null; }
			short[] realout = new short[toret.Length];

			if(VerbBuffer == null) { VerbBuffer = new Queue<short[]>(); }
			VerbBuffer.Enqueue(toret);



			if(FramesElapsed < this.Delay)
			{
				FramesElapsed = FramesElapsed + toret.Length;

			}
			else
			{
				short[] frombuf = VerbBuffer.Dequeue();
				long smllr = frombuf.Length;
				if(toret.Length < smllr) { smllr = toret.Length; }

				for(long efr = 0; efr < smllr; efr++)
				{
					toret[efr] = (short)(toret[efr] + (short)Math.Floor(frombuf[efr] * this.Decay));
				}

				/*for(int esm = 0; esm < toret.Length - 400; esm++)
				{
					toret[esm + 400] = (short)(toret[esm + 400] + (short)Math.Floor(toret[esm] * this.Decay));
				}*/

				/*for(int esm = 0; esm < toret.Length; esm = esm + 50)
				{
					short tval = toret[esm + 25];
					for(int chng = esm + 25; chng < esm + 50; chng++)
					{
						toret[chng] = tval;
					}
				}*/
			}
			

			
			return toret;
		}
	}
}

