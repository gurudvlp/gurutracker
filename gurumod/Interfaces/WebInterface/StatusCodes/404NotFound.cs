using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace gurumod.WebPages.StatusCodes
{
	
	
	public class NotFound : WebPage
	{
		//	This class provides the basis for each page that will be used in the
		//	WebInterface.  This outlines the required functions, but each individual
		//	page will obviously add their own.
	
		
		public NotFound()
		{
			base.StatusMessage = "404 Not Found";
		}
		
		public override bool Run()
		{

			this.OutgoingBuffer = "The page you requested was not found.";
			return true;
		}
		
		public bool TakeTurn()
		{
			return true;
		}
	}
}