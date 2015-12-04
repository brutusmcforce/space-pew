using System.Diagnostics;
using Lidgren.Network;
using System.IO;
using SpacePew.Common;

namespace SpacePew.Networking
{
	public class StreamingClient
	{
		private FileStream _inputStream;
		private int _sentOffset;
		private int _chunkLen;
		private byte[] _tmpBuffer;
		private NetConnection _connection;

		public StreamingClient(NetConnection conn, string fileName)
		{
			_connection = conn;
			_inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			_chunkLen = _connection.Peer.Configuration.MaximumTransmissionUnit - 20;
			_tmpBuffer = new byte[_chunkLen];
			_sentOffset = 0;
		}

		public void Heartbeat()
		{
			if (_inputStream == null)
				return;

			if (_connection.CanSendImmediately(NetDeliveryMethod.ReliableOrdered, 1))
			{
				long remaining = _inputStream.Length - _sentOffset;
				int sendBytes = (remaining > _chunkLen ? _chunkLen : (int)remaining);

				_inputStream.Read(_tmpBuffer, 0, sendBytes);

				NetOutgoingMessage message;
				if (_sentOffset == 0)
				{
					message = _connection.Peer.CreateMessage(sendBytes + 8);
					message.Write((int)UdpNetworkPacketType.LevelResponse);
					message.Write((ulong)_inputStream.Length);
					message.Write(Path.GetFileName(_inputStream.Name));

					_connection.SendMessage(message, NetDeliveryMethod.ReliableOrdered, 1);
				}

				message = _connection.Peer.CreateMessage(sendBytes + 8);
				message.Write((int)UdpNetworkPacketType.LevelResponse);
				message.Write(_tmpBuffer, 0, sendBytes);

				_connection.SendMessage(message, NetDeliveryMethod.ReliableOrdered, 1);
				_sentOffset += sendBytes;

				if (remaining - sendBytes <= 0)
				{
					_inputStream.Close();
					_inputStream.Dispose();
					_inputStream = null;
				}
			}
		}
	}
}
