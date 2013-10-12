//
//  TweakWavePlayer.cs
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
	
	public class TweakWavePlayer : WebPage
	{
		public TweakWavePlayer()
		{
		}

		public override bool Run ()
		{
			int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
			int generatorid = Int32.Parse(base.PostVars["GENERATORID"]);
			string nfilename = base.PostVars["NFILENAME"];
			
			string oenabled = base.PostVars["OENABLED"];
			if(oenabled.ToLower() == "false") { Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid].Enabled = false; }
			else { Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid].Enabled = true; }
			
			((Machines.WavFile)Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid]).Filename = nfilename;
			((Machines.WavFile)Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid]).LoadFile();

			OutgoingBuffer = "OK";

			return true;
		}

		public override bool TakeTurn ()
		{
			return base.TakeTurn ();
		}
	}

}