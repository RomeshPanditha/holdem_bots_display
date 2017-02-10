using System;
using System.Collections.Generic;
using HoldemPlayerContract;

namespace HoldemController
{

    public class Deck
    {
        private readonly Card[] _cards = new Card[52];
        private int _topCard;

        public Deck()
        {
            int i;

            for (i = 0; i < 52; i++)
            {
                var rank = (ERankType)(i % 13);
                var suit = (ESuitType)(i / 13);

                var card = new Card(rank, suit);

                _cards[i] = card;
            }

            Shuffle();
        }

        public void Shuffle()
        {
            var shuffledDeck = new Card[52];
            var unshuffledList = new List<Card>();
            int i;
            var rnd = new Random();

            _topCard = 0;

            for (i = 0; i < 52; i++)
            {
                unshuffledList.Add(_cards[i]);
            }

            for (i = 0; i < 52; i++)
            {
                var pos = rnd.Next(unshuffledList.Count);
                shuffledDeck[i] = unshuffledList[pos];
                unshuffledList.RemoveAt(pos);
            }

            // Copy shuffled deck back to deck
            for (i = 0; i < 52; i++)
            {
                _cards[i] = shuffledDeck[i];
            }
        }

        public Card DealCard()
        {
            var card = _cards[_topCard];
            _topCard++;
            return card;
        }
    }

}