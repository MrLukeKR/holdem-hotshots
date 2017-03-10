﻿#if __IOS__
using Foundation;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Urho.Gui;
using Urho.Resources;
using Urho.Urho2D;
using ZXing.Mobile;

namespace HoldemHotshots
{
	public static class UIUtils
	{
        const float DISABLED_OPACITY = 0.2f;
        const float ENABLED_OPACITY = 1f;

        public static void enableAndShow(UIElement element)
		{
            Application.InvokeOnMain(new Action(() =>
            {
                element.Visible = true;
                element.Enabled = true;
            }));
		}

		public static void disableAndHide(UIElement element)
		{
            Application.InvokeOnMain(new Action(() =>
            {
                element.Visible = false;
                element.Enabled = false;
            }));
		}

        public static void disableAccess(UIElement element)
        {
                element.Enabled = false;
                element.Opacity = DISABLED_OPACITY;
        }

        public static void enableAccess(UIElement element)
        {
                element.Enabled = true;
                element.Opacity = ENABLED_OPACITY;
        }

		//UI switching
		public static void ShowUI(List<UIElement> uiCollection) { foreach (var uiElement in uiCollection) UIUtils.enableAndShow(uiElement); }
		public static void HideUI(List<UIElement> uiCollection) { foreach (var uiElement in uiCollection) UIUtils.disableAndHide(uiElement); }
		public static void SwitchUI(List<UIElement> from, List<UIElement> to) { HideUI(from); ShowUI(to); }

        internal static void DisplayPlayerMessage(string message)
        {
            Application.InvokeOnMain(new Action(() =>
            {
                if (UIManager.playerUI == null) return;

                Text statusText = null;

                foreach (UIElement element in UIManager.playerUI) if (element.Name == "PlayerInfoText") statusText = (Text)element;

                if (statusText != null) statusText.Value = message; //TODO: Limit to a certain number of characters
            }));
        }

        internal static void UpdatePlayerBalance(uint balance)
        {
            Application.InvokeOnMain(new Action(() =>
            {
                Text statusText = null;

                foreach (UIElement element in UIManager.playerUI) if (element.Name == "PlayerBalanceText") statusText = (Text)element;

                if (statusText != null) statusText.Value = "$" + balance.ToString() + " "; //TODO: Alter the position to remove the preceding spacing
            }));
        }

        public static String GetPlayerName()
        {
            LineEdit playerName = null;

            foreach (UIElement element in UIManager.joinUI) if (element.Name == "PlayerNameBox") playerName = (LineEdit)element;

            if (playerName != null)
                if (playerName.Text.Length == 0)
                    return "Unknown Player";
                else
                    return playerName.Text;
            else
                return "Unknown Player";
        }

       public static void UpdatePlayerList(Room room)
        {
            Console.WriteLine("Updating Player List...");
            String playerList = "";

            Text playerNames = null;

            var max = room.MaxRoomSize;
            var curr = room.getRoomSize();

            for (int i = 0; i < curr; i++) playerList += room.getPlayer(i).name + "\n";
            for (int i = curr + 1; i <= max; i++) playerList += "Waiting for Player " + i + "...\n";

            Console.WriteLine("Generated List: ");
            Console.WriteLine(playerList);

            Application.InvokeOnMain(new Action(() =>
            {
                foreach (UIElement element in UIManager.lobbyUI) if (element.Name == "PlayerNames") playerNames = (Text)element;

                if (playerNames != null)
                {
                    playerNames.Value = playerList;
                    Console.WriteLine("Updated Player List Successfully!");
                }
            }
            ));
        }

        internal static uint GetBuyIn() //TODO: Get the buy in from user entry box
        {
            return 0;
        }

        public static void DisplayLobbyMessage(string message)
        {
            Text lobbyText = null;

            Application.InvokeOnMain(new Action(() =>
            {
                foreach (UIElement element in UIManager.lobbyUI) if (element.Name == "LobbyMessageText") lobbyText = (Text)element;

                if (lobbyText != null) lobbyText.Value = message; //TODO: Alter the position to remove the preceding spacing
            }));
        }

