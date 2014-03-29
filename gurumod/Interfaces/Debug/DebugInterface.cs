//
//  DebugInterface.cs
//
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
//
//  Copyright (c) 2014 Brian Murphy
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

namespace gurumod
{
	public class DebugInterface : Interface
	{
		public string IncomingCommand = "";
		public int DebugMode = 0;


		public static int DebugModeListen = 0;
		public static int DebugModeHeartbeat = 1;

		public DebugInterface ()
		{
		}

		public override bool Run (string commandstring)
		{
			return true;
		}

		public override bool TakeTurn ()
		{
			IncomingCommand = "";

			if(base.IncomingBuffer.Contains("\n"))
			{
				IncomingCommand = base.IncomingBuffer.Substring(0, base.IncomingBuffer.IndexOf("\n"));
				IncomingCommand = IncomingCommand.Trim();
				base.IncomingBuffer = base.IncomingBuffer.Substring(base.IncomingBuffer.IndexOf("\n"));
				base.IncomingBuffer = base.IncomingBuffer.TrimStart(new char[]{'\n'});
			}

			if(IncomingCommand != "") { ParseCommand(); }
			if(DebugMode == DebugModeListen) { return true; }
			if(DebugMode == DebugModeHeartbeat) { RunHeartbeat(); }

			return true;
		}


		public void ParseCommand()
		{
			if(IncomingCommand == "beat")
			{
				if(DebugMode == DebugModeHeartbeat) { DebugMode = DebugModeListen; }
				else { DebugMode = DebugModeHeartbeat; }

				base.OutgoingBuffer = "beat toggled\n";
				return;
			}

			if(IncomingCommand == "quit" || IncomingCommand == "exit")
			{
				base.OutgoingBuffer = "later\n";
				base.TerminateAfterSend = true;
				return;
			}

			if(IncomingCommand == "play")
			{
				Engine.TheTrack.EnablePlayer();
				base.OutgoingBuffer = "playback started\n";
				return;
			}

			if(IncomingCommand == "stop")
			{
				Engine.TheTrack.PlayerEnabled = false;
				base.OutgoingBuffer = "playback stopped\n";
				return;
			}
		}


		public void RunHeartbeat()
		{
			string toret = "";
			toret = Engine.TheTrack.CurrentPattern.ToString() + "\t" + Engine.TheTrack.CurrentRow.ToString() + "\n";


			base.OutgoingBuffer = toret;
			base.TerminateAfterSend = false;
		}
	}
}

