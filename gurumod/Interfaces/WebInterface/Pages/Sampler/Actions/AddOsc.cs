//
//  AddOsc.cs
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
	
	public class AddOsc : WebPage
	{
		public AddOsc()
		{

		}

		public override bool Run ()
		{
			int smpid = 0;
			Console.WriteLine("AddOsc called");
			
			if(Int32.TryParse(base.RequestParts[2], out smpid))
			{
				//	The user is trying to add an oscillator, and we have the
				//	sample id.  So if that sample exists, we need to create a enw
				//	oscillator :)
				
				if(Engine.TheTrack.Samples[smpid] != null)
				{
					/*Machines.Generator[] tmposc = new gurumod.Machines.Generator[Engine.TheTrack.Samples[smpid].WaveMachine.Generators.Length + 1];
					for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Generators.Length; eogo++)
					{
						tmposc[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Generators[eogo];
					}
					tmposc[tmposc.Length - 1] = new gurumod.Machines.Osc();
					
					//Oscilator[] tmposc = new Oscilator[Engine.TheTrack.Samples[smpid].WaveMachine.Oscs.Length + 1];
					///*for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Oscs.Length; eogo++)
					{
						tmposc[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Oscs[eogo];
					}
					
					tmposc[tmposc.Length - 1] = new Oscilator();*/
					// uncomment this too Engine.TheTrack.Samples[smpid].WaveMachine.Generators = tmposc;

					//	Test to see wtf why oscillators are all adding to the end ...
					int nextoscid = Engine.TheTrack.Samples[smpid].WaveMachine.NextGeneratorID();
					Engine.TheTrack.Samples[smpid].WaveMachine.Generators[nextoscid] = new gurumod.Machines.Osc();

					OutgoingBuffer = "OK";
				}
				else
				{
					OutgoingBuffer = "FAIL Sample " + smpid.ToString() + " is null";
				}
			}
			else
			{
				OutgoingBuffer = "FAIL Failed to parse sample id";
			}

			TerminateOnSend = true;
			return true;
		}

		public override bool TakeTurn ()
		{
			return base.TakeTurn ();
		}
	}
}