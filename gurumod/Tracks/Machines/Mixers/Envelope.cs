//
//  Envelope.cs
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
	public class Envelope : Processor
	{
		// input[0]: audio in

		[XmlElement("Attack")] public double Attack = 0.1;
		[XmlElement("AttackAmp")] public double AttackAmp = 1;
		[XmlElement("Decay")] public double Decay = 0.2;
		[XmlElement("DecayAmp")] public double DecayAmp = 0.7;
		[XmlElement("Sustain")] public double Sustain = 10.0;
		[XmlElement("Release")] public double Release = 0.5;

		[XmlIgnore()] public double CurrentAmp = 0;
		[XmlIgnore()] public int CurrentStep = 0;
		[XmlIgnore()] public long TimeIntoStep = 0;
		
		public Envelope ()
		{
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
			

			
			return toret;
		}
	}
}