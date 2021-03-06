﻿using HoldemHotshots.GameLogic.Player;
using HoldemHotshots.Networking.ClientNetworkEngine;
using HoldemHotshots.Networking.ServerNetworkEngine;
using HoldemHotshots.Utilities;
using System;
using System.Threading;
using Urho;
using Urho.Gui;

namespace HoldemHotshots.Managers
{
    /// <summary>
    /// Handles button presses on the server and client sides
    /// </summary>
    class InputManager
    {
        /// <summary>
        /// If the cancel button is pressed on the Raise screen, the screen is returned to the playing screen
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void RaiseCancelButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.UpdateRaiseBalance(1);
            UIUtils.SwitchUI(UIManager.playerUI_raise, UIManager.playerUI);
            UIUtils.ToggleCallOrCheck(ClientManager.highestBid);
        }

        /// <summary>
        /// If the confirm button is pressed on the Raise screen, a raise command is sent to the server and the UI is returned to the playing screen
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void RaiseConfirmButton_Pressed(PressedEventArgs obj)
        {
            ClientManager.session.player.Raise();
            UIUtils.SwitchUI(UIManager.playerUI_raise, UIManager.playerUI);
            UIUtils.ToggleCallOrCheck(ClientManager.highestBid);
        }

        /// <summary>
        /// Pressing the increase bet button causes the raise amount to increase
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void IncreaseBetButton_Pressed(PressedEventArgs obj)
        {
            var amount = UIUtils.GetRaiseAmount(false);
            var playerBalance = UIUtils.GetPlayerBalance();
            var button = (Button)obj.Element;
            
            if (amount + 1 <= playerBalance)
                UIUtils.UpdateRaiseBalance(amount + 1);
        }

        /// <summary>
        /// Pressing the decrease bet button causes the raise amount to decrease
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void DecreaseBetButton_Pressed(PressedEventArgs obj)
        {
            var amount = UIUtils.GetRaiseAmount(false);

            if (amount -1 >= 1)
                UIUtils.UpdateRaiseBalance(amount - 1);
        }

