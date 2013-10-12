//
//  Templates.cs
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
	
	
	public class Templates : WebPage
	{
		
		public Templates()
		{
		}
		
		public override bool Run ()
		{

			
			if (base.RequestParts.Length > 1) 
			{
				string tmppath = "";
				for(int erp = 1; erp < base.RequestParts.Length; erp++)
				{
					tmppath = tmppath + "/" + base.RequestParts[erp];
				}
				tmppath = tmppath.Substring(1);
				tmppath = tmppath.Replace("..", "");
				string fpath = Engine.PFP(Engine.Configuration.WebTemplateDir + tmppath);

				if(System.IO.File.Exists(fpath))
				{
					base.OutgoingBuffer = System.IO.File.ReadAllText(fpath);
				}
				else
				{
					base.OutgoingBuffer = "FAIL Not found";
				}


			}
			else
			{
				base.OutgoingBuffer = "FAIL You need to specify a template";
			}

			return true;
		}

	}
}
