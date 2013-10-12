
using System;
using System.Collections;
using System.Collections.Generic;


namespace gurumod.WebPages
{
	
	public class Sampler : WebPage
	{
		public string SamplerTemplate = "";
		public string SamplesTemplate = "";
		public string EachSampleTemplate = "";
		public string DetailsTemplate = "";
		public string GeneratorTemplate = "";
		public string ImportTemplate = "";
		public string MachineTemplate = "";
		public string OscTemplate = "";
		public string EachMixerTemplate = "";
		public string EachGateSrcTemplate = "";
		
		public string ProcessorTemplatePath = "";
		
		public Sampler()
		{
			
		}
		
		public override bool Run ()
		{
			if(base.RequestParts.Length > 1 && base.RequestParts[1].ToLower() == "sampledata")
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

							for(int emix = 0; emix < Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs.Length; emix++)
							{
								int inpttype = Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs[emix].SourceType;
								int inptid = Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs[emix].SourceID;
								double inptamp = Engine.TheTrack.Samples[es].WaveMachine.Processors[ep].Inputs[emix].Amplitude;


								string tdata = "{ \"sourcetype\":\""+inpttype.ToString() +"\", \"sourceid\":\""+inptid.ToString()+"\", \"amplitude\":\""+inptamp.ToString()+"\" },\n";
								inputs = inputs + tdata;
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
			else
			{
				TerminateOnSend = false;
				
				SamplesTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/samplelist.html"));
				EachSampleTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/eachsample.html"));
				SamplerTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/main.html"));
				DetailsTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/details.html"));
				GeneratorTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/generator.html"));
				ImportTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/importfile.html"));
				MachineTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/genmachine.html"));
				OscTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/eachosc.html"));
				EachMixerTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/eachmixer.html"));
				EachGateSrcTemplate = System.IO.File.ReadAllText(Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/eachmixer-eachgatesrc.html"));
				
				ProcessorTemplatePath = Engine.PFP(Engine.Configuration.WebTemplateDir + "sampler/processors/");
				
				base.OutgoingBuffer = "";
				base.Template = "";
				
				return true;
			}
		}
		
		public override bool TakeTurn()
		{
			
			//Console.WriteLine("Running Sampler Renderer");
			//base.Template = System.IO.File.ReadAllText(Engine.Configuration.WebTemplateDir + "index.html");
			base.TerminateOnSend = true;
			if(base.RequestParts.Length == 1)
			{
				//base.Template = base.Template.Replace("[FRAMEWORK_CONTENT]", BuildSampler());
				base.Template = BuildSampler();
			}
			else
			{
				if(base.RequestParts.Length > 2)
				{
					if(base.RequestParts[1] == "details")
					{
						Console.WriteLine(base.RequestParts[2]);
						base.Template = BuildDetails(Int32.Parse(base.RequestParts[2]));
					}
					else if(base.RequestParts[1] == "addosc")
					{
						int smpid = 0;
						Console.WriteLine("AddOsc called");
						
						if(Int32.TryParse(base.RequestParts[2], out smpid))
						{
							//	The user is trying to add an oscillator, and we have the
							//	sample id.  So if that sample exists, we need to create a enw
							//	oscillator :)
							
							if(Engine.TheTrack.Samples[smpid] != null)
							{
								/*Machines.Generator[] tmposc = new gurumod.Machines.Generator[Engine.TheTrack.Samples[smpid].WaveMachine.Generators.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Generators.Length; eogo++)
								{
									tmposc[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Generators[eogo];
								}
								tmposc[tmposc.Length - 1] = new gurumod.Machines.Osc();
								
								//Oscilator[] tmposc = new Oscilator[Engine.TheTrack.Samples[smpid].WaveMachine.Oscs.Length + 1];
								///*for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Oscs.Length; eogo++)
								{
									tmposc[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Oscs[eogo];
								}
								
								tmposc[tmposc.Length - 1] = new Oscilator();*/
								// uncomment this too Engine.TheTrack.Samples[smpid].WaveMachine.Generators = tmposc;

								//	Test to see wtf why oscillators are all adding to the end ...
								int nextoscid = Engine.TheTrack.Samples[smpid].WaveMachine.NextGeneratorID();
								Engine.TheTrack.Samples[smpid].WaveMachine.Generators[nextoscid] = new gurumod.Machines.Osc();

							}
						}
						
						base.Template = "OK";
					}
					else if(base.RequestParts[1] == "addmixer")
					{
						int smpid = 0;
						Console.WriteLine("AddMixer called");
						
						if(Int32.TryParse(base.RequestParts[2], out smpid))
						{
							//	The user is trying to add an oscillator, and we have the
							//	sample id.  So if that sample exists, we need to create a enw
							//	oscillator :)
							
							if(Engine.TheTrack.Samples[smpid] != null)
							{
								Machines.Processor[] tmppr = new gurumod.Machines.Processor[Engine.TheTrack.Samples[smpid].WaveMachine.Processors.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Processors.Length; eogo++)
								{
									tmppr[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Processors[eogo];
								}
								
								tmppr[tmppr.Length - 1] = new gurumod.Machines.Mixer();
								((Machines.Mixer)tmppr[tmppr.Length - 1]).Initialize();
								
								Engine.TheTrack.Samples[smpid].WaveMachine.Processors = tmppr;
								
								/*MixerSettings[] tmposc = new MixerSettings[Engine.TheTrack.Samples[smpid].WaveMachine.Mixers.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Mixers.Length; eogo++)
								{
									tmposc[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Mixers[eogo];
								}
								
								tmposc[tmposc.Length - 1] = new MixerSettings();
								
								Engine.TheTrack.Samples[smpid].WaveMachine.Mixers = tmposc;*/
							}
						}
						
						base.Template = "OK";
					}
					else if(base.RequestParts[1] == "addwaveplayer")
					{
						int smpid = 0;
						Console.WriteLine("AddWavePlayer Called");
						
						if(Int32.TryParse(base.RequestParts[2], out smpid))
						{
							if(Engine.TheTrack.Samples[smpid] != null)
							{
							/*	Machines.Generator[] tmppr = new gurumod.Machines.Generator[Engine.TheTrack.Samples[smpid].WaveMachine.Generators.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Generators.Length; eogo++)
								{
									tmppr[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Generators[eogo];
								}
								
								tmppr[tmppr.Length - 1] = new gurumod.Machines.WavFile();
								//((Machines.WavFile)tmppr[tmppr.Length - 1]).Initialize();
								
								Engine.TheTrack.Samples[smpid].WaveMachine.Generators = tmppr;*/

								////	test of wth it adds thigns to the end for
								int nextoscid = Engine.TheTrack.Samples[smpid].WaveMachine.NextGeneratorID();
								Engine.TheTrack.Samples[smpid].WaveMachine.Generators[nextoscid] = new gurumod.Machines.WavFile();

							}
						}
					}
					else if(base.RequestParts[1] == "addreverb")
					{
						int smpid = 0;
						Console.WriteLine("AddReverb Called");
						
						if(Int32.TryParse(base.RequestParts[2], out smpid))
						{
							if(Engine.TheTrack.Samples[smpid] != null)
							{
								//int nextoscid = Engine.TheTrack.Samples[smpid].WaveMachine.NextProcessorID();
								//Engine.TheTrack.Samples[smpid].WaveMachine.Processors[nextoscid] = new gurumod.Machines.Reverb();

								Machines.Processor[] tmppr = new gurumod.Machines.Processor[Engine.TheTrack.Samples[smpid].WaveMachine.Processors.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Processors.Length; eogo++)
								{
									tmppr[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Processors[eogo];
								}
								
								tmppr[tmppr.Length - 1] = new gurumod.Machines.Reverb();
								((Machines.Reverb)tmppr[tmppr.Length - 1]).Initialize();
								
								Engine.TheTrack.Samples[smpid].WaveMachine.Processors = tmppr;
							}
						}
					}
					else if(base.RequestParts[1] == "addgate")
					{
						int smpid = 0;
						Console.WriteLine("AddGate called");
						
						if(Int32.TryParse(base.RequestParts[2], out smpid))
						{
							//	The user is trying to add an oscillator, and we have the
							//	sample id.  So if that sample exists, we need to create a enw
							//	oscillator :)
							
							if(Engine.TheTrack.Samples[smpid] != null)
							{
								Machines.Processor[] tmppr = new gurumod.Machines.Processor[Engine.TheTrack.Samples[smpid].WaveMachine.Processors.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Processors.Length; eogo++)
								{
									tmppr[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Processors[eogo];
								}
								
								tmppr[tmppr.Length - 1] = new gurumod.Machines.Gate();
								((Machines.Gate)tmppr[tmppr.Length - 1]).Initialize();
								
								Engine.TheTrack.Samples[smpid].WaveMachine.Processors = tmppr;
								
								/*MixerSettings[] tmposc = new MixerSettings[Engine.TheTrack.Samples[smpid].WaveMachine.Mixers.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Mixers.Length; eogo++)
								{
									tmposc[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Mixers[eogo];
								}
								
								tmposc[tmposc.Length - 1] = new MixerSettings();
								
								Engine.TheTrack.Samples[smpid].WaveMachine.Mixers = tmposc;*/
							}
						}
						
						base.Template = "OK";
					}
					else
					{
						base.Template = "FAIL";
					}
				}
				else if(base.RequestParts.Length == 2)
				{
					int sampid = -1;
					if(base.PostVars.ContainsKey("SAMPLEID"))
					{
						sampid = Int32.Parse(base.PostVars["SAMPLEID"]);
					}
					
					if(base.RequestParts[1] == "generator" && sampid > -1)
					{
						base.Template = BuildSampleGenerator(sampid);
					}
					else if(base.RequestParts[1] == "genmachine" && sampid > -1)
					{
						base.Template = BuildMachine(sampid);
					}
					else if(base.RequestParts[1] == "import" && sampid > -1)
					{
						base.Template = BuildSampleImporter(sampid);
					}
					else if(base.RequestParts[1] == "updatesample")
					{
						if(base.PostVars.ContainsKey("SAMPLEID"))
						{
							int sid = Int32.Parse(base.PostVars["SAMPLEID"]);
							
							if(base.PostVars.ContainsKey("TITLE"))
							{
								Engine.TheTrack.Samples[sid].Name = base.PostVars["TITLE"];
							}
							
							if(base.PostVars.ContainsKey("FILENAME"))
							{
								/* filename logic */
							}
							
							if(base.PostVars.ContainsKey("LOADFILE"))
							{
								if(base.PostVars.ContainsKey("NEWFILENAME"))
								{
									string nfn = PostVars["NEWFILENAME"];
									string ogpath = "";
									
									if(System.IO.File.Exists(Engine.PFP(Engine.Configuration.PersonalSamples + nfn)))
									{
										//	The sample the user is trying to load was found in their personal
										//	collection of samples.  Not the system wide ones.
										ogpath = Engine.Configuration.PersonalSamples + nfn;
									}
									else if(System.IO.File.Exists(Engine.PFP(Engine.Configuration.SharedSamples + nfn)))
									{
										ogpath = Engine.Configuration.SharedSamples + nfn;
									}
									else
									{
										base.Template = "FAIL";
									}
									
									if(ogpath != "")
									{
										if(!System.IO.Directory.Exists(Engine.PFP(Engine.TheTrack.MyPath + "Samples")))
										{
											System.IO.Directory.CreateDirectory(Engine.PFP(Engine.TheTrack.MyPath + "Samples"));
										}
										if(!System.IO.Directory.Exists(Engine.PFP(Engine.TheTrack.MyPath + "Samples/Audio")))
										{
											System.IO.Directory.CreateDirectory(Engine.PFP(Engine.TheTrack.MyPath + "Samples/Audio"));
										}
										
										System.IO.File.Copy(Engine.PFP(ogpath), Engine.PFP(Engine.TheTrack.MyPath + "Samples/Audio/" + nfn), true);
										Engine.TheTrack.Samples[sid].Filename = nfn;
										Engine.TheTrack.Samples[sid].Loaded = false;
										Engine.TheTrack.Samples[sid].LoadSample();
										Engine.TheTrack.Samples[sid].UseWaveMachine = false;
										Engine.TheTrack.Samples[sid].UseWaveGenerator = false;
										Engine.TheTrack.Samples[sid].ID = sid;
									}
									else
									{
										base.Template = "FAIL";
									}
								}
								else
								{
									base.Template = "FAIL";
								}
							}
						}
						else
						{
							base.Template = "FAIL";
						}
					}
					
					else if(base.RequestParts[1] == "generatesample")
					{
						if(base.PostVars.ContainsKey("SAMPLEID"))
						{
							if(base.PostVars.ContainsKey("TYPE"))
							{
								int sid = Int32.Parse(base.PostVars["SAMPLEID"]);
								if(base.PostVars["TYPE"] == "silence")
								{
									base.Template = "FAIL";
								}
								else if(base.PostVars["TYPE"] == "sine")
								{
									double frequency = double.Parse(base.PostVars["FREQUENCY"]);
									int samplerate = Int32.Parse(base.PostVars["SAMPLERATE"]);
									int length = Int32.Parse(base.PostVars["LENGTHZ"]);
									//string filename = base.PostVars["FILENAME"];
									
									Engine.TheTrack.Samples[sid].UseWaveGenerator = true;
									Engine.TheTrack.Samples[sid].UseWaveMachine = false;
									Engine.TheTrack.Samples[sid].WaveGenerator.Frequency = frequency;
									Engine.TheTrack.Samples[sid].WaveGenerator.SampleRate = samplerate;
									Engine.TheTrack.Samples[sid].WaveGenerator.Length = length;
									Engine.TheTrack.Samples[sid].WaveGenerator.Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
									Engine.TheTrack.Samples[sid].WaveGenerator.WaveType = Generator.TypeSine;
									Engine.TheTrack.Samples[sid].Loaded = false;
									
									/*Engine.TheTrack.Samples[sid].UseWaveMachine = true;
									Engine.TheTrack.Samples[sid].WaveMachine.Frequency = frequency;
									Engine.TheTrack.Samples[sid].WaveMachine.SampleRate = samplerate;
									Engine.TheTrack.Samples[sid].WaveMachine.Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
									Engine.TheTrack.Samples[sid].WaveMachine.WaveType = Generator.TypeSine;
									*/
									//Engine.TheTrack.Samples[sid].ImportSample(Sample.SineWave(frequency, length, samplerate), OpenTK.Audio.OpenAL.ALFormat.Mono16, samplerate);
									Engine.TheTrack.Samples[sid].Filename = "";
									
									base.Template = "OK";
								}
								else if(base.PostVars["TYPE"] == "square")
								{
									double frequency = double.Parse(base.PostVars["FREQUENCY"]);
									int samplerate = Int32.Parse(base.PostVars["SAMPLERATE"]);
									int length = Int32.Parse(base.PostVars["LENGTHZ"]);
									
									Engine.TheTrack.Samples[sid].UseWaveGenerator = true;
									Engine.TheTrack.Samples[sid].UseWaveMachine = false;
									Engine.TheTrack.Samples[sid].WaveGenerator.Frequency = frequency;
									Engine.TheTrack.Samples[sid].WaveGenerator.SampleRate = samplerate;
									Engine.TheTrack.Samples[sid].WaveGenerator.Length = length;
									Engine.TheTrack.Samples[sid].WaveGenerator.Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
									Engine.TheTrack.Samples[sid].WaveGenerator.WaveType = Generator.TypeSquare;
									Engine.TheTrack.Samples[sid].Loaded = false;
									
									/*Engine.TheTrack.Samples[sid].UseWaveMachine = true;
									Engine.TheTrack.Samples[sid].WaveMachine.Frequency = frequency;
									Engine.TheTrack.Samples[sid].WaveMachine.SampleRate = samplerate;
									Engine.TheTrack.Samples[sid].WaveMachine.Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
									Engine.TheTrack.Samples[sid].WaveMachine.WaveType = Generator.TypeSquare;
									*/
									
									//Engine.TheTrack.Samples[sid].ImportSample(Sample.SineWave(frequency, length, samplerate), OpenTK.Audio.OpenAL.ALFormat.Mono16, samplerate);
									Engine.TheTrack.Samples[sid].Filename = "";
								}
							}
							else
							{
								base.Template = "FAIL";
							}
						}
						else
						{
							base.Template = "FAIL";
						}
					}
					else if(base.RequestParts[1] == "tweakwaveplayer")
					{
						int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
						int generatorid = Int32.Parse(base.PostVars["GENERATORID"]);
						string nfilename = base.PostVars["NFILENAME"];
						
						string oenabled = base.PostVars["OENABLED"];
						if(oenabled.ToLower() == "false") { Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid].Enabled = false; }
						else { Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid].Enabled = true; }
						
						((Machines.WavFile)Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid]).Filename = nfilename;
						((Machines.WavFile)Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[generatorid]).LoadFile();
						base.Template = "OK";
					}
					else if(base.RequestParts[1] == "tweakreverb")
					{
						int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
						int processorid = Int32.Parse(base.PostVars["PROCESSORID"]);
						string inputa = base.PostVars["INPUT0"];
						int inputaid = 0;
						int inputatype = 0;

						if(inputa.IndexOf("gen") == 0)
						{
							inputaid = Int32.Parse(inputa.Replace("gen", ""));
							inputatype = Machines.InputData.SourceTypeGenerator;
						}
						else if(inputa.IndexOf("proc") == 0)
						{
							inputaid = Int32.Parse(inputa.Replace("proc", ""));
							inputatype = Machines.InputData.SourceTypeProcessor;
						}
						else if(inputa == "-1")
						{
							inputaid = -1;
						}

						Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[0].SourceID = inputaid;
						Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[0].SourceType = inputatype;

						Logging.Log.Write("TweakReverb: new input sid "+inputaid.ToString()+" :: type "+inputatype.ToString());;
					}
					else if(base.RequestParts[1] == "tweakgate")
					{
						double mingateman = 0.5;
						Double.TryParse(base.PostVars["GATEMIN"], out mingateman);
						//double.Parse(base.PostVars["GATEMIN"]);
						double maxgateman = 1.0;
						Double.TryParse(base.PostVars["GATEMAX"], out maxgateman);
						//double.Parse(base.PostVars["GATEMAX"]);
						int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
						int processorid = Int32.Parse(base.PostVars["PROCESSORID"]);
						string inputa = base.PostVars["INPUT0"];
						string inputb = base.PostVars["INPUT1"];
						string inputc = base.PostVars["INPUT2"];
						int inputaid = 0;
						int inputbid = 0;
						int inputcid = 0;
						int inputatype = 0;
						int inputbtype = 0;
						int inputctype = 0;
						//string oenabled = base.PostVars["OENABLED"];
						
						//Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid]
						((Machines.Gate)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid]).MinGateManual = mingateman;
						((Machines.Gate)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid]).MaxGateManual = maxgateman;
						
						//if(oenabled.ToLower() == "false") { Engine.TheTrack.Samples[sampleid].WaveMachine}
						
						if(inputa.IndexOf("gen") == 0)
						{
							inputaid = Int32.Parse(inputa.Replace("gen", ""));
							inputatype = Machines.InputData.SourceTypeGenerator;
						}
						else if(inputa.IndexOf("proc") == 0)
						{
							inputaid = Int32.Parse(inputa.Replace("proc", ""));
							inputatype = Machines.InputData.SourceTypeProcessor;
						}
						else if(inputa == "-1")
						{
							inputaid = -1;
						}
						
						if(inputb.IndexOf("gen") == 0)
						{
							inputbid = Int32.Parse(inputb.Replace("gen", ""));
							inputbtype = Machines.InputData.SourceTypeGenerator;
						}
						else if(inputb.IndexOf("proc") == 0)
						{
							inputbid = Int32.Parse(inputb.Replace("proc", ""));
							inputbtype = Machines.InputData.SourceTypeProcessor;
						}
						else if(inputb == "-1")
						{
							inputbid = -1;
						}
						
						if(inputc.IndexOf("gen") == 0)
						{
							inputcid = Int32.Parse(inputc.Replace("gen", ""));
							inputctype = Machines.InputData.SourceTypeGenerator;
						}
						else if(inputc.IndexOf("proc") == 0)
						{
							inputcid = Int32.Parse(inputc.Replace("proc", ""));
							inputctype = Machines.InputData.SourceTypeProcessor;
						}
						else if(inputc == "-1")
						{
							inputcid = -1;
						}
						
						Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[0].SourceID = inputaid;
						Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[0].SourceType = inputatype;
						Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[1].SourceID = inputbid;
						Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[1].SourceType = inputbtype;
						Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[2].SourceID = inputcid;
						Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].Inputs[2].SourceType = inputctype;
					}
					else if(base.RequestParts[1] == "tweakmachine")
					{
						Console.WriteLine("A user is tweaking a sound machine");
						if(base.PostVars.ContainsKey("SAMPLEID"))
						{
							Console.WriteLine("Sample was supplied {0}", base.PostVars["SAMPLEID"]);
							if(base.PostVars.ContainsKey("TYPE"))
							{
								Console.WriteLine("Sample has a type:  {0}", base.PostVars["TYPE"]);
								int sid = Int32.Parse(base.PostVars["SAMPLEID"]);
								
								double useamp = 0.75;
								if(base.PostVars.ContainsKey("AMPLITUDE"))
								{
									double.TryParse(base.PostVars["AMPLITUDE"], out useamp);
								}
								
								string wavetype = base.PostVars["TYPE"];
								int oscid = Int32.Parse(base.PostVars["OSCID"]);
								
								string enabled = "";
								if(base.PostVars.ContainsKey("OENABLED"))
								{
									enabled = base.PostVars["OENABLED"].ToLower();
								}
								
								double frequency = double.Parse(base.PostVars["FREQUENCY"]);
								int samplerate = 44100; //Int32.Parse(base.PostVars["SAMPLERATE"]);
								
								
								Engine.TheTrack.Samples[sid].UseWaveGenerator = false;
								Engine.TheTrack.Samples[sid].UseWaveMachine = true;
								Engine.TheTrack.Samples[sid].WaveMachine.Generators[oscid].Frequency = frequency;
								Engine.TheTrack.Samples[sid].WaveMachine.Generators[oscid].SampleRate = samplerate;
								Engine.TheTrack.Samples[sid].WaveMachine.Generators[oscid].Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
								
								Engine.TheTrack.Samples[sid].WaveMachine.Generators[oscid].Amplitude = useamp;
								
								
								int tyid = 0;
								if(wavetype == "sine") { tyid = Generator.TypeSine; Console.WriteLine("Osc Wave Type: Sine"); }
								else if(wavetype == "square") { tyid = Generator.TypeSquare; Console.WriteLine("Osc Wave Type: Square");}
								else if(wavetype == "sawtooth") { tyid = Generator.TypeSawtooth; Console.WriteLine("Osc Wave Type: Sawtooth");}
								else if(wavetype == "triangle") { tyid = Generator.TypeTriangle; Console.WriteLine("Osc Wave Type: Triangle");}
								
								((Machines.Osc)Engine.TheTrack.Samples[sid].WaveMachine.Generators[oscid]).WaveType = tyid;
								
								if(enabled == "true")
								{
									Console.WriteLine("Setting oscillator {0} to enabled.", oscid);
									//Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = true;
									Engine.TheTrack.Samples[sid].WaveMachine.Generators[oscid].Enabled = true;
								}
								else if(enabled == "false")
								{
									Console.WriteLine("Setting oscillator {0} to disabled.", oscid);
									//Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = false;
									Engine.TheTrack.Samples[sid].WaveMachine.Generators[oscid].Enabled = false;
								}
								//Engine.TheTrack.Samples[sid].Loaded = true;
								
								//Engine.TheTrack.Samples[sid].ImportSample(Sample.SineWave(frequency, length, samplerate), OpenTK.Audio.OpenAL.ALFormat.Mono16, samplerate);
								Engine.TheTrack.Samples[sid].Filename = "";
								
								base.Template = "OK";
								
								
							}
							else
							{
								//	This form submission did not have 'TYPE' set, so they are
								//	not attempting to update an oscillator.  Let's check for
								//	other stuff.
								
								if(base.PostVars.ContainsKey("MIXERID"))
								{
									Console.WriteLine("TweakMachine: Tweaking mixer");
									int mixerid = Int32.Parse(base.PostVars["MIXERID"]);
									//string[] stinputs = new string[]
									string stinputa = base.PostVars["INPUT0"];
									string stinputb = base.PostVars["INPUT1"];
									string stinputaamp = "1.0";
									string stinputbamp = "1.0";
									double inputaamp = 1.0;
									double inputbamp = 1.0;
									int inputtypea = MixerSettings.SourceTypeOscillator;
									int inputtypeb = MixerSettings.SourceTypeOscillator;
									int inputa = 0;//Int32.Parse(base.PostVars["INPUTA"]);
									int inputb = 1;//Int32.Parse(base.PostVars["INPUTB"]);

									if(base.PostVars.ContainsKey("INPUT0AMP"))
									{
										stinputaamp = base.PostVars["INPUT0AMP"];
									}
									if(base.PostVars.ContainsKey("INPUT1AMP"))
									{
										stinputbamp = base.PostVars["INPUT1AMP"];
									}
									
									Console.WriteLine("stInputA,B {0} {1}", stinputa, stinputb);
									if(stinputa.IndexOf("gen") == 0)
									{
										Console.WriteLine("InputA is a generator (oscillator)");
										stinputa = stinputa.Replace("gen", "");
										inputa = Int32.Parse(stinputa);
										inputtypea = Machines.InputData.SourceTypeGenerator;
									}
									else if(stinputa.IndexOf("proc") == 0)
									{
										Console.WriteLine("InputA is a processor");
										stinputa = stinputa.Replace("proc", "");
										inputa = Int32.Parse(stinputa);
										inputtypea = Machines.InputData.SourceTypeProcessor;
									}
									
									if(stinputb.IndexOf("gen") == 0)
									{
										Console.WriteLine("InputB is a generator (oscillator)");
										stinputb = stinputb.Replace("gen", "");
										inputb = Int32.Parse(stinputb);
										inputtypeb = Machines.InputData.SourceTypeGenerator;
									}
									else if(stinputb.IndexOf("proc") == 0)
									{
										Console.WriteLine("InputB is a processor");
										stinputb = stinputb.Replace("proc", "");
										inputb = Int32.Parse(stinputb);
										inputtypeb = Machines.InputData.SourceTypeProcessor;
									}
									
									string outmethod = base.PostVars["COMBMETHOD"];
									int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
									
									Double.TryParse(stinputaamp, out inputaamp);
									Double.TryParse(stinputbamp, out inputbamp);

									if(Engine.TheTrack.Samples[sampleid].WaveMachine.Processors == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitProcessors(); }
									
									Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid].Inputs[0].SourceID = inputa;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid].Inputs[1].SourceID = inputb;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid].Inputs[0].SourceType = inputtypea;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid].Inputs[1].SourceType = inputtypeb;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid].Inputs[0].Amplitude = inputaamp;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid].Inputs[1].Amplitude = inputbamp;
									
