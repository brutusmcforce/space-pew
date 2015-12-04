using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using SpacePew.Common;

namespace SpacePew.MasterServer
{
	public class Server
	{
		public void Run()
		{
			var registeredHosts = new List<GameServer>();

			var config = new NetPeerConfiguration("masterserver");
			config.SetMessageTypeEnabled(NetIncomingMessageType.UnconnectedData, true);
			config.Port = Constants.MasterServerPort;

			var peer = new NetPeer(config);
			peer.Start();

			Console.WriteLine("Press ESC to quit");
			while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Escape)
			{
				var removed = registeredHosts.RemoveAll(g => g.Updated <= DateTime.Now.AddSeconds(-50));
				if (removed > 0)
				{
					Console.WriteLine("Removed {0} hosts from master server list", removed);
				}

				NetIncomingMessage msg;
				while ((msg = peer.ReadMessage()) != null)
				{
					switch (msg.MessageType)
					{
						case NetIncomingMessageType.UnconnectedData:
							switch ((UdpNetworkPacketType)msg.ReadByte())
							{
								case UdpNetworkPacketType.RegisterHost:
									var id = msg.ReadInt64();

									Console.WriteLine("Got registration for host " + id);

									var host = registeredHosts.FirstOrDefault(s => s.Id == id);
									if (host == null)
									{
										registeredHosts.Add(new GameServer()
										{
											Id = id,
											Endpoints = new IPEndPoint[]
											{
												msg.ReadIPEndPoint(),
												msg.SenderEndPoint
											},
											Updated = DateTime.Now
										});
									}
									else
									{
										host.Updated = DateTime.Now;
									}
									break;

								case UdpNetworkPacketType.RequestHostList:
									Console.WriteLine("Sending list of " + registeredHosts.Count + " hosts to client " + msg.SenderEndPoint);
									foreach (var server in registeredHosts)
									{
										var message = peer.CreateMessage();
										message.Write(server.Id);
										message.Write(server.Endpoints[0]);
										message.Write(server.Endpoints[1]);
										peer.SendUnconnectedMessage(message, msg.SenderEndPoint);
									}

									break;
								case UdpNetworkPacketType.RequestIntroduction:
									var clientInternal = msg.ReadIPEndPoint();
									long hostId = msg.ReadInt64();
									string token = msg.ReadString();

									Console.WriteLine(msg.SenderEndPoint + " requesting introduction to " + hostId + " (token " + token + ")");

									var host2 = registeredHosts.FirstOrDefault(s => s.Id == hostId);
									if (host2 != null)
									{
										Console.WriteLine("Sending introduction...");
										peer.Introduce(
											host2.Endpoints[0], // host internal
											host2.Endpoints[1], // host external
											clientInternal, // client internal
											msg.SenderEndPoint, // client external
											token // request token
										);
									}
									else
									{
										Console.WriteLine("Client requested introduction to nonlisted host");
									}
									break;
							}
							break;

						case NetIncomingMessageType.DebugMessage:
						case NetIncomingMessageType.VerboseDebugMessage:
						case NetIncomingMessageType.WarningMessage:
						case NetIncomingMessageType.ErrorMessage:
							Console.WriteLine(msg.ReadString());
							break;
					}
				}
			}

			peer.Shutdown("bye");
		}
	}
}
