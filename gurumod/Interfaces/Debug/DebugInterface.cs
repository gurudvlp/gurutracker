//
//  DebugInterface.cs
//
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
//
//  Copyright (c) 2014-2022 Brian Murphy
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
using System.Text.Json;

using gurumod.Logging;

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
			if(IncomingCommand == "/ping")
			{
				base.OutgoingBuffer = JsonSerializer.Serialize("pong");
				return;
			}

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

			if(IncomingCommand == "/samples/list.json")
			{
				string[] sampleList = new string[Engine.TheTrack.Samples.Length];

				for(int eSample = 0; eSample < Engine.TheTrack.Samples.Length; eSample++)
				{
					sampleList[eSample] = Engine.TheTrack.Samples[eSample].Name;
				}

				string jsonString = JsonSerializer.Serialize(Engine.TheTrack.Samples);

				base.OutgoingBuffer = _formatJson(jsonString);

				return;
			}

			//	Handle any sample related commands
			if(IncomingCommand.StartsWith("/samples/"))
			{
				string tCommand = IncomingCommand.Substring(1);
				string[] commandParts = tCommand.Split('/', StringSplitOptions.None);

				if(commandParts.Length == 3)
				{
					int sampleID = -1;
					if(!Int32.TryParse(commandParts[1], out sampleID))
					{
						Log.lWarning("Supplied sample ID was not an integer.", "DebugInterface", "ParseCommand");
						base.OutgoingBuffer = _formatJson(JsonSerializer.Serialize("Error: Sample ID was not an integer"));
						return;
					}

					if(sampleID < 0
					|| sampleID > Engine.TheTrack.Samples.Length - 1)
					{
						Log.lWarning("Supplied sample ID is out of range.", "DebugInterface", "ParseCommand");
						base.OutgoingBuffer = _formatJson(JsonSerializer.Serialize("Error: Sample ID is out of range."));
						return;
					}

					if(Engine.TheTrack.Samples[sampleID] == null)
					{
						Log.lWarning("Supplied sample ID is null.", "DebugInterface", "ParseCommand");
						base.OutgoingBuffer = _formatJson(JsonSerializer.Serialize("Error: Sample is null."));
						return;
					}

					if(commandParts[2] == "full.json")
					{
						base.OutgoingBuffer = _formatJson(JsonSerializer.Serialize(Engine.TheTrack.Samples[sampleID]));
					}
					else if(commandParts[2] == "generator.json")
					{
						base.OutgoingBuffer = _formatJson(JsonSerializer.Serialize(Engine.TheTrack.Samples[sampleID].WaveGenerator));
					}
					else if(commandParts[2] == "wavemachine.json")
					{
						base.OutgoingBuffer = _formatJson(JsonSerializer.Serialize(Engine.TheTrack.Samples[sampleID].WaveMachine));
					}

					return;
				}
			}

			//	Handle any sequencer releated commands
			if(IncomingCommand.StartsWith("/sequencer"))
			{

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

		private string _formatJson(string jsonstring)
		{
			string prettyJson = jsonstring.Length.ToString() + "\n" +
				jsonstring + "\n";

			return prettyJson;
		}
	}
}

