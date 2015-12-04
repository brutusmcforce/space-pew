using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
	/// TODO: This is not really a network messenger yet, so we need to change the functionality a little (send all message over network in the loop down there)
	/// </summary>
	public class NetworkMessenger : Microsoft.Xna.Framework.GameComponent, IDrawable
	{
		private readonly MainGame _game;
		private SpriteBatch _spriteBatch;
		private SpriteFont _netFont;
		private Rectangle _chatRectangle;
		private Rectangle _textBounds;
		private static UdpClient _client;

		private static List<NetworkMessage> _messages;

		private const int DefaultMessageLifeTime = 10;

		public NetworkMessenger(MainGame game, UdpClient client)
			: base(game)
		{
			_game = game;
			_client = client;
			_messages = new List<NetworkMessage>();
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			_spriteBatch = new SpriteBatch(_game.GraphicsDevice);
			_netFont = _game.Content.Load<SpriteFont>("Fonts\\Default");
			_chatRectangle = new Rectangle(300, 20, 300, _game.GraphicsDevice.Viewport.Height - 40);
			_textBounds = new Rectangle();

			base.Initialize();
		}

		public static void AddDeath(IEntity entity)
		{
			_client.SendDeath(entity);
		}

		public static void DisplayMessage(NetworkMessage message)
		{
			_messages.Add(message);
		}

		public static void SendMessage(NetworkMessage message)
		{
			_messages.Add(message);
			_client.SendMessage(message);

			if (message.IsChatMessage)
			{
				SoundManager.Play("Audio/Waves/message", _client.LocalPlayer.Position);
			}
		}

		private string _chatMessage = string.Empty;

		private KeyboardState _oldKeyboardState;
		private KeyboardState _currentKeyboardState;

		bool chatting = false;

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			MainGame.IsKeyboardInUse = chatting;

			_oldKeyboardState = _currentKeyboardState;

			_currentKeyboardState = Keyboard.GetState();
			Keys[] pressedKeys = _currentKeyboardState.GetPressedKeys();

			bool isShiftDown = pressedKeys.Any(KeyboardHelper.IsShift);

			foreach (Keys key in pressedKeys)
			{
				if (!_oldKeyboardState.IsKeyUp(key)) continue;
				
				if (key == Keys.T && !chatting)
				{
					chatting = true;
					continue;
				}

				if (!chatting) continue;

				switch (key)
				{
					case Keys.Back:
						_chatMessage = _chatMessage.Remove(_chatMessage.Length - 1, 1);
						break;
					case Keys.Space:
						_chatMessage = _chatMessage + " ";
						break;
					default:
						if (key == Keys.Enter && chatting)
						{
							chatting = false;
							SendMessage(new NetworkMessage()
							{
								Color = _client.LocalPlayer.Color,
								Message =
									string.Format("{0}: {1}", _client.LocalPlayer.Owner,
										_chatMessage),
								Sent = DateTime.Now,
								IsChatMessage = true
							});

							_chatMessage = string.Empty;
						}
						else
						{
							if (isShiftDown)
							{
								_chatMessage += KeyboardHelper.ToPrintableString(key, KbModifiers.None).ToUpper();
							}
							else
							{
								_chatMessage += KeyboardHelper.ToPrintableString(key, KbModifiers.None);
							}
						}
						break;
				}
			}

			for (int i = _messages.Count - 1; i >= 0; i--)
			{
				if (_messages[i].Sent.AddSeconds(DefaultMessageLifeTime) <= DateTime.Now)
				{
					_messages.RemoveAt(i);
				}
			}

			base.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			_spriteBatch.Begin();

			int yOffset = 0;

			if (chatting)
			{
				_spriteBatch.DrawString(_netFont, "Chat: " + _chatMessage,
																new Vector2(300, _game.GraphicsDevice.Viewport.Height - 140), Color.White);
			}

			foreach (var message in _messages)
			{
				DrawString(_spriteBatch,
									 _netFont,
									 message.Message,
									 _chatRectangle,
									 message.Color,
									 TextAlignment.TopLeft,
									 true,
									 new Vector2(0, (_messages.IndexOf(message) * 15) + yOffset),
									 out _textBounds);

				yOffset += _textBounds.Height;
			}

			_spriteBatch.End();
		}

		public int DrawOrder
		{
			get { return 5; }
		}

		public bool Visible
		{
			get { return true; }
		}

		private enum TextAlignment
		{
			Top,
			Left,
			Middle,
			Right,
			Bottom,
			TopLeft,
			TopRight,
			BottomLeft,
			BottomRight
		}

		private static void DrawString(SpriteBatch sb, SpriteFont fnt, string text, Rectangle r,
				Color col, TextAlignment align, bool performWordWrap, Vector2 offsett, out Rectangle textBounds)
		{
			textBounds = r;
			if (text == null) return;
			if (text == string.Empty) return;

			var lines = new StringCollection();
			lines.AddRange(text.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries));

			Rectangle tmprect = ProcessLines(fnt, r, performWordWrap, lines);

			var pos = new Vector2(r.X, r.Y);
			int aStyle = 0;

			switch (align)
			{
				case TextAlignment.Bottom:
					pos.Y = r.Bottom - tmprect.Height;
					aStyle = 1;
					break;
				case TextAlignment.BottomLeft:
					pos.Y = r.Bottom - tmprect.Height;
					aStyle = 0;
					break;
				case TextAlignment.BottomRight:
					pos.Y = r.Bottom - tmprect.Height;
					aStyle = 2;
					break;
				case TextAlignment.Left:
					pos.Y = r.Y + ((r.Height / 2) - (tmprect.Height / 2));
					aStyle = 0;
					break;
				case TextAlignment.Middle:
					pos.Y = r.Y + ((r.Height / 2) - (tmprect.Height / 2));
					aStyle = 1;
					break;
				case TextAlignment.Right:
					pos.Y = r.Y + ((r.Height / 2) - (tmprect.Height / 2));
					aStyle = 2;
					break;
				case TextAlignment.Top:
					aStyle = 1;
					break;
				case TextAlignment.TopLeft:
					aStyle = 0;
					break;
				case TextAlignment.TopRight:
					aStyle = 2;
					break;
			}

			foreach (string txt in lines)
			{
				Vector2 size = fnt.MeasureString(txt);
				switch (aStyle)
				{
					case 0:
						pos.X = r.X;
						break;
					case 1:
						pos.X = r.X + ((r.Width / 2) - (size.X / 2));
						break;
					case 2:
						pos.X = r.Right - size.X;
						break;
				}

				sb.DrawString(fnt, txt, pos + offsett, col);
				pos.Y += fnt.LineSpacing;
			}

			textBounds = tmprect;
		}

		private static Rectangle ProcessLines(SpriteFont fnt, Rectangle r, bool performWordWrap, StringCollection lines)
		{
			Rectangle bounds = r;
			bounds.Width = 0;
			bounds.Height = 0;
			int index = 0;
			float Width;
			bool lineInserted = false;
			while (index < lines.Count)
			{
				string linetext = lines[index];

				Vector2 size = fnt.MeasureString(linetext);

				if (performWordWrap && size.X > r.Width)
				{
					string endspace = string.Empty;
					if (linetext.EndsWith(" "))
					{
						endspace = " ";
						linetext = linetext.TrimEnd();
					}

					int i = linetext.LastIndexOf(" ", StringComparison.InvariantCulture);
					if (i != -1)
					{
						string lastword = linetext.Substring(i + 1);

						if (index == lines.Count - 1)
						{
							lines.Add(lastword);
							lineInserted = true;
						}
						else
						{
							if (lineInserted)
							{
								lines[index + 1] = lastword + endspace + lines[index + 1];
							}
							else
							{
								lines.Insert(index + 1, lastword);
								lineInserted = true;
							}
						}

						lines[index] = linetext.Substring(0, i + 1);

					}
					else
					{
						lineInserted = false;
						size = fnt.MeasureString(lines[index]);
						if (size.X > bounds.Width) Width = size.X;
						bounds.Height += fnt.LineSpacing;
						index++;
					}
				}
				else
				{
					lineInserted = false;
					size = fnt.MeasureString(lines[index]);
					if (size.X > bounds.Width) bounds.Width = (int)size.X;
					bounds.Height += fnt.LineSpacing;
					index++;
				}
			}

			return bounds;
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