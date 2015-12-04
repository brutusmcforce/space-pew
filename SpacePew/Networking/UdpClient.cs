using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpacePew.Extensions;
using Lidgren.Network;
using System.Collections;
using SpacePew.Models;
using SpacePew.Common;
using System.Net;

namespace SpacePew.Networking
{
	public class UdpClient : UdpBase
	{
		private string _playerName;
		private readonly NetClient _client;
		private Level _level;

		private readonly MainGame _game;

		private int _colorIndex;

		private IPEndPoint _masterServerEndpoint;

		private UdpClient()
		{

		}

		public UdpClient(MainGame game)
		{
			_game = game;
			_masterServerEndpoint = NetUtility.Resolve("spacepew.wodanaz.se", Constants.MasterServerPort); // TODO: Fixa upp masterserver någonstans
			EntitiesToSend = new List<IEntity>();

			var configuration = new NetPeerConfiguration("SpacePew");

			configuration.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
			configuration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			configuration.EnableMessageType(NetIncomingMessageType.UnconnectedData);
			configuration.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);

			_client = new NetClient(configuration);
			_client.Start();
		}

		public List<IEntity> EntitiesToSend { get; set; }

		public NetClient CurrentClient
		{
			get { return _client; }
		}

		private Player _localPlayer;
		public Player LocalPlayer
		{
			get
			{
				return _localPlayer;
			}
			set
			{
				_localPlayer = value;
			}
		}

		private readonly List<Player> _players = new List<Player>();
		public IList<Player> Players
		{
			get
			{
				return _players;
			}
		}

		public bool IsSessionAlive
		{
			get
			{
				return _client != null && _client.ConnectionStatus == NetConnectionStatus.Connected;
			}
		}

		public void JoinSession(string host, string playerName)
		{
			JoinSession(new IPEndPoint(System.Net.Dns.GetHostAddresses(host)[0], SpacePew.Common.Constants.GameServerPort), playerName);
		}

		public void JoinSession(IPEndPoint endpoint, string playerName)
		{
			_playerName = playerName;

			var hailMessage = _client.CreateMessage(playerName);
			_client.Connect(endpoint, hailMessage);

			while (_client.ServerConnection == null)
			{
				// wait
			}

			System.Threading.Thread.Sleep(500); // TODO: nya lidgren är dumt i huvet, grotta i vad skiten sätter i någon tråd se'n

			System.Diagnostics.Trace.WriteLine("Got server connection...");
			WriteLevelRequest();
			WaitForLevel();
			WriteJoiningMessage();
			WaitForJoin();
			
			if (_level.OggVorbisSong != null)
			{
				new Thread(_level.PlayLevelSong).Start();
			}
		}

		public void ExitSession(string reason)
		{
			var message = _client.CreateMessage();

			message.Write((int)UdpNetworkPacketType.PlayerDisconnecting);
			message.Write(_playerName);

			_client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);

