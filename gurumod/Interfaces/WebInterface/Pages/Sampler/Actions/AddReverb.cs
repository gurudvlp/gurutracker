//
//  AddReverb.cs
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
	
	public class AddReverb : WebPage
	{
		public AddReverb()
		{

		}

		public override bool Run ()
		{
			int smpid = 0;
			Console.WriteLine("AddReverb Called");
			
			if(Int32.TryParse(base.RequestParts[2], out smpid))
			{
				if(Engine.TheTrack.Samples[smpid] != null)
				{
					//int nextoscid = Engine.TheTrack.Samples[smpid].WaveMachine.NextProcessorID();
					//Engine.TheTrack.Samples[smpid].WaveMachine.Processors[nextoscid] = new gurumod.Machines.Reverb();

					Machines.Processor[] tmppr = new gurumod.Machines.Processor[Engine.TheTrack.Samples[smpid].WaveMachine.Processors.Length + 1];
					for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Processors.Length; eogo++)
					{
						tmppr[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Processors[eogo];
					}
					
					tmppr[tmppr.Length - 1] = new gurumod.Machines.Reverb();
					((Machines.Reverb)tmppr[tmppr.Length - 1]).Initialize();
					
					Engine.TheTrack.Samples[smpid].WaveMachine.Processors = tmppr;
					Engine.TheTrack.Samples[smpid].WaveMachine.UpdateGenProcTypes();
					OutgoingBuffer = "OK";
				} else { OutgoingBuffer = "FAIL Sample id " + smpid.ToString() + " is null"; }
			} else { OutgoingBuffer = "FAIL Failed to parse sample id"; }

			return true;
		}

		public override bool TakeTurn ()
		{
			return base.TakeTurn ();
		}

	}
}