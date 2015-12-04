using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpacePew.Extensions;
using SpacePew.Models;

namespace SpacePew
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class Minimap : GameComponent, IDrawable
	{
		private const int MinimapWidth = 160;

		private int _minimapModifier;

		private readonly MainGame _game;
		private SpriteBatch _spriteBatch;
		private int _screenWidth;
		private int _screenHeight;
		private Texture2D _miniPlayer;

		public Minimap(MainGame game)
			: base(game)
		{
			_game = game;
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			_spriteBatch = new SpriteBatch(_game.GraphicsDevice);

			_screenWidth = _game.GraphicsDevice.PresentationParameters.BackBufferWidth;
			_screenHeight = _game.GraphicsDevice.PresentationParameters.BackBufferHeight;

			_miniPlayer = TextureManager.LoadTexture("bullet");

			base.Initialize();
		}

		public void Draw(GameTime gameTime)
		{
			if (_game.NetworkClient.LocalPlayer != null)
			{
				_minimapModifier = _game.Level.Texture.Width / MinimapWidth;

				_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

				int minimapHeight = _game.Level.Texture.Height / _minimapModifier > _screenHeight ?
						_screenHeight - 20 :
						_game.Level.Texture.Height / _screenHeight;

				if (minimapHeight > _screenHeight)
					minimapHeight = _screenHeight - 20;

				var rect = new Rectangle(10, _screenHeight - minimapHeight - 10,
						_game.Level.Texture.Width / _minimapModifier,
						minimapHeight);


				//Skriv ut spelarpluppen över hela ytan i svart för att rensa
				_spriteBatch.Draw(_miniPlayer, rect, Color.Black);
				_spriteBatch.Draw(_game.Level.DeformedTexture, rect, Color.White);
				_spriteBatch.Draw(_game.Level.IndestructibleTexture, rect, Color.White);

				float xScale = _game.Level.Texture.Width / (float)rect.Width;
				float yScale = _game.Level.Texture.Height / (float)rect.Height;
				float centerX = _screenWidth / 2;
				float centerY = _screenHeight / 2;

				foreach (Player player in _game.NetworkClient.Players)
				{
					var miniMapPosition = new Vector2(rect.X - 1 + (player.Position.X + centerX) / xScale, rect.Y - 1 + (player.Position.Y + centerY) / yScale);

					_spriteBatch.Draw(_miniPlayer, miniMapPosition, player.Color);
				}

				_spriteBatch.End();
			}
		}

		public int DrawOrder
		{
			get { return 2; }
		}

		public bool Visible
		{
			get { return true; }
		}


		event EventHandler<EventArgs> IDrawable.DrawOrderChanged
		{
			add { }
			remove { }
		}

		event EventHandler<EventArgs> IDrawable.VisibleChanged
		{
			add { }
			remove { }
		}
	}
}