
using System;

namespace gurumod.WebPages
{
	
	
	public class UpdatePattern : WebPage
	{
		public string ElementTemplate = "";
		public string RowTemplate = "";
		public string SequencerTemplate = "";
		
		public UpdatePattern()
		{
		}
		
		public override bool Run ()
		{
			TerminateOnSend = false;
			
			return true;
		}
		
		public override bool TakeTurn()
		{
			Console.WriteLine("UpdatePatterns having it's turn " + PostVars.Count.ToString());
			if(PostVars.ContainsKey("SUBCOL"))
			{
				Console.WriteLine("UpdatePattern has subcol key");
				int subcol = Int32.Parse(base.PostVars["SUBCOL"]);
				int pattern = Int32.Parse(base.PostVars["PATTERN"]);
				int channel = Int32.Parse(base.PostVars["CHANNEL"]);
				int element = Int32.Parse(base.PostVars["ELEMENT"]);
				
				if(subcol == 0)
				{
					Console.WriteLine("Note {0} :: Octave {1}", PostVars["NOTE"], PostVars["OCTAVE"]);
					int note = Int32.Parse(base.PostVars["NOTE"]);
					int octave = Int32.Parse(base.PostVars["OCTAVE"]);
					
					Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[element].Note = note;
					Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[element].Octave = octave;
					
					if(note < 0)
					{
						Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[element].SampleID = -1;
					}
				}
				else if(subcol == 1)
				{
					int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
					Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[element].SampleID = sampleid;
				}
				else if(subcol == 3)
				{
					int volume = Int32.Parse(base.PostVars["VOLUME"]);
					Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[element].Volume = volume;
				}
				
				//Engine.TheTrack.Patterns[pattern].Save();
				Engine.TheTrack.Save();
			}
			TerminateOnSend = true;
			return true;
		}
	}
}