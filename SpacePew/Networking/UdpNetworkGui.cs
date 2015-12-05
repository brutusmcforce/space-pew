using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using System.Threading;
using SpacePew.Models;
#if WINDOWS
using TomShane.Neoforce.Controls;
using Lidgren.Network;
using System.Net;
using System.Diagnostics;
#endif

namespace SpacePew.Networking
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
#if WINDOWS
	public class UdpNetworkGui : Microsoft.Xna.Framework.DrawableGameComponent
	{
		private MainGame _game;

		private UdpClient _client;
		private UdpServer _server;

		private GraphicsDeviceManager _graphics;

		private Manager _manager;
		private Window _window;
		private TabControl _tabControl;
		private TextBox _nameTextBox;
		private TextBox _nameTextBox2;
		private TextBox _ipTextBox;
		private Label _nameLabel;
		private Label _nameLabel2;
		private ListBox _localGamesListBox;

		private Label _joinErrorLabel;

		private Label _ipLabel;
		private Button _createButton;
		private Button _joinButton;
		private Button _refreshButton;

		public UdpNetworkGui(MainGame game, GraphicsDeviceManager graphics, UdpClient client, UdpServer server)
			: base(game)
		{
			_game = game;
			_graphics = graphics;
			_client = client;
			_server = server;

			_client.CurrentClient.Start();

			_manager = new Manager(game, _graphics, "Default");
			_manager.Skin = new Skin(_manager, "Default");
			_manager.AutoCreateRenderTarget = true;
			_manager.TargetFrames = 60;
			_manager.LogUnhandledExceptions = false;
			_manager.ShowSoftwareCursor = true;
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			_window = new Window(_manager);
			_window.Init();
			_window.Text = "Space, pew pew!";
			_window.Width = 480;
			_window.Height = 200;
			_window.Center();
			_window.CloseButtonVisible = false;
			_window.Resizable = false;
			_window.Visible = true;

			_tabControl = new TabControl(_manager);
			_tabControl.Width = _window.Width;
			_tabControl.Height = _window.Height;
			_tabControl.Parent = _window;

			_nameLabel = new Label(_manager);
			_nameLabel.Init();
			_nameLabel.Width = 100;
			_nameLabel.Height = 24;
			_nameLabel.Text = "Name";
			_nameLabel.Left = 10;
			_nameLabel.Top = 10;

			_nameTextBox = new TextBox(_manager);
			_nameTextBox.Init();
			_nameTextBox.Width = 140;
			_nameTextBox.Height = 24;
			_nameTextBox.Left = 50;
			_nameTextBox.Top = 10;

			_nameLabel2 = new Label(_manager);
			_nameLabel2.Init();
			_nameLabel2.Width = 100;
			_nameLabel2.Height = 24;
			_nameLabel2.Text = "Name";
			_nameLabel2.Left = 10;
			_nameLabel2.Top = 10;

			_nameTextBox2 = new TextBox(_manager);
			_nameTextBox2.Init();
			_nameTextBox2.Width = 140;
			_nameTextBox2.Height = 24;
			_nameTextBox2.Left = 50;
			_nameTextBox2.Top = 10;

			_createButton = new Button(_manager);
			_createButton.Init();
			_createButton.Text = "Create game";
			_createButton.Width = 140;
			_createButton.Height = 24;
			_createButton.Left = 50;
			_createButton.Top = 40;
			_createButton.Click += _createButton_Click;

			_ipLabel = new Label(_manager);
			_ipLabel.Init();
			_ipLabel.Width = 100;
			_ipLabel.Height = 24;
			_ipLabel.Text = "Host";
			_ipLabel.Left = 10;
			_ipLabel.Top = 40;

			_ipTextBox = new TextBox(_manager);
			_ipTextBox.Init();
			_ipTextBox.Width = 140;
			_ipTextBox.Height = 24;
			_ipTextBox.Left = 50;
			_ipTextBox.Top = 40;

			_joinErrorLabel = new Label(_manager);
			_joinErrorLabel.Init();
			_joinErrorLabel.Width = 460;
			_joinErrorLabel.Height = 24;
			_joinErrorLabel.Left = 10;
			_joinErrorLabel.Top = 110;
			_joinErrorLabel.Text = string.Empty;
			_joinErrorLabel.TextColor = Color.DarkRed;

			_joinButton = new Button(_manager);
			_joinButton.Init();
			_joinButton.Text = "Join game";
			_joinButton.Width = 140;
			_joinButton.Height = 24;
			_joinButton.Left = 50;
			_joinButton.Top = 70;
			_joinButton.Anchor = Anchors.Bottom;
			_joinButton.Click += _joinButton_Click;

			_localGamesListBox = new ListBox(_manager);
			_localGamesListBox.Init();
			_localGamesListBox.Left = 200;
			_localGamesListBox.Top = 10;
			_localGamesListBox.Height = 84;
			_localGamesListBox.Width = 254;
			_localGamesListBox.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(_localGamesListBox_ItemIndexChanged);

			_refreshButton = new Button(_manager);
			_refreshButton.Init();
			_refreshButton.Text = "Refresh";
			_refreshButton.Width = 140;
			_refreshButton.Height = 24;
			_refreshButton.Left = 314;
			_refreshButton.Top = 104;
			_refreshButton.Click += _refreshButton_Click;

			_nameTextBox.Click += ChangeTextBoxColor;
			_nameTextBox2.Click += ChangeTextBoxColor;
			_ipTextBox.Click += ChangeTextBoxColor;

			_tabControl.AddPage();
			_tabControl.AddPage();
			_tabControl.TabPages[0].Text = "Create";
			_tabControl.TabPages[0].Add(_nameLabel);
			_tabControl.TabPages[0].Add(_nameTextBox);
			_tabControl.TabPages[0].Add(_createButton);

			_tabControl.TabPages[1].Text = "Join";
			_tabControl.TabPages[1].Add(_nameLabel2);
			_tabControl.TabPages[1].Add(_nameTextBox2);
			_tabControl.TabPages[1].Add(_ipLabel);
			_tabControl.TabPages[1].Add(_ipTextBox);
			_tabControl.TabPages[1].Add(_joinButton);
			_tabControl.TabPages[1].Add(_joinErrorLabel);
			_tabControl.TabPages[1].Add(_localGamesListBox);
			_tabControl.TabPages[1].Add(_refreshButton);

			_manager.Add(_window);
			_manager.Initialize();

			_client.CurrentClient.DiscoverLocalPeers(SpacePew.Common.Constants.GameServerPort);
		}

		private void _refreshButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
		{
			_localGamesListBox.Items.Clear();
			_client.CurrentClient.DiscoverLocalPeers(SpacePew.Common.Constants.GameServerPort);
		}

		private void ChangeTextBoxColor(object sender, TomShane.Neoforce.Controls.EventArgs e)
		{
			((TomShane.Neoforce.Controls.Control)sender).Color = Color.TransparentBlack;
		}

		private void _localGamesListBox_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
		{
			_ipTextBox.Text = _localGamesListBox.Items[_localGamesListBox.ItemIndex].ToString();
		}

		private void _createButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
		{
			_nameTextBox.Color = string.IsNullOrEmpty(_nameTextBox.Text) ? Color.Pink : Color.TransparentBlack;

			if (_nameTextBox.Color == Color.Pink)
			{
				return;
			}
			
			string levelPath = AppDomain.CurrentDomain.BaseDirectory + "\\Levels\\hippie.zip"; // TODO: Välja
			var level = LevelLoader.LoadLevel(levelPath, _game.Content, GraphicsDevice);

			_server.SetLevel(level);

			_window.Close();
			Trace.WriteLine("CreateSession()");
			_server.CreateSession();

			new Thread(_server.Listen).Start();

			_client.JoinSession("127.0.0.1", _nameTextBox.Text);

			_game.AddGameComponents();
			_game.Components.Remove(this);
		}

		private void _joinButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
		{
			_ipTextBox.Color = string.IsNullOrEmpty(_ipTextBox.Text) ? Color.Pink : Color.TransparentBlack;
			_nameTextBox2.Color = string.IsNullOrEmpty(_nameTextBox2.Text) ? Color.Pink : Color.TransparentBlack;

			if (_ipTextBox.Color == Color.Pink || _nameTextBox2.Color == Color.Pink)
			{
				return;
			}

			var splits = _ipTextBox.Text.Split(' ');
			if (splits.Count() > 1)
			{
				var host = Int64.Parse(splits[0]);
				_client.RequestNATIntroduction(host);
			}
			else
			{
				try
				{
					_client.JoinSession(_ipTextBox.Text, _nameTextBox2.Text);

					_game.AddGameComponents();
					_game.Components.Remove(this);
				}
				catch (NetException ex)
				{
					_joinErrorLabel.Text = ex.Message;
					return;
				}
			}
		}

		private DateTime _lastUpdate = DateTime.Now.AddSeconds(-5);
		private static Dictionary<long, IPEndPoint[]> _hostList = new Dictionary<long, IPEndPoint[]>();

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			_manager.Update(gameTime);

			if (_lastUpdate <= DateTime.Now.AddSeconds(-5))
			{
				_lastUpdate = DateTime.Now;
				_client.CurrentClient.DiscoverLocalPeers(SpacePew.Common.Constants.GameServerPort);
				_client.GetServerList();
			}
			NetIncomingMessage message;
			while ((message = _client.CurrentClient.ReadMessage()) != null)
			{
				if (message.MessageType == NetIncomingMessageType.UnconnectedData)
				{
					var id = message.ReadInt64();
					var hostInternal = message.ReadIPEndPoint();
					var hostExternal = message.ReadIPEndPoint();

					_hostList[id] = new IPEndPoint[] { hostInternal, hostExternal };

					_localGamesListBox.Items.Clear();
					foreach (var kvp in _hostList)
					{
						_localGamesListBox.Items.Add(kvp.Key.ToString() + " (" + kvp.Value[1] + ")");
					}
				}
				else if (message.MessageType == NetIncomingMessageType.DiscoveryResponse)
				{
					IPEndPoint ep = message.ReadIPEndPoint();
					if (!_localGamesListBox.Items.Contains(ep.Address.ToString()))
					{
						_localGamesListBox.Items.Add(ep.Address.ToString());
					}
				} 
				else if (message.MessageType == NetIncomingMessageType.NatIntroductionSuccess)
				{
					try
					{
						_client.JoinSession(message.SenderEndPoint, _nameTextBox2.Text);

						_game.AddGameComponents();
						_game.Components.Remove(this);
					}
					catch (NetException ex)
					{
						_joinErrorLabel.Text = ex.Message;
						return;
					}
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			_manager.BeginDraw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_manager.EndDraw();

			base.Draw(gameTime);
		}
	}
#endif
}