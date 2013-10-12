//
//  TweakGate.cs
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
	
	public class TweakGate : WebPage
	{
		public TweakGate()
		{

		}

		public override bool Run ()
		{
			double mingateman = 0.5;
			Double.TryParse(base.PostVars["GATEMIN"], out mingateman);
			//double.Parse(base.PostVars["GATEMIN"]);
			double maxgateman = 1.0;
			Double.TryParse(base.PostVars["GATEMAX"], out maxgateman);
			//double.Parse(base.PostVars["GATEMAX"]);
			int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
			int processorid = Int32.Parse(base.PostVars["PROCESSORID"]);
			string inputa = base.PostVars["INPUT0"];
			string inputb = base.PostVars["INPUT1"];
			string inputc = base.PostVars["INPUT2"];
			int inputaid = 0;
			int inputbid = 0;
			int inputcid = 0;
			int inputatype = 0;
			int inputbtype = 0;
			int inputctype = 0;
			//string oenabled = base.PostVars["OENABLED"];
			
			//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid]
			((Machines.Gate)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid]).MinGateManual = mingateman;
			((Machines.Gate)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid]).MaxGateManual = maxgateman;
			
			//if(oenabled.ToLower() == "false") { Engine.TheTrack.Samples[sampleid].WaveMachine}
			
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
			
			if(inputb.IndexOf("gen") == 0)
			{
				inputbid = Int32.Parse(inputb.Replace("gen", ""));
				inputbtype = Machines.InputData.SourceTypeGenerator;
			}
			else if(inputb.IndexOf("proc") == 0)
			{
				inputbid = Int32.Parse(inputb.Replace("proc", ""));
				inputbtype = Machines.InputData.SourceTypeProcessor;
			}
			else if(inputb == "-1")
			{
				inputbid = -1;
			}
			
			if(inputc.IndexOf("gen") == 0)
			{
				inputcid = Int32.Parse(inputc.Replace("gen", ""));
				inputctype = Machines.InputData.SourceTypeGenerator;
			}
			else if(inputc.IndexOf("proc") == 0)
			{
				inputcid = Int32.Parse(inputc.Replace("proc", ""));
				inputctype = Machines.InputData.SourceTypeProcessor;
			}
			else if(inputc == "-1")
			{
				inputcid = -1;
			}
			
			Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[0].SourceID = inputaid;
			Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[0].SourceType = inputatype;
			Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[1].SourceID = inputbid;
			Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[1].SourceType = inputbtype;
			Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[2].SourceID = inputcid;
			Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[2].SourceType = inputctype;

			OutgoingBuffer = "OK";
			return true;
		}
	}

}