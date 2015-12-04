using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Xna.Framework;

using SpacePew.Extensions;

using Lidgren.Network;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using System.Net.NetworkInformation;
using SpacePew.Models;
using SpacePew.Common;

namespace SpacePew.Networking
{
	public class UdpServer : UdpBase
	{
		private NetPeerConfiguration _configuration;
		private NetServer _server;
		private Level _level;

		private readonly List<ScoreBoardItem> _scoreBoard;
		private readonly List<string> _players;

		private readonly Vector2 _defaultPosition = new Vector2(300, 280);

		private IPEndPoint _masterServerEndpoint;

		public UdpServer()
			: base()
		{
			_masterServerEndpoint = NetUtility.Resolve("spacepew.wodanaz.se", Constants.MasterServerPort); // TODO: Fixa upp masterserver någonstans
			_scoreBoard = new List<ScoreBoardItem>();
			_players = new List<string>();
		}

		public void CreateSession()
		{
			if (_server == null)
			{
				_configuration = new NetPeerConfiguration("SpacePew")
				{
					MaximumConnections = 16,
					Port = SpacePew.Common.Constants.GameServerPort
				};

				_configuration.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
				_configuration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
				_configuration.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);

				_server = new NetServer(_configuration);
				_server.Start();
			}
		}

		public void SetLevel(Level level)
		{
			_level = level;
		}

		public void Shutdown()
		{
			if (_server != null && _server.Status == NetPeerStatus.Running)
			{
				_server.Shutdown("Quitting");
			}
		}

		public void Listen()
		{

			System.Diagnostics.Trace.WriteLine("Server.Listen()");
			while (_server.Status == NetPeerStatus.Running)
			{
				NetIncomingMessage message;
				while ((message = _server.ReadMessage()) != null)
				{
					ReadBuffer(message, message.SenderConnection);
				}

				if (NetTime.Now > lastRegistered + 60)
				{
					RegisterWithMasterServer();
				}

				StreamLevelToClients();
			}
		}

		private void StreamLevelToClients()
		{
			foreach (var conn in _server.Connections)
			{
				var client = conn.Tag as StreamingClient;
				if (client != null)
					client.Heartbeat();
			}
		}

		float lastRegistered = -60.0f;
		private void RegisterWithMasterServer()
		{
			IPAddress mask;
			IPAddress localAddress = NetUtility.GetMyAddress(out mask);
			var message = _server.CreateMessage();
			message.Write((byte)UdpNetworkPacketType.RegisterHost);
			message.Write(_server.UniqueIdentifier);
			message.Write(new IPEndPoint(localAddress, Constants.GameServerPort));

			_server.SendUnconnectedMessage(message, _masterServerEndpoint);

			lastRegistered = (float)NetTime.Now;
		}

		private void ReadBuffer(NetIncomingMessage message, NetConnection sender)
		{
			switch (message.MessageType)
			{
				case NetIncomingMessageType.Data:
				{
					var packetType = (UdpNetworkPacketType)message.ReadInt32();
					switch (packetType)
					{
						case UdpNetworkPacketType.PlayerUpdate:
							WritePlayerUpdate(message, sender);
							break;
						case UdpNetworkPacketType.EntitiesCreated:
							WriteEntities(message, sender);
							break;
						case UdpNetworkPacketType.MessageSent:
							WriteMessage(message, sender);
							break;
						case UdpNetworkPacketType.EntityCreated:
							WriteEntity(message, sender);
							break;
						case UdpNetworkPacketType.PlayerDying:
							WritePlayerDied(message, sender);
							break;
						case UdpNetworkPacketType.RequestingScoreboard:
							WriteScoreBoard(sender);
							break;
						case UdpNetworkPacketType.PlayerJoining:
						{
							string name = message.ReadString();
							_players.Add(name);
							sender.Tag = name;
							_scoreBoard.Add(new ScoreBoardItem() { Name = name, Joined = DateTime.Now });

							WritePlayerJoinedPacket(sender, name);
							break;
						}
						case UdpNetworkPacketType.PlayerDisconnecting:
						{
							string name = message.ReadString();

							WritePlayerDisconnectedPacket(sender, name);
							break;
						}
						case UdpNetworkPacketType.LevelRequest:
						{
							WriteLevelResponse(message);
							break;
						}
					}
				}
					break;
				case NetIncomingMessageType.ConnectionApproval:
					AuthorizeConnection(sender);
					break;
				case NetIncomingMessageType.DiscoveryRequest:
					WriteDiscoveryResponse(sender);
					break;
				case NetIncomingMessageType.StatusChanged:
				{
					var status = (NetConnectionStatus)message.ReadByte();
					var msg = message.ReadString();
					break;
				}
			}

			_server.Recycle(message);
		}

