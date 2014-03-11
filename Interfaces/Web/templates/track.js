var SAMPLE_TYPE_FILE = 0;
var SAMPLE_TYPE_GENERATOR = 1;
var SAMPLE_TYPE_MACHINE = 2;
var WAVE_TYPE_SINE = 0;
var WAVE_TYPE_SQUARE = 1;
var WAVE_TYPE_TRIANGLE = 2;
var WAVE_TYPE_SAWTOOTH = 3;
var WAVE_TYPE_SILENCE = 4;
var PROC_TYPE_MIXER = 0;
var PROC_TYPE_GATE = 1;
var PROC_TYPE_REVERB = 2;
var PROC_TYPE_ENVELOPE = 3;
var MIX_METHOD_ADD = 0;
var MIX_METHOD_SUBTRACT = 1;
var MIX_METHOD_MULTIPLY = 2;
var MIX_METHOD_DIVIDE = 3;
var EDITOR_MODE_SEQUENCER = 0;
var EDITOR_MODE_SAMPLER = 1;
var EDITOR_MODE_DETAILER = 2;
var SOURCE_TYPE_OSC = 0;
var SOURCE_TYPE_PROC = 1;
var GENERATOR_TYPE_OSC = 0;
var GENERATOR_TYPE_WAVE = 1;

var EditorMode = EDITOR_MODE_SEQUENCER;


var Track = new track();
var Templates = new templates();
var PatternEditor = null;
var Sampler = null;
var Detailer = null;

function ShowSequencer()
{
	ShowSequencerAt(0);
}

function ShowSequencerAt(patternid)
{
	if(PatternEditor == null)
	{ 
		PatternEditor = new patterneditor();
		document.addEventListener("keydown", function(event){
			if(EditorMode == EDITOR_MODE_SAMPLER) { return; }
			PatternEditor.KeyPress(event);
		});
	}
	
	EditorMode = EDITOR_MODE_SEQUENCER;
	PatternEditor.CurrentPattern = patternid;
	PatternEditor.Render();
}

function ShowSampler()
{
	if(Sampler == null)
	{
		Sampler = new sampler();
	}
	EditorMode = EDITOR_MODE_SAMPLER;
	Sampler.Render();
	//LoadPageCallFunction("/sampler", "ShowSamplerHandler", null);

}

function ShowDetailEditor()
{
	if(Detailer == null)
	{
		Detailer = new detailer();
	}
	EditorMode = EDITOR_MODE_DETAILER;
	Detailer.Render();
}

function templates()
{

	this.Temps = Array();
	
	this.t = function(template)
	{
		if(typeof(this.Temps[template]) == 'undefined')
		{
			return this.LoadTemplate(template);
		}
		else
		{
			return this.Temps[template];
		}
	}
	
	this.LoadTemplate = function(template)
	{
		var xo;
		if (window.XMLHttpRequest)
		{// code for IE7+, Firefox, Chrome, Opera, Safari
			xo = new XMLHttpRequest();
		}
		else
		{// code for IE6, IE5
			xo = new ActiveXObject("Microsoft.XMLHTTP");
		}

		xo.open("GET", "/templates/"+template, false);
		xo.send(null);

		this.Temps[template] = xo.responseText;
		return this.Temps[template];
	}
	
	this.Replace = function(what, withwhat, inwhat)
	{
		while(inwhat.indexOf(what) > -1) { inwhat = inwhat.replace(what, withwhat); }
		return inwhat;
	}
}