			_client.Disconnect(reason);
		}

		public void SendMessage(NetworkMessage message)
		{
			var netMessage = _client.CreateMessage();

			netMessage.Write((int)UdpNetworkPacketType.MessageSent);
			netMessage.Write(message.Color.ToVector3());
			netMessage.Write(message.Sent.ToString(CultureInfo.InvariantCulture));
			netMessage.Write(message.Message);
			netMessage.Write(message.IsChatMessage);

			_client.SendMessage(netMessage, NetDeliveryMethod.UnreliableSequenced);
		}

		public void RequestScoreBoard()
		{
			var message = _client.CreateMessage();

			message.Write((int)UdpNetworkPacketType.RequestingScoreboard);

			_client.SendMessage(message, NetDeliveryMethod.UnreliableSequenced);
		}

		private void ReadMessage(NetIncomingMessage netMessage)
		{
			var color = new Color(netMessage.ReadVector3());
			var sent = DateTime.Parse(netMessage.ReadString(), CultureInfo.InvariantCulture);
			string message = netMessage.ReadString();
			bool isChatMessage = netMessage.ReadBoolean();

			NetworkMessenger.DisplayMessage(new NetworkMessage() { Color = color, Sent = sent, Message = message, IsChatMessage = isChatMessage});
		}

		public void SendDeath(IEntity killedBy)
		{
			var message = _client.CreateMessage();

			message.Write((int)UdpNetworkPacketType.PlayerDying);
			message.Write(_playerName);
			message.Write(killedBy.Owner); // TODO: server side score board

			_client.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
		}

		private void WriteLevelRequest()
		{
			Trace.WriteLine("WriteLevelRequest");
			var message = _client.CreateMessage();

			message.Write((int)UdpNetworkPacketType.LevelRequest);

			_client.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
			System.Diagnostics.Trace.WriteLine("Sent level request...");
		}

		private void WriteJoiningMessage()
		{
			Trace.WriteLine("WriteJoiningMessage");
			
			var message = _client.CreateMessage();

			message.Write((int)UdpNetworkPacketType.PlayerJoining);
			message.Write(_playerName);

			_client.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
		}

		private void WaitForJoin()
		{
			System.Diagnostics.Trace.WriteLine("WaitForJoin");
			bool hasAnswer = false;
			while (!hasAnswer) // wait for answer
			{
				NetIncomingMessage message;
				while ((message = _client.ReadMessage()) != null)
				{
					if (message.MessageType == NetIncomingMessageType.Data)
					{
						var packetType = (UdpNetworkPacketType)message.ReadInt32();
						if (packetType == UdpNetworkPacketType.PlayerJoined)
						{
							string owner = message.ReadString();

							int colorIndex = message.ReadInt32();
							Color color = _playerColors[colorIndex];

							_colorIndex = Array.IndexOf(_playerColors, color);

							Vector2 pos = message.ReadVector2();
							Vector2 velocity = message.ReadVector2();
							float angle = message.ReadFloat();
							int collisionSize = message.ReadInt32();
							var collisionData = message.ReadBytes(collisionSize);

							_level.CollisionData = (bool[])ByteArrayToObject(collisionData);
							_level.BuildLevelFromCollisionData();

							Player p = Player.CreatePlayer(pos, owner);
							p.Color = color;
							p.Position = pos;
							p.Velocity = velocity;
							p.Angle = angle;

							_localPlayer = p;
							_players.Add(p);

							hasAnswer = true;
						}
					}
				}
			}
		}

		public void Update()
		{
			NetIncomingMessage message;

			while ((message = _client.ReadMessage()) != null)
			{
				ReadBuffer(message);
			}

			if (EntitiesToSend.Count > 0)
			{
				WriteServerShots();
			}

			WritePlayerUpdatePacket();
		}

		private void WriteServerShots()
		{
			SendEntities(EntitiesToSend);
		}

		private void WritePlayerUpdatePacket()
		{
			var message = _client.CreateMessage();

			message.Write((int)UdpNetworkPacketType.PlayerUpdate);
			message.Write(_localPlayer.Owner);
			message.Write(_colorIndex);
			message.Write(_localPlayer.Position);
			message.Write(_localPlayer.Velocity);
			message.Write(_localPlayer.Angle);
			message.Write(_localPlayer.Health);

			_client.SendMessage(message, NetDeliveryMethod.UnreliableSequenced);
		}

		private void ReadBuffer(NetIncomingMessage message)
		{
			switch (message.MessageType)
			{
				case NetIncomingMessageType.Data:
					{
						var packetType = (UdpNetworkPacketType)message.ReadInt32();

						switch (packetType)
						{
							case UdpNetworkPacketType.PlayerUpdate:
								ReadPlayers(message);
								break;
							case UdpNetworkPacketType.EntitiesCreated:
								ReadEntities(message);
								break;
							case UdpNetworkPacketType.MessageReceived:
								ReadMessage(message);
								break;
							case UdpNetworkPacketType.PlayerDied:
								ReadPlayerDied(message);
								break;
							case UdpNetworkPacketType.SendingScoreBoard:
								ReadScoreBoard(message);
								break;
							case UdpNetworkPacketType.EntityCreated:
								ReadEntity(message);
								break;
							case UdpNetworkPacketType.PlayerDisconnected:
								RemovePlayer(message);
								break;
							case UdpNetworkPacketType.LevelResponse:
								ReadLevel(message);
								break;
						}
					}
					break;
			}

			_client.Recycle(message);
		}

		private void WaitForLevel()
		{
			System.Diagnostics.Trace.WriteLine("WaitForLevel");
			while (_level == null) // wait for level
			{
				NetIncomingMessage message;
				while ((message = _client.ReadMessage()) != null)
				{
					if (message.MessageType == NetIncomingMessageType.Data)
					{
						var packetType = (UdpNetworkPacketType)message.ReadInt32();
						if (packetType == UdpNetworkPacketType.LevelResponse)
						{
							ReadLevel(message);
						}
					}
				}
			}
		}

		private static ulong _levelLength;
		private static ulong _levelReceived;
		private static FileStream _levelWriteStream;
		private static int _levelTimeStarted;
		string levelPath = string.Empty;

		private void ReadLevel(NetIncomingMessage message)
		{
			int chunkLen = message.LengthBytes;
			if (_levelLength == 0)
			{
				_levelLength = message.ReadUInt64();
				string filename = message.ReadString();

				var downloadLevelFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads\\Levels\\");
				levelPath = Path.Combine(downloadLevelFolder, filename);

				_levelWriteStream = new FileStream(levelPath, FileMode.Create, FileAccess.Write, FileShare.None);
				_levelTimeStarted = Environment.TickCount;

				return;
			}

			byte[] all = message.ReadBytes(message.LengthBytes - 4); // offset for UdpNetworkPacketType
			_levelReceived += (ulong)all.Length;
			_levelWriteStream.Write(all, 0, all.Length);

			//int v = (int)(((float)_levelReceived / (float)_levelLength) * 100.0f);
			//int passed = Environment.TickCount - _levelTimeStarted;
			//double psec = (double)passed / 1000.0;
			//double bps = (double)_levelReceived / psec;

			//var passedText = NetUtility.ToHumanReadable((long)bps) + " per second";

			if (_levelReceived >= _levelLength)
			{
				Trace.WriteLine("Got level");
				_levelWriteStream.Flush();
				_levelWriteStream.Close();
				_levelWriteStream.Dispose();

				_level = LevelLoader.LoadLevel(levelPath, _game.Content, _game.GraphicsDevice);
				_game.Level = _level;
			}
		}

		private void RemovePlayer(NetIncomingMessage message)
		{
			string name = message.ReadString();

			Player player = _players.Find(p => p.Owner == name);
			if (player != null)
			{
				EntityFactory.Instance.RemoveEntity(player);

				NetworkMessenger.DisplayMessage(new NetworkMessage()
																				 {
																					 Color = player.Color,
																					 Message = string.Format("{0} has left.", player.Owner),
																					 Sent = DateTime.Now
																				 });

				_players.Remove(player);
			}
		}

		private void ReadScoreBoard(NetIncomingMessage message)
		{
			int count = message.ReadInt32();

			var scoreBoard = new List<ScoreBoardItem>();

			for (int i = 0; i < count; i++)
			{
				var item = new ScoreBoardItem
				{
					Name = message.ReadString(),
					Kills = message.ReadInt32(),
					Deaths = message.ReadInt32(),
					Joined = DateTime.Parse(message.ReadString(), CultureInfo.InvariantCulture),
					Ping = message.ReadInt64()
				};

				scoreBoard.Add(item);
			}

			ScoreBoard.CurrentScoreBoard = scoreBoard;
		}

		private void ReadPlayerDied(NetIncomingMessage message)
		{
			string playerName = message.ReadString();

			Player player = _players.Find(p => p.Owner == playerName);

			if (player != null)
			{
				_game.AddExplosion(player.Position, 1f);
				player.Kill();
			}
		}

		private void ReadEntities(NetIncomingMessage message)
		{
			int count = message.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string entityType = message.ReadString();
				string owner = message.ReadString();
				Vector2 pos = message.ReadVector2();
				Vector2 velocity = message.ReadVector2();
				float angle = message.ReadFloat();

				EntityFactory.Instance.CreateEntity<IEntity>(Type.GetType(entityType), owner, pos, velocity, angle);
			}
		}

		private void ReadEntity(NetIncomingMessage message)
		{
			string entityType = message.ReadString();
			string owner = message.ReadString();
			Vector2 pos = message.ReadVector2();
			Vector2 velocity = message.ReadVector2();
			float angle = message.ReadFloat();

			EntityFactory.Instance.CreateEntity<IEntity>(Type.GetType(entityType), owner, pos, velocity, angle);
		}

		private void ReadPlayers(NetIncomingMessage message)
		{
			string owner = message.ReadString();

			int colorIndex = message.ReadInt32();
			Color color = _playerColors[colorIndex];

			Vector2 pos = message.ReadVector2();
			Vector2 velocity = message.ReadVector2();
			float angle = message.ReadFloat();

			if (owner == LocalPlayer.Owner)
				return;

			Player player = _players.Find(p => p.Owner == owner);
			if (player == null)
			{
				Player p = Player.CreatePlayer(new Vector2(300, 280), owner);

				p.Position = pos;
				p.Color = color;
				p.Velocity = velocity;
				p.Angle = angle;
				p.Owner = owner;

				_players.Add(p);

				NetworkMessenger.DisplayMessage(new NetworkMessage()
																				 {
																					 Color = p.Color,
																					 Message = string.Format("{0} has joined.", p.Owner),
																					 Sent = DateTime.Now
																				 });
			}
			else
			{
				int index = _players.IndexOf(player);
				player.Owner = owner;
				player.Color = color;
				player.Position = pos;
				player.Velocity = velocity;
				player.Angle = angle;

				_players[index] = player;
			}
		}

		public void SendEntities(List<IEntity> entities)
		{
			var message = _client.CreateMessage();

			message.Write((int)UdpNetworkPacketType.EntitiesCreated);
			message.Write(entities.Count);

			lock (EntitiesToSend)
			{
				for (int i = entities.Count - 1; i >= 0; i--)
				{
					message.Write(entities[i].GetType().ToString());
					message.Write(_localPlayer.Owner);
					message.Write(entities[i].Position);
					message.Write(entities[i].Velocity);
					message.Write(entities[i].Angle);
				}
			}

			EntitiesToSend.Clear();

			_client.SendMessage(message, NetDeliveryMethod.UnreliableSequenced);
		}

		public void SendEntity(IEntity entity)
		{
			var message = _client.CreateMessage();

			message.Write((int)UdpNetworkPacketType.EntityCreated);
			message.Write(entity.GetType().ToString());
			message.Write(_localPlayer.Owner);
			message.Write(entity.Position);
			message.Write(entity.Velocity);
			message.Write(entity.Angle);

			_client.SendMessage(message, NetDeliveryMethod.UnreliableSequenced);
		}

		public void GetServerList()
		{
			var listRequest = _client.CreateMessage();
			listRequest.Write((byte)UdpNetworkPacketType.RequestHostList);
			_client.SendUnconnectedMessage(listRequest, _masterServerEndpoint);
		}

		public void RequestNATIntroduction(long hostid)
		{
			if (hostid == 0)
			{
				return;
			}

			if (_masterServerEndpoint == null)
				throw new Exception("Must connect to master server first!");

			IPAddress mask;
			var message = _client.CreateMessage();
			message.Write((byte)UdpNetworkPacketType.RequestIntroduction);

			message.Write(new IPEndPoint(NetUtility.GetMyAddress(out mask), _client.Port));
			message.Write(hostid);
			message.Write(_playerName);

			_client.SendUnconnectedMessage(message, _masterServerEndpoint);
		}
	}
}