		private void WriteLevelResponse(NetIncomingMessage message)
		{
			message.SenderConnection.Tag = new StreamingClient(message.SenderConnection, _level.FilePath);
		}

		private void WriteDiscoveryResponse(NetConnection sender)
		{
			if (sender == null)
				return;

			var message = _server.CreateMessage();

			// TODO: Skicka med lite stats
			_server.SendDiscoveryResponse(message, sender.RemoteEndPoint);
		}

		private void WritePlayerDisconnectedPacket(NetConnection sender, string name)
		{
			var message = _server.CreateMessage();

			message.Write((int)UdpNetworkPacketType.PlayerDisconnected);
			message.Write(name);

			_server.SendToAll(message, sender, NetDeliveryMethod.ReliableUnordered, 0);

			_players.Remove(name);
		}

		private void WriteScoreBoard(NetConnection sender)
		{
			var message = _server.CreateMessage();

			message.Write((int)UdpNetworkPacketType.SendingScoreBoard);

			int count = _scoreBoard.Count;

			message.Write(count);

			var ping = new Ping();

			foreach (var item in _scoreBoard)
			{
				message.Write(item.Name);
				message.Write(item.Kills);
				message.Write(item.Deaths);
				message.Write(item.Joined.ToString(CultureInfo.InvariantCulture));

				//TODO: Fixa riktig ping
				//NetConnection connection = _server.Connections.Find(s => s.Tag.ToString() == item.Name);

				//long pingTime = ping.Send(connection.RemoteEndpoint.Address).RoundtripTime;
				const long pinglong = 14;
				message.Write(pinglong);
			}

			_server.SendMessage(message, sender, NetDeliveryMethod.UnreliableSequenced);
		}

		private void WriteMessage(NetIncomingMessage incomingMessage, NetConnection sender)
		{
			Vector3 colorVector = incomingMessage.ReadVector3();
			string dateString = incomingMessage.ReadString();
			string message = incomingMessage.ReadString();
			bool isChatMessage = incomingMessage.ReadBoolean();

			var outgoingMessage = _server.CreateMessage();

			outgoingMessage.Write((int)UdpNetworkPacketType.MessageReceived);
			outgoingMessage.Write(colorVector);
			outgoingMessage.Write(dateString);
			outgoingMessage.Write(message);
			outgoingMessage.Write(isChatMessage);

			_server.SendToAll(outgoingMessage, sender, NetDeliveryMethod.UnreliableSequenced, 1);
		}

		private void WritePlayerDied(NetIncomingMessage incomingMessage, NetConnection sender)
		{
			string playerName = incomingMessage.ReadString();
			string killedBy = incomingMessage.ReadString();

			if (playerName != killedBy)
			{
				AddKillScore(killedBy);
			}

			AddDeathScore(playerName);

			var message = _server.CreateMessage();

			message.Write((int)UdpNetworkPacketType.PlayerDied);
			message.Write(playerName);

			_server.SendToAll(message, sender, NetDeliveryMethod.ReliableUnordered, 0);
		}

		private void AddKillScore(string name)
		{
			ScoreBoardItem item = _scoreBoard.Find(i => i.Name == name);
			item.Kills++;
			_scoreBoard[_scoreBoard.IndexOf(item)] = item;
		}