function track()
{
	this.tempo = 180;
	this.title = "";
	this.author = "";
	this.year = 2013;
	this.genre = "";
	this.comments = "";
	
	this.patterns = Array();
	this.samples = Array();
	
	this.channels = -1;
	this.channelsmuted = Array();
	
	this.cactive = 0;
	
	this.LoadTrack = function()
	{
		var xo;
		if (window.XMLHttpRequest)
		{// code for IE7+, Firefox, Chrome, Opera, Safari
			xo = new XMLHttpRequest();
		}
		else
		{// code for IE6, IE5
			xo = new ActiveXObject("Microsoft.XMLHTTP");
		}

		xo.open("GET", "/sequencer/patterndata", false);
		xo.send(null);

		
		var pd = xo.responseText;
		var ztmpat = eval("["+pd+"]");
		var tmpat = ztmpat[0].patterns;
		//var blah = "";
		//for(var propt in tmpat) { blah = blah + propt + "\n"; }; alert(blah);
		
		/*for(var ep = 0; ep < tmpat.patterns.length; ep++)
		{
			this.patterns[ep] = new pattern();
			this.patterns[ep].id = tmpat[ep].id;
			this.patterns[ep].length = tmpat[ep].length;
			
			for(var ec = 0; ec < tmpat.patterns[ep].channels.length; ec++)
			{
				this.patterns[ep].channels[ec] = new patternchannel();
				
				for(var er = 0; er < tmpat.patterns[ep].channels[ec].rows.length; er++)
				{
					this.patterns[ep].channels[ec].rows[er] = new channelrow();
					this.patterns[ep].channels[ec].rows[er].octave = tmpat.patterns[ep].channels[ec].rows[er].octave;
					this.patterns[ep].channels[ec].rows[er].note = tmpat.patterns[ep].channels[ec].rows[er].note;
					this.patterns[ep].channels[ec].rows[er].sampleid = tmpat.patterns[ep].channels[ec].rows[er].sampleid;
					this.patterns[ep].channels[ec].rows[er].volume = tmpat.patterns[ep].channels[ec].rows[er].volume;
					this.patterns[ep].channels[ec].rows[er].effect = tmpat.patterns[ep].channels[ec].rows[er].effect;
				}
			}
		}*/
		
		for(var ep = 0; ep < tmpat.length; ep++)
		{
			this.patterns[ep] = new pattern();
			this.patterns[ep].id = tmpat[ep].id;
			this.patterns[ep].length = tmpat[ep].length;
			
			//var blah = ""; for(var propt in tmpat[ep]) { blah = blah + propt + "\n"; }; alert(blah);
			for(var ec = 0; ec < tmpat[ep].channels.length; ec++)
			{
				this.patterns[ep].channels[ec] = new patternchannel();
				
				for(var er = 0; er < tmpat[ep].channels[ec].rows.length; er++)
				{
					this.patterns[ep].channels[ec].rows[er] = new channelrow();
					this.patterns[ep].channels[ec].rows[er].octave = tmpat[ep].channels[ec].rows[er].octave;
					this.patterns[ep].channels[ec].rows[er].note = tmpat[ep].channels[ec].rows[er].note;
					this.patterns[ep].channels[ec].rows[er].sampleid = tmpat[ep].channels[ec].rows[er].sampleid;
					this.patterns[ep].channels[ec].rows[er].volume = tmpat[ep].channels[ec].rows[er].volume;
					this.patterns[ep].channels[ec].rows[er].effect = tmpat[ep].channels[ec].rows[er].effect;
				}
			}
			
			if(this.channels < 0)
			{
				this.channels = this.patterns[ep].channels.length;
				for(var ec = 0; ec < this.channels; ec++) { this.channelsmuted[ec] = false; }
			}
			
		}
		
		
		////	And to load the sample information...
		xo = null;
		if (window.XMLHttpRequest)
		{// code for IE7+, Firefox, Chrome, Opera, Safari
			xo = new XMLHttpRequest();
		}
		else
		{// code for IE6, IE5
			xo = new ActiveXObject("Microsoft.XMLHTTP");
		}

		xo.open("GET", "/sampler/sampledata", false);
		xo.send(null);

		
		var pd = xo.responseText;
		var ztmpat = eval("["+pd+"]");
		var ustmp = ztmpat[0].samples;
		
		for(var esmp = 0; esmp < ustmp.length; esmp++)
		{
			this.samples[esmp] = new sample();
			this.samples[esmp].id = ustmp[esmp].id;
			this.samples[esmp].title = ustmp[esmp].title;
			this.samples[esmp].type=ustmp[esmp].type;
			
			this.samples[esmp].ParseInfo(ustmp[esmp].info);
		}
		
	}
	
	this.ToggleMute = function(channel)
	{
		this.LoadPage("/player/togglemute/" + channel, null);
		if(this.channelsmuted[channel]) { this.channelsmuted[channel] = false; } else { this.channelsmuted[channel] = true; }
	}
	
	
	this.LoadPage = function(url, output)
	{
		if(this.cactive == 1)
		{
			setTimeout("Track.LoadPage('"+url+"', '"+output+"')", 600);
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
	
	function LoadPagePost(url, output, postvars)
	{
		if(this.cactive == 1)
		{
			setTimeout("Track.LoadPagePost('"+url+"', '"+output+"', '"+postvars+"')", 600);
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
	
	this.SaveTrack = function()
	{
		this.LoadPage("/player/save", null);
	}
	
	this.NewTrack = function()
	{
		this.LoadPage("/player/new", null);
	}
	
	this.StartPlayback = function()
	{
		this.LoadPage("/player/play", null);
	}
	
	this.PausePlayback = function()
	{
		this.LoadPage("/player/pause", null);
	}
	
	this.StopPlayback = function()
	{
		this.LoadPage("/player/stop", null);
	}
	
	this.PlaybackLoopChange = function(newstyle)
	{
		postvars = Array();
		postvars["patternid"] = PatternEditor.CurrentPattern;
		this.LoadPagePost("/player/loop/" + newstyle, null, postvars);
	}
}

function pattern()
{
	this.id = 0;
	this.length = 128;
	this.channels = Array();
}

function patternchannel()
{
	this.muted = false;
	this.rows = Array();
	
	this.Initialize = function(rows)
	{
		if(rows < 1) { return false; }
		
		for(var er = 0; er < rows; er++)
		{
			this.rows.push(new channelrow());
		}
		
		return true;
	}
}

function channelrow()
{
	this.note = "";
	this.octave = "";
	this.sampleid = "";
	this.effect = "";
	this.volume = "";
	
	this.StringNote = function()
	{
		if(this.note == -1) { return "---"; }
		else if(this.note == -2) { return "==="; }
		else if(this.note == 0) { return "C-" + this.octave; }
		else if(this.note == 1) { return "C#" + this.octave; }
		else if(this.note == 2) { return "D-" + this.octave; }
		else if(this.note == 3) { return "D#" + this.octave; }
		else if(this.note == 4) { return "E-" + this.octave; }
		else if(this.note == 5) { return "F-" + this.octave; }
		else if(this.note == 6) { return "F#" + this.octave; }
		else if(this.note == 7) { return "G-" + this.octave; }
		else if(this.note == 8) { return "G#" + this.octave; }
		else if(this.note == 9) { return "A-" + this.octave; }
		else if(this.note == 10) { return "A#" + this.octave; }
		else if(this.note == 11) { return "B-" + this.octave; }
	}
	
	this.StringSampleID = function()
	{
		if(this.sampleid < 0) { return "--"; }
		if(this.sampleid < 10) { return "0"+this.sampleid; }
		return this.sampleid;
	}
	
	this.StringVolume = function()
	{
		if(this.volume < 0) { return "---"; }
		if(this.volume < 10) { return "v0"+this.volume; }
		return "v"+this.volume;
	}
}


function sample()
{
	this.id = 0;
	this.title = "Untitled";
	this.type = SAMPLE_TYPE_FILE;
	
	this.info = null;
	
	this.ParseInfo = function(infod)
	{
		if(this.type == SAMPLE_TYPE_FILE)
		{
			this.info = new sampleifile();
			this.info.Parse(infod[0]);
		}
		else if(this.type == SAMPLE_TYPE_GENERATOR)
		{
			this.info = new sampleigenerator();
			this.info.Parse(infod[0]);
		}
		else
		{
			this.info = new sampleimachine();
			this.info.Parse(infod[0]);
		}
	}
}

function sampleifile()
{
	this.filename = "";
	this.length = "";
	this.channels = "";
	this.bitrate = "";
	this.samplerate = "";
	
	this.Parse = function(infod)
	{
		//var blah="";for(var propt in infod) { blah=blah+propt+"\n"; } alert(blah);
		this.filename = infod.filename;
		this.length = infod.length;
		this.channels = infod.channels;
		this.bitrate = infod.bitrate;
		this.samplerate = infod.samplerate;
	}
}

function sampleigenerator()
{
	this.wavetype = WAVE_TYPE_SINE;
	this.samplerate = 44100;
	this.length = 3;
	this.frequency = 440;
	
	this.Parse = function(infod)
	{
		this.wavetype = infod.wavetype;
		this.samplerate = infod.samplerate;
		this.length = infod.length;
		this.frequency = infod.frequency;
	}
}

function sampleimachine()
{
	this.oscillators = Array();
	this.processors = Array();
	
	this.Parse = function(infod)
	{
		this.oscillators = infod.oscillators;
		
		//var blah=""; for(var propt in infod.oscillators) { blah=blah+propt+"\n"; } alert(blah);
		
		for(var ep = 0; ep < infod.processors.length; ep++)
		{
			var ptype = infod.processors[ep].proctype;
			
			if(ptype == PROC_TYPE_GATE)
			{
				this.processors[ep] = new processorigate();
				
				for(var ein = 0; ein < infod.processors[ep].inputs.length; ein++)
				{
					this.processors[ep].inputs[ein] = new processorinputdata();
					this.processors[ep].inputs[ein].sourcetype = infod.processors[ep].inputs[ein].sourcetype;
					this.processors[ep].inputs[ein].sourceid = infod.processors[ep].inputs[ein].sourceid;
					
					//alert("ST: "+infod.processors[ep].inputs[ein].sourcetype+"\nSID: "+infod.processors[ep].inputs[ein].sourceid);
				}
				
				//var blah="";for(var propt in infod.processors[ep].details) {blah=blah+propt+"\n";} alert(blah);
				this.processors[ep].mingatemanual = infod.processors[ep].details[0].mingatemanual;
				this.processors[ep].maxgatemanual = infod.processors[ep].details[0].maxgatemanual;
			}
			else if(ptype == PROC_TYPE_REVERB)
			{
				this.processors[ep] = new processorireverb();
				
				this.processors[ep].inputs[0] = new processorinputdata();
				this.processors[ep].inputs[0].sourcetype = infod.processors[ep].inputs[0].sourcetype;
				this.processors[ep].inputs[0].sourceid = infod.processors[ep].inputs[0].sourceid;
			}
			else if(ptype == PROC_TYPE_ENVELOPE)
			{
				this.processors[ep] = new processorienvelope();
				
				this.processors[ep].inputs[0] = new processorinputdata();
				this.processors[ep].inputs[0].sourcetype = infod.processors[ep].inputs[0].sourcetype;
				this.processors[ep].inputs[0].sourceid = infod.processors[ep].inputs[0].sourceid;
				this.processors[ep].attack = infod.processors[ep].details[0].attack;
				this.processors[ep].decay = infod.processors[ep].details[0].decay;
				this.processors[ep].sustain = infod.processors[ep].details[0].sustain;
				this.processors[ep].release = infod.processors[ep].details[0].release;
				this.processors[ep].attackamp = infod.processors[ep].details[0].attackamp;
				this.processors[ep].decayamp = infod.processors[ep].details[0].decayamp;
			}
			else
			{
				this.processors[ep] = new processorimixer();
				//this.processors[ep].inputs = infod.processors[ep].inputs;
				
				//var blah=""; for(var propt in infod.processors[ep].inputs) { blah=blah+propt+"\n"; } alert(blah);
				for(var ein = 0; ein < infod.processors[ep].inputs.length; ein++)
				{
					this.processors[ep].inputs[ein] = new processorinputdata();
					this.processors[ep].inputs[ein].sourcetype = infod.processors[ep].inputs[ein].sourcetype;
					this.processors[ep].inputs[ein].sourceid = infod.processors[ep].inputs[ein].sourceid;
					this.processors[ep].inputs[ein].amplitude = infod.processors[ep].inputs[ein].amplitude;
					
				}
				
				this.processors[ep].method = infod.processors[ep].details[0].method;
			}
		}
	}
}

function machineoscillator()
{
	this.generatortype = GENERATOR_TYPE_OSC;
	this.wavetype = WAVE_TYPE_SINE;
	this.frequency = 440;
	this.amplitude = 0.75;
	this.enabled = true;
	this.details = Array();
}

function machinewaveplayer()
{
	this.filename = "";
	this.channels = 1;
	this.bitrate = 0;
	this.samplerate = 44100;
}

function machineprocessor()
{
	this.proctype = PROC_TYPE_MIXER;
}

function processorimixer()
{
	this.proctype = PROC_TYPE_MIXER;
	this.inputa = "osc_0";
	this.inputb = "osc_0";
	this.method = MIX_METHOD_ADD;
	this.inputs = Array();
}

function processorigate()
{
	this.proctype = PROC_TYPE_GATE;
	this.gatemin = "osc_0";
	this.mingatemanual = 0.2;
	this.gatemax = "man";
	this.maxgatemanual = 0.7;
	this.audiosource = "osc_0";
	this.inputs = Array();
}

function processorireverb()
{
	this.proctype = PROC_TYPE_REVERB;
	this.inputs = Array();
}

function processorienvelope()
{
	this.proctype = PROC_TYPE_ENVELOPE;
	this.attack = 100;
	this.decay = 200;
	this.sustain = 500;
	this.release = 50;
	this.attackamp = 1;
	this.decayamp = 0.7;
	this.inputs = Array();
}

function processorinputdata()
{
	this.sourcetype = SOURCE_TYPE_OSC;
	this.sourceid = 0;
	this.amplitude = 1;
	
	this.UpdateFromString = function(nstring)
	{
		if(nstring == -1)
		{
			this.sourcetype = -1;
			this.sourceid = -1;
			return true;
		}
		
		if(nstring.substr(0, 3) == "gen")
		{
			var nid = nstring.substr(3);
			this.sourcetype = SOURCE_TYPE_OSC;
			this.sourceid = nid;
			return true;
		}
		
		if(nstring.substr(0, 4) == "proc")
		{
			var nid = nstring.substr(4);
			this.sourcetype = SOURCE_TYPE_PROC;
			this.sourceid = nid;
			return true;
		}
		
		return false;
	}
}