        /// <summary>
        /// Pressing the exit button on the raise screen will switch the UI back to the menuUI
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void RaiseExitButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.playerUI_raise, UIManager.menuUI);
        }

        /// <summary>
        /// Pressing the all-in button will send an all-in command to the server
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void AllInButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("All  In");
            ClientManager.session.player.AllIn();
        }

        /// <summary>
        /// Pressing the raise button will switch to the Raise UI
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void RaiseButton_Pressed(PressedEventArgs obj)
        {
            UIManager.CreatePlayerRaiseUI();
            if (UIUtils.GetPlayerBalance() > 0)
                UIUtils.SwitchUI(UIManager.playerUI, UIManager.playerUI_raise);
            else
                UIUtils.DisplayPlayerMessage("Insufficient Chips!");
        }

        /// <summary>
        /// Pressing the QR Code button will convert the information stored about the server into a QR code to make scanning the information easier (clients can share the code amongst each other)
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void QrCodeButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.ConvertClientInfoToQR();
        }

        /// <summary>
        /// Pressing the call button will send a call command to the server
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void CallButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Call");
            ClientManager.session.player.Call();
        }

        /// <summary>
        /// Pressing the check button will send a check command to the server
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void CheckButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Check");
            ClientManager.session.player.Check();
        }

        /// <summary>
        /// Pressing the fold button will send a fold command to the server
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void FoldButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Fold");
            ClientManager.session.player.Fold();
        }

        /// <summary>
        /// Pressing the exit button on the playing screen will disconnect the user and return to the menu
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void PlayerExitButton_Pressed(PressedEventArgs obj)
        {
            ClientManager.session.Disconnect();
            UIUtils.SwitchUI(UIManager.playerUI, UIManager.menuUI);
            SceneManager.ShowScene(SceneManager.menuScene);
        }

        /// <summary>
        /// Pressing the exit button on the table screen will disconnect all players and return to the menu
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void TableExitButton_Pressed(PressedEventArgs obj)
        {
            Session.DisposeOfSockets();
            UIUtils.SwitchUI(UIManager.tableUI, UIManager.menuUI);
            SceneManager.ShowScene(SceneManager.menuScene);
        }

        /// <summary>
        /// Pressing the exit button on the Settings UI will return to the menu
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void SettingsExitButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.settingsUI, UIManager.menuUI);
        }

        /// <summary>
        /// Pressing the join button will create the Join UI and switch to it
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void JoinButton_Pressed(PressedEventArgs obj)
        {
            if (UIManager.joinUI.Count == 0) UIManager.CreateJoinUI();
            UIUtils.SwitchUI(UIManager.menuUI, UIManager.joinUI);
        }

        /// <summary>
        /// Pressing the game restart button will reset all play information, switch the order of the players and create a new game
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void GameRestartButton_Pressed(PressedEventArgs obj)
        {
            foreach(ServerPlayer player in Session.Lobby.players)
                player.Reset();

            ServerPlayer backOfQueue = Session.Lobby.players[0];
            Session.Lobby.players.RemoveAt(0);
            Session.Lobby.players.Add(backOfQueue);

            foreach (UIElement element in UIManager.tableUI)
                if (element.Name == "GameRestartButtonNoAutoLoad")
                    UIUtils.DisableAndHide(element);

            SceneManager.CreateHostScene();
            SceneManager.ShowScene(SceneManager.hostScene);

            SceneUtils.InitPlayerInformation(Session.Lobby.players);

            new Thread(UIUtils.RestartGame).Start();
        }

        /// <summary>
        /// Pressing the host button will initialise the server and switch to the lobby UI
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void HostButton_Pressed(PressedEventArgs obj)
        {
            try
            {
                Session.Getinstance().Init();
            }
            catch { }
            if (UIManager.lobbyUI.Count == 0)
                UIManager.CreateLobbyUI();
            foreach (UIElement elem in UIManager.lobbyUI)
                if (elem.Name == "StartGameButton")
                    UIUtils.DisableAccess(elem);
            UIUtils.SwitchUI(UIManager.menuUI, UIManager.lobbyUI);
        }

        /// <summary>
        /// Pressing the settings button will switch to the settings UI screen
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void SettingsButton_Pressed(PressedEventArgs obj)
        {
            UIManager.CreateSettingsUI();
            UIUtils.SwitchUI(UIManager.menuUI, UIManager.settingsUI);
        }

        /// <summary>
        /// Pressing the start game button will create a new game and switch to the taable UI
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void StartGameButton_Pressed(PressedEventArgs obj)
        {
            if (Session.Lobby.players.Count < 2)
                return;

            UIManager.CreateTableUI();
            SceneManager.CreateHostScene();

            SceneUtils.InitPlayerInformation(Session.Lobby.players);

            new Thread(UIUtils.StartGame).Start();
        }

        /// <summary>
        /// Pressing the back button on the join screen will switch to the menu UI
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void JoinBackButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.joinUI, UIManager.menuUI);
        }

        /// <summary>
        /// Pressing the back button on the lobby disconnects from any currently connected players and switches to the menu UI
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void LobbyBackButton_Pressed(PressedEventArgs obj)
        {
            Session.DisposeOfSockets();
            UIUtils.SwitchUI(UIManager.lobbyUI, UIManager.menuUI);
        }

        /// <summary>
        /// Pressing the scan QR button switches to a camera screen in which the user can scan a QR code in order to connect to the server
        /// </summary>
        /// <param name="obj">Input event</param>
        public static async void ScanQRButton_Pressed(PressedEventArgs obj)
        {
            var result  = await UIUtils.GetQRCode();

            if(result.Length > 0)
                UIUtils.UpdateServerAddress(result);
        }

        /// <summary>
        /// If the player name textbox is altered and becomes empty, it is replaced with a default string
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void PlayerNameBox_TextChanged(TextChangedEventArgs obj)
        {
            UIUtils.AlterLineEdit("PlayerNameBox", "Enter Player Name", UIManager.joinUI);
        }

        /// <summary>
        /// If the join lobby button is pressed, the player connects to the server and switches to the playing UI
        /// </summary>
        /// <param name="obj">Input event</param>
        public static void JoinLobbyButton_Pressed(PressedEventArgs obj)
        {
            if (!(UIUtils.ValidateServer() && UIUtils.ValidatePort() &&UIUtils.ValidateKey() && UIUtils.ValidateIV()))
                return;

            string address = ClientManager.serverAddress;
            string port = ClientManager.serverPort;
            string key = ClientManager.serverKey;
            string iv = ClientManager.serverIV;

            var newPlayer = new ClientPlayer(UIUtils.GetPlayerName(), 0);
           
            ClientManager.session = new ClientSession(address, int.Parse(port), newPlayer, key, iv);

            if (ClientManager.session.Connect())
            {
                ClientManager.session.Init();
                UIManager.CreatePlayerUI();              
                SceneManager.CreatePlayScene();

                newPlayer.Init();

                SceneManager.ShowScene(SceneManager.playScene);
                UIUtils.SwitchUI(UIManager.joinUI, UIManager.playerUI);

                UIUtils.ToggleCallOrCheck(0);

                Application.InvokeOnMain(new Action(() => UIUtils.DisableIO()));
            }
        }
    }
}