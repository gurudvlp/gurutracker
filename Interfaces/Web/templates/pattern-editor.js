function patterneditor()
{
	//this.CurrentChannel = 0;
	this.CurrentCol = 0;
	
	this.__defineGetter__("CurrentChannel", function() { return this.CurrentCol; });
	this.__defineSetter__("CurrentChannel", function(val) { this.CurrentCol = val; });
	this.CurrentSubCol = 0;
		//	CurrentSubCol 0: Note
		//	CurrentSubCol 1: Sample ID
		//	CurrentSubCol 2: Effect
		//	CurrentSubCol 3: Volume
	this.CurrentRow = 0;
	this.CurrentPattern = 0;
	this.VolumeSubColPresses = 0;
	this.VolumePressFirst = 0;
	this.VolumePressSecond = 0;
	this.SampleSubColPresses = 0;
	this.SamplePressFirst = 0;
	this.SamplePressSecond = 0;
	
	this.cactive = 0;
	
	
	this.KeyPress = function(event)
	{
		if(this.CurrentSubCol != 3) { this.VolumeSubColPresses = 0; }
		if(this.CurrentSubCol != 1) { this.SampleSubColPresses = 0; }
		
		if(event.keyCode == 37)
		{
			//	left arrow
			this.UnHighlightCurrentElement();
			
			if(this.CurrentSubCol == 0)
			{
				if(this.CurrentCol > 0)
				{
					this.CurrentCol = this.CurrentCol - 1;
					this.CurrentSubCol = 3;
				}
			}
			else
			{
				this.CurrentSubCol = this.CurrentSubCol - 1;
			}
			
			this.SetChannelRow(this.CurrentCol, this.CurrentRow);
		}
		else if(event.keyCode == 38)
		{
			//	Up arrow
			if(this.CurrentRow > 0) { this.SetChannelRow(this.CurrentCol, this.CurrentRow - 1); }
		}
		else if(event.keyCode == 39)
		{
			//	right arrow
			this.UnHighlightCurrentElement();
			if(this.CurrentSubCol == 3)
			{
				if(this.CurrentCol < 7)
				{
					this.CurrentCol = this.CurrentCol + 1;
					this.CurrentSubCol = 0;
				}
			}
			else
			{
				this.CurrentSubCol = this.CurrentSubCol + 1;
			}
			
			this.SetChannelRow(this.CurrentCol, this.CurrentRow);
		}
		else if(event.keyCode == 40)
		{
			//	Down arrow
			if(this.CurrentRow < 127) { this.SetChannelRow(this.CurrentCol, this.CurrentRow + 1); }
		}
		else
		{
			//alert(event.keyCode);
			
			if(this.CurrentSubCol == 0)
			{
				//	A note is selected right now
				if(event.keyCode == 46) { this.SetCurrentNote(-1, 5, "---"); }
				else if(event.keyCode == 65) { this.SetCurrentNote(0, 5, "C-5"); }
				else if(event.keyCode == 83) { this.SetCurrentNote(1, 5, "C#5"); }
				else if(event.keyCode == 68) { this.SetCurrentNote(2, 5, "D-5"); }
				else if(event.keyCode == 70) { this.SetCurrentNote(3, 5, "D#5"); }
				else if(event.keyCode == 71) { this.SetCurrentNote(4, 5, "E-5"); }
				else if(event.keyCode == 72) { this.SetCurrentNote(5, 5, "F-5"); }
				else if(event.keyCode == 74) { this.SetCurrentNote(6, 5, "F#5"); }
				else if(event.keyCode == 75) { this.SetCurrentNote(7, 5, "G-5"); }
				else if(event.keyCode == 76) { this.SetCurrentNote(8, 5, "G#5"); }
				else if(event.keyCode == 59) { this.SetCurrentNote(9, 5, "A-5"); }
				else if(event.keyCode == 2229) { this.SetCurrentNote(10, 5, "A#5"); } // Should be the real note for that key
				else if(event.keyCode == 222) { this.SetCurrentNote(11, 5, "B-5"); }
				else if(event.keyCode == 81) { this.SetCurrentNote(0, 6, "C-4"); }
				else if(event.keyCode == 87) { this.SetCurrentNote(1, 4, "C#4"); }
				else if(event.keyCode == 69) { this.SetCurrentNote(2, 4, "D-4"); }
				else if(event.keyCode == 82) { this.SetCurrentNote(3, 4, "D#4"); }
				else if(event.keyCode == 84) { this.SetCurrentNote(4, 4, "E-4"); }
				else if(event.keyCode == 89) { this.SetCurrentNote(5, 4, "F-4"); }
				else if(event.keyCode == 85) { this.SetCurrentNote(6, 4, "F#4"); }
				else if(event.keyCode == 73) { this.SetCurrentNote(7, 4, "G-4"); }
				else if(event.keyCode == 79) { this.SetCurrentNote(8, 4, "G#4"); }
				else if(event.keyCode == 80) { this.SetCurrentNote(9, 4, "A-4"); }
				else if(event.keyCode == 219) { this.SetCurrentNote(10, 4, "A#4"); }
				else if(event.keyCode == 221) { this.SetCurrentNote(11, 4, "B-4"); }
				else if(event.keyCode == 90) { this.SetCurrentNote(0, 6, "C-6"); }
				else if(event.keyCode == 88) { this.SetCurrentNote(1, 6, "C#6"); }
				else if(event.keyCode == 67) { this.SetCurrentNote(2, 6, "D-6"); }
				else if(event.keyCode == 86) { this.SetCurrentNote(3, 6, "D#6"); }
				else if(event.keyCode == 66) { this.SetCurrentNote(4, 6, "E-6"); }
				else if(event.keyCode == 78) { this.SetCurrentNote(5, 6, "F-6"); }
				else if(event.keyCode == 77) { this.SetCurrentNote(6, 6, "F#6"); }
				else if(event.keyCode == 188) { this.SetCurrentNote(7, 6, "G-6"); }
				else if(event.keyCode == 190) { this.SetCurrentNote(8, 6, "G#6"); }
				else if(event.keyCode == 191) { this.SetCurrentNote(9, 6, "A-6"); }
				else if(event.keyCode == 8999) { this.SetCurrentNote(10, 6, "A#6"); }
				else if(event.keyCode == 8599) { this.SetCurrentNote(11, 6, "B-6"); }
				else if(event.keyCode == 61) { this.SetCurrentNote(-2, 5, "==="); }
				else { alert(event.keyCode); }
			}
			else if(this.CurrentSubCol == 1)
			{
				if(event.keyCode > 47 && event.keyCode < 58)
				{
					if(this.SampleSubColPresses == 0) { this.SamplePressFirst = event.keyCode - 48; }
					else { this.SamplePressSecond = event.keyCode - 48; }
					this.SampleSubColPresses++;
					
					if(this.SampleSubColPresses == 2)
					{
						this.SampleSubColPresses = 0;
						smpstr = this.SamplePressFirst.toString() + this.SamplePressSecond.toString();
						this.SetCurrentSample(smpstr);
					}
				}
			}
			else if(this.CurrentSubCol == 3)
			{
				if(event.keyCode > 47 && event.keyCode < 58)
				{
					if(this.VolumeSubColPresses == 0) { this.VolumePressFirst = event.keyCode - 48; }
					else { this.VolumePressSecond = event.keyCode - 48; }
					this.VolumeSubColPresses++;
					
					if(this.VolumeSubColPresses == 2)
					{
						this.VolumeSubColPresses = 0;
						volstr = this.VolumePressFirst.toString() + this.VolumePressSecond.toString();
						this.SetCurrentVolume(volstr);
					}
				}
				else if(event.keyCode == 46)
				{
					this.SetCurrentVolume('-1');
				}
			}
		}
	}
	
	
	this.Render = function()
	{
		var maintmp = Templates.t("sequencer/main.html");
		var pghead = Templates.t("sequencer/header.html");
		var ectmp = Templates.t("sequencer/eachchannel.html");
		var pgrid = "";
		for(var erow = 0; erow < Track.patterns[this.CurrentPattern].length; erow++)
		{
			var trow = "";
			var ertmp = Templates.t("sequencer/eachrow.html");
			var eelmtmp = Templates.t("sequencer/eachelement.html");
			ertmp = Templates.Replace("[ROW]", erow, ertmp);
			
			var frow = "";
			for(var echn = 0; echn < Track.patterns[this.CurrentPattern].channels.length; echn++)
			{
				eelmtmp = Templates.t("sequencer/eachelement.html");
				eelmtmp = Templates.Replace("[ROW]", erow, eelmtmp);
				eelmtmp = Templates.Replace("[CHANNEL]", echn, eelmtmp);
				eelmtmp = Templates.Replace("[NOTE]", Track.patterns[this.CurrentPattern].channels[echn].rows[erow].StringNote(), eelmtmp);
				eelmtmp = Templates.Replace("[SAMPLEID]", Track.patterns[this.CurrentPattern].channels[echn].rows[erow].StringSampleID(), eelmtmp);
				eelmtmp = Templates.Replace("[VOLUME]", Track.patterns[this.CurrentPattern].channels[echn].rows[erow].StringVolume(), eelmtmp);
				
				frow = frow + eelmtmp;
			}
			
			ertmp = ertmp.replace("[ELEMENTS]", frow);
			pgrid = pgrid + ertmp;
		}
		
		var fchan = "";
		for(var ec = 0; ec < Track.patterns[this.CurrentPattern].channels.length; ec++)
		{
			var tchan = ectmp;
			tchan = Templates.Replace("[CHANNEL]", ec, tchan);
			tchan = Templates.Replace("[CHANNELTITLE]", "Channel "+ec, tchan);
			fchan = fchan + tchan;
		}
		
		pghead = Templates.Replace("[CHANNELS]", fchan, pghead);
		maintmp = Templates.Replace("[EACHROW]", pghead+pgrid, maintmp);
		
		document.getElementById('page_content').innerHTML = maintmp;
	}
	
	
	this.HighlightCurrentElement = function()
	{
		document.getElementById('elementsub_' + this.CurrentCol + '_' + this.CurrentRow + "_" + this.CurrentSubCol).style.backgroundColor = "";
	}
	
	this.UnHighlightCurrentElement = function()
	{
		document.getElementById('elementsub_' + this.CurrentCol + '_' + this.CurrentRow + "_" + this.CurrentSubCol).style.backgroundColor = "#FFFFFF";
	}
	
	this.SetChannelRow = function(channel, row)
	{
		//alert("SetchannelRow");
		this.UnHighlightCurrentRow();
		this.CurrentCol = channel;
		this.CurrentRow = row;
		this.HighlightCurrentRow();
	}
	
	this.SetChannelRowSub = function(channel, row, sub)
	{
		this.UnHighlightCurrentRow();
		this.CurrentCol = channel;
		this.CurrentRow = row;
		this.CurrentSubCol = sub;
		this.HighlightCurrentRow();
	}
	
	this.HighlightCurrentRow = function()
	{
		//alert("done highlighting row");
		//document.getElementById('sequencer_row_' + CurrentRow).className = "gurumod_sequencer_row_highlighted";
		document.getElementById('sequencer_row_' + this.CurrentRow).style.backgroundColor = "#404040";
		document.getElementById('elementsub_' + this.CurrentCol + '_' + this.CurrentRow + "_" + this.CurrentSubCol).style.backgroundColor = "#EEEEEE";
	}
	
	this.UnHighlightCurrentRow = function()
	{
		//alert("Unhighlighting "+this.CurrentCol+", "+this.CurrentRow+", "+this.CurrentSubCol);
		//getElementById('sequencer_row_' + CurrentRow).className = "gurumod_sequencer_row_blank";
		document.getElementById('sequencer_row_' + this.CurrentRow).style.backgroundColor = "#FFFFFF";
		document.getElementById('elementsub_' + this.CurrentCol + '_' + this.CurrentRow + "_" + this.CurrentSubCol).style.backgroundColor = "#FFFFFF";
		//alert("done highlighting row");
	}
	
	this.SetCurrentVolume = function(volstr)
	{
		if(volstr > -1)
		{
			document.getElementById('elementsub_' + this.CurrentCol + "_" + this.CurrentRow + "_" + this.CurrentSubCol).innerHTML = "v" + volstr;
		}
		else
		{
			document.getElementById('elementsub_' + this.CurrentCol + "_" + this.CurrentRow + "_" + this.CurrentSubCol).innerHTML = "---";
		}
		
		Track.patterns[this.CurrentPattern].channels[this.CurrentCol].rows[this.CurrentRow].volume = volstr;
		
		postvars = Array();
		postvars['subcol'] = 3;
		postvars['pattern'] = this.CurrentPattern;
		postvars['channel'] = this.CurrentCol;
		postvars['element'] = this.CurrentRow;
		postvars['volume'] = volstr;
		
		
		this.LoadPagePost("/updatepattern", null, postvars);
	}
	
	this.LoadPage = function(url, output)
	{
		if(this.cactive == 1)
		{
			setTimeout("PatternEditor.LoadPage('"+url+"', '"+output+"')", 600);
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
	
	this.LoadPagePost = function(url, output, postvars)
	{
		if(this.cactive == 1)
		{
			setTimeout("PatternEditor.LoadPagePost('"+url+"', '"+output+"', '"+postvars+"')", 600);
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
	
	this.SetCurrentSample = function(sampleid)
	{
		sampname = sampleid;
		if(sampleid < 0) { sampname = "--"; } else { sampname = sampleid; }
		document.getElementById('elementsub_' + this.CurrentCol + "_" + this.CurrentRow + "_" + this.CurrentSubCol).innerHTML = sampname;
		
		Track.patterns[this.CurrentPattern].channels[this.CurrentCol].rows[this.CurrentRow].sampleid = sampleid;
		
		postvars = Array();
		postvars['subcol'] = 1;
		postvars['pattern'] = this.CurrentPattern;
		postvars['channel'] = this.CurrentCol;
		postvars['element'] = this.CurrentRow;
		postvars['sampleid'] = sampleid;
		
		this.LoadPagePost("/updatepattern", null, postvars);
	}
	
	this.SetCurrentNote = function(noteid, octave, notename)
	{
		
		document.getElementById('elementsub_' + this.CurrentCol + "_" + this.CurrentRow + "_" + this.CurrentSubCol).innerHTML = notename;
		
		Track.patterns[this.CurrentPattern].channels[this.CurrentCol].rows[this.CurrentRow].note = noteid;
		Track.patterns[this.CurrentPattern].channels[this.CurrentCol].rows[this.CurrentRow].octave = octave;
		
		postvars = Array();
		postvars['subcol'] = 0;
		postvars['pattern'] = this.CurrentPattern;
		postvars['channel'] = this.CurrentCol;
		postvars['element'] = this.CurrentRow;
		postvars['note'] = noteid;
		postvars['octave'] = octave;
		
		this.LoadPagePost("/updatepattern", null, postvars);
	}
}