									/*Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceAID = inputa;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceBID = inputb;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceTypeA = inputtypea;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceTypeB = inputtypeb;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].GateMax = gatemax;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].GateMin = gatemin;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MaxGateControlSource = gatemaxsrc;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MinGateControlSource = gateminsrc;
									*/
									
									Console.WriteLine("outmethod {0}", outmethod);
									if(outmethod == "add") { ((Machines.Mixer)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid]).CombineMethod = Machines.Mixer.CombineMethodAdd; }
									else if(outmethod == "subtract") { ((Machines.Mixer)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid]).CombineMethod = Machines.Mixer.CombineMethodSubtract; }
									else if(outmethod == "multiply") { ((Machines.Mixer)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid]).CombineMethod = Machines.Mixer.CombineMethodMultiply; }
									else if(outmethod == "divide") { ((Machines.Mixer)Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid]).CombineMethod = Machines.Mixer.CombineMethodDivide; }
									
									/*if(outmethod == "add")
									{
										Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MixMethod = MixerSettings.MixMethodAdd;
									}
									else if(outmethod == "subtract")
									{
										Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MixMethod = MixerSettings.MixMethodSubtract;
									}
									else if(outmethod == "multiply")
									{
										Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MixMethod = MixerSettings.MixMethodMultiply;
									}
									else if(outmethod == "divide")
									{
										Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MixMethod = MixerSettings.MixMethodDivide;
									}*/
									
									base.Template = "OK";
								}
								else
								{
									base.Template = "FAIL";
								}
							}
						}
						else
						{
							base.Template = "FAIL";
						}
					}
					else
					{
						base.Template = "FAIL";
					}
				}
				else
				{
					base.Template = "FAIL";
				}
			}
			
			base.OutgoingBuffer = base.Template;
			base.Template = "";
			
			TerminateOnSend = true;
			return true;
		}
		
		public string BuildSampler()
		{
			string toret = SamplerTemplate;
			toret = toret.Replace("[SAMPLELIST]", BuildSampleList());
			toret = toret.Replace("[DETAILS]", BuildDetails(0));
			
			
			return toret;
		}
		
		public string BuildSampleItem(int sampleid)
		{
			string toret = EachSampleTemplate;
			
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			toret = toret.Replace("[SAMPLENAME]", Engine.TheTrack.Samples[sampleid].Name);
			return toret;
		}
		
		public string BuildSampleList()
		{
			string toret = "";
			for(int esamp = 0; esamp < Track.MaxSamples; esamp++)
			{
				if(Engine.TheTrack.Samples[esamp] != null)
				{
					toret = toret + BuildSampleItem(esamp);
				}
			}
			
			return SamplesTemplate.Replace("[EACHSAMPLE]", toret);
		}
		
		public string BuildDetails(int sampleid)
		{
			string toret = this.DetailsTemplate;
			toret = toret.Replace("[FILENAME]", Engine.TheTrack.Samples[sampleid].Filename);
			toret = toret.Replace("[TITLE]", Engine.TheTrack.Samples[sampleid].Name);
			toret = toret.Replace("[BITRATE]", Engine.TheTrack.Samples[sampleid].BitRate.ToString());
			toret = toret.Replace("[CHANNELS]", Engine.TheTrack.Samples[sampleid].channels.ToString());
			toret = toret.Replace("[BITSPERSAMPLE]", Engine.TheTrack.Samples[sampleid].bits_per_sample.ToString());
			toret = toret.Replace("[SAMPLERATE]", Engine.TheTrack.Samples[sampleid].sample_rate.ToString());
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			
			if(Engine.TheTrack.Samples[sampleid].SoundData != null)
			{
				toret = toret.Replace("[LENGTH]", (((float)Engine.TheTrack.Samples[sampleid].SoundData.Length) / Engine.TheTrack.Samples[sampleid].sample_rate).ToString());
			}
			else
			{
				toret = toret.Replace("[LENGTH]", "?");
			}
			
			if(Engine.TheTrack.Samples[sampleid].UseWaveGenerator)
			{
				toret = toret.Replace("[WORKBENCH]", BuildSampleGenerator(sampleid));
			}
			else if(Engine.TheTrack.Samples[sampleid].UseWaveMachine)
			{
				toret = toret.Replace("[WORKBENCH]", BuildMachine(sampleid));
			}
			else
			{
				toret = toret.Replace("[WORKBENCH]", BuildSampleImporter(sampleid));
			}
			
			
			return toret;
		}
		
		public string BuildSampleImporter(int sampleid)
		{
			string toret = ImportTemplate;
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			toret = toret.Replace("[FILENAME]", Engine.TheTrack.Samples[sampleid].Filename);
			
			return toret;
		}
		
		public string BuildSampleGenerator(int sampleid)
		{
			string toret = GeneratorTemplate;
			toret = toret.Replace("[GENERATORFREQUENCY]", Engine.TheTrack.Samples[sampleid].WaveGenerator.Frequency.ToString());
			toret = toret.Replace("[GENSAMPLERATE]", Engine.TheTrack.Samples[sampleid].WaveGenerator.SampleRate.ToString());
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			
			int ttyp = Engine.TheTrack.Samples[sampleid].WaveGenerator.WaveType;
			
			
			if(ttyp == Generator.TypeSine)
			{
				toret = toret.Replace("[GENTYPESQUARE]", "");
				toret = toret.Replace("[GENTYPESINE]", " selected");
			}
			else if(ttyp == Generator.TypeSquare)
			{
				toret = toret.Replace("[GENTYPESINE]", "");
				toret = toret.Replace("[GENTYPESQUARE]", " selected");
			}
			else
			{
				toret = toret.Replace("[GENTYPESINE]", "");
				toret = toret.Replace("[GENTYPESQUARE]", "");
			}
			
			return toret;
		}
		
		public string BuildMachine(int sampleid)
		{
			string toret = MachineTemplate;
			
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Generators == null)
			{
				Engine.TheTrack.Samples[sampleid].WaveMachine.InitGenerators();
			}
			
			/*if(Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs == null)
			{
				Engine.TheTrack.Samples[sampleid].WaveMachine.InitOscillators();
			}*/
			
			/*for(int em = 0; em < Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs.Length; em++)
			{
				
			
			}*/
			
			//toret = toret.Replace("[EACHOSC]", BuildOscillators(sampleid));
			toret = toret.Replace("[EACHGENERATOR]", BuildSoundGenerators(sampleid));
			
			toret = toret.Replace("[EACHPROCESSOR]", BuildProcessors(sampleid));
			//toret = toret.Replace("[EACHMIXER]", BuildMixers(sampleid));
			
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			
			return toret;
		}
		
		/*public string BuildOscillator(int sampleid, int oscid)
		{
			string toret = this.OscTemplate;
			
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs[oscid] == null) { return toret; }
			
			int gtyp = Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs[oscid].WaveType;
			Console.WriteLine("WaveType: {0}", gtyp);
			
			
			if(gtyp == Generator.TypeSine)
			{
				toret = toret.Replace("[GENMACSELTYPESQUARE]", "");
				toret = toret.Replace("[GENMACSELTYPESINE]", " selected");
				toret = toret.Replace("[GENMACSELTYPETRIANGLE]", "");
				toret = toret.Replace("[GENMACSELTYPESAWTOOTH]", "");
			}
			else if(gtyp == Generator.TypeSquare)
			{
				toret = toret.Replace("[GENMACSELTYPESQUARE]", " selected");
				toret = toret.Replace("[GENMACSELTYPESINE]", "");
				toret = toret.Replace("[GENMACSELTYPETRIANGLE]", "");
				toret = toret.Replace("[GENMACSELTYPESAWTOOTH]", "");
			}
			else if(gtyp == Generator.TypeTriangle)
			{
				toret = toret.Replace("[GENMACSELTYPESQUARE]", "");
				toret = toret.Replace("[GENMACSELTYPESINE]", "");
				toret = toret.Replace("[GENMACSELTYPETRIANGLE]", " selected");
				toret = toret.Replace("[GENMACSELTYPESAWTOOTH]", "");
			}
			else if(gtyp == Generator.TypeSawtooth)
			{
				toret = toret.Replace("[GENMACSELTYPESQUARE]", "");
				toret = toret.Replace("[GENMACSELTYPESINE]", "");
				toret = toret.Replace("[GENMACSELTYPETRIANGLE]", "");
				toret = toret.Replace("[GENMACSELTYPESAWTOOTH]", " selected");
			}
			else
			{
				toret = toret.Replace("[GENMACSELTYPESQUARE]", "");
				toret = toret.Replace("[GENMACSELTYPESINE]", "");
				toret = toret.Replace("[GENMACSELTYPETRIANGLE]", "");
				toret = toret.Replace("[GENMACSELTYPESAWTOOTH]", "");
			}
			
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs[oscid].Enabled)
			{
				toret = toret.Replace("[GENMACOSC]", " checked=\"checked\"");
			}
			else
			{
				toret = toret.Replace("[GENMACOSC]", "");
			}
			
			toret = toret.Replace("[GENSAMPLERATE" + "]", Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs[oscid].SampleRate.ToString());
			toret = toret.Replace("[GENERATORMACHINEFREQUENCY]", Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs[oscid].Frequency.ToString());
			toret = toret.Replace("[GENMACHINEAMPLITUDE]", Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs[oscid].Amplitude.ToString());
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			toret = toret.Replace("[OSCID]", oscid.ToString());
			
			return toret;
		}
		
		public string BuildOscillators(int sampleid)
		{
			string toret = "";
			
			if(Engine.TheTrack.Samples == null) { return toret; }
			if(Engine.TheTrack.Samples[sampleid] == null) { return toret; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine == null) { return toret; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitOscillators(); }
			
			for(int eosc = 0; eosc < Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs.Length; eosc++)
			{
				toret = toret + BuildOscillator(sampleid, eosc);
			}
			
			return toret;
		}*/
		
		public string BuildSoundGenerators(int sampleid)
		{
			string toret = "";
			
			if(Engine.TheTrack.Samples == null) { return toret; }
			if(Engine.TheTrack.Samples[sampleid] == null) { return toret; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine == null) { return toret; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Generators == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitGenerators(); }
			
			for(int emix = 0; emix < Engine.TheTrack.Samples[sampleid].WaveMachine.Generators.Length; emix++)
			{
				if(Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[emix] != null)
				{
					string ptype = Engine.TheTrack.Samples[sampleid].WaveMachine.Generators[emix].GetType().ToString();
					
					WebPage procpage = new WebPages.Samples.Generators.Oscillator();
					
					if(ptype.ToLower() == "gurumod.machines.osc")
					{
						procpage = new gurumod.WebPages.Samples.Generators.Oscillator();
						((gurumod.WebPages.Samples.Generators.Oscillator)procpage).SampleID = sampleid;
						((gurumod.WebPages.Samples.Generators.Oscillator)procpage).GeneratorID = emix;
					}
					else if(ptype.ToLower() == "gurumod.machines.wavfile")
					{
						procpage = new gurumod.WebPages.Samples.Generators.WavPlayer();
						((gurumod.WebPages.Samples.Generators.WavPlayer)procpage).SampleID = sampleid;
						((gurumod.WebPages.Samples.Generators.WavPlayer)procpage).GeneratorID = emix;
					}
					
					procpage.InHeaders = base.InHeaders;
					procpage.RequestParts = base.RequestParts;
					procpage.PostVars = base.PostVars;
					
					procpage.Run();
					procpage.TakeTurn();
					toret = toret + procpage.OutgoingBuffer;
				}
				
				//WebPage procpage;
				//gurumod.WebPages.Samples.Generators.Oscillator procpage = new gurumod.WebPages.Samples.Generators.Oscillator();
				/*procpage.InHeaders = base.InHeaders;
				procpage.RequestParts = base.RequestParts;
				procpage.PostVars = base.PostVars;
				procpage.SampleID = sampleid;
				procpage.GeneratorID = egen;
				procpage.Run();
				procpage.TakeTurn();
				toret = toret + procpage.OutgoingBuffer;*/
			}
			
			return toret;
		}
		
		/*public string BuildMixers(int sampleid)
		{
			string toret = "";
			
			if(Engine.TheTrack.Samples == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid] == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitMixers(); }
		
			for(int emix = 0; emix < Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers.Length; emix++)
			{
				toret = toret + BuildMixer(sampleid, emix);
			}
			
			return toret;
		}*/
		
		public string BuildProcessors(int sampleid)
		{
			string toret = "";
			
			if(Engine.TheTrack.Samples == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid] == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Processors == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitProcessors(); }
		
			for(int emix = 0; emix < Engine.TheTrack.Samples[sampleid].WaveMachine.Processors.Length; emix++)
			{
				//Console.WriteLine("BuildProcessors: Sample {0}, Type {1}", sampleid, Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[emix].GetType().ToString());
				//toret = toret + BuildProcessor(sampleid, emix);
				
				if(Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[emix] != null)
				{
					string ptype = Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[emix].GetType().ToString();
					
					WebPage procpage = new WebPages.Samples.Processors.Mixer();
					
					if(ptype.ToLower() == "gurumod.machines.mixer")
					{
						procpage = new gurumod.WebPages.Samples.Processors.Mixer();
						((gurumod.WebPages.Samples.Processors.Mixer)procpage).SampleID = sampleid;
						((gurumod.WebPages.Samples.Processors.Mixer)procpage).ProcessorID = emix;
					}
					else if(ptype.ToLower() == "gurumod.machines.gate")
					{
						procpage = new gurumod.WebPages.Samples.Processors.Gate();
						((gurumod.WebPages.Samples.Processors.Gate)procpage).SampleID = sampleid;
						((gurumod.WebPages.Samples.Processors.Gate)procpage).ProcessorID = emix;
					}
					
					procpage.InHeaders = base.InHeaders;
					procpage.RequestParts = base.RequestParts;
					procpage.PostVars = base.PostVars;
					
					procpage.Run();
					procpage.TakeTurn();
					toret = toret + procpage.OutgoingBuffer;
				}
			}
			
			return toret;
		}
		
		/*public string BuildProcessor(int sampleid, int processorid)
		{
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Processors == null)
			{
				Console.WriteLine("Processors is null :(");
				return "";
			}
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid] == null)
			{
				Console.WriteLine("Processor {0} is null", processorid);
				return "";
			}
			
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[processorid].GetType().ToString().ToLower() == "gurumod.machines.mixer")
			{
				return BuildMixer(sampleid, processorid);
				
			}
			
			return "";
		}*/
		
		/*public string BuildMixer(int sampleid, int mixerid)
		{
			string toret = System.IO.File.ReadAllText(Engine.PFP(ProcessorTemplatePath + "gurumod.machines.mixer.html"));
			string mn = "MIXER" + mixerid.ToString();
			
			if(Engine.TheTrack.Samples == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid] == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Processors == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitProcessors(); }
		
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Processors[mixerid] == null) { return ""; }
			
			toret = toret.Replace("[PROCESSORID]", mixerid.ToString());
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			
			
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MixMethod == MixerSettings.MixMethodAdd)
			{
				toret = toret.Replace("[TYPEADDSELECTED]", " selected");
				toret = toret.Replace("[TYPESUBTRACTSELECTED]", "");
				toret = toret.Replace("[TYPEMULTIPLYSELECTED]", "");
				toret = toret.Replace("[TYPEDIVIDESELECTED]", "");
			}
			else if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MixMethod == MixerSettings.MixMethodMultiply)
			{
				toret = toret.Replace("[TYPEADDSELECTED]", "");
				toret = toret.Replace("[TYPESUBTRACTSELECTED]", "");
				toret = toret.Replace("[TYPEMULTIPLYSELECTED]", " selected");
				toret = toret.Replace("[TYPEDIVIDESELECTED]", "");
			}
			else if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MixMethod == MixerSettings.MixMethodDivide)
			{
				toret = toret.Replace("[TYPEADDSELECTED]", "");
				toret = toret.Replace("[TYPESUBTRACTSELECTED]", "");
				toret = toret.Replace("[TYPEMULTIPLYSELECTED]", "");
				toret = toret.Replace("[TYPEDIVIDESELECTED]", " selected");
			}
			else
			{
				toret = toret.Replace("[TYPEADDSELECTED]", "");
				toret = toret.Replace("[TYPESUBTRACTSELECTED]", " selected");
				toret = toret.Replace("[TYPEMULTIPLYSELECTED]", "");
				toret = toret.Replace("[TYPEDIVIDESELECTED]", "");
			}
			
			return toret;
		}*/
		
		/*public string BuildGateSourceOptions(int sampleid, int mixerid, int iselected)
		{
			string toret = "";
			bool mansel = false;
			if(iselected == -1) { mansel = true; }
			toret = toret + BuildGateSourceOption("-1", mansel, "Manual");
			
			for(int i = 0; i < Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs.Length; i++)
			{
				bool selected = false;
				if(iselected == i) { selected = true; }
				toret = toret + BuildGateSourceOption(i.ToString(), selected, "Oscillator " + i.ToString());
			}
			
			return toret;
		}
		
		public string BuildGateSourceOption(string optionvalue, bool selected, string name)
		{
			string toret = EachGateSrcTemplate;
			if(!selected) { toret = toret.Replace("[SELECTED]", ""); }
			else { toret = toret.Replace("[SELECTED]", " selected"); }
			
			toret = toret.Replace("[VALUE]", optionvalue);
			toret = toret.Replace("[NAME]", name);
			return toret;
		}*/

	
		
		/*public string BuildInputOptions(int sampleid, int mixerid, int iselected, int SourceType)
		{
			string toret = "";
			bool mansel = false;
			//if(iselected == -1) { mansel = true; }
			//toret = toret + BuildGateSourceOption("-1", mansel, "Manual");
			
			for(int i = 0; i < Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs.Length; i++)
			{
				bool selected = false;
				if(iselected == i && SourceType == MixerSettings.SourceTypeOscillator) { selected = true; }
				toret = toret + BuildInputOption("osc" + i.ToString(), selected, "Oscillator " + i.ToString());
			}
			
			for(int i = 0; i < Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers.Length; i++)
			{
				bool selected = false;
				if(iselected == i && SourceType == MixerSettings.SourceTypeMixer) { selected = true; }
				toret = toret + BuildInputOption("mix" + i.ToString(), selected, "Mixer " + i.ToString());
			}
			
			return toret;
		}
		
		public string BuildInputOption(string optionvalue, bool selected, string name)
		{
			string toret = EachGateSrcTemplate;
			if(!selected) { toret = toret.Replace("[SELECTED]", ""); }
			else { toret = toret.Replace("[SELECTED]", " selected"); }
			
			toret = toret.Replace("[VALUE]", optionvalue);
			toret = toret.Replace("[NAME]", name);
			return toret;
		}*/
	}
}