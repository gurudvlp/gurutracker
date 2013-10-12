
using System;

namespace gurumod.WebPages
{
	
	
	public class Home : WebPage
	{
		
		public Home()
		{
		}
		
		public override bool Run ()
		{
			base.Template = System.IO.File.ReadAllText (Engine.PFP (Engine.Configuration.WebTemplateDir + "index.html"));
			
			if (base.RequestParts.Length > 1) {
				if (base.RequestParts [1].ToLower () == "track.js") 
				{
					string toret = System.IO.File.ReadAllText (Engine.PFP (Engine.Configuration.WebTemplateDir + "track.js"));
					base.OutgoingBuffer = toret;
					base.ContentType = "application/javascript";
				} 
				else
				{
					base.OutgoingBuffer = "Unknown action";
				}
			}
			else
			{
				base.Template = base.Template.Replace ("[FRAMEWORK_CONTENT]", Engine.EngineName + " " + Engine.EngineVersion);
				base.OutgoingBuffer = base.Template;
			}

			return true;
		}

	}
}
