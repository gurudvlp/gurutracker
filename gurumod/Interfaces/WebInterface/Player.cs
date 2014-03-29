
using System;

namespace gurumod.WebPages
{
	
	
	public class Player : WebPage
	{
		public string ElementTemplate = "";
		public string RowTemplate = "";
		public string SequencerTemplate = "";
		
		public Player()
		{
		}
		
		public override bool Run ()
		{
			TerminateOnSend = false;
			
			return true;
		}
		
		public override bool TakeTurn()
		{
			base.OutgoingBuffer = "FAIL";
			if(base.RequestParts.Length < 2) { return true; }
			
			
			if(base.RequestParts[1].ToLower() == "play")
			{
				Engine.TheTrack.PlayerEnabled = true;
				base.OutgoingBuffer = "OK";
			}
			else if(base.RequestParts[1].ToLower() == "stop")
			{
				Engine.TheTrack.PlayerEnabled = false;
				Engine.TheTrack.CurrentRow = 0;
				Engine.TheTrack.CurrentPattern = 0;
				base.OutgoingBuffer = "OK";
			}
			else if(base.RequestParts[1] == "pause")
			{
				Engine.TheTrack.PlayerEnabled = false;
				base.OutgoingBuffer = "OK";
			}
			else if(base.RequestParts[1] == "togglemute")
			{
				if(base.RequestParts.Length < 3) { base.OutgoingBuffer = "FAIL"; }
				else
				{
					int cid = -1;
					if(Int32.TryParse(base.RequestParts[2], out cid))
					{
						if(cid > -1 && cid < Engine.TheTrack.ChannelCount)
						{
							try
							{
								if(Engine.TheTrack.ChannelMuted[cid])
								{
									Engine.TheTrack.ChannelMuted[cid] = false;
								}
								else
								{
									Engine.TheTrack.ChannelMuted[cid] = true;
								}
							}
							catch(Exception ex)
							{
								Console.WriteLine("Exceping toggling mute");
								Console.WriteLine(ex.Message);
							}
						}
					}	
				}
			}
			else if(base.RequestParts[1] == "loop")
			{
				if(base.RequestParts.Length < 3) { base.OutgoingBuffer = "FAIL"; }
				else
				{
					int patternid = -1;
					if(base.PostVars.ContainsKey("PATTERNID"))
					{
						Int32.TryParse(base.PostVars["PATTERNID"], out patternid);
					}
					
					if(base.RequestParts[2] == "off")
					{
						Engine.TheTrack.LoopEnabled = false;
					}
					else if(base.RequestParts[2] == "pattern")
					{
						Engine.TheTrack.LoopEnabled = true;
						Engine.TheTrack.LoopPattern = true;
						if(patternid > -1) { Engine.TheTrack.CurrentPattern = patternid; }
					}
					else if(base.RequestParts[2] == "track")
					{
						Engine.TheTrack.LoopEnabled = true;
						Engine.TheTrack.LoopPattern = false;
					}
				}
			}
			else if(base.RequestParts[1] == "save")
			{
				Engine.TheTrack.Save();
				base.OutgoingBuffer = "OK";
			}
			else if(base.RequestParts[1] == "new")
			{
				Engine.TheTrack = new Track();
				
				if(Engine.CommandFlags.ContainsKey("-f"))
				{
					Engine.CommandFlags["-f"] = Engine.PFP(Engine.ConfigPath + "track.xml");
				}
				else
				{
					Engine.CommandFlags.Add("-f", Engine.PFP(Engine.ConfigPath + "track.xml"));
				}
			}
			else if(base.RequestParts[1] == "newpattern")
			{
				Engine.TheTrack.AddPattern();
				base.OutgoingBuffer = "OK";
			}
			else if(base.RequestParts[1] == "duplicatepattern")
			{
				if(base.RequestParts.Length > 2)
				{
					int pid = -1;
					if(Int32.TryParse(base.RequestParts[2], out pid))
					{
						int newpid = Engine.TheTrack.AddPattern(pid);
						//Engine.TheTrack.Patterns[newpid] = Engine.TheTrack.Patterns[pid];
					}
					else
					{
						base.OutgoingBuffer = "FAIL";
					}
				}
				else
				{
					base.OutgoingBuffer = "FAIL";
				}
			}
			else
			{
				base.OutgoingBuffer = "FAIL";
			}
			
			TerminateOnSend = true;
			return true;
		}
	}
}