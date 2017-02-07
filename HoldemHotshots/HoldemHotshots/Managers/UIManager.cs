﻿using System;
using System.Collections.Generic;
using System.IO;
#if __IOS__
using Foundation;
#endif
using Urho;
using Urho.Gui;
using Urho.Resources;
using Urho.Urho2D;
using ZXing.Mobile;
using System.Threading;
using System.Text.RegularExpressions;

namespace HoldemHotshots
{
	public static class UIManager
	{
        static private readonly uint QRStringLength = 21;
		static public Graphics graphics;
		static public ResourceCache cache;
		static public UI ui;

		//Menu UIs
		static public List<UIElement> menuUI     { get; internal set; } = new List<UIElement>();
		static public List<UIElement> joinUI     { get; internal set; } = new List<UIElement>();
		static public List<UIElement> hostUI     { get; internal set; } = new List<UIElement>();
        static public List<UIElement> lobbyUI    { get; internal set; } = new List<UIElement>();
        static public List<UIElement> settingsUI { get; internal set; } = new List<UIElement>();

        //In-Game UIs
        static public List<UIElement> playerUI { get; internal set; } = new List<UIElement>();
		static public List<UIElement> tableUI  { get; internal set; } = new List<UIElement>();

		static public void SetReferences(ResourceCache resCache, Graphics currGraphics, UI currUI) { cache = resCache; graphics = currGraphics; ui = currUI; }

		static public void CreateMenuUI()
		{
			if (menuUI.Count > 0)
				return;

			//Size parameters
			var logoWidthAndHeight = (graphics.Width / 5) * 3;

			var buttonWidth = graphics.Width / 8;
			var buttonHeight = graphics.Width / 3;

			var settingsButtonWidthAndHeight = graphics.Width / 10;

            //Create UI objects
			var settingsButton = new Button()
			{
				Name = "SettingsButton",
				Texture = cache.GetTexture2D("Textures/settingsButton.png"),
				Size = new IntVector2(settingsButtonWidthAndHeight, settingsButtonWidthAndHeight),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Visible = false,
				Enabled = false
			};

			var gameLogo = new BorderImage()
			{
				Name = "GameLogo",
				Texture = cache.GetTexture2D("Textures/gameTitle.png"),
				BlendMode = BlendMode.Replace,
				Size = new IntVector2(logoWidthAndHeight, logoWidthAndHeight),
				Position = new IntVector2((graphics.Width / 2) - ((graphics.Width / 5) * 3) / 2, 0)
			};

			var joinButton = new Button()
			{
				Name = "JoinGameButton",
				Texture = cache.GetTexture2D("Textures/joinGameButton.png"),
				BlendMode = BlendMode.Replace,
				Size = new IntVector2(buttonHeight, buttonWidth),
				Position = new IntVector2((graphics.Width - buttonWidth) / 5, (graphics.Height / 6) * 5)
			};

			var hostButton = new Button()
			{
				Name = "HostGameButton",
				Texture = cache.GetTexture2D("Textures/hostGameButton.png"),
				BlendMode = BlendMode.Replace,
				Size = new IntVector2(buttonHeight, buttonWidth),
				Position = new IntVector2(((graphics.Width - buttonWidth) / 5) * 3, (graphics.Height / 6) * 5)
			};

			var copyrightNotice = new Text()
			{
				Name = "CopyrightNotice",
				Value = "Copyright Advantage Software Group 2016 ~ 2017. All Rights Reserved.",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Bottom
			};

			copyrightNotice.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
			copyrightNotice.SetFont(cache.GetFont("Fonts/arial.ttf"), 10);

			//Subscribe to Events
			settingsButton.Pressed += SettingsButton_Pressed;
			joinButton.Pressed += JoinButton_Pressed;
			hostButton.Pressed += HostButton_Pressed;

            //Add to the MenuUI List

            menuUI.Add(settingsButton);
            menuUI.Add(settingsButton);
            menuUI.Add(gameLogo);
            menuUI.Add(joinButton);
            menuUI.Add(hostButton);
            menuUI.Add(copyrightNotice);

            AddToUI(menuUI);
		}

