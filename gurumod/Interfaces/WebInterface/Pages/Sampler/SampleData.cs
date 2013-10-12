//
//  SampleData.cs
//
//  Author:
//       guru <${AuthorEmail}>
//
//  Copyright (c) 2013 guru
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
using System.Collections;
using System.Collections.Generic;


namespace gurumod.WebPages
{
	
	public class SampleData : WebPage
	{

		public SampleData()
		{

		}

		public override bool Run ()
		{
			string toret = "";

			for(int es = 0; es < Engine.TheTrack.Samples.Length; es++)
			{
				string trw = "{ \"id\":\"" + Engine.TheTrack.Samples[es].ID.ToString() + "\", " +
								"\"title\":\"" + Engine.TheTrack.Samples[es].Name + "\", " +
						"\"type\":\"";

				int sampletype = 0;
				if(Engine.TheTrack.Samples[es].UseWaveGenerator) { trw = trw + "1"; sampletype = 1; }
				else if(Engine.TheTrack.Samples[es].UseWaveMachine) { trw = trw + "2"; sampletype = 2; }
				else { trw = trw + "0"; }

				trw = trw + "\", \"info\": [ {";

				string infod = "";
				if(sampletype == 0)
				{
					infod = "\"filename\":\"" + Engine.TheTrack.Samples[es].Filename + "\", ";

					if(Engine.TheTrack.Samples[es].SoundData != null)
					{
						infod = infod + "\"length\":\"" + (((float)Engine.TheTrack.Samples[es].SoundData.Length) / Engine.TheTrack.Samples[es].sample_rate).ToString() + "\", ";
					}
					else
					{
						infod = infod + "\"length\":\"0\", ";
					}

					infod = infod + "\"channels\":\""+Engine.TheTrack.Samples[es].channels.ToString()+"\", " +
									"\"bitrate\":\""+Engine.TheTrack.Samples[es].BitRate.ToString()+"\", " +
							"\"samplerate\":\""+Engine.TheTrack.Samples[es].sample_rate.ToString() + "\"";
				}
				else if(sampletype == 1)
				{
					infod = "\"wavetype\":\""+Engine.TheTrack.Samples[es].WaveGenerator.WaveType.ToString() + "\", " +
							"\"samplerate\":\""+Engine.TheTrack.Samples[es].WaveGenerator.SampleRate.ToString() + "\", " +
							"\"length\":\""+Engine.TheTrack.Samples[es].WaveGenerator.Length.ToString()+"\", " +
							"\"frequency\":\""+Engine.TheTrack.Samples[es].WaveGenerator.Frequency.ToString() + "\"";

				}
				else if(sampletype == 2)
				{
					string oscret = "";
					for(int eo = 0; eo < Engine.TheTrack.Samples[es].WaveMachine.Generators.Length; eo++)
					{
						if(Engine.TheTrack.Samples[es].WaveMachine.Generators[eo] != null)
						{
							string tosc = "";
							int gentype = Engine.TheTrack.Samples[es].WaveMachine.Generators[eo].GeneratorType;

							if(Engine.TheTrack.Samples[es].WaveMachine.Generators[eo].Enabled)
							{
								tosc = tosc + "\"enabled\":\"1\", ";
							}
							else
							{
								tosc = tosc + "\"enabled\":\"0\", ";
							}
							tosc = tosc + "\"generatortype\":\""+Engine.TheTrack.Samples[es].WaveMachine.Generators[eo].GeneratorType.ToString()+"\", ";

							if(gentype == gurumod.Machines.Generator.GeneratorTypeOscillator)
							{
								tosc = tosc + "\"wavetype\":\"" + ((gurumod.Machines.Osc)Engine.TheTrack.Samples[es].WaveMachine.Generators[eo]).WaveType.ToString() + "\", ";
							}

								tosc = tosc + "\"frequency\":\""+Engine.TheTrack.Samples[es].WaveMachine.Generators[eo].Frequency.ToString()+"\", " +
									"\"amplitude\":\""+Engine.TheTrack.Samples[es].WaveMachine.Generators[eo].Amplitude.ToString() + "\"";


							Dictionary<string, string> moredet = Engine.TheTrack.Samples[es].WaveMachine.Generators[eo].Details();
							if(moredet.Count > 0)
							{
								tosc = tosc + ", \"details\": [ { ";
								foreach(KeyValuePair<string, string> kvp in moredet)
								{
									tosc = tosc + "\""+kvp.Key+"\":\""+kvp.Value+"\", ";
								}
								if(tosc.Length > 2 && tosc.Substring(tosc.Length - 2) == ", ") { tosc = tosc.Substring(0, tosc.Length - 2); }
								tosc = tosc + "} ]";
							}
							else
							{
								tosc = tosc + ", \"details\": [ ]";
							}

							oscret = oscret + "{ "+tosc+" },\n";
						}
					}
					if(oscret.Length > 2 && oscret.Substring(oscret.Length - 2) == ",\n") { oscret = oscret.Substring(0, oscret.Length - 2); }


					string proret = "";
					for(int ep = 0; ep < Engine.TheTrack.Samples[es].WaveMachine.Processors.Length; ep++)
					{
						string prbld = "";
						string proctype = Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].GetType().ToString();

						int prtype = 0;
						if(proctype.ToLower() == "gurumod.machines.mixer") { prtype = 0; }
						else if( proctype.ToLower() == "gurumod.machines.gate") { prtype = 1; }
						else { prtype = 2; }

						prbld = "\"proctype\":\""+prtype.ToString() + "\", ";
						string inputs = "";

						try
						{
							for(int emix = 0; emix < Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs.Length; emix++)
							{
								int inpttype = Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs[emix].SourceType;
								int inptid = Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs[emix].SourceID;
								double inptamp = Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs[emix].Amplitude;


								string tdata = "{ \"sourcetype\":\""+inpttype.ToString() +"\", \"sourceid\":\""+inptid.ToString()+"\", \"amplitude\":\""+inptamp.ToString()+"\" },\n";
								inputs = inputs + tdata;
							}
						}
						catch(Exception ex)
						{
							Console.WriteLine("Exception looping processor data.");
							Console.WriteLine("Sample: {0}  :: Processor: {1}", es, ep);
							if(Engine.TheTrack.Samples[es].WaveMachine == null) { Console.WriteLine("WaveMachine is null."); }
							else
							{
								if(Engine.TheTrack.Samples[es].WaveMachine.Processors == null) { Console.WriteLine("Processors is null."); }
								else
								{
									if(Engine.TheTrack.Samples[es].WaveMachine.Processors[ep] == null)
									{
										Console.WriteLine("Processors[ep:{0}] is null", ep);
									}
									else
									{
										if(Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs == null)
										{
											Console.WriteLine("Processor Inputs are null");
										}
									}
								}
							}

							Environment.Exit(0);
						}
						if(inputs.Length > 2 && inputs.Substring(inputs.Length - 2) == ",\n") { inputs = inputs.Substring(0, inputs.Length - 2); }

						prbld = prbld + " \"inputs\":[" + inputs + "], ";

						string infos = "";
						if(prtype == 0)
						{
							//	mixer
							int inptcmd = ((gurumod.Machines.Mixer)Engine.TheTrack.Samples[es].WaveMachine.Processors[ep]).CombineMethod;
							infos = "\"method\":\""+inptcmd.ToString()+"\"";
						}
						else if(prtype == 1)
						{
							//	gate
							double ming = ((gurumod.Machines.Gate)Engine.TheTrack.Samples[es].WaveMachine.Processors[ep]).MinGateManual;
							double maxg = ((gurumod.Machines.Gate)Engine.TheTrack.Samples[es].WaveMachine.Processors[ep]).MaxGateManual;
							infos = "\"mingatemanual\":\""+ming.ToString()+"\", " +
								"\"maxgatemanual\":\""+maxg.ToString() + "\"";
						}

						prbld = prbld + "\"details\": [ {"+infos+"} ]";
						proret = proret + "{ " + prbld + " },\n";
					}
					if(proret.Length > 2 && proret.Substring(proret.Length - 2) == ",\n") { proret = proret.Substring(0, proret.Length - 2); }

					infod = "\"oscillators\": \n[" + oscret + "], \n" +
						"\"processors\": \n[" + proret + "]\n";


					//infod = "\"sampletype\":\"special\"";
				}

				
				trw = trw + infod + "} ] },\n";
				toret = toret + trw;
			}

			if(toret.Length > 2 && toret.Substring(toret.Length - 2) == ",\n") { toret = toret.Substring(0, toret.Length - 2); }
			toret = "{\"samples\":[\n" + toret + "\n] }";
			base.OutgoingBuffer = toret;

			return true;
		}

		public override bool TakeTurn ()
		{
			return base.TakeTurn ();
		}
	}
}