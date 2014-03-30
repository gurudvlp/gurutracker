//
//  TweakReverb.cs
//
//  Author:
//       guru <${AuthorEmail}>
//
//  Copyright (c) 2013 guru
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
using System.Collections;
using System.Collections.Generic;


namespace gurumod.WebPages.Actions
{
	
	public class TweakReverb : WebPage
	{

		public TweakReverb()
		{

		}

		public override bool Run ()
		{
			int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
			int processorid = Int32.Parse(base.PostVars["PROCESSORID"]);
			string inputa = base.PostVars["INPUT0"];
			int inputaid = 0;
			int inputatype = 0;
			double delay = double.Parse(base.PostVars["DELAY"]);
			double decay = double.Parse(base.PostVars["DECAY"]);

			if(inputa.IndexOf("gen") == 0)
			{
				inputaid = Int32.Parse(inputa.Replace("gen", ""));
				inputatype = Machines.InputData.SourceTypeGenerator;
			}
			else if(inputa.IndexOf("proc") == 0)
			{
				inputaid = Int32.Parse(inputa.Replace("proc", ""));
				inputatype = Machines.InputData.SourceTypeProcessor;
			}
			else if(inputa == "-1")
			{
				inputaid = -1;
			}

			Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[0].SourceID = inputaid;
			Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[0].SourceType = inputatype;
			((Machines.Reverb)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid]).Delay = delay;
			((Machines.Reverb)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid]).Decay = decay;

			OutgoingBuffer = "OK";
			return true;
		}
	}

}