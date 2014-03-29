//
//  SampleDataXML.cs
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
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.IO;

namespace gurumod.WebPages
{
	public class SampleDataXML : WebPage
	{
		public SampleDataXML ()
		{
		}

		public override bool Run ()
		{
			XmlSerializer ser = new XmlSerializer(typeof(Sample[]));
			StringWriter writer = new StringWriter();
			ser.Serialize(writer, Engine.TheTrack.Samples);

			base.OutgoingBuffer = writer.ToString();

			return true;
		}

		public override bool TakeTurn ()
		{
			return base.TakeTurn ();
		}
	}
}

