﻿using HoldemHotshots.Managers;
using HoldemHotshots.GameLogic.Player;
using HoldemHotshots.Networking.ServerNetworkEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Urho;
using Urho.Actions;
using Urho.Audio;
using HoldemHotshots.Utilities;

namespace HoldemHotshots.GameLogic
{
    /// <summary>
    /// Managers the player interactions with the game and the table hand
    /// </summary>
    class Table
    {
        private Deck deck = new Deck();
        public List<Card> hand { get; } = new List<Card>();
        private Pot pot = new Pot(50, 100);
        Node soundnode;
        SoundSource sound;

        private Room room { get;  set; }
        
        public Table(Room room)
        {
            setRoom(room);
            InitSound();
            ServerCommandManager.SetPot(pot);
            foreach (ServerPlayer player in room.players)
                player.pot = pot;
            deck.Shuffle();
        }
        
        /// <summary>
        /// Sends a reset command to each player in the room
        /// </summary>
        internal void ResetTable()
        {
            foreach (ServerPlayer player in room.players)
                player.Reset();
        }

        /// <summary>
        /// Initialises the scene's sound node
        /// </summary>
        private void InitSound()
        {
                soundnode = SceneManager.hostScene.GetChild("SFX", true);
                sound = soundnode.GetComponent<SoundSource>(true);
        }
        
        /// <summary>
        /// Deals the first 3 table cards
        /// </summary>
        public void Flop() {
            for (int i = 0; i < 3; i++)
            {
                DealToTable(i);
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Deals a card to the table at the given index
        /// </summary>
        /// <param name="index">Index to add the card to</param>
        public void DealToTable(int index)
        {
            Card newCard;
            Node newCardNode;

            hand.Add(deck.TakeCard());
            newCard = hand[index];
            newCardNode = newCard.node;
            newCardNode.Position = Card.CARD_TABLE_DEALING_POSITION;

            DoAnimation(index, newCard, newCardNode);
        }

        /// <summary>
        /// Adds a card to the scene and performs a card animation on the table
        /// </summary>
        /// <param name="index">Index of card to animate</param>
        /// <param name="newCard">New card to add to the scene</param>
        /// <param name="newCardNode">Node to add to the scene</param>
        private void DoAnimation(int index, Card newCard, Node newCardNode)
        {
            Application.InvokeOnMain(new Action(() => SceneManager.hostScene.AddChild(newCardNode)));

            AnimateCardDeal(index, newCard);
        }

        /// <summary>
        /// Gets the first two players in the game and makes them pay a blind
        /// </summary>
        public void ApplyBlinds()
        {
            ServerPlayer smallBlindPlayer = room.players[0];
            ServerPlayer bigBlindPlayer   = room.players[1];
            
            pot.PayIn(smallBlindPlayer.ApplyBlind(pot.smallBlind), pot.smallBlind);
            pot.PayIn(bigBlindPlayer.ApplyBlind(pot.bigBlind), pot.bigBlind);

            smallBlindPlayer.DisplayMessage("Paid Small Blind");
            bigBlindPlayer.DisplayMessage("Paid Big Blind");

            Application.InvokeOnMain(new Action(() =>
            {
                SceneUtils.UpdatePlayerInformation(smallBlindPlayer.name, "Paid Small Blind");
                SceneUtils.UpdatePlayerInformation(bigBlindPlayer.name, "Paid Big Blind");
            }));

            foreach (ServerPlayer player in room.players)
                player.connection.SetHighestBid(pot.stake);
        }
    
        /// <summary>
        /// Animates the dealing of the cards to the table
        /// </summary>
        /// <param name="index">Index of card to animate</param>
        /// <param name="card">Card to animate</param>
        private void AnimateCardDeal(int index, Card card)
        {
            Console.WriteLine(card.ToString());
            card.node.RunActions(new Parallel(new RotateBy(0f, 0, 0, 90), new MoveTo(0.1f, Card.CARD_TABLE_POSITIONS[index])));
            Application.InvokeOnMain(new Action(() => sound.Play(UIManager.cache.GetSound("Sounds/Swish.wav"))));
            //Sound copyright http://www.freesfx.co.uk
        }
        
        /// <summary>
        /// Deals a card to each player
        /// </summary>
        public void DealToPlayers(){
            ServerPlayer currPlayer = null;
            for (int i = 0; i < room.players.Count; i++)
            {
                currPlayer = room.players[i];
                if (!currPlayer.folded)
                {
                    Console.WriteLine("Dealing card to " + currPlayer.name);
                    currPlayer.GiveCard(deck.TakeCard());
                }
            }
        }

        /// <summary>
        /// Checks to see if all players have paid in the correct stake
        /// </summary>
        /// <param name="remainingPlayers">Amount of players that are unfolded</param>
        /// <returns>True/False if the game should progress to the next round</returns>
        private bool NextRoundCheck(int remainingPlayers)
        {
            Console.WriteLine("POT STAKE: " + pot.stake);

            if (room.GetRemainingPlayers() == 1)
                return true;

            foreach (ServerPlayer player in room.players)
                if (!player.folded)
                {
                    Console.WriteLine(player.name + " STAKE: " + player.currentStake);
                    
                    if (player.currentStake != pot.stake)
                        return false;
                }

            if (pot.stake == 0 && remainingPlayers > 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Gets the bets from the players until all players have played in the same amount
        /// </summary>
        public void PlaceBets() {
            ServerPlayer currentPlayer = null;

            do {
                for (int i = 0; i < room.players.Count; i++)
                {
                    if (room.GetRemainingPlayers() > 1)
                    {
                        currentPlayer = room.players[i];
                        currentPlayer.TakeTurn();

                        while (!currentPlayer.hasTakenTurn && !currentPlayer.folded)
                            Thread.Sleep(1000);

                        currentPlayer.hasTakenTurn = false;    
                    }

                    if (NextRoundCheck(room.players.Count - i - 1))
                        break;
                }

            } while(!NextRoundCheck(0));

            pot.ResetStake();

            foreach (ServerPlayer player in room.players)
                player.ResetStake();
            
        }

        /// <summary>
        /// Performs the evaluation of the game to determine a winner
        /// </summary>
        public void Showdown() {
            var winners  = CardRanker.EvaluateGame(this, room.players);
            var winnings = pot.Cashout();
            int winningsPerPlayer = (int)winnings / winners.Count;
            uint remainder = (uint)((int)winnings % winners.Count);

            if (remainder != 0) //If the winnings can't be split, leave the remainder in the pot
                pot.LeaveRemainder(remainder);
            
            foreach (ServerPlayer winner in winners)
            {
                winner.DisplayMessage("You Win!");
                winner.GiveChips((uint)winningsPerPlayer);
            }
        }

        /// <summary>
        /// Sets the Table's room
        /// </summary>
        /// <param name="room">Room to link to the Table</param>
    internal void setRoom(Room room) { this.room = room; }
  }
}