		private static void CreateJoinUI()
		{
			if (joinUI.Count > 0)
				return;

			//Size parameters
			var avatarWidthAndHeight = graphics.Width / 3;
			var backButtonWidthAndHeight = graphics.Width / 10;
			var nameBoxHeight = (graphics.Height / 20);
			var nameBoxWidth = (graphics.Width / 3) * 2;
            var serverBoxWidth = (graphics.Width / 2);

            //Create UI objects

            var joinBackButton = new Button()
			{
				Name = "JoinBackButton",
				Texture = cache.GetTexture2D("Textures/backButton.png"),
				Size = new IntVector2(backButtonWidthAndHeight, backButtonWidthAndHeight),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Visible = false,
				Enabled = false
			};

			var playerAvatar = new Button()
			{
				Name = "PlayerAvatar",
				Size = new IntVector2(avatarWidthAndHeight, avatarWidthAndHeight),
				Position = new IntVector2(0, graphics.Height / 8),
				HorizontalAlignment = HorizontalAlignment.Center,
				Visible = false,
				Enabled = false,
				Opacity = 0.6f
			};

			var playerNameBox = new LineEdit()
			{
				Name = "PlayerNameBox",
				Size = new IntVector2(nameBoxWidth, nameBoxHeight),
				Position = new IntVector2(0, playerAvatar.Position.Y + playerAvatar.Height + nameBoxHeight / 2),
				HorizontalAlignment = HorizontalAlignment.Center,
				Editable = true,
				Visible = false,
				Enabled = false,
				MaxLength = 15,
				Opacity = 0.6f
			};

			var serverAddressBox = new LineEdit()
			{
				Name = "ServerAddressBox",
				Size = new IntVector2(nameBoxWidth, nameBoxHeight),
				Position = new IntVector2(0, playerNameBox.Position.Y + playerNameBox.Height + nameBoxHeight / 2),
                HorizontalAlignment = HorizontalAlignment.Center,
                Editable = true,
				Opacity = 0.6f,
				MaxLength = QRStringLength
			};

			//ServerAddressBox TextElement properties
			serverAddressBox.TextElement.SetFont(cache.GetFont("Fonts/arial.ttf"), 20);
			serverAddressBox.TextElement.Value = "Enter Server IP Address";
			serverAddressBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 0.6f));
			serverAddressBox.TextElement.HorizontalAlignment = HorizontalAlignment.Center;
			serverAddressBox.TextElement.VerticalAlignment = VerticalAlignment.Center;

            var serverPortBox = new LineEdit()
            {
                Name = "ServerPortBox",
                Size = new IntVector2(nameBoxWidth, nameBoxHeight),
                Position = new IntVector2(0, serverAddressBox.Position.Y + nameBoxHeight + nameBoxHeight / 2),
                HorizontalAlignment = HorizontalAlignment.Center,
                Editable = true,
                Opacity = 0.6f,
                MaxLength = 5
            };

