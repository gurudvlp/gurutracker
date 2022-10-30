using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace gurumod.WebPages.StatusCodes
{
	
	
	public class MovedPermanently : WebPage
	{
		//	This class provides the basis for each page that will be used in the
		//	WebInterface.  This outlines the required functions, but each individual
		//	page will obviously add their own.
	
		
		public MovedPermanently()
		{
			base.StatusMessage = "301 Moved Permanently";
		}
		
		public override bool Run()
		{

			this.OutgoingBuffer = "The page has moved permanently.";
			return true;
		}
		
		/*public bool TakeTurn()
		{
			return true;
		}*/
	}
}