
using System;

using gurumod.Logging;

namespace gurumod.WebPages
{

	//	This was included with this project in an early release.  Probably 2012.
	//	The web server for this was originally created for another purpose,
	//	which is the only reason I can imagine that BBCode support is even
	//	included.  This will likely be removed in the future.
	public class BBCode
	{

		string plainmessage = "";
		
		public BBCode (string MessageToEncode)
		{
			plainmessage = MessageToEncode;
		}
		
		public string EncodedMessage
		{
			get
			{
				string toret = plainmessage;
				toret = EncodeSmilies(toret);
				toret = EncodeIMG(toret);
				toret = EncodeURL(toret);
				toret = EncodeB(toret);
				toret = EncodeI(toret);
				toret = EncodeU(toret);
				toret = EncodeColor(toret);
				toret = EncodeQuote(toret);
				
				return toret;
			}
		}
		
		public string EncodeSmilies(string message)
		{
			string[,] Smilies = new string[16, 2];
			Smilies[0,0] = ":oops:"; Smilies[0,1] = "<img src=\"/images/smilies/icon_redface.gif\" border=\"0\">";
			Smilies[10,0] = ":D"; Smilies[10,1] = "<img src=\"/images/smilies/icon_biggrin.gif\" border=\"0\">";
			Smilies[1,0] = ":)"; Smilies[1,1] = "<img src=\"/images/smilies/icon_smile.gif\" border=\"0\">";
			Smilies[2,0] = ":("; Smilies[2,1] = "<img src=\"/images/smilies/icon_sad.gif\" border=\"0\">";
			Smilies[3,0] = ":o"; Smilies[3,1] = "<img src=\"/images/smilies/icon_surprised.gif\" border=\"0\">";
			Smilies[4,0] = "8o"; Smilies[4,1] = "<img src=\"/images/smilies/icon_eek.gif\" border=\"0\">";
			Smilies[5,0] = ":?"; Smilies[5,1] = "<img src=\"/images/smilies/icon_confused.gif\" border=\"0\">";
			Smilies[6,0] = "8)"; Smilies[6,1] = "<img src=\"/images/smilies/icon_cool.gif\" border=\"0\">";
			Smilies[7,0] = ":lol:"; Smilies[7,1] = "<img src=\"/images/smilies/icon_lol.gif\" border=\"0\">";
			Smilies[8,0] = ":x"; Smilies[8,1] = "<img src=\"/images/smilies/icon_mad.gif\" border=\"0\">";
			Smilies[9,0] = ":P"; Smilies[9,1] = "<img src=\"/images/smilies/icon_razz.gif\" border=\"0\">";
			
			Smilies[11,0] = ":cry:"; Smilies[11,1] = "<img src=\"/images/smilies/icon_cry.gif\" border=\"0\">";
			Smilies[12,0] = ":evil:"; Smilies[12,1] = "<img src=\"/images/smilies/icon_evil.gif\" border=\"0\">";
			Smilies[13,0] = ":twisted:"; Smilies[13,1] = "<img src=\"/images/smilies/icon_twisted.gif\" border=\"0\">";
			Smilies[14,0] = ":roll:"; Smilies[14,1] = "<img src=\"/images/smilies/icon_rolleyes.gif\" border=\"0\">";
			Smilies[15,0] = ":wink:"; Smilies[15,1] = "<img src=\"/images/smilies/icon_wink.gif\" border=\"0\">";
			
			for(int esmiley = 0; esmiley < 16; esmiley++)
			{
				message = message.Replace(Smilies[esmiley,0], Smilies[esmiley,1]);
			}
			
			return message;
		}
		
		public string EncodeB(string message)
		{
			message = message.Replace("[B]", "[b]");
			message = message.Replace("[/B]", "[/b]");
			message = message.Replace("[b]", "<b>");
			message = message.Replace("[/b]", "</b>");
			return message;
		}
		
		public string EncodeU(string message)
		{
			message = message.Replace("[U]", "[u]");
			message = message.Replace("[/U]", "[/u]");
			message = message.Replace("[u]", "<u>");
			message = message.Replace("[/u]", "</u>");
			return message;
		}
		
