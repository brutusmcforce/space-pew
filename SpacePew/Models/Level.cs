using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;
using Microsoft.Xna.Framework.Media;

namespace SpacePew.Models
{
	public class Tile
	{
		public Point Position { get; set; }
		public Texture2D Texture { get; set; }
		public Tile(Texture2D texure, Point position)
		{
			Texture = texure;
			Position = position;
		}
	}

	public class TiledTexture : List<Tile>
	{
		public int TileWidth { get; set; }
		public int TileHeight { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int XTiles { get; set; }
		public int YTiles { get; set; }
		public Vector2 Position { get; set; }

		public void GetData(Color[] textureData)
		{
			var buffer = new Color[TileWidth * TileHeight];
			for (var j = 0; j < XTiles; j++)
			{
				for (var i = 0; i < YTiles; i++)
				{
					var tile = this[i * XTiles + j];
					tile.Texture.GetData(buffer);

					for (var x = 0; x < TileWidth; x++)
					{
						for (var y = 0; y < TileHeight; y++)
						{
							var xpos = tile.Position.X + x;
							var ypos = tile.Position.Y + y;
	 						textureData[ypos * Width + xpos] = buffer[y * TileWidth + x];
						}
					}
				}
			}
		}

		public void SetData(Color[] textureData)
		{
			var buffer = new Color[TileWidth * TileHeight];
			for (var j = 0; j < XTiles; j++)
			{
				for (var i = 0; i < YTiles; i++)
				{
					var tile = this[i * XTiles + j];
					for (var x = 0; x < TileWidth; x++)
					{
						for (var y = 0; y < TileHeight; y++)
						{
							var xpos = tile.Position.X + x;
							var ypos = tile.Position.Y + y;
							buffer[y * TileWidth + x] = textureData[ypos * Width + xpos];
						}
					}

					tile.Texture.SetData(buffer);
				}
			}
		}

		public void SaveAsPng(Stream s, int width, int height)
		{
			throw new NotImplementedException("Har inte hunnit med.. borde gå att kopiera nån av Get/Set ovan och skapa upp en bild.");
		}
	}

	public class Level
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string FilePath { get; set; }
		public byte[] OggVorbisSong { get; set; }
		public TiledTexture Texture { get; set; }
		public TiledTexture IndestructibleTexture { get; set; }
		public TiledTexture DeformedTexture { get; set; }
		private Color[] _deformedTextureData;

		public Queue<MapHit> Hits { get; set; }

		public bool[] CollisionData
		{
			get;
			set;
		}

		public bool[] IndestructibleCollisionData
		{
			get;
			set;
		}

		private int _width;
		private int _height;

		public Level(GraphicsDevice device)
		{
		}

		public Level()
		{ }

		public void Initialize()
		{
			_width = Texture.Width;
			_height = Texture.Height;

			Hits = new Queue<MapHit>();

			var textureData = new Color[Texture.Width * Texture.Height];
			Texture.GetData(textureData);

			var indestructibleTextureData = new Color[IndestructibleTexture.Width * IndestructibleTexture.Height];
			IndestructibleTexture.GetData(indestructibleTextureData);

			CollisionData = new bool[Texture.Width * Texture.Height];

			_deformedTextureData = new Color[DeformedTexture.Width * DeformedTexture.Height];
			for (int i = _deformedTextureData.Length - 1; i >= 0; i--)
			{
				_deformedTextureData[i] = textureData[i].A > 0 ? textureData[i] : Color.Transparent;
			}

			DeformedTexture.SetData(_deformedTextureData);

			IndestructibleCollisionData = new bool[IndestructibleTexture.Width * IndestructibleTexture.Height];
			for (int i = 0; i < indestructibleTextureData.Length; i++)
			{
				if (indestructibleTextureData[i].A == 0)
				{
					IndestructibleCollisionData[i] = true;
				}
			}
		}

		public void BuildLevelFromCollisionData()
		{
			for (int i = 0; i < CollisionData.Length; i++)
			{
				if (CollisionData[i])
				{
					_deformedTextureData[i] = Color.Transparent;
				}
			}

			DeformedTexture.SetData(_deformedTextureData);
		}