		private void AddDeathScore(string name)
		{
			ScoreBoardItem item = _scoreBoard.Find(i => i.Name == name);
			item.Deaths++;
			_scoreBoard[_scoreBoard.IndexOf(item)] = item;
		}

		private void WriteEntities(NetIncomingMessage incomingMessage, NetConnection sender)
		{
			int count = incomingMessage.ReadInt32();

			var message = _server.CreateMessage();

			message.Write((int)UdpNetworkPacketType.EntitiesCreated);
			message.Write(count);

			for (int i = 0; i < count; i++)
			{
				message.Write(incomingMessage.ReadString());
				message.Write(incomingMessage.ReadString());
				message.Write(incomingMessage.ReadVector2());
				message.Write(incomingMessage.ReadVector2());
				message.Write(incomingMessage.ReadFloat());
			}

			_server.SendToAll(message, sender, NetDeliveryMethod.UnreliableSequenced, 1);
		}

		private void AuthorizeConnection(NetConnection sender)
		{
			string name = Encoding.ASCII.GetString(sender.RemoteHailMessage.Data);
			if (!string.IsNullOrEmpty(name) && !_players.Contains(name))
			{
				sender.Approve();
			}
			else
			{
				sender.Deny("A player with that name is already playing.");
			}
		}

		private void WritePlayerJoinedPacket(NetConnection sender, string name)
		{
			System.Diagnostics.Trace.WriteLine("In WritePlayerJoined()...");

			var message = _server.CreateMessage();

			message.Write((int)UdpNetworkPacketType.PlayerJoined);

			message.Write(name);
			message.Write(_server.Connections.IndexOf(sender));
			message.Write(_defaultPosition); // TODO: positions should be stored on the level somehow
			message.Write(Vector2.Zero);
			message.Write(0f);

			_level.BuildCollisionDataFromLevel();

			byte[] collisionData = ObjectToByteArray(_level.CollisionData);
			message.Write(collisionData.Length);
			message.Write(collisionData);

			_server.SendMessage(message, sender, NetDeliveryMethod.ReliableUnordered);
			System.Diagnostics.Trace.WriteLine("Sent joined...");
		}

		private void WriteEntity(NetIncomingMessage message, NetConnection sender)
		{
			string entityType = message.ReadString();
			string owner = message.ReadString();
			Vector2 pos = message.ReadVector2();
			Vector2 velocity = message.ReadVector2();
			float angle = message.ReadFloat();

			WriteEntityPacket(sender, entityType, owner, ref pos, ref velocity, angle);
		}

		private void WriteEntityPacket(NetConnection connectionToExclude, string entityType, string owner, ref Vector2 pos, ref Vector2 velocity, float angle)
		{
			var message = _server.CreateMessage();

			message.Write((int)UdpNetworkPacketType.EntityCreated);
			message.Write(entityType);
			message.Write(owner);
			message.Write(pos);
			message.Write(velocity);
			message.Write(angle);

			_server.SendToAll(message, connectionToExclude, NetDeliveryMethod.UnreliableSequenced, 1);
		}

		private void WritePlayerUpdate(NetIncomingMessage message, NetConnection sender)
		{
			string owner = message.ReadString();
			int colorIndex = message.ReadInt32();
			Vector2 pos = message.ReadVector2();
			Vector2 velocity = message.ReadVector2();
			float angle = message.ReadFloat();

			WritePlayerUpdatePacket(sender, owner, colorIndex, ref pos, ref velocity, angle);
		}

		private void WritePlayerUpdatePacket(NetConnection connectionToExclude, string owner, int colorIndex, ref Vector2 pos, ref Vector2 velocity, float angle)
		{
			var message = _server.CreateMessage();

			message.Write((int)UdpNetworkPacketType.PlayerUpdate);
			message.Write(owner);
			message.Write(colorIndex);
			message.Write(pos);
			message.Write(velocity);
			message.Write(angle);

			_server.SendToAll(message, connectionToExclude, NetDeliveryMethod.UnreliableSequenced, 1);
		}
	}
}