		public string EncodeI(string message)
		{
			message = message.Replace("[I]", "[i]");
			message = message.Replace("[/I]", "[/i]");
			message = message.Replace("[i]", "<i>");
			message = message.Replace("[/i]", "</i>");
			return message;
		}
		
		public string EncodeURL(string message)
		{
			message = message.Replace("[URL", "[url");
			message = message.Replace("[/URL]", "[/url]");
			
			if(!message.Contains("[url")) { return message; }
			if(!message.Contains("[/url]")) { return message; }
			
			string preurl = message.Substring(0, message.LastIndexOf("[url") + 4);
			string trem = message.Substring(preurl.Length);
			string posturl = trem.Substring(trem.IndexOf("[/url]"));
			string urledtext = trem.Substring(0, trem.Length - posturl.Length);
			
			posturl = posturl.Substring(6);
			preurl = preurl.Substring(0, preurl.Length - 4);
			
			string linkurl = "";
			if(urledtext.Substring(0, 1) == "=")
			{
				linkurl = urledtext.Substring(1);
				linkurl = linkurl.Substring(0, linkurl.IndexOf("]"));
				urledtext = urledtext.Replace("=" + linkurl + "]", "");
				/*urledtext = urledtext.Substring(1);
				linkurl = urledtext.Substring(urledtext.IndexOf("]"));
				urledtext = urledtext.Substring(linkurl.Length);*/
			}
			else
			{
				urledtext = urledtext.Substring(1);
				linkurl = urledtext;
			}
			
			
			message = preurl + "<a href=\"" + linkurl + "\" target=\"_top\">" + urledtext + "</a>" + posturl;
			
			return EncodeURL(message);
		}
		
		public string EncodeIMG(string message)
		{
			message = message.Replace("[IMG]", "[img]");
			message = message.Replace("[img]", "<img src=\"");
			message = message.Replace("[/img]", "\"/>");
			
			return message;
		}
		
		public string EncodeColor(string message)
		{
			message = message.Replace("[COLOR=", "[color=");
			message = message.Replace("[/COLOR]", "[/color]");
			
			try
			{
				if(!message.Contains("[color=")) { return message; }
				if(!message.Contains("[/color]")) { return message; }
				string precolor = message.Substring(0, message.IndexOf("[color="));
				string postcolor = message.Substring(message.IndexOf("[/color]"));
				
				//int coloredstartpoint = precolor.Length + 7;
				//int coloredlength = message.
				string coloredtext = message.Substring(precolor.Length + 7, message.Length - (precolor.Length + 7) - (message.Length - message.IndexOf("[/color]")));
				
				//postcolor = postcolor.Replace("[/color]", "");
				postcolor = postcolor.Substring(8);
				
				//precolor = EncodeColor(precolor);
				//postcolor = EncodeColor(postcolor);
				
				string colortouse = coloredtext.Substring(0, coloredtext.IndexOf("]"));
				coloredtext = coloredtext.Substring(coloredtext.IndexOf("]") + 1);
				
				message = precolor + "<span style=\"color: " + colortouse + "\">" + coloredtext + "</span>" + postcolor;
				
				/*Logging.Write("BBCode::");
				Logging.Write("\t" + precolor);
				Logging.Write("\t" + colortouse);
				Logging.Write("\t" + coloredtext);
				Logging.Write("\t" + postcolor);*/
			}
			catch(Exception ex)
			{
				Log.lWarning("Exception processing BBCode color.", "BBCode", "EncodeColor");
				Log.lWarning(ex.Message);
			}
			
			return EncodeColor(message);
		}
		
		public string EncodeQuote(string message)
		{
			message = message.Replace("[QUOTE]", "[quote]");
			message = message.Replace("[/QUOTE]", "[/quote]");
			
			if(!message.Contains("[quote]")) { return message; }
			if(!message.Contains("[/quote]")) { return message; }
			
			message = message.Replace("[quote]", "<div class=\"forum_post_quote\">");
			message = message.Replace("[/quote]", "</div>");
			
			return message;
		}
		
	}
}