        public static void UpdatePotBalance(uint amount)
        {
            Text potText = null;

            Application.InvokeOnMain(new Action(() =>
            {
                foreach (UIElement element in UIManager.tableUI) if (element.Name == "PotInfoText") potText = (Text)element;

                if (potText != null) potText.Value = "Pot\n$"+ amount; //TODO: Alter the position to remove the preceding spacing
            }));
        }

        internal static void disableIO()
        {
            Console.WriteLine("Disabling IO");
            foreach (UIElement element in UIManager.playerUI)
            {
                if (element.Name.Contains("Button") && element.Name != "PlayerExitButton")
                {
                    disableAccess(element);
                }
            }
        }

        internal static void enableIO()
        {
                DisplayPlayerMessage("It's Your Turn!");
                foreach (UIElement element in UIManager.playerUI) if (element.Name.Contains("Button") && element.Name != "PlayerExitButton") enableAccess(element);
        }

        internal static uint PopRaiseAmount()
        {
            Text elmnt = null;
            uint amount;
            foreach (UIElement element in UIManager.playerUI_raise) if (element.Name == "CurrentBetText") elmnt = (Text)element;
            amount = uint.Parse(elmnt.Value.Substring(1));
            elmnt.Value = "$0";
            return amount;
        }

        public static uint GetRaiseAmount()
        {
            Text elmnt = null;
            uint amount;
            foreach (UIElement element in UIManager.playerUI_raise) if (element.Name == "CurrentBetText") elmnt = (Text)element;
            amount = uint.Parse(elmnt.Value.Substring(1));
            return amount;
        }

        internal static void UpdateRaiseBalance(uint amount)
        {
            Text elmnt = null;
            foreach (UIElement element in UIManager.playerUI_raise) if (element.Name == "CurrentBetText") elmnt = (Text)element;
            elmnt.Value = "$" + amount;
        }

        [SecurityCritical]
        static public void GenerateQRCode(String qrDataString, bool isServer)
        {
            if (isServer)
                Application.InvokeOnMain(new Action(() => CreateQRCode(qrDataString, isServer)));
            else
                CreateQRCode(qrDataString, isServer);
        }

        public static void CreateQRCode(String data, bool isServer)
        {
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
                var bitmap = barcodeWriter.Write(data);

                Texture2D qrCodeImage = new Texture2D();
                qrCodeImage.SetSize(512, 512, Graphics.RGBFormat, TextureUsage.Dynamic);
                qrCodeImage.SetNumLevels(1);

                MemoryStream stream = new MemoryStream();

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

                Image image = new Image();
                image.Load(new MemoryBuffer(stream));

                qrCodeImage.SetData(image, false);

                if (isServer)
                    ShowServerAddress(qrCodeImage, data);
                else
                    ShowClientAddress(qrCodeImage);
            }
        }

        private static void ShowServerAddress(Texture qrCodeImg, String address)
        {
            BorderImage qrCode = null;
            Text addressText = null;

            foreach (var element in UIManager.lobbyUI) { if (element.Name == "AddressQRCode") qrCode = (BorderImage)element; }
            foreach (var element in UIManager.lobbyUI) { if (element.Name == "AddressText") addressText = (Text)element; }

            if (qrCode != null) qrCode.Texture = qrCodeImg;
            if (addressText != null) addressText.Value = address;
        }

        public static void UpdateServerAddress(string value)
        {
            var address = value.Split(':');
            LineEdit serverAddress = null;
            LineEdit serverPort = null;

            foreach (var element in UIManager.joinUI) { if (element.Name == "ServerAddressBox") serverAddress = (LineEdit)element; }
            foreach (var element in UIManager.joinUI) { if (element.Name == "ServerPortBox") serverPort = (LineEdit)element; }
            if (serverAddress != null) { Application.InvokeOnMain(new Action(() => serverAddress.Text = address[0])); }
            if (serverPort != null) { Application.InvokeOnMain(new Action(() => serverPort.Text = address[1])); }
        }

