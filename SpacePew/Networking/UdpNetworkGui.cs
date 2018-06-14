using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using SpacePew.Models;
#if WINDOWS
using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Entities.TextValidators;
using GeonBit.UI.DataTypes;
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
        private SpriteBatch _spriteBatch;
        private UserInterface _uiManager;

        private UdpClient _client;
        private UdpServer _server;

        private GraphicsDeviceManager _graphics;

        private Panel _panel;
        private PanelTabs _tabControl;
        private TextInput _nameTextBox;
        private TextInput _nameTextBox2;
        private TextInput _ipTextBox;
        private Label _nameLabel;
        private Label _nameLabel2;
        private SelectList _localGamesListBox;

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
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);

            _client.CurrentClient.Start();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            UserInterface.Initialize(_game.Content, BuiltinThemes.hd);
            UserInterface.Active.UseRenderTarget = true;
            UserInterface.Active.IncludeCursorInRenderTarget = true;

            int topPanelHeight = 65;

            _panel = new Panel(new Vector2(0, topPanelHeight + 2), PanelSkin.Simple, Anchor.TopCenter);
            _tabControl = new PanelTabs();
            _nameLabel = new Label("Name");
            _nameTextBox = new TextInput(false);
            _nameLabel2 = new Label("Name");
            _nameTextBox2 = new TextInput(false);
            _createButton = new Button("Create game");

            _createButton.OnClick = (Entity btn) =>
            {
                if (string.IsNullOrEmpty(_nameTextBox.TextParagraph.Text))
                {
                    return;
                }

                string levelPath = AppDomain.CurrentDomain.BaseDirectory + "\\Levels\\hippie.zip"; // TODO: Välja
                var level = LevelLoader.LoadLevel(levelPath, _game.Content, GraphicsDevice);

                _server.SetLevel(level);

                Trace.WriteLine("CreateSession()");
                _server.CreateSession();

                new Thread(_server.Listen).Start();

                _client.JoinSession("127.0.0.1", _nameTextBox.TextParagraph.Text);

                _game.AddGameComponents();
                _game.Components.Remove(this);
            };
            
            _ipLabel = new Label("Host");

            _ipTextBox = new TextInput(false);

            _joinErrorLabel = new Label(string.Empty);

            _joinButton = new Button("Join game");
            _joinButton.OnClick = (Entity btn) =>
            {
                if (string.IsNullOrEmpty(_ipTextBox.TextParagraph.Text) || string.IsNullOrEmpty(_nameTextBox2.TextParagraph.Text))
                {
                    return;
                }

                var splits = _ipTextBox.TextParagraph.Text.Split(' ');
                if (splits.Count() > 1)
                {
                    var host = Int64.Parse(splits[0]);
                    _client.RequestNATIntroduction(host);
                }
                else
                {
                    try
                    {
                        _client.JoinSession(_ipTextBox.TextParagraph.Text, _nameTextBox2.TextParagraph.Text);

                        _game.AddGameComponents();
                        _game.Components.Remove(this);
                    }
                    catch (NetException ex)
                    {
                        _joinErrorLabel.Text = ex.Message;
                        return;
                    }
                }
            };

            _localGamesListBox = new SelectList();
            _localGamesListBox.OnValueChange = (Entity list) =>
            {
                if (_localGamesListBox.SelectedValue != null)
                {
                    _ipTextBox.Value = _localGamesListBox.SelectedValue;
                }
            };

            _refreshButton = new Button("Refresh");
            _refreshButton.OnClick = (Entity btn) =>
            {
                _localGamesListBox.ClearItems();
                _client.CurrentClient.DiscoverLocalPeers(SpacePew.Common.Constants.GameServerPort);
            };


            UserInterface.Active.AddEntity(_panel);
            _panel.AddChild(_tabControl);

            var createTab = _tabControl.AddTab("Create");
            createTab.panel.AddChild(_nameLabel);
            createTab.panel.AddChild(_nameTextBox);
            createTab.panel.AddChild(_createButton);

            var joinTab = _tabControl.AddTab("Join");

            joinTab.panel.AddChild(_nameLabel2);
            joinTab.panel.AddChild(_nameTextBox2);
            joinTab.panel.AddChild(_ipLabel);
            joinTab.panel.AddChild(_ipTextBox);
            joinTab.panel.AddChild(_joinButton);
            joinTab.panel.AddChild(_joinErrorLabel);
            joinTab.panel.AddChild(_localGamesListBox);
            joinTab.panel.AddChild(_refreshButton);

            _client.CurrentClient.DiscoverLocalPeers(SpacePew.Common.Constants.GameServerPort);
        }

        private DateTime _lastUpdate = DateTime.Now.AddSeconds(-5);
        private static Dictionary<long, IPEndPoint[]> _hostList = new Dictionary<long, IPEndPoint[]>();

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            UserInterface.Active.Update(gameTime);

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

                    _localGamesListBox.ClearItems();
                    foreach (var kvp in _hostList)
                    {
                        _localGamesListBox.AddItem(kvp.Key.ToString() + " (" + kvp.Value[1] + ")");
                    }
                }
                else if (message.MessageType == NetIncomingMessageType.DiscoveryResponse)
                {
                    IPEndPoint ep = message.ReadIPEndPoint();
                    //if (!_localGamesListBox.Items.Contains(ep.Address.ToString()))
                    //{
                        _localGamesListBox.AddItem(ep.Address.ToString());
                    //}
                }
                else if (message.MessageType == NetIncomingMessageType.NatIntroductionSuccess)
                {
                    try
                    {
                        _client.JoinSession(message.SenderEndPoint, _nameTextBox2.TextParagraph.Text);

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
            UserInterface.Active.Draw(_spriteBatch);

            GraphicsDevice.Clear(Color.Black);

            UserInterface.Active.DrawMainRenderTarget(_spriteBatch);

            base.Draw(gameTime);
        }
    }
#endif
}