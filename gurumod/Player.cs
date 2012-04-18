using System;

namespace gurumod
{
	public class Player
	{
		public Track TheTrack = new Track();
		private bool KeepRunning = true;
		
		public Player ()
		{
			TheTrack.EnablePlayer();
		}
		
		public void Run()
		{
			while(KeepRunning)
			{
				TheTrack.TakeTurn();
			}
		}
	}
}

