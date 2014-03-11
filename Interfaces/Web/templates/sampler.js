function sampler()
{
	this.CurrentSample = 0;
	this.cactive = 0;
	
	this.Render = function()
	{
		var maintmp = Templates.t("sampler/main.html");
		var samplelstoutline = Templates.t("sampler/samplelist.html");
		var eachsmpltmp = Templates.t("sampler/eachsample.html");
		var details = Templates.t("sampler/details.html");
		
		var fullsl = "";
		for(var es = 0; es < Track.samples.length; es++)
		{
			
			var tsl = eachsmpltmp;
			tsl = Templates.Replace("[SAMPLEID]", es, tsl);
			tsl = Templates.Replace("[SAMPLENAME]", Track.samples[es].title, tsl);
			fullsl = fullsl + tsl;
		}
		
		samplelstoutline = Templates.Replace("[EACHSAMPLE]", fullsl, samplelstoutline);
		maintmp = Templates.Replace("[SAMPLELIST]", samplelstoutline, maintmp);
		
		details = Templates.Replace("[SAMPLEID]", this.CurrentSample, details);
		details = Templates.Replace("[TITLE]", Track.samples[this.CurrentSample].title, details);
		
		if(Track.samples[this.CurrentSample].type == SAMPLE_TYPE_FILE)
		{
			details = Templates.Replace("[LENGTH]", Track.samples[this.CurrentSample].length, details);
			details = Templates.Replace("[BITSPERSAMPLE]", Track.samples[this.CurrentSample].bitrate, details);
			details = Templates.Replace("[SAMPLERATE]", Track.samples[this.CurrentSample].samplerate, details);
			details = Templates.Replace("[FILENAME]", Track.samples[this.CurrentSample].filename, details);
			details = Templates.Replace("[CHANNELS]", Track.samples[this.CurrentSample].channels, details);
		}
		else
		{
			details = Templates.Replace("[LENGTH]", "N/A", details);
			details = Templates.Replace("[BITSPERSAMPLE]", "N/A", details);
			details = Templates.Replace("[SAMPLERATE]", "N/A", details)
			details = Templates.Replace("[FILENAME]", "", details);
			details = Templates.Replace("[CHANNELS]", Track.samples[this.CurrentSample].channels, details);
		}
		
		var workbench = "";
		if(Track.samples[this.CurrentSample].type == SAMPLE_TYPE_FILE) { workbench = this.BuildFileWorkbench(); }
		else if(Track.samples[this.CurrentSample].type == SAMPLE_TYPE_GENERATOR) { workbench = this.BuildGeneratorWorkbench(); }
		else { workbench = this.BuildMachineWorkbench(); }
		
		details = Templates.Replace("[WORKBENCH]", workbench, details);
		
		maintmp = Templates.Replace("[DETAILS]", details, maintmp);
		document.getElementById("page_content").innerHTML = maintmp;
		
		if(Track.samples[this.CurrentSample].type == SAMPLE_TYPE_MACHINE)
		{
			for(var ep = 0; ep < Track.samples[this.CurrentSample].info.processors.length; ep++)
			{
				
				
				
				if(Track.samples[this.CurrentSample].info.processors[ep].proctype == PROC_TYPE_MIXER)
				{
					var campz = Array(
					Track.samples[this.CurrentSample].info.processors[ep].inputs[0].amplitude,
					Track.samples[this.CurrentSample].info.processors[ep].inputs[1].amplitude);
					
					var cmeth = Track.samples[this.CurrentSample].info.processors[ep].method;
					var miname = "sampler_gm_mixer"+ep+"_";
					var onames = Array(
						miname+"add",
						miname+"sub",
						miname+"mul",
						miname+"div");
						
					if(cmeth == MIX_METHOD_ADD) { miname = miname + "add"; }
					else if(cmeth == MIX_METHOD_SUBTRACT) { miname = miname + "sub"; }
					else if(cmeth == MIX_METHOD_MULTIPLY) { miname = miname + "mul"; }
					else { miname = miname + "div"; }
					
					for(var eoic = 0; eoic < onames.length; eoic++)
					{
						var ticon = document.getElementById(onames[eoic]);
						ticon.width = 24;
						ticon.style.borderWidth = "4px";
						ticon.style.borderColor = "#990000";
					}
					
					var methimg = document.getElementById(miname);
					methimg.width = 24;
					methimg.style.borderWidth = "4px";
					methimg.style.borderColor = "#00FF00";
					
					//alert("cmeth: "+cmeth);
					
					for(var ei = 0; ei < 2; ei++)
					{
						this.RenderMixerVolume(ep, ei);
					}
				}
				else if(Track.samples[this.CurrentSample].info.processors[ep].proctype == PROC_TYPE_GATE)
				{
					this.RenderGateManual(ep, 0);
					this.RenderGateManual(ep, 1);
				}
			}
		}
	}
	
	this.ShowSampleImporter = function(sampleid)
	{
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		this.LoadPagePost("/sampler/import", null, postvars);
		Track.samples[sampleid].type = SAMPLE_TYPE_FILE;
		Track.samples[sampleid].info = new sampleifile();
		this.Render();
	}
	
	this.ShowSampleGenerator = function(sampleid)
	{
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		this.LoadPagePost("/sampler/generator", null, postvars);
		
		Track.samples[sampleid].type = SAMPLE_TYPE_GENERATOR;
		Track.samples[sampleid].info = new sampleigenerator();
		this.Render();
	}
	
	this.ShowSoundMachine = function(sampleid)
	{
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		this.LoadPagePost("/sampler/genmachine", null, postvars);
		
		Track.samples[sampleid].type = SAMPLE_TYPE_MACHINE;
		Track.samples[sampleid].info = new sampleimachine();
		
		Track.samples[sampleid].info.oscillators.push(new machineoscillator());
		
		var nmx = new processorimixer();
		nmx.proctype = PROC_TYPE_MIXER;
		nmx.method = MIX_METHOD_ADD;
		
		var ins = new processorinputdata();
		var dualins = Array(ins, ins);
		nmx.inputs = dualins;
		
		Track.samples[sampleid].info.processors.push(nmx);
		this.Render();
		
		//this.AddMixer(sampleid);
		/*var nmix = new processorimixer();
		var nminput = new processorinputdata();
		nminput.sourcetype = SOURCE_TYPE_OSC;
		nminput.sourceid = 0;
		nminput.amplitude = 1;
		
		nmix.inputs.push(nminput);
		nmix.inputs.push(nminput);
		Track.samples[sampleid].info.processors.push(nmix);
		this.Render();*/
	}
	
	this.SaveSample = function(sampleid)
	{
		var postvars = Array();
		postvars["title"] = (document.getElementById("sampler_details_title")).value;
		postvars["filename"] = (document.getElementById("sampler_details_filename")).value;
		postvars["sampleid"] = sampleid;
		this.LoadPagePost("/sampler/updatesample", null, postvars);
		//sampler_list_[SAMPLEID]_name
		document.getElementById("sampler_list_" + sampleid + "_name").innerHTML = postvars["title"];
		Track.samples[sampleid].title = postvars["title"];
	}
	
	this.RenderMixerVolume = function(mixerid, inputid)
	{
		
		var ep = mixerid;
		var ei = inputid;
		var campz = Track.samples[this.CurrentSample].info.processors[ep].inputs[inputid].amplitude;
		
		var cvba = document.getElementById("sampler_gm_processor"+ep+"_input"+ei+"amp");
		var atx = cvba.getContext("2d");
		atx.fillStyle = "#A0A0A0";
		atx.fillRect(0, 0, 199, 19);
		var atxlen = parseInt(parseInt(campz * 200));
		//atx = cvba.getContext("2d");
		atx.fillStyle = "#0000D0";
		atx.fillRect(0, 0, atxlen, 19);
		
		atx.fillStyle = "#00C000";
		atx.font = "12px Arial";
		atx.fillText(parseInt(campz * 100)+"%", 3, 16);
		
		cvba.addEventListener("click", function(evt)
		{
			var rect = cvba.getBoundingClientRect();
			var mx = evt.clientX - rect.left;
			var my = evt.clientY - rect.top;
			
			var namp = parseInt(mx / 2) / 100;
			
			Track.samples[Sampler.CurrentSample].info.processors[ep].inputs[ei].amplitude = namp;
			Sampler.TweakSoundMixer(Sampler.CurrentSample, ep, "");
			
			Sampler.RenderMixerVolume(ep, ei);
		}, false);
	}
	
	this.RenderGateManual = function(gateid, minmaxid)
	{
		var minman = Track.samples[this.CurrentSample].info.processors[gateid].mingatemanual;
		var maxman = Track.samples[this.CurrentSample].info.processors[gateid].maxgatemanual;
		var manman = minman;
		
		var gateobj = "sampler_gm_processor"+gateid+"_";
		if(minmaxid == 0) { gateobj = gateobj + "mingateman"; manman = minman;}
		else { gateobj = gateobj + "maxgateman"; manman = maxman; }
		var manlen = parseInt(manman * 2 * 100);
		
		var gatectr = document.getElementById(gateobj);
		var atx = gatectr.getContext("2d");
		
		atx.fillStyle = "#A0A0A0";
		atx.fillRect(0, 0, 199, 19);
		
		atx.fillStyle = "#0000D0";
		atx.fillRect(0, 0, manlen, 19);
		
		atx.fillStyle = "#00C000";
		atx.font = "12px Arial";
		atx.fillText(parseInt(manlen / 2), 3, 16);
		
		gatectr.addEventListener("click", function(evt)
		{
			var rect = gatectr.getBoundingClientRect();
			var mx = evt.clientX - rect.left;
			var my = evt.clientY - rect.top;
			
			var ngate = parseInt(mx / 2) / 100;
			
			if(minmaxid == 0) { Track.samples[Sampler.CurrentSample].info.processors[gateid].mingatemanual = ngate; }
			else { Track.samples[Sampler.CurrentSample].info.processors[gateid].maxgatemanual = ngate; }
			
			Sampler.TweakSoundGate(Sampler.CurrentSample, gateid, gateid);
			Sampler.RenderGateManual(gateid, minmaxid);
		}, false);
	}
	
	this.BuildFileWorkbench = function()
	{
		var fftmp = Templates.t("sampler/importfile.html");
		fftmp = Templates.Replace("[FILENAME]", Track.samples[this.CurrentSample].filename, fftmp);
		fftmp = Templates.Replace("[SAMPLEID]", this.CurrentSample, fftmp);
		return fftmp;
	}
	
	this.BuildGeneratorWorkbench = function()
	{
		var fftmp = Templates.t("sampler/generator.html");
		fftmp = Templates.Replace("[SAMPLEID]", this.CurrentSample, fftmp);
		
		if(Track.samples[this.CurrentSample].wavetype == WAVE_TYPE_SINE)
		{
			fftmp = Templates.Replace("[GENSELTYPESINE]", " selected=selected", fftmp);
			fftmp = Templates.Replace("[GENSELTYPESQUARE]", "", fftmp);
		}
		else
		{
			fftmp = Templates.Replace("[GENSELTYPESINE]", "", fftmp);
			fftmp = Templates.Replace("[GENSELTYPESQUARE]", " selected=selected", fftmp);
		}
		
		fftmp = Templates.Replace("[GENSAMPLERATE]", Track.samples[this.CurrentSample].samplerate, fftmp);
		fftmp = Templates.Replace("[GENERATORFREQUENCY]", Track.samples[this.CurrentSample].frequency, fftmp);
		
		
		return fftmp;
	}
	
	this.BuildMachineWorkbench = function()
	{
		var gmtmp = Templates.t("sampler/genmachine.html");
		var osctmp = Templates.t("sampler/generators/gurumod.machines.osc.html");
		var wavtmp = Templates.t("sampler/generators/gurumod.machines.wavfile.html");
		var gatetmp = Templates.t("sampler/processors/gurumod.machines.gate.html");
		var mixertmp = Templates.t("sampler/processors/gurumod.machines.mixer.html");
		var reverbtmp = Templates.t("sampler/processors/gurumod.machines.reverb.html");
		var envelopetmp = Templates.t("sampler/processors/gurumod.machines.envelope.html");
		
		gmtmp = Templates.Replace("[SAMPLEID]", this.CurrentSample, gmtmp);
		
		var fullgens = "";
		for(var eg = 0; eg < Track.samples[this.CurrentSample].info.oscillators.length; eg++)
		{
			
			if(Track.samples[this.CurrentSample].info.oscillators[eg].generatortype == GENERATOR_TYPE_OSC)
			{
				var tgen = osctmp;
				tgen = Templates.Replace("[GENERATORID]", eg, tgen);
				var wtype = Track.samples[this.CurrentSample].info.oscillators[eg].wavetype;
				
				var gentypeiconname = "sine";
				if(wtype == WAVE_TYPE_SINE) { gentypeiconname = "sine"; tgen = Templates.Replace("[GENSELSINE]", " selected=selected", tgen); }
				else if(wtype == WAVE_TYPE_SQUARE) { gentypeiconname = "square"; tgen = Templates.Replace("[GENSELSQUARE]", " selected=selected", tgen); }
				else if(wtype == WAVE_TYPE_TRIANGLE) { gentypeiconname="triangle"; tgen = Templates.Replace("[GENSELTRIANGLE]", " selected=selected", tgen); }
				else if(wtype == WAVE_TYPE_SAWTOOTH) { gentypeiconname="sawtooth"; tgen = Templates.Replace("[GENSELSAWTOOTH]", " selected=selected", tgen); }
				
				tgen = Templates.Replace("[GENSELSINE]", "", tgen);
				tgen = Templates.Replace("[GENSELSQUARE]", "", tgen);
				tgen = Templates.Replace("[GENSELTRIANGLE]", "", tgen);
				tgen = Templates.Replace("[GENSELSAWTOOTH]", "", tgen);
				
				tgen = Templates.Replace("[ICONNAME]", gentypeiconname, tgen);
				
				if(Track.samples[this.CurrentSample].info.oscillators[eg].enabled == 1) { tgen = Templates.Replace("[GENENABLED]", " checked=checked", tgen); }
				else { tgen = Templates.Replace("[GENENABLED]", "", tgen); }
				
				tgen = Templates.Replace("[GENERATORMACHINEFREQUENCY]", Track.samples[this.CurrentSample].info.oscillators[eg].frequency, tgen);
				tgen = Templates.Replace("[GENMACHINEAMPLITUDE]", Track.samples[this.CurrentSample].info.oscillators[eg].amplitude, tgen);
				tgen = Templates.Replace("[SAMPLEID]", this.CurrentSample, tgen);
				
				fullgens = fullgens + tgen;
			}
			else if(Track.samples[this.CurrentSample].info.oscillators[eg].generatortype == GENERATOR_TYPE_WAVE)
			{
				var tgen = wavtmp;
				tgen = Templates.Replace("[GENERATORID]", eg, tgen);
				
				if(Track.samples[this.CurrentSample].info.oscillators[eg].enabled == 1) { tgen = Templates.Replace("[GENENABLED]", " checked=checked", tgen); }
				else { tgen = Templates.Replace("[GENENABLED]", "", tgen); }
				
				tgen = Templates.Replace("[FILENAME]", Track.samples[this.CurrentSample].info.oscillators[eg].details.filename, tgen);
				tgen = Templates.Replace("[SAMPLEID]", this.CurrentSample, tgen);
				tgen = Templates.Replace("[TIME]", Track.samples[this.CurrentSample].info.oscillators[eg].details.length, tgen);
				tgen = Templates.Replace("[SAMPLERATE]", Track.samples[this.CurrentSample].info.oscillators[eg].details.samplerate, tgen);
				
				fullgens = fullgens + tgen;
			}
			else
			{
				alert("Unknown generator type: "+Track.samples[this.CurrentSample].info.oscillators[eg].generatortype);
			}
		}
		
		gmtmp = Templates.Replace("[EACHGENERATOR]", fullgens, gmtmp);
		
		var fullprocs = "";
		for(var ep = 0; ep < Track.samples[this.CurrentSample].info.processors.length; ep++)
		{
			var prtype = Track.samples[this.CurrentSample].info.processors[ep].proctype;
			var tproc = "";
			var upfunc = "";
			var upelms = null;
			
			if(prtype == PROC_TYPE_MIXER)
			{
				tproc = mixertmp;
				tproc = Templates.Replace("[PROCESSORID]", ep, tproc);
				
				upfunc = "Sampler.TweakSoundMixer";
				upelms = Array(this.CurrentSample, ep, ep);
				
				var cmeth = Track.samples[this.CurrentSample].info.processors[ep].method;
				var campa = Track.samples[this.CurrentSample].info.processors[ep].inputs[0].amplitude;
				var campb = Track.samples[this.CurrentSample].info.processors[ep].inputs[1].amplitude;
				
				if(cmeth == MIX_METHOD_ADD) { tproc = Templates.Replace("[TYPEADDSELECTED]", " selected=selected", tproc); }
				if(cmeth == MIX_METHOD_SUBTRACT) { tproc = Templates.Replace("[TYPESUBTRACTSELECTED]", " selected=selected", tproc); }
				if(cmeth == MIX_METHOD_MULTIPLY) { tproc = Templates.Replace("[TYPEMULTIPLYSELECTED]", " selected=selected", tproc); }
				if(cmeth == MIX_METHOD_DIVIDE) { tproc = Templates.Replace("[TYPEDIVIDESELECTED]", " selected=selected", tproc); }
				
				tproc = Templates.Replace("[TYPEADDSELECTED]", "", tproc);
				tproc = Templates.Replace("[TYPESUBTRACTSELECTED]", "", tproc);
				tproc = Templates.Replace("[TYPEMULTIPLYSELECTED]", "", tproc);
				tproc = Templates.Replace("[TYPEDIVIDESELECTED]", "", tproc);
				
				tproc = Templates.Replace("[SAMPLEID]", this.CurrentSample, tproc);
				tproc = Templates.Replace("[INPUTAMPLITUDE:0]", campa, tproc);
				tproc = Templates.Replace("[INPUTAMPLITUDE:1]", campb, tproc);
				
				
				
			}
			else if(prtype == PROC_TYPE_GATE)
			{
				upfunc = "Sampler.TweakSoundGate";
				upelms = Array(this.CurrentSample, ep, ep);
				
				tproc = gatetmp;
				tproc = Templates.Replace("[PROCESSORID]", ep, tproc);
				tproc = Templates.Replace("[MINGATE]", Track.samples[this.CurrentSample].info.processors[ep].mingatemanual, tproc);
				tproc = Templates.Replace("[MAXGATE]", Track.samples[this.CurrentSample].info.processors[ep].maxgatemanual, tproc);
				tproc = Templates.Replace("[SAMPLEID]", this.CurrentSample, tproc);
			}
			else if(prtype == PROC_TYPE_REVERB)
			{
				tproc = reverbtmp;
				tproc = Templates.Replace("[PROCESSORID]", ep, tproc);
				tproc = Templates.Replace("[SAMPLEID]", this.CurrentSample, tproc);
			}
			else if(prtype == PROC_TYPE_ENVELOPE)
			{
				upfunc = "Sampler.TweakEnvelope";
				upelms = Array(this.CurrentSample, ep, ep);
				
				tproc = envelopetmp;
				tproc = Templates.Replace("[PROCESSORID]", ep, tproc);
				tproc = Templates.Replace("[SAMPLEID]", this.CurrentSample, tproc);
				tproc = Templates.Replace("[ATTACK]", Track.samples[this.CurrentSample].info.processors[ep].attack, tproc);
				tproc = Templates.Replace("[DECAY]", Track.samples[this.CurrentSample].info.processors[ep].decay, tproc);
				tproc = Templates.Replace("[SUSTAIN]", Track.samples[this.CurrentSample].info.processors[ep].sustain, tproc);
				tproc = Templates.Replace("[RELEASE]", Track.samples[this.CurrentSample].info.processors[ep].release, tproc);
				tproc = Templates.Replace("[ATTACKAMP]", Track.samples[this.CurrentSample].info.processors[ep].attackamp, tproc);
				tproc = Templates.Replace("[DECAYAMP]", Track.samples[this.CurrentSample].info.processors[ep].decayamp, tproc);
			}
			
			for(var ein = 0; ein < Track.samples[this.CurrentSample].info.processors[ep].inputs.length; ein++)
			{
				tproc = Templates.Replace("[SELECTINPUT:"+ein+"]", this.BuildInputList(ep, ein, upfunc, upelms), tproc);
			}
			
			fullprocs = fullprocs + tproc;
		}
		
		gmtmp = Templates.Replace("[EACHPROCESSOR]", fullprocs, gmtmp);
		
		return gmtmp;
	}
	
	this.OscillatorIconName = function(oscid)
	{
		var osct = Track.samples[this.CurrentSample].info.oscillators[oscid].wavetype;
		
		if(osct == WAVE_TYPE_SAWTOOTH) { return "sawtooth"; }
		else if(osct == WAVE_TYPE_SQUARE) { return "square"; }
		else if(osct == WAVE_TYPE_TRIANGLE) { return "triangle"; }
		else { return "sine"; }
	}
	
	this.BuildInputList = function(procid, inputid, updatefunc, funcelms)
	{
		var inopttmp = Templates.t("sampler/processors/inputoption.html");
		var inseltmp = Templates.t("sampler/processors/inputselect.html");
		var csel = Track.samples[this.CurrentSample].info.processors[procid].inputs[inputid].sourceid;
		var ctype = Track.samples[this.CurrentSample].info.processors[procid].inputs[inputid].sourcetype;
		var proctype = Track.samples[this.CurrentSample].info.processors[procid].proctype;
		
		var allopts = "";
		for(var eo = 0; eo < Track.samples[this.CurrentSample].info.oscillators.length; eo++)
		{
			var topt = inopttmp;
			topt = Templates.Replace("[VALUE]", "gen"+eo, topt);
			topt = Templates.Replace("[NAME]", "Oscillator " + eo, topt);
			
			if(ctype == SOURCE_TYPE_OSC && eo == csel) { topt = Templates.Replace("[SELECTED]", " selected=selected", topt); }
			else { topt = Templates.Replace("[SELECTED]", "", topt); }
			
			allopts = allopts + topt;
		}
		
		for(var eo = 0; eo < Track.samples[this.CurrentSample].info.processors.length; eo++)
		{
			var topt = inopttmp;
			topt = Templates.Replace("[VALUE]", "proc"+eo, topt);
			topt = Templates.Replace("[NAME]", "Processor " + eo, topt);
			
			if(ctype == SOURCE_TYPE_PROC && eo == csel) { topt = Templates.Replace("[SELECTED]", " selected=selected", topt); }
			else { topt = Templates.Replace("[SELECTED]", "", topt); }
			
			allopts = allopts + topt;
		}
		
		if(proctype == PROC_TYPE_GATE)
		{
			var topt = inopttmp;
			topt = Templates.Replace("[VALUE]", "-1", topt);
			topt = Templates.Replace("[NAME]", "Manual", topt);
			
			if(eo < 0) { topt = Templates.Replace("[SELECTED]", " selected=selected", topt); }
			
			allopts = allopts + topt;
		}
		
		inseltmp = Templates.Replace("[EACHINPUTOPTION]", allopts, inseltmp);
		inseltmp = Templates.Replace("[PROCESSORID]", procid, inseltmp);
		inseltmp = Templates.Replace("[INPUTID]", inputid, inseltmp);
		
		if(updatefunc == null || updatefunc == "")
		{
			inseltmp = Templates.Replace("[ONCHANGE]", "", inseltmp);
		}
		else
		{
			var fullfnc = "javascript:"+updatefunc+"(";
			
			if(funcelms != null && funcelms.length > 0)
			{
				for(var eelm = 0; eelm < funcelms.length; eelm++)
				{
					
					fullfnc = fullfnc+"'"+funcelms[eelm]+"'";
					if(eelm < funcelms.length - 1) { fullfnc = fullfnc + ", "; }
				}
			}
			
			fullfnc = fullfnc + ");";
			
			inseltmp = Templates.Replace("[ONCHANGE]", fullfnc, inseltmp);
		}
		
		return inseltmp;
	}
	
	this.TweakOscillator = function(sampleid, oscid)
	{
		var ec = oscid;
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		//alert("hi");
		var tmp = document.getElementById("sampler_genmachine_type" + ec);
		//postvars["type"] = tmp.options[tmp.selectedIndex].value;
		var tint = parseInt(Track.samples[sampleid].info.oscillators[oscid].wavetype, 10);
		
		if(tint == WAVE_TYPE_SAWTOOTH) { postvars["type"] = "sawtooth"; }
		else if(tint == WAVE_TYPE_TRIANGLE) { postvars["type"] = "triangle"; }
		else if(tint == WAVE_TYPE_SQUARE) { postvars["type"] = "square"; }
		else { postvars["type"] = "sine"; }
		
		postvars["frequency"] = (document.getElementById("sampler_genmachine_frequency" + ec)).value;
		postvars["amplitude"] = (document.getElementById("sampler_genmachine_amplitude" + ec)).value;
		postvars["oscid"] = oscid;
		
		if((document.getElementById("sampler_genmachine_usegen" + ec)).checked == true)
		{
			postvars["oenabled"] = "true";
			Track.samples[sampleid].info.oscillators[ec].enabled = 1;
		}
		else
		{
			postvars["oenabled"] = "false";
			Track.samples[sampleid].info.oscillators[ec].enabled = 0;
		}
		
		//Track.samples[sampleid].info.oscillators[ec].wavetype = postvars["type"];
		Track.samples[sampleid].info.oscillators[ec].frequency = postvars["frequency"];
		Track.samples[sampleid].info.oscillators[ec].amplitude = postvars["amplitude"];
		
		
		this.LoadPagePost("/sampler/tweakmachine", null, postvars);
		
	}
	
	this.LoadPagePost = function(url, output, postvars)
	{
		if(this.cactive == 1)
		{
			setTimeout("Sampler.LoadPagePost('"+url+"', '"+output+"', '"+postvars+"')", 600);
			return;
		}

		this.cactive = 1;

		var xo;
		
		if (window.XMLHttpRequest) { xo = new XMLHttpRequest(); }// code for IE7+, Firefox, Chrome, Opera, Safari
		else { xo = new ActiveXObject("Microsoft.XMLHTTP"); }// code for IE6, IE5

		xo.open("POST", url, false);
		
		var postbody = "";
		for(var eitem in postvars)
		{
			if(postbody != "") { postbody = postbody + "&"; }
			postbody = postbody + encodeURIComponent(eitem) + "=" + encodeURIComponent(postvars[eitem]);
		}
		
		//Send the proper header information along with the request
		xo.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
		xo.setRequestHeader("Content-length", postbody.length);
		xo.setRequestHeader("Connection", "close");
		
		xo.send(postbody);

		if(output == null)
		{
			//	The function was directed to not display the output.
		}
		else
		{
			document.getElementById(output).innerHTML = xo.responseText;
		}

		this.cactive = 0;
	}
	
	this.LoadPage = function(url, output)
	{
		if(this.cactive == 1)
		{
			setTimeout("Sampler.LoadPage('"+url+"', '"+output+"')", 600);
			return;
		}

		this.cactive = 1;

		var xo;
		if (window.XMLHttpRequest) { xo = new XMLHttpRequest(); } // code for IE7+, Firefox, Chrome, Opera, Safari
		else { xo = new ActiveXObject("Microsoft.XMLHTTP"); } // code for IE6, IE5

		xo.open("GET", url, false);
		xo.send(null);

		if(output == null)
		{
			//	Function was directed to not display the output anywhere.
		}
		else
		{
			document.getElementById(output).innerHTML = xo.responseText;
		}
		
		this.cactive = 0;
		
	}
	
	this.AddOscillator = function(sampleid)
	{
		this.LoadPage("/sampler/addosc/"+sampleid, null);
		
		Track.samples[this.CurrentSample].info.oscillators.push(new machineoscillator());
		this.Render();
	}
	
	this.AddMixer = function(sampleid)
	{
		this.LoadPage("/sampler/addmixer/" + sampleid, null);
		
		var nmx = new processorimixer();
		nmx.proctype = PROC_TYPE_MIXER;
		nmx.method = MIX_METHOD_ADD;
		
		var ins = new processorinputdata();
		var dualins = Array(ins, ins);
		nmx.inputs = dualins;
		
		Track.samples[sampleid].info.processors.push(nmx);
		this.Render();

	}
	
	this.AddReverb = function(sampleid)
	{
		this.LoadPage("/sampler/addreverb/"+sampleid, null);
		
		var nmx = new processorireverb();
		nmx.proctype = PROC_TYPE_REVERB;
		nmx.inputs = Array(new processorinputdata());
		
		Track.samples[sampleid].info.processors.push(nmx);
		this.Render();
	}
	
	this.AddGate = function(sampleid)
	{
		this.LoadPage("/sampler/addgate/"+sampleid, null);
		var nmx = new processorigate();
		nmx.proctype = PROC_TYPE_GATE;
		nmx.inputs = Array(new processorinputdata(), new processorinputdata(), new processorinputdata());
		
		this.Render();
	}
	
	this.AddEnvelope = function(sampleid)
	{
		this.LoadPage("/sampler/addenvelope/"+sampleid, null);
		var nmx = new processorienvelope();
		nmx.inputs = Array(new processorinputdata());
		Track.samples[sampleid].info.processors.push(nmx);
		
		this.Render();
	}
	
	this.RotateOscWaveType = function(sampleid, oscid)
	{
		var cwt = parseInt(Track.samples[sampleid].info.oscillators[oscid].wavetype, 10);
		cwt = cwt + 1;
		if(cwt > 3) { cwt = 0; }
		Track.samples[sampleid].info.oscillators[oscid].wavetype = cwt;
		
		//alert("sid: "+sampleid+"\noscid: "+oscid+"\ncwt: "+cwt);
		this.TweakOscillator(sampleid, oscid);
		
		var oimg = document.getElementById("sampler_genmachine_gentypeimg"+oscid);
		oimg.src = "/images/oscillators/"+this.OscillatorIconName(oscid)+".png";
		
		//this.Render();
	}
	
	this.TweakSoundGate = function(sampleid, mixerid, mixerlabel)
	{
		
		var ec = mixerid;
		
		var mingatediv = document.getElementById("sampler_gm_processor"+mixerid+"_mingatectrl");
		var maxgatediv = document.getElementById("sampler_gm_processor"+mixerid+"_maxgatectrl");
		
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		postvars["processorid"] = mixerid;
		
		//postvars["gatemin"] = (document.getElementById("sampler_genmachine_processor" + ec + "_mingateman")).value;
		//postvars["gatemax"] = (document.getElementById("sampler_genmachine_processor" + ec + "_maxgateman")).value;
		postvars["gatemin"] = Track.samples[sampleid].info.processors[ec].mingateman;
		postvars["gatemax"] = Track.samples[sampleid].info.processors[ec].maxgateman;
		
		
		var tmp = document.getElementById("sampler_gm_processor" + ec + "_input0");
		postvars["input0"] = tmp.options[tmp.selectedIndex].value;
		
		tmp = document.getElementById("sampler_gm_processor" + ec + "_input1");
		postvars["input1"] = tmp.options[tmp.selectedIndex].value;
		
		tmp = document.getElementById("sampler_gm_processor" + ec + "_input2");
		postvars["input2"] = tmp.options[tmp.selectedIndex].value;
		
		Track.samples[sampleid].info.processors[ec].inputs[0].UpdateFromString(postvars["input0"]);
		Track.samples[sampleid].info.processors[ec].inputs[1].UpdateFromString(postvars["input1"]);
		Track.samples[sampleid].info.processors[ec].inputs[2].UpdateFromString(postvars["input2"]);
		
		if(postvars["input1"] == -1)
		{
			mingatediv.style.display = "block";
		}
		else { mingatediv.style.display = "none"; }
		
		if(postvars["input2"] == -1) { maxgatediv.style.display = "block"; }
		else { maxgatediv.style.display = "none"; }
		
		this.LoadPagePost("/sampler/tweakgate", null, postvars);
		
		//alert("tweaked gate");
	}
	
	this.SetMixerMethod = function(sampleid, procid, method)
	{
		Track.samples[sampleid].info.processors[procid].method = method;
		this.TweakSoundMixer(sampleid, procid, procid);
		
		var mtypes = Array("add", "sub", "mul", "div");
		var umeth = 0;
		if(method == "add") { umeth = 0; }
		else if(method == "subtract") { umeth = 1; }
		else if(method == "multiply") { umeth = 2; }
		else { umeth = 3; }
		
		for(var emt = 0; emt < mtypes.length; emt++)
		{
			var methimg = document.getElementById("sampler_gm_mixer"+procid+"_"+mtypes[emt]);
			methimg.style.borderWidth = "4px";
			methimg.style.borderColor = "#990000";
			methimg.width = 24;
		}
		
		var hlt = document.getElementById("sampler_gm_mixer"+procid+"_"+mtypes[umeth]);
		//hlt.style.borderWidth = "2px";
		//hlt.width = "64";
		hlt.style.borderWidth = "4px";
		hlt.style.borderColor = "#00FF00";
		hlt.width = 24;
	}
	
	this.TweakEnvelope = function(sampleid, procid, proclabel)
	{
		var ec = procid;
		//alert(ec);
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		postvars["processorid"] = procid;
		
		var tmp = document.getElementById("sampler_gm_processor"+ec+"_input0");
		postvars["input0"] = tmp.options[tmp.selectedIndex].value;
		
		postvars["attack"] = document.getElementById("sampler_gm_processor"+ec+"_attack").value;
		postvars["decay"] = document.getElementById("sampler_gm_processor"+ec+"_decay").value;
		postvars["sustain"] = document.getElementById("sampler_gm_processor"+ec+"_sustain").value;
		postvars["release"] = document.getElementById("sampler_gm_processor"+ec+"_release").value;
		postvars["attackamp"] = document.getElementById("sampler_gm_processor"+ec+"_attackamp").value;
		postvars["decayamp"] = document.getElementById("sampler_gm_processor"+ec+"_decayamp").value;
		
		Track.samples[sampleid].info.processors[procid].attack = postvars["attack"];
		Track.samples[sampleid].info.processors[procid].decay = postvars["decay"];
		Track.samples[sampleid].info.processors[procid].sustain = postvars["sustain"];
		Track.samples[sampleid].info.processors[procid].release = postvars["release"];
		Track.samples[sampleid].info.processors[procid].attackamp = postvars["attackamp"];
		Track.samples[sampleid].info.processors[procid].decayamp = postvars["decayamp"];
		
		this.LoadPagePost("/sampler/tweakenvelope", null, postvars);
	}
	
	this.TweakSoundMixer = function(sampleid, mixerid, mixerlabel)
	{
		
		var ec = mixerid;
		
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		postvars["mixerid"] = mixerid;
		
		//postvars["gatemin"] = (document.getElementById("sampler_gm_mixer" + ec + "_gatemin")).value;
		//postvars["gatemax"] = (document.getElementById("sampler_gm_mixer" + ec + "_gatemax")).value;
		
		//tmp = document.getElementById("sampler_gm_mixer" + ec + "_gateminsrc");
		//postvars["gateminsrc"] = tmp.options[tmp.selectedIndex].value;
		//tmp = document.getElementById("sampler_gm_mixer" + ec + "_gatemaxsrc");
		//postvars["gatemaxsrc"] = tmp.options[tmp.selectedIndex].value;
		
		var tmp = document.getElementById("sampler_gm_processor" + ec + "_input0");
		postvars["input0"] = tmp.options[tmp.selectedIndex].value;
		
		tmp = document.getElementById("sampler_gm_processor" + ec + "_input1");
		postvars["input1"] = tmp.options[tmp.selectedIndex].value;
		
		postvars["input0amp"] = Track.samples[sampleid].info.processors[ec].inputs[0].amplitude;
		postvars["input1amp"] = Track.samples[sampleid].info.processors[ec].inputs[1].amplitude;
		
		tmp = document.getElementById("sampler_gm_processor" + ec + "_type");
		//postvars["combmethod"] = tmp.options[tmp.selectedIndex].value;
		postvars["combmethod"] = Track.samples[sampleid].info.processors[ec].method;
		
		Track.samples[sampleid].info.processors[ec].inputs[0].UpdateFromString(postvars["input0"]);
		Track.samples[sampleid].info.processors[ec].inputs[1].UpdateFromString(postvars["input1"]);
		Track.samples[sampleid].info.processors[ec].method=postvars["combmethod"];
		
		this.LoadPagePost("/sampler/tweakmachine", null, postvars);
		
	}
	
	this.TweakSoundMachine = function(sampleid, oscid)
	{
		var ec = oscid;
		//alert("TWM: Sampleid: "+sampleid+" :: oscid: "+ec);
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		//alert("hi");
		var tmp = document.getElementById("sampler_genmachine_type" + ec);
		postvars["type"] = tmp.options[tmp.selectedIndex].value;
		//postvars["type"] = Track.samples[sampleid].info.oscillators[ec].wavetype;
		postvars["frequency"] = (document.getElementById("sampler_genmachine_frequency" + ec)).value;
		postvars["amplitude"] = (document.getElementById("sampler_genmachine_amplitude" + ec)).value;
		postvars["oscid"] = oscid;
		
		
		if((document.getElementById("sampler_genmachine_useosc" + ec)).checked == true)
		{
			postvars["oenabled"] = "true";
			Track.samples[sampleid].info.oscillators[ec].enabled = 1;
		}
		else
		{
			postvars["oenabled"] = "false";
			Track.samples[sampleid].info.oscillators[ec].enabled = 0;
		}
		//alert("test");
		
		Track.samples[sampleid].info.oscillators[ec].wavetype = postvars["type"];
		Track.samples[sampleid].info.oscillators[ec].frequency = postvars["frequency"];
		Track.samples[sampleid].info.oscillators[ec].amplitude = postvars["amplitude"];
		this.LoadPagePost("/sampler/tweakmachine", null, postvars);
	}
	
	this.ShowDetails = function(sampleid)
	{
		this.CurrentSample = sampleid;
		this.Render();
	}
	
	this.ToggleMinimizeProcessor = function(procid)
	{
		var prcont = document.getElementById("sampler_genmachine_processor"+procid+"_content");
		var pricon = document.getElementById("sampler_genmachine_processor"+procid+"_minicon");
		
		var curdisp = prcont.style.display;
		
		if(curdisp == "" || curdisp == "block")
		{
			prcont.style.display = "none";
			pricon.src = "/images/toolicons/list-add.png";
		}
		else
		{
			prcont.style.display = "block";
			pricon.src = "/images/toolicons/list-remove.png";
		}
	}
	
	this.AddWavePlayer = function(sampleid)
	{
		this.LoadPage("/sampler/addwaveplayer/"+sampleid, null);
		
		var nwp = new machineoscillator();
		nwp.generatortype = GENERATOR_TYPE_WAVE;
		var nwpdet = Array();
		nwpdet["filename"] = "";
		nwpdet["channels"] = 1;
		nwpdet["bitrate"] = 0;
		nwpdet["samplerate"] = 44100;
		
		nwp.details = nwpdet;
		Track.samples[sampleid].info.oscillators.push(nwp);
		this.Render();
	}
	
	this.TweakWavePlayer = function(sampleid, mixerid)
	{
		var ec = mixerid;
		
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		postvars["generatorid"] = mixerid;
		postvars["nfilename"] = (document.getElementById("sampler_genmachine_filename" + ec)).value;
		
		if((document.getElementById("sampler_genmachine_usegen" + ec)).checked == true)
		{
			postvars["oenabled"] = "true";
			Track.samples[sampleid].info.oscillators[ec].enabled = 1;
		}
		else
		{
			postvars["oenabled"] = "false";
			Track.samples[sampleid].info.oscillators[ec].enabled = 0;
		}
		
		Track.samples[sampleid].info.oscillators[ec].details.filename = postvars["nfilename"];
		
		this.LoadPagePost("/sampler/tweakwaveplayer", null, postvars);
	}
	
	this.TweakReverb = function(sampleid, procid)
	{
		var ec = procid;
		
		var postvars = Array();
		postvars["sampleid"] = sampleid;
		postvars["processorid"] = procid;
		
		//postvars["gatemin"] = (document.getElementById("sampler_gm_mixer" + ec + "_gatemin")).value;
		//postvars["gatemax"] = (document.getElementById("sampler_gm_mixer" + ec + "_gatemax")).value;
		
		//tmp = document.getElementById("sampler_gm_mixer" + ec + "_gateminsrc");
		//postvars["gateminsrc"] = tmp.options[tmp.selectedIndex].value;
		//tmp = document.getElementById("sampler_gm_mixer" + ec + "_gatemaxsrc");
		//postvars["gatemaxsrc"] = tmp.options[tmp.selectedIndex].value;
		
		var tmp = document.getElementById("sampler_gm_processor" + ec + "_input0");
		postvars["input0"] = tmp.options[tmp.selectedIndex].value;
		
	
		
		Track.samples[sampleid].info.processors[ec].inputs[0].UpdateFromString(postvars["input0"]);

		
		this.LoadPagePost("/sampler/tweakreverb", null, postvars);
	}
}
