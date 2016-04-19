
using System;

namespace gurumod.WebPages
{
	
	
	public class Image : WebPage
	{
		public static string ImagePath = Engine.PFP(Engine.Configuration.WebTemplateDir + "images");
		
		public Image()
		{
			base.ContentType = "image/png";
		}
		
		public override bool Run ()
		{
			//
			//	To send an image back to the client, we basically just need to
			//	open the file and send it.

			base.TerminateOnSend = true;

			string requestedfile = "";
			
			if(base.RequestParts.Length > 1)
			{
				//
				//	If this is a special subset of images, then we need
				//	to determine that now.
				//		ie this is a map icon
				//
				
				if(base.RequestParts.Length > 2)
				{
					//
					//	Image categories go here.
					//
					
					requestedfile = Image.ImagePath + "/" + base.RequestParts[1] + "/" + base.RequestParts[2];
				}
				else
				{
					requestedfile = Image.ImagePath + "/" + base.RequestParts[1];
				}
				//	RequestParts[1] is the file name
				if(System.IO.File.Exists(requestedfile))
				{
					base.UseAsciiOutput = false;
					base.OutgoingByteBuffer = System.IO.File.ReadAllBytes(requestedfile);
					
					base.StatusMessage = HTTPStatusCodes.OK + " OK";
				}
				else
				{
					//	file not found, 404
					base.StatusMessage = HTTPStatusCodes.NotFound + " NOT FOUND";
					base.ContentType = "text/html";
					
					base.OutgoingBuffer = "Page not found :(" + requestedfile;
				}
			}
			
			return true;
		}

	}
}
