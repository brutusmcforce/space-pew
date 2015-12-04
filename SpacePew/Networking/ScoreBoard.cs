using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using SpacePew.Models;


namespace SpacePew.Networking
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class ScoreBoard : Microsoft.Xna.Framework.GameComponent, IDrawable
	{
		public static List<ScoreBoardItem> CurrentScoreBoard { get; set; }

		private readonly MainGame _game;
		private readonly UdpClient _client;
		private Texture2D _scoreBoardTexture;

		private SpriteBatch _spriteBatch;

		private SpriteFont _scoreFont;

		public ScoreBoard(MainGame game, UdpClient client)
			: base(game)
		{
			_game = game;
			_client = client;

			CurrentScoreBoard = new List<ScoreBoardItem>();
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			_spriteBatch = new SpriteBatch(_game.GraphicsDevice);

			_scoreFont = _game.Content.Load<SpriteFont>("Fonts\\Default");
			_scoreBoardTexture = _game.Content.Load<Texture2D>("scoreboard");

			base.Initialize();
		}

		bool requestedScoreBoard;

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Tab))
			{
				if (!requestedScoreBoard)
				{
					_client.RequestScoreBoard();
				}

				requestedScoreBoard = true;
			}
			else
			{
				requestedScoreBoard = false;
			}

			base.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Tab))
			{
				_spriteBatch.Begin();

				_spriteBatch.Draw(_scoreBoardTexture, new Vector2(20, 20), Color.White);

				foreach (ScoreBoardItem item in CurrentScoreBoard)
				{
					Player player = ((List<Player>)_client.Players).Find(p => p.Owner == item.Name);

					if (player != null)
					{
						Color color = player.Color;

						int yOffset = (CurrentScoreBoard.IndexOf(item) * 15) + 70;

						_spriteBatch.DrawString(_scoreFont, item.Name, new Vector2(35, yOffset), color);
						_spriteBatch.DrawString(_scoreFont, item.Kills.ToString(CultureInfo.InvariantCulture), new Vector2(185, yOffset), color);
						_spriteBatch.DrawString(_scoreFont, item.Deaths.ToString(CultureInfo.InvariantCulture), new Vector2(243, yOffset), color);
						_spriteBatch.DrawString(_scoreFont, (DateTime.Now - item.Joined).Minutes.ToString(CultureInfo.InvariantCulture),
																		new Vector2(314, yOffset), color);
						_spriteBatch.DrawString(_scoreFont, item.Ping.ToString(CultureInfo.InvariantCulture), new Vector2(373, yOffset), color);
					}
				}

				_spriteBatch.End();
			}
		}

		public int DrawOrder
		{
			get { return 5; }
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