﻿using MixedRealityPoker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixedRealityPoker.Game.PokerObjects
{
    class Deck : Initialisable
    {
        private Card[] cards = new Card[52];

        public void init()
        {

        }

        public void loadTextures() //Provide texture loading from the Asset/Resource manager of each system (iOS/Android)
        {

        }

        public void shuffle()
        {

        }
    }
}
