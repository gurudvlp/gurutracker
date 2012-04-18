
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
			base.Template = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "index.html"));
			
			
			
			base.Template = base.Template.Replace("[FRAMEWORK_CONTENT]", Engine.EngineName + " " + Engine.EngineVersion);
			base.OutgoingBuffer = base.Template;
			
			return true;
		}

	}
}
