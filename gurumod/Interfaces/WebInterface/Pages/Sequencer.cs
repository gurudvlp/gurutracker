using System;

namespace gurumod.WebPages
{
	
	
	public class Sequencer : WebPage
	{
		public string ElementTemplate = "";
		public string RowTemplate = "";
		public string SequencerTemplate = "";
		public string HeaderTemplate = "";
		public string PatternOptionsTemplate = "";
		public string PatternOptionsEachCellTemplate = "";
		
		public Sequencer()
		{
		}
		
		public override bool Run ()
		{
			TerminateOnSend = false;
			
			return true;
		}
		
		public override bool TakeTurn()
		{
			//base.Template = System.IO.File.ReadAllText(Engine.Configuration.WebTemplateDir + "index.html");
			ElementTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/eachelement.html"));
			RowTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/eachrow.html"));
			SequencerTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/main.html"));
			HeaderTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/eachchannel.html"));
			PatternOptionsTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/patternoptions.html"));
			PatternOptionsEachCellTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/eachpatterncell.html"));
			
			Console.WriteLine("Running Sequence Renderer");
			
			//base.Template = base.Template.Replace("[FRAMEWORK_CONTENT]", BuildSequencer(0));
			
			if(base.RequestParts.Length < 2)
			{
				if(base.PostVars.ContainsKey("PATTERNID"))
				{
					int pid = Int32.Parse(base.PostVars["PATTERNID"]);
					
					if(pid > -1) { base.Template = BuildSequencer(pid); }
					else
					{
						//	Show the newest pattern in the lizzt
						int high = 0;
						for(int epat = 0; epat < Engine.TheTrack.Patterns.Length; epat++)
						{
							if(Engine.TheTrack.Patterns[epat] != null) { high = epat; }
						}
						
						base.Template = BuildSequencer(high);
					}
				}
				else
				{
					base.Template = BuildSequencer(0);
				}
			}
			else
			{
				if(base.RequestParts[1] == "patternoptions")
				{
					base.Template = BuildPatternOptions();
					
				}
				else
				{
					base.Template = "FAIL";
				}
			}
			
			base.OutgoingBuffer = base.Template;
			TerminateOnSend = true;
			return true;
		}
		
		public string BuildElement(int pattern, int channel, int row)
		{
			string toret = ElementTemplate;
			
			
			toret = toret.Replace("[NOTE]", Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[row].NoteString);
			toret = toret.Replace("[SAMPLEID]", Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[row].SampleIDString);
			toret = toret.Replace("[VOLUME]", Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[row].VolumeString);
			toret = toret.Replace("[CHANNEL]", channel.ToString());
			toret = toret.Replace("[ROW]", row.ToString());
			
			return toret;
		}
		
		public string BuildChannelHeader(int channel)
		{
			return HeaderTemplate.Replace("[CHANNEL]", channel.ToString()).Replace("[CHANNELTITLE]", "Channel " + channel.ToString());
		}
		
		public string BuildRow(int pattern, int row)
		{
			string toret = "";
			
			try
			{
				for(int ech = 0; ech < Engine.TheTrack.ChannelCount; ech++)
				{
					toret = toret + BuildElement(pattern, ech, row);
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Exception building row");
				//toret = "";
			}
			
			return RowTemplate.Replace("[ELEMENTS]", toret).Replace("[ROW]", row.ToString());
		}
		
		public string BuildSequencer(int pattern)
		{
			string toret = "";
			
			for(int ech = 0; ech < Engine.TheTrack.ChannelCount; ech++)
			{
				toret = toret + BuildChannelHeader(ech);
			}
			
			toret = "<td valign='top'>&nbsp;</td>" + toret;
			
			for(int erow = 0; erow < Engine.TheTrack.Patterns[pattern].RowCount; erow++)
			{
				toret = toret + BuildRow(pattern, erow);
			}
			
			
			return SequencerTemplate.Replace("[EACHROW]", toret);
		}
		
		public string BuildPatternOptions()
		{
			string toret = "";
			
			for(int epat = 0; epat < Engine.TheTrack.Patterns.Length; epat++)
			{
				if(Engine.TheTrack.Patterns[epat] != null)
				{
					string tout = this.PatternOptionsEachCellTemplate;
					tout = tout.Replace("[PATTERNID]", epat.ToString());
					toret = toret + tout;
				}
			}
			
			toret = PatternOptionsTemplate.Replace("[EACHPATTERNCELL]", toret);
			return toret;
		}
	}
}

