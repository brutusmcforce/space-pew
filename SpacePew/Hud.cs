using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpacePew.Models;


namespace SpacePew
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class Hud : GameComponent, IDrawable
	{
		SpriteBatch _spriteBatch;
		readonly MainGame _game;

		SpriteFont _consoleFont;

		public Hud(MainGame game)
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
			_consoleFont = _game.Content.Load<SpriteFont>("Fonts\\ConsoleFont");

			base.Initialize();
		}

		public void Draw(GameTime gameTime)
		{
			if (_game.NetworkClient.LocalPlayer != null)
			{
				_spriteBatch.Begin();
				_spriteBatch.DrawString(_consoleFont,
						string.Format("Health: {0}",
						_game.NetworkClient.LocalPlayer.Health),
						new Vector2(300, _game.GraphicsDevice.Viewport.Height - 100),
						Color.White);

				_spriteBatch.DrawString(_consoleFont,
						string.Format("Fuel: {0}",
						_game.NetworkClient.LocalPlayer.Fuel),
						new Vector2(300, _game.GraphicsDevice.Viewport.Height - 75),
						Color.White);

				_spriteBatch.DrawString(_consoleFont,
						string.Format("Heat #1: {0}",
						_game.NetworkClient.LocalPlayer.Weapon.Heat),
						new Vector2(500, _game.GraphicsDevice.Viewport.Height - 100),
						Color.White);

				_spriteBatch.DrawString(_consoleFont,
						string.Format("Heat #2: {0}",
						_game.NetworkClient.LocalPlayer.SecondaryWeapon.Heat),
						new Vector2(500, _game.GraphicsDevice.Viewport.Height - 75),
						Color.White);

				_spriteBatch.DrawString(_consoleFont,
						string.Format("Entities: {0}",
						((List<IEntity>)EntityFactory.Instance.Entities).Count),
						new Vector2(500, _game.GraphicsDevice.Viewport.Height - 50),
						Color.White);

				_spriteBatch.DrawString(_consoleFont,
					string.Format("Pos X: {0}",
					_game.NetworkClient.LocalPlayer.Position.X),
					new Vector2(700, _game.GraphicsDevice.Viewport.Height - 100),
					Color.White);

				_spriteBatch.DrawString(_consoleFont,
					string.Format("Pos Y: {0}",
					_game.NetworkClient.LocalPlayer.Position.Y),
					new Vector2(700, _game.GraphicsDevice.Viewport.Height - 75),
					Color.White);

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