using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

using gurumod.Logging;

namespace gurumod.WebPages
{
	
	
	public class Sequencer : WebPage
	{
		public string ElementTemplate = "";
		public string RowTemplate = "";
		public string SequencerTemplate = "";
		public string HeaderTemplate = "";
		public string PatternOptionsTemplate = "";
		public string PatternOptionsEachCellTemplate = "";
		
		public Sequencer()
		{
		}
		
		public override bool Run ()
		{
			if(base.RequestParts.Length == 1)
			{
				TerminateOnSend = false;
			}
			else
			{
				if(base.RequestParts[1].ToLower() == "patterndata")
				{
					string toret = "FAIL Unknown reason";
					if(Engine.TheTrack == null) { toret = "FAIL Track data is null"; }
					else if(Engine.TheTrack.Patterns == null) { toret = "FAIL Pattern data is null"; }
					else
					{
						toret = "";
						for(int ep = 0; ep < Engine.TheTrack.Patterns.Length; ep++)
						{
							if(Engine.TheTrack.Patterns[ep] != null)
							{
								string chnll = "";
								for(int ec = 0; ec < Engine.TheTrack.Patterns[ep].ChannelCount; ec++)
								{
									string rowret = "";
									for(int er = 0; er < Engine.TheTrack.Patterns[ep].Channels[ec].Elements.Length; er++)
									{
										string trow = "";
										trow = "{ \"octave\":\""+Engine.TheTrack.Patterns[ep].Channels[ec].Elements[er].Octave.ToString()+"\", " +
												" \"note\":\""+Engine.TheTrack.Patterns[ep].Channels[ec].Elements[er].Note.ToString()+"\", " +
												" \"sampleid\":\""+Engine.TheTrack.Patterns[ep].Channels[ec].Elements[er].SampleID.ToString()+"\", " +
												" \"volume\":\""+Engine.TheTrack.Patterns[ep].Channels[ec].Elements[er].Volume.ToString()+"\", " +
												" \"effect\":\""+Engine.TheTrack.Patterns[ep].Channels[ec].Elements[er].SpecialControl.ToString()+"\" },\n";

										rowret = rowret + trow;
									}

									if(rowret.Length > 2 && rowret.Substring(rowret.Length - 2) == ",\n") { rowret = rowret.Substring(0, rowret.Length - 2); }
									chnll = chnll + "{ \"rows\":[ "+rowret+" ] },\n";
								}

								if(chnll.Length > 2 && chnll.Substring(chnll.Length - 2) == ",\n") { chnll = chnll.Substring(0, chnll.Length - 2); }

								toret = toret + "{\"id\":\""+ep.ToString()+"\", \"length\":\""+Engine.TheTrack.Patterns[ep].RowCount.ToString()+"\", \"channels\":[ " + chnll + "] },\n";

							}
						}
					}

					if(toret.Length > 2 && toret.Substring(toret.Length - 2) == ",\n") { toret = toret.Substring(0, toret.Length - 2); }
					toret = "{ \"patterns\": [\n" + toret + "\n]}";
					base.OutgoingBuffer = toret;
				}
				else if(base.RequestParts[1].ToLower() == "patternbin")
				{
					string toret = "FAIL Unknown reason.";
					if(Engine.TheTrack == null) { toret = "FAIL The Track is null."; }
					else if(Engine.TheTrack.Patterns == null) { toret = "FAIL Patterns is null."; }
					else
					{
						/*MemoryStream ms = new MemoryStream();
						BinaryFormatter formatter = new BinaryFormatter();

						formatter.Serialize(ms, Engine.TheTrack.Patterns);

						base.UseAsciiOutput = false;
						base.OutgoingByteBuffer = ms.ToArray();
						base.TerminateOnSend = true;*/
						toret = "FAIL Binary Serialization is obsolete";
					}

					if(toret.Length > 0) { base.OutgoingBuffer = toret;}
				}
				else if(base.RequestParts[1].ToLower() == "patternxml")
				{

					XmlSerializer ser = new XmlSerializer(typeof(Pattern[]));
					StringWriter writer = new StringWriter();
					ser.Serialize(writer, Engine.TheTrack.Patterns);

					base.OutgoingBuffer = writer.ToString();
					/*XmlSerializer s = new XmlSerializer( typeof(Pattern[]) );
					MemoryStream ms = new MemoryStream();
					XmlTextWriter xw = new XmlTextWriter(ms, System.Text.Encoding.UTF8);


					s.Serialize( xw, Engine.TheTrack.Patterns);
					xw.Flush();
					xw.Close();


					base.OutgoingByteBuffer = ms.ToArray();
					base.UseAsciiOutput = false;
					base.TerminateOnSend = true;*/
				}
				else
				{
					base.OutgoingBuffer = "FAIL Unknown action '" + base.RequestParts[1] + "'";
				}
			}
			
			return true;
		}
		
