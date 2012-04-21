// 
//  Oscillator.cs
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

namespace gurumod.WebPages.Samples.Generators
{
	public class Oscillator : WebPage
	{
		public int SampleID = 0;
		public int GeneratorID = 0;
		public string OscTemplate = "";
		
		public Oscillator ()
		{
		}
		
		public override bool Run ()
		{
			OscTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/generators/gurumod.machines.osc.html"));
			
			base.TerminateOnSend = false;
			base.OutgoingBuffer = "";
			base.Template = "";
			
			return true;
		}
		
		public override bool TakeTurn ()
		{
			base.TerminateOnSend = true;
			
			if(Engine.TheTrack.Samples == null) { return true; }
			if(Engine.TheTrack.Samples[SampleID] == null) { return true; }
			if(Engine.TheTrack.Samples[SampleID].WaveMachine == null) { return true; }
			if(Engine.TheTrack.Samples[SampleID].WaveMachine.Generators == null) { Engine.TheTrack.Samples[SampleID].WaveMachine.InitGenerators(); }
			if(Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID] == null) { return true; }
			
			OscTemplate = OscTemplate.Replace("[GENERATORID]", GeneratorID.ToString());
			OscTemplate = OscTemplate.Replace("[SAMPLEID]", SampleID.ToString());
			
			int wavetype = ((Machines.Osc)Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID]).WaveType;
			double frequency = ((Machines.Osc)Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID]).Frequency;
			double ampl = ((Machines.Osc)Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID]).Amplitude;
			bool enabled = ((Machines.Osc)Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID]).Enabled
				;
			OscTemplate = OscTemplate.Replace("[GENERATORMACHINEFREQUENCY]", frequency.ToString());
			OscTemplate = OscTemplate.Replace("[GENMACHINEAMPLITUDE]", ampl.ToString());
			
			if(wavetype == Generator.TypeSine) { OscTemplate = OscTemplate.Replace("[GENSELSINE]", " selected"); } else { OscTemplate = OscTemplate.Replace("[GENSELSINE]", ""); }
			if(wavetype == Generator.TypeSquare) { OscTemplate = OscTemplate.Replace("[GENSELSQUARE]", " selected"); } else { OscTemplate = OscTemplate.Replace("[GENSELSQUARE]", ""); }
			if(wavetype == Generator.TypeSawtooth) { OscTemplate = OscTemplate.Replace("[GENSELSAWTOOTH]", " selected"); } else { OscTemplate = OscTemplate.Replace("[GENSELSAWTOOTH]", ""); }
			if(wavetype == Generator.TypeTriangle) { OscTemplate = OscTemplate.Replace("[GENSELTRIANGLE]", " selected"); } else { OscTemplate = OscTemplate.Replace("[GENSELTRIANGLE]", ""); }
			
			if(enabled)
			{
				OscTemplate = OscTemplate.Replace("[GENENABLED]", " checked=\"checked\"");
			}
			else
			{
				OscTemplate = OscTemplate.Replace("[GENENABLED]", "");
			}
			
			base.OutgoingBuffer = OscTemplate;
			
			
			return true;
		}
	}
}

