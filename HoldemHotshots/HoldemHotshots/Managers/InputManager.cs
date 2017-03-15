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
    class InputManager
    {
        public static void RaiseCancelButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.playerUI_raise, UIManager.playerUI);
        }

        public static void RaiseConfirmButton_Pressed(PressedEventArgs obj)
        {
            ClientManager.session.player.Raise();
            UIUtils.SwitchUI(UIManager.playerUI_raise, UIManager.playerUI);
        }

        public static void IncreaseBetButton_Pressed(PressedEventArgs obj)
        {
            var amount = UIUtils.GetRaiseAmount(false);
            var playerBalance = 1000; //TODO: Get player balance

            if (amount + 1 < playerBalance)
                UIUtils.UpdateRaiseBalance(UIUtils.GetRaiseAmount(false) + 1);
        }

        public static void DecreaseBetButton_Pressed(PressedEventArgs obj)
        {
            var amount = UIUtils.GetRaiseAmount(false);
            if (amount > 0)
                UIUtils.UpdateRaiseBalance(amount - 1);
        }

        public static void RaiseExitButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.playerUI_raise, UIManager.menuUI);
        }

        public static void AllInButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("All  In");
            ClientManager.session.player.AllIn();
        }

        public static void RaiseButton_Pressed(PressedEventArgs obj)
        {
            UIManager.CreatePlayerRaiseUI();
            UIUtils.SwitchUI(UIManager.playerUI, UIManager.playerUI_raise);
        }

        public static void CallButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Call");
            ClientManager.session.player.Call();
        }

        public static void CheckButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Check");
            ClientManager.session.player.Check();
        }

        public static void FoldButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.DisplayPlayerMessage("Fold");
            ClientManager.session.player.Fold();
        }

        public static void PlayerExitButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.playerUI, UIManager.menuUI);
            SceneManager.ShowScene(SceneManager.menuScene);
        }

        public static void TableExitButton_Pressed(PressedEventArgs obj)
        {
            Session.DisposeOfSockets();

            UIUtils.SwitchUI(UIManager.tableUI, UIManager.menuUI);
            SceneManager.ShowScene(SceneManager.menuScene);
        }

        public static void SettingsExitButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.settingsUI, UIManager.menuUI);
        }
        
        public static void JoinButton_Pressed(PressedEventArgs obj)
        {
            if (UIManager.joinUI.Count == 0) UIManager.CreateJoinUI();
            UIUtils.SwitchUI(UIManager.menuUI, UIManager.joinUI);
        }

        public static void HostButton_Pressed(PressedEventArgs obj)
        {
            Session.getinstance().init();
            if (UIManager.lobbyUI.Count == 0) UIManager.CreateLobbyUI();
            UIUtils.SwitchUI(UIManager.menuUI, UIManager.lobbyUI);
        }

        public static void SettingsButton_Pressed(PressedEventArgs obj)
        {
            UIManager.CreateSettingsUI();
            UIUtils.SwitchUI(UIManager.menuUI, UIManager.settingsUI);
        }
        
        public static void StartGameButton_Pressed(PressedEventArgs obj)
        {
            UIManager.CreateTableUI();
            SceneManager.CreateHostScene();

            new Thread(UIUtils.StartGame).Start();
        }

        public static void JoinBackButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.joinUI, UIManager.menuUI);
        }

        public static void LobbyBackButton_Pressed(PressedEventArgs obj)
        {
            UIUtils.SwitchUI(UIManager.lobbyUI, UIManager.menuUI);
            Session.DisposeOfSockets();
        }

        public static async void ScanQRButton_Pressed(PressedEventArgs obj)
        {
            var result  = await UIUtils.GetQRCode();
            var trimmed = result.Substring(0, Math.Min(UIManager.QR_STRING_LENGTH, result.Length));

            UIUtils.UpdateServerAddress(trimmed);
        }

        public static void PlayerNameBox_TextChanged(TextChangedEventArgs obj)
        {
            UIUtils.AlterLineEdit("PlayerNameBox", "Enter Player Name", UIManager.joinUI);
        }

        public static void ServerAddressBox_TextChanged(TextChangedEventArgs obj)
        {
            UIUtils.AlterLineEdit("ServerAddressBox", "Enter Server IP Address", UIManager.joinUI);
            UIUtils.AlterJoin(UIUtils.ValidateServer() && UIUtils.ValidatePort());
        }

        public static void ServerPortBox_TextChanged(TextChangedEventArgs obj)
        {
            UIUtils.AlterLineEdit("ServerPortBox", "Enter Server IP Port", UIManager.joinUI);
            UIUtils.AlterJoin(UIUtils.ValidateServer() && UIUtils.ValidatePort());
        }

        public static void JoinLobbyButton_Pressed(PressedEventArgs obj)
        {
            if (!(UIUtils.ValidateServer() && UIUtils.ValidatePort()))
                return;
            
            LineEdit ipAddress = null, port = null;
            
            foreach (UIElement element in UIManager.joinUI)
                if (element.Name == "ServerAddressBox")
                    ipAddress = (LineEdit)element;

            foreach (UIElement element in UIManager.joinUI)
                if (element.Name == "ServerPortBox")
                    port = (LineEdit)element;
            
            var newPlayer = new ClientPlayer(UIUtils.GetPlayerName(), 0);
           
            ClientManager.session = new ClientSession(ipAddress.Text, int.Parse(port.Text), newPlayer); //TODO: Refactor this to be non-static

            if (ClientManager.session.Connect())
            {
                ClientManager.session.Init();

                UIManager.CreatePlayerUI();
                SceneManager.CreatePlayScene();

                newPlayer.Init();

                SceneManager.ShowScene(SceneManager.playScene);
                UIUtils.SwitchUI(UIManager.joinUI, UIManager.playerUI);
                Application.InvokeOnMain(new Action(() => UIUtils.DisableIO()));
            }
            else
            {
                //TODO: Error message
            }
        }
    }
}