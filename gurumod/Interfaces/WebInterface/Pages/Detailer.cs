//
//  Detailer.cs
//
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
//
//  Copyright (c) 2014 Brian Murphy
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
	public class Detailer : WebPage
	{
		public Detailer ()
		{
		}

		public override bool Run ()
		{

			if(base.RequestParts.Length > 1)
			{
				if(base.RequestParts[1].ToLower() == "updatedetails")
				{
					Engine.TheTrack.Author = PostVars["AUTHOR"];
					Engine.TheTrack.Year = Int32.Parse(PostVars["YEAR"]);
					Engine.TheTrack.Title = PostVars["TITLE"];
					Engine.TheTrack.Genre = PostVars["GENRE"];

					base.OutgoingBuffer = "OK";
				}
				else if(base.RequestParts[1].ToLower() == "updatecomments")
				{
					Engine.TheTrack.Comments = PostVars["COMMENTS"];
				}
			}
			else
			{
				base.OutgoingBuffer = "FAIL";
			}

			TerminateOnSend = true;
			return true;
		}
	}
}

