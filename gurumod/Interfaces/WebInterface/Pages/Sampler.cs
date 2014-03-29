
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
				return this.RunSubPage(new WebPages.SampleData());
			}
			else if(base.RequestParts.Length > 1 && base.RequestParts[1].ToLower() == "sampledataxml")
			{
				return this.RunSubPage(new WebPages.SampleDataXML());
			}
			else if(base.RequestParts.Length > 2 && base.RequestParts[1].ToLower() == "machinedataxml")
			{
				return this.RunSubPage(new WebPages.MachineDataXML());
			}
			else if(base.RequestParts.Length > 3 && base.RequestParts[1].ToLower() == "machinegeneratorxml")
			{
				return this.RunSubPage(new WebPages.MachineGeneratorXML());
			}
			else if(base.RequestParts.Length > 3 && base.RequestParts[1].ToLower() == "machineprocessorxml")
			{
				return this.RunSubPage(new WebPages.MachineProcessorXML());
			}
			else
			{
				TerminateOnSend = false;

				/*
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
				*/
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
				//base.Template = BuildSampler();
				Console.WriteLine("BuildSampler() called..  this is obsolete.");
			}
			else
			{
				if(base.RequestParts.Length > 2)
				{
					if(base.RequestParts[1] == "details")
					{
						//Console.WriteLine(base.RequestParts[2]);
						//base.Template = BuildDetails(Int32.Parse(base.RequestParts[2]));
						Console.WriteLine("details subpage called..  this is obsolete.");
					}
					else if(base.RequestParts[1] == "addosc")
					{
						return this.RunSubPage(new WebPages.Actions.AddOsc());

					}
					else if(base.RequestParts[1] == "addmixer")
					{
						return this.RunSubPage(new WebPages.Actions.AddMixer());
					}
					else if(base.RequestParts[1] == "addwaveplayer")
					{
						return this.RunSubPage(new WebPages.Actions.AddWavePlayer());
					}
					else if(base.RequestParts[1] == "addreverb")
					{
						return this.RunSubPage(new WebPages.Actions.AddReverb());
					}
					else if(base.RequestParts[1] == "addgate")
					{
						return this.RunSubPage(new WebPages.Actions.AddGate());
					}
					else if(base.RequestParts[1] == "addenvelope")
					{
						return this.RunSubPage(new WebPages.Actions.AddEnvelope());
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
						//base.Template = BuildSampleGenerator(sampid);
						//	TODO: Convert the sample to a 'Wave Generator'
					}
					else if(base.RequestParts[1] == "genmachine" && sampid > -1)
					{
						//base.Template = BuildMachine(sampid);

						/*if(Engine.TheTrack.Samples[sampid].WaveMachine.Generators == null
						|| Engine.TheTrack.Samples[sampid].WaveMachine.Generators[0] == null)
						{
							Engine.TheTrack.Samples[sampid].WaveMachine.InitGenerators();
						}

						if(Engine.TheTrack.Samples[sampid].WaveMachine.Processors == null
						|| Engine.TheTrack.Samples[sampid].WaveMachine.Processors[0] == null)
						{
							Engine.TheTrack.Samples[sampid].WaveMachine.InitProcessors();
						}*/
						Engine.TheTrack.Samples[sampid].UseWaveMachine = true;
						Engine.TheTrack.Samples[sampid].UseWaveGenerator = false;

						if(Engine.TheTrack.Samples[sampid].WaveMachine == null)
						{
							Console.WriteLine("/sampler/genmachine - Machine is null");
						}
						else
						{
							if(Engine.TheTrack.Samples[sampid].WaveMachine.Processors == null)
							{
								Console.WriteLine("Machine Processors are null");
							}
							else
							{
								Console.WriteLine("Machine has {0} Processors", Engine.TheTrack.Samples[sampid].WaveMachine.Processors.Length);

								for(int ep = 0; ep < Engine.TheTrack.Samples[sampid].WaveMachine.Processors.Length; ep++)
								{
									if(Engine.TheTrack.Samples[sampid].WaveMachine.Processors[ep] == null)
									{
										Console.WriteLine("\tProcessors[{0}] is null", ep);
									}
									else
									{
										Engine.TheTrack.Samples[sampid].WaveMachine.Processors[ep].InitInputs();
										if(Engine.TheTrack.Samples[sampid].WaveMachine.Processors[ep].Inputs == null)
										{
											Console.WriteLine("\tProcessors[{0}].Inputs is null", ep);
										}
										else
										{
											Console.WriteLine("\tProcessors[{0}].Inputs is not null", ep);
										}
									}
								}
							}
						}
					}
					else if(base.RequestParts[1] == "import" && sampid > -1)
					{
						//base.Template = BuildSampleImporter(sampid);
						//	TODO: Set sample as a wave file
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

							if(base.PostVars.ContainsKey("ARTIST"))
							{
								Engine.TheTrack.Samples[sid].Artist = base.PostVars["ARTIST"];
							}

							if(base.PostVars.ContainsKey("YEAR"))
							{
								Engine.TheTrack.Samples[sid].Year = Int32.Parse(base.PostVars["YEAR"]);
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
						return this.RunSubPage(new WebPages.Actions.TweakWavePlayer());
					}
					else if(base.RequestParts[1] == "tweakreverb")
					{
						return this.RunSubPage(new WebPages.Actions.TweakReverb());
					}
					else if(base.RequestParts[1] == "tweakgate")
					{
						return this.RunSubPage(new WebPages.Actions.TweakGate());
					}
					else if(base.RequestParts[1] == "tweakmachine")
					{
						return this.RunSubPage(new WebPages.Actions.TweakMachine());
					}
					else if(base.RequestParts[1] == "tweakenvelope")
					{
						return this.RunSubPage(new WebPages.Actions.TweakEnvelope());
					}
					else if(base.RequestParts[1] == "listwavsamples")
					{
						return this.RunSubPage(new WebPages.Actions.ListWavSamples());
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
		
		/*public string BuildSampler()
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
		}*/
		
		/*public string BuildDetails(int sampleid)
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
		}*/
		
		/*public string BuildSampleImporter(int sampleid)
		{
			string toret = ImportTemplate;
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			toret = toret.Replace("[FILENAME]", Engine.TheTrack.Samples[sampleid].Filename);
			
			return toret;
		}*/
		
		/*public string BuildSampleGenerator(int sampleid)
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
		}*/
		
		/*public string BuildMachine(int sampleid)
		{
			string toret = MachineTemplate;
			

			
			/*if(Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs == null)
			{
				Engine.TheTrack.Samples[sampleid].WaveMachine.InitOscillators();
			}*/
			
			/*for(int em = 0; em < Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs.Length; em++)
			{
				
			
			}*/
			
			//toret = toret.Replace("[EACHOSC]", BuildOscillators(sampleid));
		/*	toret = toret.Replace("[EACHGENERATOR]", BuildSoundGenerators(sampleid));
			
			toret = toret.Replace("[EACHPROCESSOR]", BuildProcessors(sampleid));
			//toret = toret.Replace("[EACHMIXER]", BuildMixers(sampleid));
			
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			
			return toret;
		}*/
		
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

		public bool RunSubPage(WebPage wp)
		{
			wp.IncomingContent = IncomingContent;
			wp.InHeaders = InHeaders;
			wp.PostVars = PostVars;
			wp.RequestParts = RequestParts;
			wp.TerminateOnSend = TerminateOnSend;
			wp.Run();

			TerminateOnSend = wp.TerminateOnSend;
			OutgoingBuffer = wp.OutgoingBuffer;
			OutgoingByteBuffer = wp.OutgoingByteBuffer;
			OutgoingHeaders = wp.OutgoingHeaders;
			ContentType = wp.ContentType;
			UseAsciiOutput = wp.UseAsciiOutput;

			return true;
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