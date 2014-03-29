//
//  MachineGeneratorXML.cs
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
	public class MachineGeneratorXML : WebPage
	{
		public MachineGeneratorXML ()
		{
		}

		public override bool Run ()
		{
			int sampleid = 0;
			int generatorid = 0;

			if(!Int32.TryParse(base.RequestParts[2], out sampleid))
			{
				base.OutgoingBuffer = "";
				return true;
			}

			if(!Int32.TryParse(base.RequestParts[3], out generatorid))
			{
				base.OutgoingBuffer = "";
				return true;
			}

			if(Engine.TheTrack.Samples[sampleid] == null) { base.OutgoingBuffer = ""; return true; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid] == null) { base.OutgoingBuffer = ""; return true; }

			Engine.TheTrack.Samples[sampleid].WaveMachine.UpdateGenProcTypes();

			XmlSerializer ser = new XmlSerializer(Type.GetType(Engine.TheTrack.Samples[sampleid].WaveMachine.GeneratorTypes[generatorid]));
			StringWriter writer = new StringWriter();
			ser.Serialize(writer, Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid]);

			base.OutgoingBuffer = writer.ToString();

			return true;
		}

		public override bool TakeTurn ()
		{
			return base.TakeTurn ();
		}
	}
}

