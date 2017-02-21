﻿using System;

namespace HoldemHotshots
{

    class CommandManager
    {
        private static CommandManager commandManager;
        private ServerConnection connection;
        private ClientPlayer player;

        private CommandManager(ServerConnection connection, ClientPlayer player)
        {
            this.connection = connection;
            this.player = player;
        }

        public static CommandManager getInstance(ServerConnection connection,ClientPlayer player)
        {
            if (commandManager == null)
            {
                commandManager = new CommandManager(connection,player);
            }

            return commandManager;
        }

        public void runCommand(String command)
        {
            String[] args = command.Split();

            Console.WriteLine("Client '" + command + "' received command '" + command + "'");

            switch (args[0])
            {
                case "MAX_PLAYERS_ERROR":
                    //TODO: Call Max Players Method
                    break;
                case "GET_PLAYER_NAME":
                    sendPlayerName();
                    break;
                case "ANIMATE_CARD":
                    animateCard();
                    break;
                case "GIVE_CARD":
                    if (args.Length == 3) giveCard(int.Parse(args[1]), int.Parse(args[2]));
                    else Console.WriteLine("Insufficient arguments for command 'Raise'");
                    break;
                case "TAKE_TURN":
                    takeTurn();
                    break;
                case "SEND_BUY_IN":
                    if(args.Length == 2) sendBuyIn();
                    break;
                case "PLAYER_KICKED":
                    playerKicked();
                    break;
                case "CURRENT_STATE":
                    sentCurrentState();
                    break;
                case "GIVE_CHIPS":
                    if(args.Length == 2) giveChips(uint.Parse(args[1]));
                    break;
                case "START_GAME":
                    startGame();
                    break;
                case "RETURN_TO_LOBBY":
                    returnToLobby();
                    break;
                case "TAKE_CHIPS":
                    if (args.Length == 2) takeChips(uint.Parse(args[1]));
                    break;
                case "PING":
                    Pong();
                    break;
                default:
                    Console.WriteLine("Client recieved a message from server that was not found");
                    break;

            }


        }

        private void Ping()
        {
            connection.sendMessage("PING");
        }

        private void Pong()
        {
            connection.sendMessage("PONG");
        }

        private void sendPlayerName()
        {
            Console.WriteLine("Sending name...");
            connection.sendMessage(UIUtils.GetPlayerName());
            Console.WriteLine("Name sent");
        }

        private void animateCard()
        {
            int cardindex = int.Parse(connection.getResponse());

            //TODO : Fix the issue with animate card (void to int error) 
            player.animateCard(cardindex);
        }

        private void giveCard(int suit, int rank)
        {
            player.giveCard(suit,rank);
        }

        private void takeTurn()
        {
            player.takeTurn();
        }

        private void sendBuyIn()
        {
            //TODO: implement send buyin
            int buyin = int.Parse(connection.getResponse());
            player.setBuyIn(buyin);
        }

        private void playerKicked()
        {
            //TODO: call player.kicked
        }

        private void sentCurrentState()
        {
            string state = connection.getResponse();
            //TODO: pass state to method object that needs it
        }

        private void giveChips(uint amount)
        {
            player.giveChips(amount);
        }

        private void startGame()
        {
           //TODO: call start game method on correct object
        }

        private void returnToLobby()
        {
          //TODO: call return to lobby method on correct object
        }

        private void takeChips(uint amount)
        {
            player.takeChips(amount);

        }

    }
}