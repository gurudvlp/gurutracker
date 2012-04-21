// 
//  WavPlayer.cs
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
	public class WavPlayer : WebPage
	{
		public int SampleID = 0;
		public int GeneratorID = 0;
		public string WavPlayerTemplate = "";
		
		public WavPlayer ()
		{
		}
		
		public override bool Run ()
		{
			base.TerminateOnSend = false;
			base.OutgoingBuffer = "";
			
			WavPlayerTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/generators/gurumod.machines.wavfile.html"));
			
			
			return true;
		}
		
		public override bool TakeTurn ()
		{
			base.TerminateOnSend = true;
			
			WavPlayerTemplate = WavPlayerTemplate.Replace("[GENERATORID]", GeneratorID.ToString());
			
			if(Engine.TheTrack.Samples[this.SampleID].WaveMachine.Generators[GeneratorID].Enabled)
			{ WavPlayerTemplate = WavPlayerTemplate.Replace("[GENENABLED]", " checked=\"checked\""); }
			else { WavPlayerTemplate = WavPlayerTemplate.Replace("[GENENABLED]", ""); }
			
			WavPlayerTemplate = WavPlayerTemplate.Replace("[FILENAME]", ((Machines.WavFile)Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID]).Filename);
			
			double timelenofsound = 0.0;
			int channels = ((Machines.WavFile)Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID]).Channels;
			int bitrate = ((Machines.WavFile)Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID]).BitRate;
			int samplerate = ((Machines.WavFile)Engine.TheTrack.Samples[SampleID].WaveMachine.Generators[GeneratorID]).SampleRate;
			
			WavPlayerTemplate = WavPlayerTemplate.Replace("[TIME]", "");
			WavPlayerTemplate = WavPlayerTemplate.Replace("[CHANNELS]", channels.ToString());
			WavPlayerTemplate = WavPlayerTemplate.Replace("[BITRATE]", bitrate.ToString());
			WavPlayerTemplate = WavPlayerTemplate.Replace("[SAMPLERATE]", samplerate.ToString());
			
			base.OutgoingBuffer = WavPlayerTemplate;
			return true;
		}
	}
}

