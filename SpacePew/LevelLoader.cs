using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using SpacePew.Models;
using Rectangle = System.Drawing.Rectangle;

namespace SpacePew
{
	static class LevelLoader
	{
		public static Level LoadLevel(string filePath, ContentManager cm, GraphicsDevice device)
		{
			var levelResources = GetResourceList(filePath);
			if (!levelResources.ContainsKey("leveldata.xml"))
				throw new InvalidOperationException("Level: " + filePath + " doesnt contain any leveldata.xml");
			
			var levelData = GetXmlFile(levelResources["leveldata.xml"]);
			string baseTexture = levelData.SelectSingleNode("//baseTexture").InnerText.ToLower();
			string indestructibleTexture = levelData.SelectSingleNode("//indestructibleTexture").InnerText.ToLower();

			var spriteBatch = new SpriteBatch(device);
			var level = new Level();
			level.FilePath = filePath;
			level.Texture = LoadTextureTiles(levelResources[baseTexture], device, spriteBatch);
			level.IndestructibleTexture = LoadTextureTiles(levelResources[indestructibleTexture], device, spriteBatch);
			level.DeformedTexture = new TiledTexture
			{
				Width = level.Texture.Width,
				Height = level.Texture.Height,
				TileHeight = level.Texture.TileHeight,
				TileWidth = level.Texture.TileWidth,
				XTiles = level.Texture.XTiles,
				YTiles = level.Texture.YTiles
			};

			level.Texture.ForEach(t => level.DeformedTexture.Add(new Tile(
					new RenderTarget2D(device, level.Texture.TileWidth, level.Texture.TileHeight, false,
							SurfaceFormat.Color, DepthFormat.Depth24,
							device.PresentationParameters.MultiSampleCount,
							RenderTargetUsage.PreserveContents
					),
					t.Position)));

			if (levelData.SelectSingleNode("//song") != null)
			{
				string songName = levelData.SelectSingleNode("//song").InnerText.ToLower();
				level.OggVorbisSong = levelResources[songName];
			}

			level.Initialize();
			return level;
		}

		private static XmlDocument GetXmlFile(byte[] rawData)
		{
			var d = new XmlDocument();
			d.LoadXml(System.Text.Encoding.UTF8.GetString(rawData));
			return d;
		}

		public static TiledTexture LoadTextureTiles(Byte[] bitmapData, GraphicsDevice device, SpriteBatch spriteBatch, int tileWidth = 0, int tileHeight = 0)
		{
			using (var ms = new MemoryStream(bitmapData))
			{
				ms.Seek(0, SeekOrigin.Begin);
				using (var image = Image.FromStream(ms))
				{
					return LoadTextureTiles(image, device, spriteBatch, tileWidth, tileHeight);
				}
			}
		}

		public static TiledTexture LoadTextureTiles(string fileName, GraphicsDevice device, SpriteBatch spriteBatch, int tileWidth = 0, int tileHeight = 0)
		{
			using (var image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + fileName.Replace("/", "\\")))
			{
				return LoadTextureTiles(image, device, spriteBatch, tileWidth, tileHeight);
			}
		}

		public static TiledTexture LoadTextureTiles(Image image, GraphicsDevice device, SpriteBatch spriteBatch, int tileWidth = 0, int tileHeight = 0)
		{
			//OBS!!! Sätt kartans bredd el höjd till ett primtal o det är kört iom att då kan man inte hitta en jämn multiple och en ojämn kommer att användas
			//och det kommer att pricka i framtiden.
			if (tileWidth == 0)
				tileWidth = FindNearbyMultiple(image.Width, 1000);
			if (tileHeight == 0)
				tileHeight = FindNearbyMultiple(image.Height, 1000);


			int tilesX = (image.Width % tileWidth == 0)
											 ? (int)Math.Floor(image.Width / (double)tileWidth)
											 : (int)Math.Ceiling(image.Width / (double)tileWidth);

			int tilesY = (image.Height % tileHeight == 0)
											 ? (int)Math.Floor(image.Height / (double)tileHeight)
											 : (int)Math.Ceiling(image.Height / (double)tileHeight);

			var ret = new TiledTexture
					{
						TileWidth = tileWidth,
						TileHeight = tileHeight,
						XTiles = tilesX,
						YTiles = tilesY,
						Width = image.Width,
						Height = image.Height
					};
			for (var j = 0; j < tilesX; j++)
			{
				for (var i = 0; i < tilesY; i++)
				{
					var part = new Bitmap(tileWidth, tileHeight, image.PixelFormat);
					var graphics = Graphics.FromImage(part);
					var srcRect = new Rectangle(j * tileWidth, i * tileHeight, tileWidth, tileHeight);
					graphics.DrawImage(image, 0, 0, srcRect, GraphicsUnit.Pixel);
					using (var ms = new MemoryStream())
					{
						//TODO: något vettigare än så här för att få fram bytesen... tar ju år och dagar att ladda nu.
						part.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
						ms.Seek(0, SeekOrigin.Begin);
						
						ret.Add(new Tile(Texture2D.FromStream(device, ms), new Microsoft.Xna.Framework.Point(j * tileWidth, i * tileHeight)));
					}
				}
			}
			return ret;
		}

		private static int FindNearbyMultiple(int totLen, int chunkLen)
		{
			chunkLen = totLen / (int)Math.Ceiling(totLen / (double)chunkLen);

			var step = 0;
			while (step < 100)
			{
				var t = chunkLen - step;
				if ((totLen % t) == 0)
					return t;
				t = chunkLen + step;
				if ((totLen % t) == 0)
					return t;
				step++;
			}
			return FindNearbyMultiple(totLen - 1, chunkLen);
		}

		private static Dictionary<string, byte[]> GetResourceList(string file)
		{
			var resources = new Dictionary<string, byte[]>();
			using (var stream = new ZipInputStream(File.OpenRead(file)))
			{
				ZipEntry entry = null;
				while ((entry = stream.GetNextEntry()) != null)
				{
					if (entry.Name.Length > 0)
					{
						var buf = new byte[(int)entry.Size];
						stream.Read(buf, 0, buf.Length);
						resources.Add(entry.Name.ToLower().Trim(), buf);
					}
				}
			}

			return resources;
		}
	}
}