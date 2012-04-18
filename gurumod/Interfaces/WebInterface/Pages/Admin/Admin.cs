
using System;

namespace gurumod.WebPages
{
	
	
	public class Admin : WebPage
	{
		
		public Admin()
		{
		}
		
		public override bool Run ()
		{
			string toret = "";
			
			if(base.RequestParts.Length == 1)
			{
				toret = "Admin Section";
			}
			else if(base.RequestParts.Length > 1)
			{
				if(base.RequestParts[1].ToUpper() == "PAGECAT")
				{
					WebPage PageObject = new AdminPageCat();
					PageObject.UserID = base.UserID;
					PageObject.InHeaders = base.InHeaders;
					PageObject.RequestParts = base.RequestParts;
					PageObject.IncomingContent = base.IncomingContent;
					//Logging.Write("PostVars");
					PageObject.PostVars = base.PostVars;
					PageObject.Run();
					
					toret = PageObject.Template;
				}
				else if(base.RequestParts[1].ToUpper() == "ENGINE")
				{
					WebPage PageObject = new AdminEngineConfig();
					PageObject.UserID = base.UserID;
					PageObject.InHeaders = base.InHeaders;
					PageObject.RequestParts = base.RequestParts;
					PageObject.IncomingContent = base.IncomingContent;
					//Logging.Write("PostVars");
					PageObject.PostVars = base.PostVars;
					PageObject.Run();
					
					toret = PageObject.Template;
				}
				else
				{
					toret = "FAIL Invalid category (" + base.RequestParts[1] + ")";
				}
			}
			
			base.Template = toret;
			base.OutgoingBuffer = toret;
			
			return true;
		}

	}
}