		public void BuildCollisionDataFromLevel()
		{
			for (int i = 0; i < _deformedTextureData.Length; i++)
			{
				if (_deformedTextureData[i] == Color.Transparent)
				{
					CollisionData[i] = true;
				}
			}
		}

		/// <summary>
		/// Returns the number of pixels of the entity that collides with the background
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="centerX"></param>
		/// <param name="centerY"></param>
		/// <returns></returns>
		public int Collide(IEntity entity, int centerX, int centerY)
		{
			int entityWidth = entity.Texture.Width;
			int entityHeight = entity.Texture.Height;

			int posX = (int)entity.Position.X - (int)entity.Origin.X + centerX;
			int posY = (int)entity.Position.Y - (int)entity.Origin.Y + centerY;

			if (posX < 1 || posY < 1 || posX > Texture.Width - entityWidth - 1 || posY > Texture.Height - entityHeight - 1)
				return 1000000000;

			int hit = 0;

			Color[] data = entity.GetTextureData();

			var player = entity as Player;
			bool isLandingAngle = player != null && (MathHelper.ToDegrees(player.Angle) >= 345 || MathHelper.ToDegrees(player.Angle) <= 15) && player.Velocity.Y > 0;

			for (int y = 0; y < entityHeight; y++)
			{
				for (int x = 0; x < entityWidth; x++)
				{
					Color colorInPixel = data[(x - entity.Texture.Bounds.X) + (y - entity.Texture.Bounds.Y) * entity.Texture.Bounds.Width];
					if(colorInPixel.A != 0)
					{
						if (CollisionData[(posX + x) + (posY + y) * _width] == false)
						{
							if (isLandingAngle)
							{
								if (CollisionData[(posX + x) + (posY + y + 1) * _width] == false &&
										CollisionData[(posX + x + player.Texture.Width) + (posY + y + 1) * _width] == false)
								{
									if (player.Velocity.Y > 200f)
									{
										var yVelocity = (int)player.Velocity.Y;
										SoundManager.Play("Audio/Waves/thump", player.Position);
										player.Velocity = new Vector2(player.Velocity.X / 3, -player.Velocity.Y / 3);
										return (yVelocity - 200) / 50;
									}

									player.Land();
									return 0;
								}
							}

							entity.CollideWithLevel(this);

							CollisionData[(posX + x) + (posY + y) * _width] = true;

							Hits.Enqueue(new MapHit(entity));

							hit++;
						}
						else if (IndestructibleCollisionData[(posX + x) + (posY + y) * _width] == false)
						{
							if (isLandingAngle)
							{
								if (IndestructibleCollisionData[(posX + x) + (posY + y + 1) * _width] == false &&
										IndestructibleCollisionData[(posX + x + player.Texture.Width) + (posY + y + 1) * _width] == false)
								{
									if (player.Velocity.Y > 200f)
									{
										var yVelocity = (int)player.Velocity.Y;
										SoundManager.Play("Audio/Waves/thump", player.Position);
										player.Velocity = new Vector2(player.Velocity.X / 3, -player.Velocity.Y / 3);
										return (yVelocity - 200) / 50;
									}

									player.Land();
									return 0;
								}
							}

							entity.CollideWithLevel(this);

							hit++;
						}
					}
				}
			}

			if (player != null)
			{
				return hit / 20;
			}

			return hit;
		}

		public void StopLevelSong()
		{
			_stopMusic = true;
		}

		private bool _stopMusic;
		public void PlayLevelSong()
		{
			using (var ms = new MemoryStream(this.OggVorbisSong))
			{
				using (var vorbis = new NVorbis.NAudioSupport.VorbisWaveReader(ms))
				{
					using (var waveOut = new NAudio.Wave.WaveOut())
					{
						waveOut.Init(vorbis);
						waveOut.Play();

						while (!_stopMusic)
						{
							Thread.Sleep(100);
						}
					}
				}
			}
		}
	}
}