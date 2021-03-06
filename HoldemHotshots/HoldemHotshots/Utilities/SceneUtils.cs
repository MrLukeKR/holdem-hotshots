﻿using HoldemHotshots.GameLogic;
using HoldemHotshots.GameLogic.Player;
using HoldemHotshots.Managers;
using System;
using System.Collections.Generic;
using Urho;
using Urho.Gui;

namespace HoldemHotshots.Utilities
{
    /// <summary>
    /// Provide helpful functions to manipulate scenes
    /// </summary>
    class SceneUtils
    {
        public static readonly Vector3[] PLAYER_POSITIONS = {
            Card.CARD_TABLE_POSITIONS[2] - new Vector3(2.75f, -4.50f, 0), //TODO: Normalise this to be some percentage of the screen size
            Card.CARD_TABLE_POSITIONS[2] - new Vector3(2.75f, -2.75f, 0),
            Card.CARD_TABLE_POSITIONS[2] - new Vector3(2.75f, -1.00f, 0),
            Card.CARD_TABLE_POSITIONS[2] - new Vector3(2.75f,  1.00f, 0),
            Card.CARD_TABLE_POSITIONS[2] - new Vector3(2.75f,  2.75f, 0),
            Card.CARD_TABLE_POSITIONS[2] - new Vector3(2.75f,  4.50f, 0),
        };

        public static readonly Vector3 PLAYER_CARD1_OFFSET = new Vector3(0.6f,  0.4f, 0);
        public static readonly Vector3 PLAYER_CARD2_OFFSET = new Vector3(0.6f, -0.4f, 0);

        /// <summary>
        /// Searchs a scene for a node with the given name
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="sceneToSearch">Scene that contains the node</param>
        /// <returns>The node found with the given name</returns>
        public static Node FindNode(string nodeName, Scene sceneToSearch)
        {
            foreach (Node node in sceneToSearch.Children)
                if (node.Name == nodeName)
                    return node;
            return null;
        }

        /// <summary>
        /// Finds a component from a node in a given scene
        /// </summary>
        /// <typeparam name="T">Type of component</typeparam>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="sceneToSearch">Scene that contains the node</param>
        /// <returns>The component from the node found with the given name</returns>
        public static T FindComponent<T>(string nodeName, Scene sceneToSearch) where T : Component
        {
            return FindNode(nodeName, sceneToSearch).GetComponent<T>();
        }

        /// <summary>
        /// Updates the displayed value of the Pot's balance
        /// </summary>
        /// <param name="amount">Pot balance</param>
        public static void UpdatePotBalance(uint amount)
        {
            Application.InvokeOnMain(new Action(() =>
            {
               Text3D potText = FindComponent<Text3D>("PotInfoText", SceneManager.hostScene);
              
                if (potText != null)
                    potText.Text = "Pot\n$" + amount;
            }));
        }
        
        /// <summary>
        /// Updates the displayed information for a given player's name
        /// </summary>
        /// <param name="playerName">Player to update</param>
        /// <param name="information">Information to display by the player</param>
        public static void UpdatePlayerInformation(string playerName, string information)
        {
            Text3D playerText = FindComponent<Text3D>(playerName, SceneManager.hostScene);

            if (playerText != null)
                playerText.Text = playerName + "\n" + information;
        }

        /// <summary>
        /// Shows the player's cards on the Table screen
        /// </summary>
        /// <param name="index">Index of the player</param>
        /// <param name="playerName">Player's name</param>
        /// <param name="hand">String version of player's hand</param>
        /// <param name="card1">First card</param>
        /// <param name="card2">Second card</param>
        /// <param name="folded">True/False of whether the player has folded</param>
        public static void ShowPlayerCards(int index, string playerName, string hand, Card card1, Card card2, bool folded)
        {
            Application.InvokeOnMain(new Action(() =>
            {
                card1.node.Rotate(new Quaternion(0, 0, -90), TransformSpace.Local);
                card1.node.Position = PLAYER_POSITIONS[index] + PLAYER_CARD1_OFFSET;
                card1.node.Scale = new Vector3(0.75f, 1.05f, 0);
                
                card2.node.Rotate(new Quaternion(0, 0, -90), TransformSpace.Local);
                card2.node.Position = PLAYER_POSITIONS[index] + PLAYER_CARD2_OFFSET;
                card2.node.Scale = new Vector3(0.75f, 1.05f, 0);

                if (folded)
                {
                    card1.HideCard();
                    card2.HideCard();
                }

                SceneManager.hostScene.AddChild(card1.node);
                SceneManager.hostScene.AddChild(card2.node);

                if (folded)
                    UpdatePlayerInformation(playerName, "Folded");
                else
                    UpdatePlayerInformation(playerName, hand);
            }));   
        }

        /// <summary>
        /// Displays winner information
        /// </summary>
        /// <param name="player">Winning player</param>
        /// <param name="hand">Winning hand</param>
        public static void DisplayWinner(ServerPlayer player, CardRanker.Hand hand)
        {
            Application.InvokeOnMain(new Action(() =>
            {
                Text3D message = FindComponent<Text3D>("WinnerText", SceneManager.hostScene);

                message.Text = player.name + " wins!\n" + CardRanker.ToString(hand);
                SpeechManager.Speak(player.name + " wins with a " + CardRanker.ToString(hand));
            }));
        }

        /// <summary>
        /// Displays multiple winner information
        /// </summary>
        /// <param name="drawingPlayers">Winning players</param>
        /// <param name="hand">Winning hand</param>
        public static void DisplayWinners(List<ServerPlayer> drawingPlayers, CardRanker.Hand hand)
        {
            Application.InvokeOnMain(new Action(() =>
            {
                Text3D message       = FindComponent<Text3D>("WinnerText", SceneManager.hostScene);
                string winnerMessage = "";

                foreach (ServerPlayer player in drawingPlayers)
                {
                    if (player == drawingPlayers[drawingPlayers.Count - 1])
                        winnerMessage += " and ";
                    else if (player != drawingPlayers[0])
                        winnerMessage += ", ";

                    winnerMessage += player.name;
                }

                SpeechManager.Speak(winnerMessage + " win with " + CardRanker.ToString(hand) + "s");
                message.Text = "Draw!\n" + CardRanker.ToString(hand);
            }));
        }

        /// <summary>
        /// Displays initial player information on the screen
        /// </summary>
        /// <param name="players">Array of players</param>
        public static void InitPlayerInformation(List<ServerPlayer> players)
        {
            Application.InvokeOnMain(new Action(() =>
            {
                int i = 0;
                foreach (ServerPlayer player in players)
                {
                    var playerNameNode = SceneManager.hostScene.CreateChild(player.name);
                    var playerName = playerNameNode.CreateComponent<Text3D>();

                    playerName.Text = player.name;
                    playerName.TextAlignment = HorizontalAlignment.Center;
                    playerName.HorizontalAlignment = HorizontalAlignment.Center;
                    playerName.SetFont(Application.Current.ResourceCache.GetFont("Fonts/arial.ttf"), 20);
                    playerNameNode.Position = PLAYER_POSITIONS[i++];
                    playerNameNode.Rotate(new Quaternion(0, 0, -90),TransformSpace.Local);
                }
            }));
        }
    }
}