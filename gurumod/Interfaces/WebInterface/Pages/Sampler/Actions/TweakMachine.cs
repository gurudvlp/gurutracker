//
//  TweakMachine.cs
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


namespace gurumod.WebPages.Actions
{
	
	public class TweakMachine : WebPage
	{
		public TweakMachine()
		{

		}

		public override bool Run ()
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
					
					OutgoingBuffer = "OK";
					
					
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
						
						OutgoingBuffer = "OK";
					}
					else
					{
						OutgoingBuffer = "FAIL";
					}
				}
			}
			else
			{
				OutgoingBuffer = "FAIL";
			}

			return true;
		}
	}

}