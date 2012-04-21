// 
//  Gate.cs
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

namespace gurumod.WebPages.Samples.Processors
{
	public class Gate : WebPage
	{
		public string GateTemplate = "";
		public int SampleID = 0;
		public int ProcessorID = 0;
		public string InputOptionTemplate = "";
		public string InputSelectTemplate = "";
		
		public Gate ()
		{
		}
		
		public override bool Run ()
		{
			base.TerminateOnSend = false;
			base.OutgoingBuffer = "";
			base.Template = "";
			
			
			GateTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/processors/gurumod.machines.gate.html"));
			InputOptionTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/processors/inputoption.html"));
			InputSelectTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/processors/inputselect.html"));
			
			return true;
		}
		
		public override bool TakeTurn ()
		{
			TerminateOnSend = true;
			
			GateTemplate = GateTemplate.Replace("[PROCESSORID]", ProcessorID.ToString());
			GateTemplate = GateTemplate.Replace("[MINGATE]", ((Machines.Gate)Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID]).MinGateManual.ToString());
			GateTemplate = GateTemplate.Replace("[MAXGATE]", ((Machines.Gate)Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID]).MaxGateManual.ToString());
			GateTemplate = BuildInputSelects(GateTemplate);
			
			base.OutgoingBuffer = GateTemplate;
			return true;
		}
		
		public string BuildInputSelects(string tmpl)
		{
			if(Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID].Inputs != null)
			{
				for(int ein = 0; ein < Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID].InputCount; ein++)
				{
					
					
					if(Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID].Inputs[ein] != null)
					{
						tmpl = tmpl.Replace("[SELECTINPUT:" + ein.ToString() + "]", BuildInputOptions(ein));
					}
					else
					{
						Console.WriteLine("Input {0} is null", ein);
					}
					
					
				}
			}
			else
			{
				Console.WriteLine("This processors input's are null!");
			}
			
			return tmpl;
		}
		
		public string BuildInputOptions(int inputid)
		{
			string toret = "";
			int incount = Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID].InputCount;
			int seltype = Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID].Inputs[inputid].SourceType;
			int selid = Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID].Inputs[inputid].SourceID;
			string keyval = Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID].Inputs[inputid].InputKey();
			
			for(int egen = 0; egen < Engine.TheTrack.Samples[SampleID].WaveMachine.Generators.Length; egen++)
			{
				if(Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[egen] != null)
				{
					string thisoption = InputOptionTemplate;
					thisoption = thisoption.Replace("[VALUE]", "gen" + egen.ToString());
					thisoption = thisoption.Replace("[NAME]", "Wave Generator " + egen.ToString());
					
					//if(selid == egen && seltype == Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[ProcessorID].Inputs[inputid].SourceType)
					if(selid == egen && seltype == Machines.InputData.SourceTypeGenerator)
					{
						thisoption = thisoption.Replace("[SELECTED]", " selected");
						Console.WriteLine("GeneratorSelected {0} for input {1}", egen, inputid);
					}
					else
					{
						thisoption = thisoption.Replace("[SELECTED]", "");
					}
					
					toret = toret + thisoption;
				}
			}
			
			for(int eprc = 0; eprc < Engine.TheTrack.Samples[SampleID].WaveMachine.Processors.Length; eprc++)
			{
				if(Engine.TheTrack.Samples[SampleID].WaveMachine.Processors[eprc] != null)
				{
					string thisoption = InputOptionTemplate;
					thisoption = thisoption.Replace("[VALUE]", "proc" + eprc.ToString());
					thisoption = thisoption.Replace("[NAME]", "Processor " + eprc.ToString());
					
					if(selid == eprc && seltype == Machines.InputData.SourceTypeProcessor)
					{
						thisoption = thisoption.Replace("[SELECTED]", " selected");
						Console.WriteLine("ProcessorSelected {0} for input {1}", eprc, inputid);
					}
					else
					{
						thisoption = thisoption.Replace("[SELECTED]", "");
						
						
					}
					
					toret = toret + thisoption;
				}
			}
			
			if(inputid > 0)
			{
				string manual = InputOptionTemplate;
				manual = manual.Replace("[VALUE]", "-1");
				manual = manual.Replace("[NAME]", "Manual");
				if(selid == -1)
				{
					manual = manual.Replace("[SELECTED]", " selected");
				}
				else
				{
					manual = manual.Replace("[SELECTED]", "");
				}
				
				toret =toret + manual;
			}
			string oret = InputSelectTemplate.Replace("[PROCESSORID]", ProcessorID.ToString());
			oret = oret.Replace("[INPUTID]", inputid.ToString());
			return oret.Replace("[EACHINPUTOPTION]", toret);
		}
	}
}

