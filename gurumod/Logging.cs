// 
//  Logging.cs
//  
//  Author:
//       Brian Murphy <gurudvlp@gmail.com>
// 
//  Copyright (c) 2012 - 2022 Brian Murphy
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

namespace gurumod.Logging
{
	public class Log
	{
		public Log ()
		{
		}
		
		public static void Write(string instring)
		{
			if(Engine.Configuration.DisplayDebug) { Console.WriteLine(instring); }
		}

		public static void lInfo(string message)
		{
			string msgstr = String.Format("{0} INFO {1}", logTimestamp(), message);
			Write(msgstr);
		}
		
		public static void lInfo(string message, string tclass, string tfunction)
		{
			string msgstr = String.Format("{0}: {1}: {2}", tclass, tfunction, message);
			lInfo(msgstr);
		}

		public static void lWarning(string message)
		{
			string msgstr = String.Format("{0} INFO {1}", logTimestamp(), message);
			Write(msgstr);
		}
		
		public static void lWarning(string message, string tclass, string tfunction)
		{
			string msgstr = String.Format("{0}: {1}: {2}", tclass, tfunction, message);
			lWarning(msgstr);
		}

		public static void lError(string message)
		{
			string msgstr = String.Format("{0} INFO {1}", logTimestamp(), message);
			Write(msgstr);

			Environment.Exit(1);
		}
		
		public static void lError(string message, string tclass, string tfunction)
		{
			string msgstr = String.Format("{0}: {1}: {2}", tclass, tfunction, message);
			lError(msgstr);
		}

		private static string logTimestamp()
		{
			DateTime dt = DateTime.Now;
			
			string currentTime = String.Format("{0:s}", dt);

			return currentTime;
		}
	}
}

