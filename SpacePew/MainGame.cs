using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SpacePew.Camera;
using SpacePew.Models;
using SpacePew.Models.Weapons;
using SpacePew.Networking;
using SpacePew.ParticleSystem;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SpacePew.Extensions;
using System.Linq;
using System.Windows.Forms;

namespace SpacePew
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class MainGame : Game
	{
		public static int CenterX;
		public static int CenterY;

		public UdpServer NetworkServer { get; private set; }
		public UdpClient NetworkClient { get; private set; }

		private Level _level;
		public Level Level
		{
			get { return _level; }
			set { _level = value; }
		}

		private SpriteBatch _spriteBatch;
		public SpriteBatch SpriteBatch
		{
			get
			{
				return _spriteBatch;
			}
		}

		readonly GraphicsDeviceManager _graphics;
		readonly EntityFactory _entityFactory;

		Hud _hud;
		Minimap _minimap;

		private readonly UdpNetworkGui _udpClientGui;

		private KeyboardState _currentKeyboardState;
		private GamePadState _currentGamePadState;

		private Texture2D _backgroundTexture;

		private ExplosionParticleSystem _explosion;
		private ExplosionSmokeParticleSystem _smoke;
		private SmokePlumeParticleSystem _smokePlume;

		private NetworkMessenger _networkMessenger;

		private Song _gameSong;
		public Song GameSong
		{
			get { return _gameSong; }
		}

		private ICamera2D _camera;

		private readonly Random _randomizer;

		public static bool IsKeyboardInUse;

		public void AddGameComponents()
		{
			_hud = new Hud(this);
			Components.Add(_hud);

			_minimap = new Minimap(this);
			Components.Add(_minimap);

			_camera = new Camera2D(this)
			{
				Focus = NetworkClient.LocalPlayer
			};

			Components.Add((IGameComponent)_camera);

			_explosion = new ExplosionParticleSystem(this, 1, _camera);
			Components.Add(_explosion);

			_smoke = new ExplosionSmokeParticleSystem(this, 2, _camera);
			Components.Add(_smoke);

			_smokePlume = new SmokePlumeParticleSystem(this, 9, _camera);
			Components.Add(_smokePlume);

			_networkMessenger = new NetworkMessenger(this, NetworkClient);
			Components.Add(_networkMessenger);

			Components.Add(new ScoreBoard(this, NetworkClient));
		}

		public void AddExplosion(Vector2 position, float size)
		{
			_explosion.AddParticles(position, size);
			_smoke.AddParticles(position, size);
		}

		public MainGame()
		{
			Trace.Listeners.Add(new TextWriterTraceListener("debug.log"));
			Trace.AutoFlush = true;
			Trace.Indent();

			Trace.WriteLine(string.Empty);
			Trace.WriteLine(DateTime.Now);
			Trace.WriteLine("------------------------------------------------------------------------------------------------------------------");

			Window.Title = "Space, pew pew!";
			Content.RootDirectory = "Content";

			NetworkServer = new UdpServer();
			NetworkClient = new UdpClient(this);

			_graphics = new GraphicsDeviceManager(this);
			_entityFactory = new EntityFactory(this);

			SoundManager.Initialize(this);
			TextureManager.Initialize(this);

			IsMouseVisible = true;

			_graphics.CreateDevice();

			Cursor.Hide();

			_graphics.PreferredBackBufferWidth = 1366;
			_graphics.PreferredBackBufferHeight = 768;

			//var screen = Screen.AllScreens.First(e => e.Primary);

			//Window.IsBorderless = true;
			//Window.Position = new Point(screen.Bounds.X, screen.Bounds.Y);
			//_graphics.PreferredBackBufferWidth = screen.Bounds.Width;
			//_graphics.PreferredBackBufferHeight = screen.Bounds.Height;

			_graphics.ApplyChanges();

			CenterX = _graphics.PreferredBackBufferWidth / 2;
			CenterY = _graphics.PreferredBackBufferHeight / 2;

			_randomizer = new Random();

			_udpClientGui = new UdpNetworkGui(this, _graphics, (UdpClient)NetworkClient, (UdpServer)NetworkServer);
			Components.Add(_udpClientGui);
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			_udpClientGui.Initialize();

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_backgroundTexture = TextureManager.LoadTexture("stars");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			if (_level != null)
			{
				_level.StopLevelSong();
			}

			if (NetworkClient.IsSessionAlive)
			{
				NetworkClient.ExitSession("Quitting...");
			}

			NetworkServer.Shutdown();

			Trace.WriteLine("Exiting");
			Trace.Unindent();
			Trace.Flush();

			base.OnExiting(sender, args);
		}

		/// <summary>
		/// Handles input.
		/// </summary>
		private void HandleInput()
		{
			_currentKeyboardState = Keyboard.GetState();
			_currentGamePadState = GamePad.GetState(PlayerIndex.One);

			// Check for exit.
			if (IsActive && IsPressed(Microsoft.Xna.Framework.Input.Keys.Escape, Buttons.Back))
			{
				Exit();
			}
		}

		/// <summary>
		/// Checks if the specified button is pressed on either keyboard or gamepad.
		/// </summary>
		bool IsPressed(Microsoft.Xna.Framework.Input.Keys key, Buttons button)
		{
			return (_currentKeyboardState.IsKeyDown(key) || _currentGamePadState.IsButtonDown(button));
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			HandleInput();

			if (!NetworkClient.IsSessionAlive || NetworkClient.LocalPlayer == null)
			{
				// If we are not in a network session, update the
				// menu screen that will let us create or join one.
			}
			else
			{
				while (WeaponBase.FiredShots.Count > 0)
				{
					NetworkClient.EntitiesToSend.Add(WeaponBase.FiredShots.Dequeue());
				}

				NetworkClient.Update();

				NetworkClient.LocalPlayer.HandleKeyboard(Keyboard.GetState());

				var entitiesToKill = new HashSet<IEntity>();

				//fixa en metod för entity som sköter det här..
				foreach (var entity in _entityFactory.Entities)
				{
					entity.Update(gameTime);

					foreach (var player in NetworkClient.Players)
					{
						if (player.IsCollisionWith(entity))
						{
							player.CollideWith(entity);

							if (!(entity is Player))
							{
								AddExplosion(entity.Position, 0.05f);

								entitiesToKill.Add(entity);
							}
						}

						if (player == NetworkClient.LocalPlayer && player.Health <= 0)
						{
							AddExplosion(player.Position, 1f);
							player.Kill();

							NetworkClient.SendDeath(entity);

							var message = new NetworkMessage
							{
								Sent = DateTime.Now
							};

							if (player.Owner == entity.Owner)
							{
								message.Color = player.Color;
								message.Message = string.Format("- {0} commited suicide.", player.Owner);
							}
							else
							{
								var killer = ((List<Player>)NetworkClient.Players).Find(p => p.Owner == entity.Owner);
								message.Message = string.Format("{0} killed {1}.", entity.Owner, player.Owner);
								message.Color = killer.Color;
							}

							NetworkMessenger.SendMessage(message);
						}
					}

					entity.Health -= _level.Collide(entity, CenterX, CenterY);

					if (NetworkClient.LocalPlayer.Health <= 0)
					{
						AddExplosion(NetworkClient.LocalPlayer.Position, 1f);
						NetworkClient.LocalPlayer.Kill();

						NetworkMessenger.AddDeath(NetworkClient.LocalPlayer);
						NetworkMessenger.SendMessage(new NetworkMessage
						{
							Color = NetworkClient.LocalPlayer.Color,
							Message = GetRandomCrashMessage(),
							Sent = DateTime.Now
						});
					}

					if (!(entity is Player) && entity.Health <= 0)
					{
						entitiesToKill.Add(entity);

						AddExplosion(entity.Position, 0.05f);
					}
				}

				_entityFactory.RemoveEntities(entitiesToKill);
			}

			base.Update(gameTime);
		}

		private string GetRandomCrashMessage()
		{
			int random = _randomizer.Next(0, 3);
			var crashMessages = new List<string>
                                             {
                                                 "{0} is driving under influence.",
                                                 "{0} crashed.",
                                                 "{0} sent himself straight towards the wall."
                                             };

			return string.Format(crashMessages[random], NetworkClient.LocalPlayer.Owner);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			if (NetworkClient.IsSessionAlive)
			{
				if (!BeginDraw() || NetworkClient.LocalPlayer == null)
					return;

				_graphics.GraphicsDevice.Clear(Color.Black);

				RenderDeformation();
				RenderTiledBackground();
				RenderLevel();
				RenderEntities();
			}

			base.Draw(gameTime);
		}

		private static readonly BlendState
			opaqueExceptAlpha = new BlendState
			{
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,
				ColorDestinationBlend = Blend.InverseDestinationAlpha,
				AlphaDestinationBlend = Blend.InverseDestinationAlpha,
				ColorWriteChannels = ColorWriteChannels.Alpha
			};

		void RenderDeformation()
		{
			var hits = new List<MapHit>();
			while (_level.Hits.Count > 0)
			{
				hits.Add(_level.Hits.Dequeue());
			}

			foreach (var tile in _level.DeformedTexture)
			{
				_graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)tile.Texture);
				_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend); // TODO: fixa blendstaten där uppe så att den tar hänsyn till alpha i source
				foreach (var hit in hits)
				{
					_spriteBatch.Draw(hit.Texture,
						hit.Position + _camera.ScreenCenter - tile.Position.AsVector2D(),
						null,
						Color.Black,
						hit.Angle,
						hit.Origin,
						1,
						SpriteEffects.None,
						1);
				}

				_spriteBatch.End();
			}
			
			_graphics.GraphicsDevice.SetRenderTarget(null);
		}

		private void RenderTiledBackground()
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);

			var pos = Vector2.Zero;
			var scrollPos = NetworkClient.LocalPlayer.Position / 3.0f;

			int startX = ((int)scrollPos.X) % _backgroundTexture.Width;
			int startY = ((int)scrollPos.Y) % _backgroundTexture.Height;

			for (int y = -startY; y < GraphicsDevice.Viewport.Height; y += _backgroundTexture.Height)
			{
				for (int x = -startX; x < GraphicsDevice.Viewport.Width; x += _backgroundTexture.Width)
				{
					pos.X = x; pos.Y = y;
					_spriteBatch.Draw(_backgroundTexture, pos, Color.White);
				}
			}

			_spriteBatch.End();
		}

		void RenderLevel()
		{
			var bounds = new Rectangle(0, 0, _level.Texture.TileWidth, _level.Texture.TileHeight);
			var viewPort = new Rectangle(
					(int)_camera.Position.X - _graphics.GraphicsDevice.Viewport.Width / 2,
					(int)_camera.Position.Y - _graphics.GraphicsDevice.Viewport.Height / 2,
					_graphics.GraphicsDevice.Viewport.Width + _graphics.GraphicsDevice.Viewport.Width / 2,
					_graphics.GraphicsDevice.Viewport.Height + _graphics.GraphicsDevice.Viewport.Height / 2);

			for (var i = 0; i < _level.Texture.Count; i++)
			{
				var tile = _level.Texture[i];
				var deformedTile = _level.DeformedTexture[i];
				var indestructibleTile = _level.IndestructibleTexture[i];

				bounds.X = tile.Position.X;
				bounds.Y = tile.Position.Y;
				if (viewPort.Intersects(bounds))
				{
					_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

					_spriteBatch.Draw(indestructibleTile.Texture, new Vector2(indestructibleTile.Position.X - _camera.Position.X, indestructibleTile.Position.Y - _camera.Position.Y));
					_spriteBatch.Draw(deformedTile.Texture, new Vector2(deformedTile.Position.X - _camera.Position.X, deformedTile.Position.Y - _camera.Position.Y));

					_spriteBatch.End();
				}
			}
		}

		private void RenderEntities()
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _camera.Transform);

			foreach (var entity in _entityFactory.Entities)
			{
				_spriteBatch.Draw(entity.Texture,
					entity.Position,
					null,
					entity.Color,
					entity.Angle,
					entity.Origin,
					1,
					SpriteEffects.None,
					1);
			}

			_spriteBatch.End();
		}
	}
}