            //ServerAddressBox TextElement properties
            serverPortBox.TextElement.SetFont(cache.GetFont("Fonts/arial.ttf"), 20);
            serverPortBox.TextElement.Value = "Enter Server IP Port";
            serverPortBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 0.6f));
            serverPortBox.TextElement.HorizontalAlignment = HorizontalAlignment.Center;
            serverPortBox.TextElement.VerticalAlignment = VerticalAlignment.Center;

            var scanQRButton = new Button()
            {
                Name = "ScanQRButton",
                Texture = cache.GetTexture2D("Textures/scanQRButton.png"),
                BlendMode = BlendMode.Replace,
                Size = new IntVector2((graphics.Width / 6) , graphics.Width / 6),
                Position = new IntVector2(0, serverPortBox.Position.Y + nameBoxHeight + nameBoxHeight / 2),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var joinLobbyButton = new Button()
            {
                Name = "JoinLobbyButton",
                Texture = cache.GetTexture2D("Textures/joinLobbyButton.png"),
                BlendMode = BlendMode.Replace,
                Size = new IntVector2((graphics.Width / 3) * 2, graphics.Width / 5),
                Position = new IntVector2(0, (graphics.Height / 6) * 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Opacity = 0.2f
			};

			//PlayerNameBox TextElement properties
			playerNameBox.TextElement.SetFont(cache.GetFont("Fonts/arial.ttf"), 20);
			playerNameBox.TextElement.Value = "Enter Player Name";
			playerNameBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 0.6f));
			playerNameBox.TextElement.HorizontalAlignment = HorizontalAlignment.Center;
			playerNameBox.TextElement.VerticalAlignment = VerticalAlignment.Center;

			//Subscribe to Events
			joinBackButton.Pressed += JoinBackButton_Pressed;
			playerAvatar.Pressed += PlayerAvatar_Pressed;
			playerNameBox.TextChanged += PlayerNameBox_TextChanged;
			serverAddressBox.TextChanged += ServerAddressBox_TextChanged;
            serverPortBox.TextChanged += ServerPortBox_TextChanged;
            scanQRButton.Pressed += ScanQRButton_Pressed;
			joinLobbyButton.Pressed += JoinLobbyButton_Pressed;

			//Add to the HostUI List           
			joinUI.Add(joinBackButton);
			joinUI.Add(playerAvatar);
			joinUI.Add(playerNameBox);
			joinUI.Add(serverAddressBox);
            joinUI.Add(serverPortBox);
            joinUI.Add(scanQRButton);
			joinUI.Add(joinLobbyButton);

			AddToUI(joinUI);
		}

        static private void CreateHostUI()
		{
			if (hostUI.Count > 0)
				return;

			//Size parameters
			var hostButtonWidth = (graphics.Width / 3) * 2;
			var hostButtonHeight = graphics.Width / 5;
			var lobbyBoxWidth = (graphics.Width / 3) * 2;
			var lobbyBoxHeight = graphics.Height / 20;
			var backButtonWidthAndHeight = graphics.Width / 10;

			//Create UI objects
			var hostBackButton = new Button()
			{
				Name = "HostBackButton",
				Texture = cache.GetTexture2D("Textures/backButton.png"),
				Size = new IntVector2(backButtonWidthAndHeight, backButtonWidthAndHeight),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Visible = false,
				Enabled = false
			};

			var buyInAmountBox = new LineEdit()
			{
				Name = "BuyInAmountBox",
				Size = new IntVector2(lobbyBoxWidth, lobbyBoxHeight),
				Position = new IntVector2(0, graphics.Height / 2 - lobbyBoxHeight),
				HorizontalAlignment = HorizontalAlignment.Center,
				Editable = true,
				Opacity = 0.6f,
				MaxLength = 15,
			};
            
            buyInAmountBox.Cursor.SetColor(new Color(0.0f, 0.0f, 0.0f, 0.0f));

			//BuyInAmountBox TextElement properties
			buyInAmountBox.TextElement.SetFont(cache.GetFont("Fonts/arial.ttf"), 20);
			buyInAmountBox.TextElement.Value = "Enter Buy~In Amount";
			buyInAmountBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 0.6f));
			buyInAmountBox.TextElement.HorizontalAlignment = HorizontalAlignment.Center;
			buyInAmountBox.TextElement.VerticalAlignment = VerticalAlignment.Center;
		
			var createLobbyButton = new Button()
			{
				Name = "CreateLobbyButton",
				Texture = cache.GetTexture2D("Textures/createLobbyButton.png"),
				BlendMode = BlendMode.Replace,
				Size = new IntVector2(hostButtonWidth, hostButtonHeight),
				Position = new IntVector2(0, (graphics.Height / 6) * 5),
				HorizontalAlignment = HorizontalAlignment.Center
			};
            
			//Subscribe to Events
			hostBackButton.Pressed += HostBackButton_Pressed;
			createLobbyButton.Pressed += CreateLobbyButton_Pressed;
            buyInAmountBox.TextChanged += BuyInAmountBox_TextChanged;

			hostUI.Add(hostBackButton);
			hostUI.Add(buyInAmountBox);
			hostUI.Add(createLobbyButton);

			AddToUI(hostUI);
		}

        private static void BuyInAmountBox_TextChanged(TextChangedEventArgs obj)
        {
            AlterNumericLineEdit("BuyInAmountBox", "Enter Buy~In Amount", hostUI);
        }

        static void CreateTableUI()
        {
            if (tableUI.Count > 0)
                return;

            var exitButtonWidthAndHeight = graphics.Width / 10;

            var tableExitButton = new Button()
            {
                Name = "TableExitButton",
                Texture = cache.GetTexture2D("Textures/exitButtonLandscape.png"),
                Size = new IntVector2(exitButtonWidthAndHeight, exitButtonWidthAndHeight),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            };

            tableExitButton.Pressed += TableExitButton_Pressed;
            
            tableUI.Add(tableExitButton);

            AddToUI(tableUI);
        }
        
        static void CreatePlayerUI()
        {
            if (playerUI.Count > 0)
                return;

            var exitButtonWidthAndHeight = graphics.Width / 10;
            var actionButtonWidthAndHeight = graphics.Height / 7;
            var fontSize = graphics.Height/25;
            
            var balanceText = new Text()
            {
                Name = "PlayerBalanceText",
                Value = "$0",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            };

            balanceText.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            balanceText.SetFont(cache.GetFont("Fonts/vladimir.ttf"), fontSize);

            var playerInfoText = new Text()
            {
                Name = "PlayerInfoText",
                TextAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };

            playerInfoText.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            playerInfoText.SetFont(cache.GetFont("Fonts/vladimir.ttf"), fontSize);

            var playerExitButton = new Button()
            {
                Name = "PlayerExitButton",
                Texture = cache.GetTexture2D("Textures/exitButton.png"),
                Size = new IntVector2(exitButtonWidthAndHeight, exitButtonWidthAndHeight),
                HorizontalAlignment=HorizontalAlignment.Left,
                VerticalAlignment=VerticalAlignment.Top
            };

            var callButton = new Button()
            {
                Name = "CallButton",
                Texture = cache.GetTexture2D("Textures/ActionButtons/call.png"),
                Size = new IntVector2(actionButtonWidthAndHeight, actionButtonWidthAndHeight),
                Position = new IntVector2(0, graphics.Height - actionButtonWidthAndHeight)
            };

            var raiseButton = new Button()
            {
                Name = "RaiseButton",
                Texture = cache.GetTexture2D("Textures/ActionButtons/raise.png"),
                Size = new IntVector2(actionButtonWidthAndHeight, actionButtonWidthAndHeight),
                Position = new IntVector2(0, callButton.Position.Y - actionButtonWidthAndHeight - actionButtonWidthAndHeight / 10)
            };

            var allInButton = new Button()
            {
                Name = "AllInButton",
                Texture = cache.GetTexture2D("Textures/ActionButtons/allIn.png"),
                Size = new IntVector2(actionButtonWidthAndHeight, actionButtonWidthAndHeight),
                Position = new IntVector2(0, raiseButton.Position.Y - actionButtonWidthAndHeight - actionButtonWidthAndHeight / 10)
            };

            var checkButton = new Button()
            {
                Name = "CheckButton",
                Texture = cache.GetTexture2D("Textures/ActionButtons/check.png"),
                Size = new IntVector2(actionButtonWidthAndHeight, actionButtonWidthAndHeight),
                Position = new IntVector2(actionButtonWidthAndHeight + actionButtonWidthAndHeight / 10, callButton.Position.Y)
            };
            
            
            var foldButton = new Button()
            {
                Name = "FoldButton",
                Texture = cache.GetTexture2D("Textures/ActionButtons/fold.png"),
                Size = new IntVector2(actionButtonWidthAndHeight, actionButtonWidthAndHeight),
                Position = new IntVector2(checkButton.Position.X + actionButtonWidthAndHeight + actionButtonWidthAndHeight / 10, callButton.Position.Y)
            };

            playerExitButton.Pressed += PlayerExitButton_Pressed;
            foldButton.Pressed += FoldButton_Pressed;
            checkButton.Pressed += CheckButton_Pressed;
            callButton.Pressed += CallButton_Pressed;
            raiseButton.Pressed += RaiseButton_Pressed;
            allInButton.Pressed += AllInButton_Pressed;

            playerUI.Add(foldButton);
            playerUI.Add(checkButton);
            playerUI.Add(callButton);
            playerUI.Add(raiseButton);
            playerUI.Add(allInButton);
            playerUI.Add(playerInfoText);
            playerUI.Add(balanceText);
            playerUI.Add(playerExitButton);

            AddToUI(playerUI);
        }

        public static void CreateSettingsUI()
        {
            if (settingsUI.Count > 0)
                return;

            AddToUI(settingsUI);
        }

        public static void CreateLobbyUI()
        {
            if (lobbyUI.Count > 0)
                return;

            var lobbyBoxWidth = (graphics.Width / 3) * 2;
            var lobbyBoxHeight = graphics.Height / 20;
            var qrScreenWidth = (graphics.Width / 5) * 3;
            var backButtonWidthAndHeight = graphics.Width / 10;
            var fontSize = graphics.Height / 25;
            var playerFontSize = fontSize / 2;

            var lobbyBackButton = new Button()
            {
                Name = "LobbyBackButton",
                Texture = cache.GetTexture2D("Textures/backButton.png"),
                Size = new IntVector2(backButtonWidthAndHeight, backButtonWidthAndHeight),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Visible = false,
                Enabled = false
            };

            var addressQRCode = new BorderImage()
            {
                Name = "AddressQRCode",
                Position = new IntVector2(0,lobbyBackButton.Position.Y + lobbyBackButton.Height + lobbyBackButton.Height / 2),
                HorizontalAlignment = HorizontalAlignment.Center,
                Size = new IntVector2(qrScreenWidth, qrScreenWidth),
                Visible = false,
                Enabled = false
            };
            
            var addressText = new Text()
            {
                Name = "AddressText",
                Position = new IntVector2(0, addressQRCode.Position.Y + addressQRCode.Height + lobbyBoxHeight / 2),
                HorizontalAlignment = HorizontalAlignment.Center,
                Visible = false,
                Enabled = false
            };

            addressText.SetFont(cache.GetFont("Fonts/arial.ttf", true), 20);
            addressText.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

            var playersText = new Text()
            {
                Name = "PlayersText",
                Value = "Players in Room",
                TextAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = new IntVector2(0, addressText.Position.Y + graphics.Height / 20),
                Visible = false,
                Enabled = false
            };

            playersText.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            playersText.SetFont(cache.GetFont("Fonts/vladimir.ttf"), fontSize);

            var player1Name = new Text()
            {
                Name = "Player1Name",
                Value = "Waiting for player 1...",
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = new IntVector2(0 , playersText.Position.Y + playersText.Height),
                Visible = false,
                Enabled = false
            };

            player1Name.SetFont("Fonts/arial.ttf", playerFontSize);
            player1Name.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

            var player2Name = new Text()
            {
                Name = "Player2Name",
                Value = "Waiting for player 2...",
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = new IntVector2(0, player1Name.Position.Y + player1Name.Height),
                Visible = false,
                Enabled = false
            };

            player2Name.SetFont("Fonts/arial.ttf", playerFontSize);
            player2Name.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

            var player3Name = new Text()
            {
                Name = "Player3Name",
                Value = "Waiting for player 3...",
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = new IntVector2(0, player2Name.Position.Y + player2Name.Height),
                Visible = false,
                Enabled = false
            };

            player3Name.SetFont("Fonts/arial.ttf", playerFontSize);
            player3Name.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

            var player4Name = new Text()
            {
                Name = "Player4Name",
                Value = "Waiting for player 4...",
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = new IntVector2(0, player3Name.Position.Y + player3Name.Height),
                Visible = false,
                Enabled = false
            };

            player4Name.SetFont("Fonts/arial.ttf", playerFontSize);
            player4Name.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

            var player5Name = new Text()
            {
                Name = "Player5Name",
                Value = "Waiting for player 5...",
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = new IntVector2(0, player4Name.Position.Y + player4Name.Height),
                Visible = false,
                Enabled = false
            };

            player5Name.SetFont("Fonts/arial.ttf", playerFontSize);
            player5Name.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

            var player6Name = new Text()
            {
                Name = "Player6Name",
                Value = "Waiting for player 6...",
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = new IntVector2(0, player5Name.Position.Y + player5Name.Height),
                Visible = false,
                Enabled = false
            };

            player6Name.SetFont("Fonts/arial.ttf", playerFontSize);
            player6Name.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));

            var startGameButton = new Button()
            {
                Name = "StartGameButton",
                //Texture = cache.GetTexture2D("Textures/startGameButton.png"),
                BlendMode = BlendMode.Replace,
                Size = new IntVector2((graphics.Width / 3) * 2, graphics.Width / 5),
                Position = new IntVector2(0, (graphics.Height / 6) * 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Visible = false,
                Enabled = false
            };

            lobbyBackButton.Pressed += LobbyBackButton_Pressed;
            startGameButton.Pressed += StartGameButton_Pressed;

            lobbyUI.Add(lobbyBackButton);
            lobbyUI.Add(addressQRCode);
            lobbyUI.Add(playersText);
            lobbyUI.Add(addressText);
            lobbyUI.Add(player1Name);
            lobbyUI.Add(player2Name);
            lobbyUI.Add(player3Name);
            lobbyUI.Add(player4Name);
            lobbyUI.Add(player5Name);
            lobbyUI.Add(player6Name);
            lobbyUI.Add(startGameButton);

            AddToUI(lobbyUI);
        }
        
        private static void AllInButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("All  In");
        }

        private static void RaiseButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Raise");
        }

        private static void CallButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Call");
        }

        private static void CheckButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Check");
        }

        private static void FoldButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Fold");
        }

        private static void PlayerExitButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(playerUI, menuUI);
            SceneManager.ShowScene(SceneManager.menuScene);
        }
        
        private static void TableExitButton_Pressed(PressedEventArgs obj)
        {
            Session.DisposeOfSockets();

            UIUtils.SwitchUI(tableUI, menuUI);
            SceneManager.ShowScene(SceneManager.menuScene);
        }

        static void JoinButton_Pressed(PressedEventArgs obj) 
		{ 
			if (joinUI.Count == 0) CreateJoinUI(); 
			UIUtils.SwitchUI(menuUI, joinUI);
		}

		static void HostButton_Pressed(PressedEventArgs obj) 
		{ 
			if (hostUI.Count == 0) CreateHostUI();
            if (lobbyUI.Count == 0) CreateLobbyUI();
            UIUtils.SwitchUI(menuUI, hostUI);
		}

		static void PlayerAvatar_Pressed(PressedEventArgs obj)
		{
			//TODO: Implement player avater press
		}

		static void SettingsButton_Pressed(PressedEventArgs obj)
		{
            CreateSettingsUI();
            UIUtils.SwitchUI(menuUI, settingsUI);
		}

		static void JoinBackButton_Pressed(PressedEventArgs obj) { UIUtils.SwitchUI(joinUI, menuUI); }
        static void HostBackButton_Pressed(PressedEventArgs obj) { UIUtils.SwitchUI(hostUI, menuUI); } 
        static void LobbyBackButton_Pressed(PressedEventArgs obj) { UIUtils.SwitchUI(lobbyUI, hostUI); Session.DisposeOfSockets(); } //TODO: Move this when the "waiting in lobby" UI is implemented

        static void PlayerNameBox_TextChanged(TextChangedEventArgs obj) { AlterLineEdit("PlayerNameBox", "Enter Player Name", joinUI); }
		static void ServerAddressBox_TextChanged(TextChangedEventArgs obj) { AlterLineEdit("ServerAddressBox", "Enter Server IP Address", joinUI); }
        static void ServerPortBox_TextChanged(TextChangedEventArgs obj) { AlterLineEdit("ServerPortBox", "Enter Server IP Port", joinUI); }

        static private void AlterLineEdit(String boxName, String emptyText, List<UIElement> uiCollection)
		{
			LineEdit textBox = null;

			foreach (var element in uiCollection)
				if (element.Name == boxName) { textBox = (LineEdit)element; break; }

			if (textBox == null) return;

			if (textBox.Text.Length > 0) { textBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 1.0f)); }
			else
			{
				textBox.TextElement.Value = emptyText;
				textBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 0.6f));
			}
		}

        static private void AlterNumericLineEdit(String boxName, String emptyText, List<UIElement> uiCollection) {
            LineEdit textBox = null;

            foreach (var element in uiCollection)
                if (element.Name == boxName) { textBox = (LineEdit)element; break; }

            if (textBox == null) return;

            if (textBox.Text.Length > 0) {
                var value = Regex.Replace(textBox.Text, "[^0-9.]","");
                textBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 1.0f));
                textBox.Text = value;
            }
            else
            {
                textBox.TextElement.Value = emptyText;
                textBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 0.6f));
            }
        }
        
        static void JoinLobbyButton_Pressed(PressedEventArgs obj)
		{
            CreatePlayerUI();
            SceneManager.CreatePlayScene();

			Node cameraNode = SceneManager.playScene.GetChild("MainCamera", true);
			var camera = cameraNode.GetComponent<Camera>();
            
            SceneManager.StopMusic(SceneManager.menuScene);
            SceneManager.ShowScene(SceneManager.playScene);
            UIUtils.SwitchUI(joinUI, playerUI);


            //THE FOLLOWING IS FOR DEBUGGING PURPOSES AND SHOULD BE DELETED WHEN FINISHED
            var tempRoom = new Room();

            var tempPlayer = new Player("TESTING", 100, null);

            tempPlayer.Init();

            tempRoom.addPlayer(tempPlayer);

            var tempTable = new Table(tempRoom, 1000);

            for(int i = 0; i < 2; i++) tempTable.dealToPlayers();
            //---------------------------------------------------------------------------
        }

		static private async void GetQRCode() //TODO: See if there is a way to move this to a QRUtils class
		{
			var scanner = new MobileBarcodeScanner();
			var options = new MobileBarcodeScanningOptions();

			options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE };

			scanner.TopText = "Join Lobby";
			scanner.BottomText = "Scan the QR code on the host's device";
			scanner.CancelButtonText = "Cancel";

			var result = await scanner.Scan(options);

			if (result == null)
				return;

			Console.WriteLine("Found QR: " + result.Text);

			var size = result.Text.Length;

            String trimmedResult;
            
            if (size > QRStringLength) //123.456.789.000:65535
                trimmedResult = result.Text.Substring(0, (int)QRStringLength);
            else
                trimmedResult = result.Text.Substring(0, size);
            
            UpdateServerAddress(trimmedResult);
		}

		static public void GenerateQRCode(String qrDataString) //TODO: Move to a QRUtils class when GetQRCode() can also be moved
		{
			var barcodeWriter = new BarcodeWriter
			{
				Format = ZXing.BarcodeFormat.QR_CODE,
				Options = new ZXing.Common.EncodingOptions
				{
					Width = 512,
					Height = 512,
					Margin = 0
				}
			};

			barcodeWriter.Renderer = new BitmapRenderer();
			var bitmap = barcodeWriter.Write(qrDataString);


			Texture2D qrCodeImage = new Texture2D();
			qrCodeImage.SetSize(512, 512, Graphics.RGBFormat, TextureUsage.Dynamic);
			qrCodeImage.SetNumLevels(1);

			var stream = new MemoryStream();

#if __ANDROID__

				bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 100, stream);
