using System;

namespace gurumod.WebPages
{
	
	
	public class AdminEngineConfig : WebPage
	{
		//
		//	This class is an Admin Page Category page.  It needs to determine what value to output
		//	or input and do it up.
		//
		
		public AdminEngineConfig()
		{
		}
		
		public override bool Run ()
		{
			base.Template = "";
			
			string toret = "";
			int catid = -1;
			
			if(RequestParts.Length == 2)
			{
				//
				//	Dump a list of categories.
				//
				
			}
			else if(RequestParts.Length == 3)
			{
				//
				//	Peform a sub action for the EngineConfig
				//
				
				
				if(RequestParts[2].ToUpper() == "SAVE")
				{
					/*if(gurumod.Launcher.TheEngine.Save())
					{
						toret = "OK";
					}
					else
					{
						toret = "FAIL The Engine state failed to save.";
					}*/
				}
			}
			
			
			base.Template = toret;
			base.OutgoingBuffer = base.Template;
			
			return true;
		}

	}
}