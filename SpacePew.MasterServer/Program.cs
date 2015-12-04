using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpacePew.MasterServer
{
	public class Program
	{
		static void Main(string[] args)
		{
			var server = new Server();
			server.Run();
		}
	}
}
