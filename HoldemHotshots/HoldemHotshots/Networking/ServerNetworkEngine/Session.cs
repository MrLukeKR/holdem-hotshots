﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace HoldemHotshots
{
    class Session
    {
        private static Session networkEngine;
        private static List<ListenerThread> listenerThreads = new List<ListenerThread>();
        public static Room Lobby;

        private Session()
        {
            //Leave blank for singleton design pattern
        }

        public static Session getinstance()
        {
            Console.WriteLine("Getting Instance...");
            if(networkEngine == null)
            {
                Console.WriteLine("Creating Instance...");
                networkEngine = new Session();
            }
            Console.WriteLine("Created instance!");
            return networkEngine;
        }
        public void init()
        {
            Console.WriteLine("Creating New Room...");
            Lobby = new Room();
            Console.WriteLine("Created Room!");
            Console.WriteLine("Creating Listener thread...");
            ListenerThread listener = new ListenerThread();
            Console.WriteLine("Created Listener thread...");
            Console.WriteLine("Starting new Thread...");

            listenerThreads.Add(listener);

            Thread listenThread = new Thread(listener.Start);
            listenThread.Start();

            Console.WriteLine("Thread Started...");
            
            /*
            ServerCommandListenerThread commandListener = new ServerCommandListenerThread(connection,player);
            Thread commandListenerThread = new Thread(commandListener.Start);
            */
        }

        public static void DisposeOfSockets()
        {
            foreach(ListenerThread lThread in listenerThreads) lThread.ShutdownSocket();
            listenerThreads.Clear();
        }

        internal Room getRoom()
        {
            return Lobby;
        }
    }
}