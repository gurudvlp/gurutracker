
using System;

namespace gurumod.WebPages
{
	
	
	public class AdminPageCat : WebPage
	{
		//
		//	This class is an Admin Page Category page.  It needs to determine what value to output
		//	or input and do it up.
		//
		
		public AdminPageCat()
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
			else if(RequestParts.Length == 4)
			{
				//
				//	Peform a sub action for the page categories.
				//
				
				
				if(!Int32.TryParse(RequestParts[3], out catid))
				{
					//	No valid ID was sent
					toret = "FAIL You must specify a category ID.";
				}
				else
				{
					if(catid < 0 || catid >= WebInterface.Categories.Length)
					{
						//	ID is out of valid range.
						toret = "FAIL Category ID (" + catid.ToString() + ") must be ( [0] - [" + WebInterface.Categories.Length.ToString() + "] ).";
					}
					else
					{
						if(WebInterface.Categories[catid] == null)
						{
							//	ID provided was not a valid category ID
							toret = "FAIL Category ID:" + catid.ToString() + " is null.";
						}
						else
						{
							//	It appears as though the ID provided is a valid one, so
							//	let's see if the command provided is as well.
							
							string rqCommand = RequestParts[2].ToUpper();
							if(rqCommand == "NAME") { toret = WebInterface.Categories[catid].Name; }
							else if(rqCommand == "TYPE") { toret = WebInterface.Categories[catid].ObjectType; }
							else if(rqCommand == "COMMAND") { toret = WebInterface.Categories[catid].Command; }
							else { toret = "FAIL Invalid argument (" + rqCommand + "): Expecting [name, type, command]"; }
						}
					}
				}
			}
			else if(RequestParts.Length == 6)
			{
				//
				//	attempt to create a new one
				//
				
				string rqCommand = RequestParts[2].ToUpper();
				
				if(rqCommand == "NEW")
				{
					int newid = Category.NextCategory();
					
					if(newid < 0)
					{
						toret = "FAIL No new category slots are available.";
					}
					else
					{
						string nname = RequestParts[3];
						string ntype = RequestParts[4];
						string ncommand = RequestParts[5];
						
						try
						{
							Type typetype = Type.GetType(ntype, true);
							
							WebInterface.Categories[newid] = new Category(nname, ncommand, ntype);
							Category.SaveAll();
							toret = "OK";
						}
						catch(Exception ex)
						{
							toret = "FAIL Invalid Type (" + ntype + ")";
						}
					}
				}
				else
				{
					toret = "FAIL Invalid argument (" + rqCommand + "): Expecting [new]"; 
				}
			}
			
			base.Template = toret;
			base.OutgoingBuffer = base.Template;
			
			return true;
		}

	}
}
