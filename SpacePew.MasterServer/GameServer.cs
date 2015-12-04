using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpacePew.MasterServer
{
	public class GameServer
	{
		public long Id { get; set; }
		public IPEndPoint[] Endpoints { get; set; }
		public DateTime Updated { get; set; }
	}
}
