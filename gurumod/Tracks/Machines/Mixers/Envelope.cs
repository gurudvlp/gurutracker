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

		[XmlElement("Attack")] public double Attack = 100;
		[XmlElement("AttackAmp")] public double AttackAmp = 1;
		[XmlElement("Decay")] public double Decay = 200;
		[XmlElement("DecayAmp")] public double DecayAmp = 0.7;
		[XmlElement("Sustain")] public double Sustain = 500.0;
		[XmlElement("Release")] public double Release = 50;

		[XmlIgnore()] public double CurrentAmp = 0;
		[XmlIgnore()] public int CurrentStep = 0;
		[XmlIgnore()] public double TimeIntoStep = 0;
		[XmlIgnore()] public int FramesIntoStep = 0;
		
		public Envelope ()
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

			toret = this.Inputs.Length.ToString("D3") + Processor.ProcTypeEnvelope.ToString("D3");


			for(int ein = 0; ein < this.Inputs.Length; ein++)
			{
				string sid = this.Inputs[ein].SourceID.ToString("D3");
				string stype = this.Inputs[ein].SourceType.ToString("D3");
				string amp = this.Inputs[ein].Amplitude.ToString("0.00");

				toret = toret + sid + stype + amp;
			}

			//	For envelopes:
				//		aaaaaaaaaabbbbddddddddddeeeessssssssssrrrrrrrrrr
				//			a: 10 digit attack
				//			b: 4 digit attack amplitude (x.xx)
				//			d: 10 digit decay
				//			e: 4 digit decay amplitude (x.xx)
				//			s: 10 digit sustain
				//			r: 10 digit release

			string attack = this.Attack.ToString("0000000.00");
			string attamp = this.AttackAmp.ToString("0.00");
			string decay = this.Decay.ToString("0000000.00");
			string decamp = this.DecayAmp.ToString("0.00");
			string sustain = this.Sustain.ToString("0000000.00");
			string release = this.Release.ToString("0000000.00");

			toret = toret + attack + attamp + decay + decamp + sustain + release;

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
			
			if(this.IsNoteNew)
			{
				this.CurrentStep = 1;
				this.TimeIntoStep = 0;
				this.CurrentAmp = 0;
			}

			if(this.CurrentStep == 0) { return null; }

			for(int es = 0; es < toret.Length; es++)
			{
				if(this.CurrentStep == 0)
				{
					toret[es] = 0;
				}

				if(this.CurrentStep == 1)
				{
					//	Attack..  build up to max amplitude
					int fts = FramesPerTime(Attack);
					if(FramesIntoStep > fts)
					{
						FramesIntoStep = 0;
						this.CurrentStep++;
					}
					else
					{
						double prog = FramesIntoStep / fts;
						prog = prog * AttackAmp;
						toret[es] = (short)(toret[es] * prog);
					}
				}

				if(this.CurrentStep == 2)
				{
					//	Decay.. build back to plateau
					int fts = FramesPerTime(Decay);
					if(FramesIntoStep > fts)
					{
						FramesIntoStep = 0;
						this.CurrentStep++;
					}
					else
					{
						double prog = FramesIntoStep / fts;
						prog = Math.Abs(AttackAmp - DecayAmp) * prog;
						if(AttackAmp < DecayAmp) { prog = prog + AttackAmp; }
						else { prog = prog + DecayAmp; }
						toret[es] = (short)(toret[es] * prog);
					}
				}

				if(this.CurrentStep == 3)
				{
					//	Sustain..  keep the same volume until release
					int fts = FramesPerTime(Sustain);
					if(FramesIntoStep > fts)
					{
						FramesIntoStep = 0;
						this.CurrentStep++;
					}
					else
					{
						toret[es] = (short)(toret[es] * DecayAmp);
					}
				}

				if(this.CurrentStep == 4)
				{
					//	Release..  go back to silence
					int fts = FramesPerTime(Release);
					if(FramesIntoStep > fts)
					{
						this.CurrentStep = 0;
						FramesIntoStep = 0;
					}
					else
					{
						double prog = FramesIntoStep / fts;
						toret[es] = (short)(toret[es] * (DecayAmp - (prog * DecayAmp)));
					}
				}

				FramesIntoStep++;
			}
			
			return toret;
		}

		public int FramesPerTime(double millitime)
		{
			// 44100:1000 = x:millitime
			double xx = (44100 * millitime) / 1000;
			return (int)Math.Floor(xx);
		}
	}
}