#endif

#if __IOS__

			using (NSData imageData = bitmap.AsJPEG())
			{
				Byte[] myByteArray = new Byte[imageData.Length];
				System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
				stream.Write(myByteArray, 0, myByteArray.Length);
			}


			#endif
			stream.Position = 0;

			var image = new Image();
			image.Load(new MemoryBuffer(stream));

			qrCodeImage.SetData(image, true);

			ShowAddress(qrCodeImage, qrDataString);
		}

		private static void ShowAddress(Texture qrCodeImg, String address)
		{
			BorderImage qrCode = null;
			Text addressText = null;

			foreach (var element in lobbyUI) { if (element.Name == "AddressQRCode") qrCode = (BorderImage)element; }
			foreach (var element in lobbyUI) { if (element.Name == "AddressText") addressText = (Text)element; }

			if(qrCode!=null) qrCode.Texture = qrCodeImg;
			if (addressText != null) addressText.Value = address;
		}

		private static void UpdateServerAddress(String value)
		{
            var address = value.Split(':');
			LineEdit serverAddress = null;
            LineEdit serverPort = null;

			foreach (var element in joinUI) { if (element.Name == "ServerAddressBox") serverAddress = (LineEdit)element; }
            foreach (var element in joinUI) { if (element.Name == "ServerPortBox") serverPort = (LineEdit)element; }
            if (serverAddress != null) { Application.InvokeOnMain(new Action(() => serverAddress.Text = address[0])); }
            if (serverPort != null) { Application.InvokeOnMain(new Action(() => serverPort.Text = address[1])); }
        }
        
        static void ScanQRButton_Pressed(PressedEventArgs obj)
        {
            GetQRCode();
        }

        static void CreateLobbyButton_Pressed(PressedEventArgs obj)
		{
            UIUtils.SwitchUI(hostUI, lobbyUI);
            Session.getinstance().init();
        }

        static void StartGameButton_Pressed(PressedEventArgs obj)
        {
            CreateTableUI();
            SceneManager.CreateHostScene();

            Node cameraNode = SceneManager.hostScene.GetChild("MainCamera", true);
            var camera = cameraNode.GetComponent<Camera>();

            SceneManager.StopMusic(SceneManager.menuScene);
            SceneManager.ShowScene(SceneManager.hostScene);
            UIUtils.SwitchUI(lobbyUI, tableUI);

            //THE FOLLOWING IS FOR DEBUGGING PURPOSES AND SHOULD BE DELETED WHEN FINISHED
            var tempRoom = new Room();

            var game = new PokerGame(tempRoom, SceneManager.hostScene, ui, cache, 1000);

            game.start();
            //---------------------------------------------------------------------------
        }

        static public void AddToUI(List<UIElement> elements)
		{
			foreach (var element in elements) { ui.Root.AddChild(element); }
		}
	}
}
