
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace gurumod
{

	[XmlRoot("Category")]
	public class Category
	{
		[XmlElement("Name")] public string Name = "";
		[XmlElement("Command")] public string Command = "";
		[XmlElement("Type")] public string ObjectType;
		
		public Category()
		{
		}
		
		public Category(string name, string command, string stype)
		{
			this.Name = name;
			this.Command = command;
			
			this.ObjectType = stype;
			
			
		}
		
		public static int NextCategory()
		{
			//
			//	Determine and return the next available category ID.
			//
			
			for(int eCat = 0; eCat < WebInterface.Categories.Length; eCat++)
			{
				if(WebInterface.Categories[eCat] == null)
				{
					return eCat;
				}
			}
			
			return -1;
		}
		
		public bool Save(int catid)
		{
			try
			{
				XmlSerializer s = new XmlSerializer( this.GetType() );
				TextWriter w = new StreamWriter(Engine.PFP(Engine.ConfigPath + "Interfaces/Web/PageCat/" + catid.ToString() + ".xml" ));
				s.Serialize( w, this );
				w.Close();
			}
			catch(Exception ex)
			{
				Console.WriteLine("btEngine: WebInterface: PageCat: Save failed.");
				Console.WriteLine(ex.Message);
								
				Console.WriteLine("Inner Exception: {0}", (ex.InnerException).Message);
				Console.WriteLine("Base Exception: {0}", (ex.GetBaseException()).Message);
				return false;
			}
			
			return true;
		}
		
		public static void SaveAll()
		{
			for(int eCat = 0; eCat < WebInterface.Categories.Length; eCat++)
			{
				if(WebInterface.Categories[eCat] != null)
				{
					WebInterface.Categories[eCat].Save(eCat);
				}
			}
		}
	}
}
