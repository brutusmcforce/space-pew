using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;
using SpacePew.Common;
using System.Net;

namespace SpacePew.Networking
{
	public abstract class UdpBase
	{
		protected Color[] _playerColors = 
        {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.Purple,
            Color.White,
            Color.Red,
            Color.LightBlue,
            Color.Orange,
            Color.Gray
        };

		protected byte[] ObjectToByteArray(object obj)
		{
			if (obj == null)
				return null;

			var memoryStream = new MemoryStream();
			var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
			var formatter = new BinaryFormatter();
			formatter.Serialize(gZipStream, obj);

			gZipStream.Close();
			memoryStream.Close();

			return memoryStream.ToArray();
		}

		protected object ByteArrayToObject(byte[] bytes)
		{
			var memoryStream = new MemoryStream();
			var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);

			var formatter = new BinaryFormatter();

			memoryStream.Write(bytes, 0, bytes.Length);
			memoryStream.Seek(0, SeekOrigin.Begin);

			var obj = formatter.Deserialize(gzipStream);

			gzipStream.Close();
			memoryStream.Close();

			return obj;
		}
	}
}
