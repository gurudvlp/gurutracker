//
//  Scripts.cs
//
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
//
//  Copyright (c) 2013 Brian Murphy
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//


using System;

namespace gurumod.WebPages
{
	
	
	public class Scripts : WebPage
	{
		
		public Scripts()
		{
		}
		
		public override bool Run ()
		{

			
			if (base.RequestParts.Length > 1) {
				if (base.RequestParts [1].ToLower () == "track.js") 
				{
					string toret = System.IO.File.ReadAllText (Engine.PFP (Engine.Configuration.WebTemplateDir + "track.js"));
					base.OutgoingBuffer = toret;
					base.ContentType = "application/javascript";
				}
				else if(base.RequestParts[1].ToLower() == "pattern-editor.js")
				{
					string toret = System.IO.File.ReadAllText (Engine.PFP (Engine.Configuration.WebTemplateDir + "pattern-editor.js"));
					base.OutgoingBuffer = toret;
					base.ContentType = "application/javascript";
				}
				else if(base.RequestParts[1].ToLower() == "sampler.js")
				{
					string toret = System.IO.File.ReadAllText (Engine.PFP (Engine.Configuration.WebTemplateDir + "sampler.js"));
					base.OutgoingBuffer = toret;
					base.ContentType = "application/javascript";
				}
				else if(base.RequestParts[1].ToLower() == "detailer.js")
				{
					string toret = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "detailer.js"));
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
				base.OutgoingBuffer = "Not even sure how you got here.";
			}

			return true;
		}

	}
}
