using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace gurumod
{
	public class MixerSettings
	{
		[XmlElement("SourceTypeA")] public int SourceTypeA = 0;
		[XmlElement("SourceTypeB")] public int SourceTypeB = 0;
		[XmlElement("SourceAID")] public int SourceAID = 0;
		[XmlElement("SourceBID")] public int SourceBID = 1;
		[XmlElement("MixMethod")] public int MixMethod = 0;
		[XmlElement("GateMin")] public double GateMin = 0.0;
		[XmlElement("GateMax")] public double GateMax = 1.0;
		[XmlElement("MinGateControlSource")] public int MinGateControlSource = -1;
		[XmlElement("MaxGateControlSource")] public int MaxGateControlSource = -1;
		
		[XmlIgnore()] public static int SourceTypeOscillator = 0;
		[XmlIgnore()] public static int SourceTypeMixer = 1;
		
		[XmlIgnore()] public static int MixMethodAdd = 0;
		[XmlIgnore()] public static int MixMethodSubtract = 1;
		[XmlIgnore()] public static int MixMethodMultiply = 2;
		[XmlIgnore()] public static int MixMethodDivide = 3;
		
		
		public MixerSettings ()
		{
		}
	}
}