		public override bool TakeTurn()
		{
			//base.Template = System.IO.File.ReadAllText(Engine.Configuration.WebTemplateDir + "index.html");
			ElementTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/eachelement.html"));
			RowTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/eachrow.html"));
			SequencerTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/main.html"));
			HeaderTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/eachchannel.html"));
			PatternOptionsTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/patternoptions.html"));
			PatternOptionsEachCellTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sequencer/eachpatterncell.html"));
			
			Console.WriteLine("Running Sequence Renderer");
			
			//base.Template = base.Template.Replace("[FRAMEWORK_CONTENT]", BuildSequencer(0));
			
			if(base.RequestParts.Length < 2)
			{
				if(base.PostVars.ContainsKey("PATTERNID"))
				{
					int pid = Int32.Parse(base.PostVars["PATTERNID"]);
					
					if(pid > -1) { base.Template = BuildSequencer(pid); }
					else
					{
						//	Show the newest pattern in the lizzt
						int high = 0;
						for(int epat = 0; epat < Engine.TheTrack.Patterns.Length; epat++)
						{
							if(Engine.TheTrack.Patterns[epat] != null) { high = epat; }
						}
						
						base.Template = BuildSequencer(high);
					}
				}
				else
				{
					base.Template = BuildSequencer(0);
				}
			}
			else
			{
				if(base.RequestParts[1] == "patternoptions")
				{
					base.Template = BuildPatternOptions();
					
				}
				else
				{
					base.Template = "FAIL";
				}
			}
			
			base.OutgoingBuffer = base.Template;
			TerminateOnSend = true;
			return true;
		}
		
		public string BuildElement(int pattern, int channel, int row)
		{
			string toret = ElementTemplate;
			
			
			toret = toret.Replace("[NOTE]", Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[row].NoteString);
			toret = toret.Replace("[SAMPLEID]", Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[row].SampleIDString);
			toret = toret.Replace("[VOLUME]", Engine.TheTrack.Patterns[pattern].Channels[channel].Elements[row].VolumeString);
			toret = toret.Replace("[CHANNEL]", channel.ToString());
			toret = toret.Replace("[ROW]", row.ToString());
			
			return toret;
		}
		
		public string BuildChannelHeader(int channel)
		{
			return HeaderTemplate.Replace("[CHANNEL]", channel.ToString()).Replace("[CHANNELTITLE]", "Channel " + channel.ToString());
		}
		
		public string BuildRow(int pattern, int row)
		{
			string toret = "";
			
			try
			{
				for(int ech = 0; ech < Engine.TheTrack.ChannelCount; ech++)
				{
					toret = toret + BuildElement(pattern, ech, row);
				}
			}
			catch(Exception ex)
			{
				Log.lWarning("Exception building row.", "Sequencer", "BuildRow");
				Log.lWarning(ex.Message, "Sequencer", "BuildRow");
			}
			
			return RowTemplate.Replace("[ELEMENTS]", toret).Replace("[ROW]", row.ToString());
		}
		
		public string BuildSequencer(int pattern)
		{
			string toret = "";
			
			for(int ech = 0; ech < Engine.TheTrack.ChannelCount; ech++)
			{
				toret = toret + BuildChannelHeader(ech);
			}
			
			toret = "<td valign='top'>&nbsp;</td>" + toret;
			
			for(int erow = 0; erow < Engine.TheTrack.Patterns[pattern].RowCount; erow++)
			{
				toret = toret + BuildRow(pattern, erow);
			}
			
			
			return SequencerTemplate.Replace("[EACHROW]", toret);
		}
		
		public string BuildPatternOptions()
		{
			string toret = "";
			
			for(int epat = 0; epat < Engine.TheTrack.Patterns.Length; epat++)
			{
				if(Engine.TheTrack.Patterns[epat] != null)
				{
					string tout = this.PatternOptionsEachCellTemplate;
					tout = tout.Replace("[PATTERNID]", epat.ToString());
					toret = toret + tout;
				}
			}
			
			toret = PatternOptionsTemplate.Replace("[EACHPATTERNCELL]", toret);
			return toret;
		}
	}
}

