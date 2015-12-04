using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpacePew.Networking
{
	public class ScoreBoardItem
	{
		public string Name { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public DateTime Joined { get; set; }
		public long Ping { get; set; }
	}
}