        static public async Task<string> GetQRCode() //TODO: See if there is a way to move this to a QRUtils class
        {
            var scanner = new MobileBarcodeScanner();
            var options = new MobileBarcodeScanningOptions();

            options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE };

            scanner.TopText = "Join Lobby";
            scanner.BottomText = "Scan the QR code on the host's device";
            scanner.CancelButtonText = "Cancel";

            var result = await scanner.Scan(options);

            if (result != null)
                return result.Text;
            else
                return "";

        }

        private static void ShowClientAddress(Texture qrCodeImg)
        {
            BorderImage qrCode = null;

            foreach (var element in UIManager.joinUI) { if (element.Name == "ClientQRCode") qrCode = (BorderImage)element; }

            if (qrCode != null) qrCode.Texture = qrCodeImg;
        }

        static private void Countdown()
        {
            var soundnode = SceneManager.hostScene.GetChild("SFX", true);
            var sound = soundnode.GetComponent<SoundSource>(true);
            Application.InvokeOnMain(new Action(() => sound.Play(UIManager.cache.GetSound("Sounds/Shuffle.wav"))));

            foreach (UIElement element in UIManager.lobbyUI) if (element.Name != "LobbyMessageText") UIUtils.disableAndHide(element);

            for (int i = 3; i > 0; i--)
            {
                DisplayLobbyMessage("Starting game in " + i);
                Thread.Sleep(1000);
            }
        }

        static public void StartGame()
        {
            Countdown();

            Application.InvokeOnMain(new Action(() => SceneManager.ShowScene(SceneManager.hostScene)));
            UIUtils.SwitchUI(UIManager.lobbyUI, UIManager.tableUI);


            DisplayLobbyMessage("Players in Room"); //Reset the message

            var game = new PokerGame(Session.getinstance().getRoom());
            game.Start();
        }

        static public void AddToUI(List<UIElement> elements)
        {
            foreach (var element in elements) { UIManager.ui.Root.AddChild(element); }
        }

        static public bool ValidateServer()
        {
            LineEdit server = null;
            foreach (var element in UIManager.joinUI) if (element.Name == "ServerAddressBox") server = (LineEdit)element;

            if (server.Text.Length > 0 && Regex.IsMatch(server.Text, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")) return true;
            else return false;
        }

        static public bool ValidatePort()
        {
            LineEdit port = null;
            foreach (var element in UIManager.joinUI) if (element.Name == "ServerPortBox") port = (LineEdit)element;
            if (port.Text.Length > 0) return true;
            else return false;
        }

        static public void AlterJoin(bool enable)
        {
            Button joinButton = null;
            foreach (var element in UIManager.joinUI) if (element.Name == "JoinLobbyButton") joinButton = (Button)element;

            if (enable)
            {
                LineEdit server = null;
                LineEdit port = null;

                foreach (var element in UIManager.joinUI) if (element.Name == "ServerAddressBox") server = (LineEdit)element;
                foreach (var element in UIManager.joinUI) if (element.Name == "ServerPortBox") port = (LineEdit)element;

                UIUtils.GenerateQRCode(server.Text + ':' + port.Text, false);

                UIUtils.enableAccess(joinButton);
            }
            else if (!enable)
                UIUtils.disableAccess(joinButton);
        }

        static public void AlterLineEdit(String boxName, String emptyText, List<UIElement> uiCollection)
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

        static public void AlterNumericLineEdit(String boxName, String emptyText, List<UIElement> uiCollection)
        {
            LineEdit textBox = null;

            foreach (var element in uiCollection)
                if (element.Name == boxName) { textBox = (LineEdit)element; break; }

            if (textBox == null) return;

            if (textBox.Text.Length > 0)
            {
                var value = Regex.Replace(textBox.Text, "[^0-9.]", "");
                textBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 1.0f));
                textBox.Text = value;
            }
            else
            {
                textBox.TextElement.Value = emptyText;
                textBox.TextElement.SetColor(new Color(0.0f, 0.0f, 0.0f, 0.6f));
            }
        }

    }
}
