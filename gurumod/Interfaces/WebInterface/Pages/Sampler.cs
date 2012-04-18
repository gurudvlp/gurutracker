
using System;

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
		
		public Sampler()
		{
			
		}
		
		public override bool Run ()
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
			base.OutgoingBuffer = "";
			base.Template = "";
			
			return true;
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
								Oscilator[] tmposc = new Oscilator[Engine.TheTrack.Samples[smpid].WaveMachine.Oscs.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Oscs.Length; eogo++)
								{
									tmposc[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Oscs[eogo];
								}
								
								tmposc[tmposc.Length - 1] = new Oscilator();
								
								Engine.TheTrack.Samples[smpid].WaveMachine.Oscs = tmposc;
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
								MixerSettings[] tmposc = new MixerSettings[Engine.TheTrack.Samples[smpid].WaveMachine.Mixers.Length + 1];
								for(int eogo = 0; eogo < Engine.TheTrack.Samples[smpid].WaveMachine.Mixers.Length; eogo++)
								{
									tmposc[eogo] = Engine.TheTrack.Samples[smpid].WaveMachine.Mixers[eogo];
								}
								
								tmposc[tmposc.Length - 1] = new MixerSettings();
								
								Engine.TheTrack.Samples[smpid].WaveMachine.Mixers = tmposc;
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
								
								if(base.PostVars["TYPE"] == "silence")
								{
									base.Template = "FAIL";
								}
								else if(base.PostVars["TYPE"] == "sine")
								{
									if(base.PostVars.ContainsKey("OSCID"))
									{
										Console.WriteLine("Type set as SINE");
										int oscid = Int32.Parse(base.PostVars["OSCID"]);
										string enabled = "";
										if(base.PostVars.ContainsKey("OENABLED"))
										{
											enabled = base.PostVars["OENABLED"].ToLower();
										}
										
										double frequency = double.Parse(base.PostVars["FREQUENCY"]);
										int samplerate = 44100; //Int32.Parse(base.PostVars["SAMPLERATE"]);
										//int length = Int32.Parse(base.PostVars["LENGTHZ"]);
										//string filename = base.PostVars["FILENAME"];
										
										/*Engine.TheTrack.Samples[sid].UseWaveGenerator = true;
										Engine.TheTrack.Samples[sid].WaveGenerator.Frequency = frequency;
										Engine.TheTrack.Samples[sid].WaveGenerator.SampleRate = samplerate;
										Engine.TheTrack.Samples[sid].WaveGenerator.Length = length;
										Engine.TheTrack.Samples[sid].WaveGenerator.Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
										Engine.TheTrack.Samples[sid].WaveGenerator.WaveType = Generator.TypeSine;
										Engine.TheTrack.Samples[sid].Loaded = false;
										*/
										
										Engine.TheTrack.Samples[sid].UseWaveGenerator = false;
										Engine.TheTrack.Samples[sid].UseWaveMachine = true;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Frequency = frequency;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].SampleRate = samplerate;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].WaveType = Generator.TypeSine;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Amplitude = useamp;
										if(enabled == "true")
										{
											Console.WriteLine("Setting oscillator {0} to enabled.", oscid);
											Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = true;
										}
										else if(enabled == "false")
										{
											Console.WriteLine("Setting oscillator {0} to disabled.", oscid);
											Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = false;
										}
										//Engine.TheTrack.Samples[sid].Loaded = true;
										
										//Engine.TheTrack.Samples[sid].ImportSample(Sample.SineWave(frequency, length, samplerate), OpenTK.Audio.OpenAL.ALFormat.Mono16, samplerate);
										Engine.TheTrack.Samples[sid].Filename = "";
										
										base.Template = "OK";
									}
									else
									{
										base.Template = "FAIL";
									}
								}
								else if(base.PostVars["TYPE"] == "square")
								{
									if(PostVars.ContainsKey("OSCID"))
									{
										Console.WriteLine("Type set as SQUARE");
										
										double frequency = double.Parse(base.PostVars["FREQUENCY"]);
										int samplerate = 44100; //Int32.Parse(base.PostVars["SAMPLERATE"]);
										
										string enabled = "";
										if(base.PostVars.ContainsKey("OENABLED"))
										{
											enabled = base.PostVars["OENABLED"].ToLower();
										}
										//int length = Int32.Parse(base.PostVars["LENGTHZ"]);
										
										/*Engine.TheTrack.Samples[sid].UseWaveGenerator = true;
										Engine.TheTrack.Samples[sid].WaveGenerator.Frequency = frequency;
										Engine.TheTrack.Samples[sid].WaveGenerator.SampleRate = samplerate;
										Engine.TheTrack.Samples[sid].WaveGenerator.Length = length;
										Engine.TheTrack.Samples[sid].WaveGenerator.Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
										Engine.TheTrack.Samples[sid].WaveGenerator.WaveType = Generator.TypeSquare;
										Engine.TheTrack.Samples[sid].Loaded = false;
										*/
										int oscid = Int32.Parse(base.PostVars["OSCID"]);
										
										Engine.TheTrack.Samples[sid].UseWaveGenerator = false;
										Engine.TheTrack.Samples[sid].UseWaveMachine = true;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Frequency = frequency;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].SampleRate = samplerate;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].WaveType = Generator.TypeSquare;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Amplitude = useamp;
										
										if(enabled == "true")
										{
											Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = true;
										}
										else if(enabled == "false")
										{
											Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = false;
										}
										//Engine.TheTrack.Samples[sid].ImportSample(Sample.SineWave(frequency, length, samplerate), OpenTK.Audio.OpenAL.ALFormat.Mono16, samplerate);
										Engine.TheTrack.Samples[sid].Filename = "";
										base.Template = "OK";
									}
									else
									{
										base.Template = "FAIL";
									}
								}
								else if(base.PostVars["TYPE"] == "triangle")
								{
									if(PostVars.ContainsKey("OSCID"))
									{
										Console.WriteLine("Type set as TRIANGLE");
										double frequency = double.Parse(base.PostVars["FREQUENCY"]);
										int samplerate = 44100; //Int32.Parse(base.PostVars["SAMPLERATE"]);
										
										string enabled = "";
										if(base.PostVars.ContainsKey("OENABLED"))
										{
											enabled = base.PostVars["OENABLED"].ToLower();
										}
									
										int oscid = Int32.Parse(base.PostVars["OSCID"]);
										
										Engine.TheTrack.Samples[sid].UseWaveGenerator = false;
										Engine.TheTrack.Samples[sid].UseWaveMachine = true;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Frequency = frequency;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].SampleRate = samplerate;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].WaveType = Generator.TypeTriangle;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Amplitude = useamp;
										
										if(enabled == "true")
										{
											Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = true;
										}
										else if(enabled == "false")
										{
											Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = false;
										}
										//Engine.TheTrack.Samples[sid].ImportSample(Sample.SineWave(frequency, length, samplerate), OpenTK.Audio.OpenAL.ALFormat.Mono16, samplerate);
										Engine.TheTrack.Samples[sid].Filename = "";
										base.Template = "OK";
									}
									else
									{
										base.Template = "FAIL";
									}
								}
								else if(base.PostVars["TYPE"] == "sawtooth")
								{
									if(PostVars.ContainsKey("OSCID"))
									{
										Console.WriteLine("Type set as SAWTOOTH");
										double frequency = double.Parse(base.PostVars["FREQUENCY"]);
										int samplerate = 44100; //Int32.Parse(base.PostVars["SAMPLERATE"]);
										
										string enabled = "";
										if(base.PostVars.ContainsKey("OENABLED"))
										{
											enabled = base.PostVars["OENABLED"].ToLower();
										}
									
										int oscid = Int32.Parse(base.PostVars["OSCID"]);
										
										Engine.TheTrack.Samples[sid].UseWaveGenerator = false;
										Engine.TheTrack.Samples[sid].UseWaveMachine = true;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Frequency = frequency;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].SampleRate = samplerate;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Format = OpenTK.Audio.OpenAL.ALFormat.Mono16;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].WaveType = Generator.TypeSawtooth;
										Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Amplitude = useamp;
										
										if(enabled == "true")
										{
											Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = true;
										}
										else if(enabled == "false")
										{
											Engine.TheTrack.Samples[sid].WaveMachine.Oscs[oscid].Enabled = false;
										}
										//Engine.TheTrack.Samples[sid].ImportSample(Sample.SineWave(frequency, length, samplerate), OpenTK.Audio.OpenAL.ALFormat.Mono16, samplerate);
										Engine.TheTrack.Samples[sid].Filename = "";
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
								//	This form submission did not have 'TYPE' set, so they are
								//	not attempting to update an oscillator.  Let's check for
								//	other stuff.
								
								if(base.PostVars.ContainsKey("MIXERID"))
								{
									Console.WriteLine("TweakMachine: Tweaking mixer");
									int mixerid = Int32.Parse(base.PostVars["MIXERID"]);
									string stinputa = base.PostVars["INPUTA"];
									string stinputb = base.PostVars["INPUTB"];
									int inputtypea = MixerSettings.SourceTypeOscillator;
									int inputtypeb = MixerSettings.SourceTypeOscillator;
									int inputa = 0;//Int32.Parse(base.PostVars["INPUTA"]);
									int inputb = 1;//Int32.Parse(base.PostVars["INPUTB"]);
									
									Console.WriteLine("stInputA,B {0} {1}", stinputa, stinputb);
									if(stinputa.IndexOf("osc") == 0)
									{
										Console.WriteLine("InputA is an oscillator");
										stinputa = stinputa.Replace("osc", "");
										inputa = Int32.Parse(stinputa);
										inputtypea = MixerSettings.SourceTypeOscillator;
									}
									else if(stinputa.IndexOf("mix") == 0)
									{
										Console.WriteLine("InputA is a mixer");
										stinputa = stinputa.Replace("mix", "");
										inputa = Int32.Parse(stinputa);
										inputtypea = MixerSettings.SourceTypeMixer;
									}
									
									if(stinputb.IndexOf("osc") == 0)
									{
										Console.WriteLine("InputB is an oscillator");
										stinputb = stinputb.Replace("osc", "");
										inputb = Int32.Parse(stinputb);
										inputtypeb = MixerSettings.SourceTypeOscillator;
									}
									else if(stinputb.IndexOf("mix") == 0)
									{
										Console.WriteLine("InputB is a mixer");
										stinputb = stinputb.Replace("mix", "");
										inputb = Int32.Parse(stinputb);
										inputtypeb = MixerSettings.SourceTypeMixer;
									}
									
									string outmethod = base.PostVars["COMBMETHOD"];
									int sampleid = Int32.Parse(base.PostVars["SAMPLEID"]);
									double gatemin = double.Parse(base.PostVars["GATEMIN"]);
									double gatemax = double.Parse(base.PostVars["GATEMAX"]);
									int gateminsrc = Int32.Parse(base.PostVars["GATEMINSRC"]);
									int gatemaxsrc = Int32.Parse(base.PostVars["GATEMAXSRC"]);
									
									if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitMixers(); }
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceAID = inputa;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceBID = inputb;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceTypeA = inputtypea;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceTypeB = inputtypeb;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].GateMax = gatemax;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].GateMin = gatemin;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MaxGateControlSource = gatemaxsrc;
									Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MinGateControlSource = gateminsrc;
									
									if(outmethod == "add")
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
									}
									
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
			
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs == null)
			{
				Engine.TheTrack.Samples[sampleid].WaveMachine.InitOscillators();
			}
			
			/*for(int em = 0; em < Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs.Length; em++)
			{
				
			
			}*/
			
			toret = toret.Replace("[EACHOSC]", BuildOscillators(sampleid));
			toret = toret.Replace("[EACHMIXER]", BuildMixers(sampleid));
			
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			
			return toret;
		}
		
		public string BuildOscillator(int sampleid, int oscid)
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
		}
		
		public string BuildMixers(int sampleid)
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
		}
		
		public string BuildMixer(int sampleid, int mixerid)
		{
			string toret = EachMixerTemplate;
			string mn = "MIXER" + mixerid.ToString();
			
			if(Engine.TheTrack.Samples == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid] == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine == null) { return ""; }
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitMixers(); }
		
			if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid] == null) { return ""; }
			
			toret = toret.Replace("[MIXERID]", mixerid.ToString());
			toret = toret.Replace("[SAMPLEID]", sampleid.ToString());
			toret = toret.Replace("[MIXERGATEMIN]", Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].GateMin.ToString());
			toret = toret.Replace("[MIXERGATEMAX]", Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].GateMax.ToString());
			
			toret = toret.Replace("[EACHMINGATESOURCE]", BuildGateSourceOptions(sampleid, mixerid, Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MinGateControlSource));
			toret = toret.Replace("[EACHMAXGATESOURCE]", BuildGateSourceOptions(sampleid, mixerid, Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].MaxGateControlSource));
			
			toret = toret.Replace("[EACHINPUTAOPTION]", BuildInputOptions(sampleid, mixerid, Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceAID, Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceTypeA));
			toret = toret.Replace("[EACHINPUTBOPTION]", BuildInputOptions(sampleid, mixerid, Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceBID, Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceTypeB));
			//[INAOSC0SELECTED]
			//	Temporarty Mixer A testing code
			/*try
			{
				for(int eosc = 0; eosc < Engine.TheTrack.Samples[sampleid].WaveMachine.Oscs.Length; eosc++)
				{
					/* cuttenpasta from the BuildMacine method originally.  leaving commented for debugging
					 * 
					 * if(Engine.TheTrack.Samples == null) { Console.WriteLine("Sample set is null!"); }
					if(Engine.TheTrack.Samples[sampleid] == null) { Console.WriteLine("Sample {0} is null.", sampleid); }
					if(Engine.TheTrack.Samples[sampleid].WaveMachine == null) { Console.WriteLine("The wave machine for sample {0} is null!", sampleid); }
					//*//*
					if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers == null) { Engine.TheTrack.Samples[sampleid].WaveMachine.InitMixers(); }
					
					if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceAID == eosc)
					{
						toret = toret.Replace("[INAOSC" + eosc.ToString() + "SELECTED]", " selected");
					}
					else
					{
						toret = toret.Replace("[INAOSC0SELECTED]", "");
					}
					
					if(Engine.TheTrack.Samples[sampleid].WaveMachine.Mixers[mixerid].SourceBID == eosc)
					{
						toret = toret.Replace("[INBOSC" + eosc.ToString() + "SELECTED]", " selected");
					}
					else
					{
						toret = toret.Replace("[INBOSC0SELECTED]", "");
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Exception while formating mixer html");
				Console.WriteLine(ex.Message);
			}*/
			
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
		}
		
		public string BuildGateSourceOptions(int sampleid, int mixerid, int iselected)
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
		}
		
		public string BuildInputOptions(int sampleid, int mixerid, int iselected, int SourceType)
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
		}
	}
}