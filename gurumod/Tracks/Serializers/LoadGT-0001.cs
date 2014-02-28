//
//  LoadGT-0001.cs
//
//  Author:
//       guru <${AuthorEmail}>
//
//  Copyright (c) 2014 guru
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
using System.Text;
using System.IO;

namespace gurumod.Serializers
{
	public class LoadGT_0001
	{



		public LoadGT_0001 ()
		{
		}


		public static Track Load(BinaryReader reader)
		{
			Track thetrack = new Track();

			int nopats = 0;
			int nosamps = 0;
			int nochans = 0;
			int year = 2014;
			int tempo = 145;
			int patlen = 128;

			if(!GrabInt(reader, out nopats, 5)
			|| !GrabInt(reader, out nosamps, 5)
			|| !GrabInt(reader, out nochans, 3)
			   || !GrabInt(reader, out year, 4)
			   || !GrabInt(reader, out tempo, 3)
			   || !GrabInt(reader, out patlen, 4))
			{
				Console.WriteLine("Track header is corrupted.");
				return null;
			}



			Console.WriteLine("Patterns: {0}\nSamples: {1}\nChannels: {2}\nYear: {3}\nTempo: {4}\nPattern Len: {5}", nopats, nosamps, nochans, year, tempo, patlen);

			string author = GrabString(reader);
			string title = GrabString(reader);
			string genre = GrabString(reader);
			string website = GrabString(reader);
			string email = GrabString(reader);
			string comments = GrabString(reader);

			Console.WriteLine("{0} {1} {2} {3} {4} {5}", author, title, genre, website, email, comments);

			int[] mutedchannels = new int[nochans];
			for(int ec = 0; ec < nochans; ec++)
			{
				int yesno = 0;
				if(!GrabInt(reader, out yesno, 1))
				{
					Console.WriteLine("Corruption while loading channel mute data.");
					return null;
				}

				mutedchannels[ec] = yesno;
			}


			string patternlist = GrabString(reader);
			int[] patseq;
			if(patternlist == "") { patseq = new int[1]; }
			else
			{
				patseq = new int[patternlist.Length / 5];
				for(int ep = 0; ep < patternlist.Length / 5; ep++)
				{
					patseq[ep] = Int32.Parse(patternlist.Substring(ep * 5, 5));
				}
			}

			Pattern[] inpatterns = new Pattern[nopats];

			for(int ep = 0; ep < nopats; ep++)
			{
				//		rrrr...ccc...onnvvvsssiiiii...
				//			r: 4 digit number of rows in pattern
				//			c: 3 digit channel number/id
				//			o: 1 digit octave
				//			n: 2 digit note
				//			v: 3 digit volume
				//			s: 3 digitl special value
				//			i: 5 digit sample id
				int tpatternrows = 0;
				if(!GrabInt(reader, out tpatternrows, 4))
				{
					Console.WriteLine("Failed to grab the number of rows for the current pattern.");
					return null;
				}
				inpatterns[ep] = new Pattern();
				inpatterns[ep].Channels = new PatternChannel[nochans];


				for(int ech = 0; ech < nochans; ech++)
				{
					int tchid = 0;
					if(!GrabInt(reader, out tchid, 3))
					{
						Console.WriteLine("Failed to grab channel ID");
						return null;
					}


					inpatterns[ep].Channels[tchid] = new PatternChannel(tpatternrows);
					inpatterns[ep].Channels[tchid].ChannelID = tchid;



					for(int erow = 0; erow < tpatternrows; erow++)
					{
						Console.WriteLine("{0} {1} {2}", ep, ech, erow);
						int octave = 5;
						int note = -1;
						int volume = -1;
						int specialcontrol = -1;
						int sampleid = 0;

						if(!GrabInt(reader, out octave, 1)) { Console.WriteLine("Deserializer failed to parse octave."); return null; }
						if(!GrabInt(reader, out note, 2)) { Console.WriteLine("Deserializer failed to parse note."); return null; }
						if(!GrabInt(reader, out volume, 3)) { Console.WriteLine("Deserializer failed to parse volume."); return null; }
						if(!GrabInt(reader, out specialcontrol, 3)) { Console.WriteLine("Deserializer failed to parse specialcontrol."); return null; }
						if(!GrabInt(reader, out sampleid, 5)) { Console.WriteLine("Deserializer failed to parse sampleid."); return null; }

						inpatterns[ep].Channels[tchid].Elements[erow].Octave = octave;
						inpatterns[ep].Channels[tchid].Elements[erow].Note = note;
						inpatterns[ep].Channels[tchid].Elements[erow].Volume = volume;
						inpatterns[ep].Channels[tchid].Elements[erow].SpecialControl = specialcontrol;
						inpatterns[ep].Channels[tchid].Elements[erow].SampleID = sampleid;

						Console.WriteLine("{0}-{1} {2} {3} {4}", note, octave, sampleid, specialcontrol, volume);
					}
				}
			}


			return thetrack;
		}

		public static bool GrabInt(BinaryReader reader, out int intval, int length)
		{
			intval = 0;
			byte[] buf = new byte[length];
			buf = reader.ReadBytes (length);

			if(buf.Length < length) { Console.WriteLine("Data not long enough to retrieve what was asked for."); return false; }

			string tnm = Encoding.UTF8.GetString(buf);
			if(!Int32.TryParse(tnm, out intval))
			{
				Console.WriteLine("Data was not an integer. {0}", tnm);
				intval = -1;
				//return false;
			}

			return true;
		}

		public static bool GrabLong(BinaryReader reader, out long intval, int length)
		{
			intval = 0;
			byte[] buf = new byte[length];
			buf = reader.ReadBytes (length);

			if(buf.Length < length) { Console.WriteLine("Data not long enough to retrieve what was asked for."); return false; }

			string tnm = Encoding.UTF8.GetString(buf);
			if(!Int64.TryParse(tnm, out intval))
			{
				Console.WriteLine("Data was not an integer.");
				return false;
			}

			return true;
		}

		public static string GrabString(BinaryReader reader)
		{
			byte[] inb = new byte[1];
			bool keepgoin = true;
			string toret = "";

			while(keepgoin)
			{
				inb = reader.ReadBytes(1);
				if(inb.Length < 1) { keepgoin = false; }
				else
				{
					if(Encoding.UTF8.GetString(inb) == "\0") { keepgoin = false; }
					else { toret = toret + Encoding.UTF8.GetString(inb); }

				}

			}

			return toret;
		}
	}
}

