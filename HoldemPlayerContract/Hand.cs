using System;
using System.Collections.Generic;

namespace HoldemPlayerContract
{
    [Serializable]
    public class Hand
    {
        private readonly Card[] _cards;
        private readonly int[] _rankCount;
        private readonly int[] _suitCount;

        private EHandType _rank;
        private ERankType[] _subRank;
        private int _numSubRanks;

        public EHandType HandRank()
        {
            return _rank;
        }

        public int NumSubRanks()
        {
            return _numSubRanks;
        }

        public ERankType SubRank(int level)
        {
            return _subRank[level];
        }


        public Hand(IReadOnlyList<Card> pCards) 
        {
            if (pCards.Count < 5)
            {
                throw new Exception("not enough cards to form a hand");
            }

            _cards = new Card[5];
            _rankCount = new int[13];
            _suitCount = new int[4];
            _subRank = new ERankType[5];

            for (var i = 0; i < 5; i++)
            {
                _cards[i] = pCards[i];
            }

            _evaluate();

            for (var i = _numSubRanks; i < 5; i++)
            {
                _subRank[i] = ERankType.RankUnknown;
            }
        }

        public string HandValueStr()
        {
            int i;
            var strVal = "";

            for (i = 0; i < 5; i++)
            {
                strVal += _cards[i].ValueStr() + " ";
            }

            return strVal;
        }

        public string HandRankStr()
        {
            string sRankStr;
            switch (_rank)
            {
                case EHandType.HandStraightFlush: sRankStr = "Straight Flush"; break;
                case EHandType.HandFours: sRankStr = "Four of a kind"; break;
                case EHandType.HandFullHouse: sRankStr = "Full House"; break;
                case EHandType.HandFlush: sRankStr = "Flush"; break;
                case EHandType.HandStraight: sRankStr = "Straight"; break;
                case EHandType.HandThrees: sRankStr = "Three of a kind"; break;
                case EHandType.HandTwoPair: sRankStr = "Two pair"; break;
                case EHandType.HandPair: sRankStr = "Pair"; break;
                case EHandType.HandRunt: sRankStr = "Runt"; break;
                default: sRankStr = "???"; break;
            }

            /*
                        int i;
                        for (i = 0; i < _numSubRanks; i++)
                        {
                            sRankStr += " " + _subRank[i];
                        }
            */
            return sRankStr;
        }

        // -1 = hand1 is better
        // 0  = hands are equal
        // 1  = hand2 is better
        public static int CompareHands(Hand pHand1, Hand pHand2)
        {
            if (pHand1.HandRank() > pHand2.HandRank())
            {
                return -1;
            }

            if (pHand2.HandRank() > pHand1.HandRank())
            {
                return 1;
            }

            // Hands are of same rank, compare sub ranks

            //		assert(pHand1.numSubRanks() == pHand2.numSubRanks());

            var numSubRanks = pHand1.NumSubRanks();
            int currSubRank;

            for (currSubRank = 0; currSubRank < numSubRanks; currSubRank++)
            {
                if (pHand1.SubRank(currSubRank) > pHand2.SubRank(currSubRank))
                {
                    return -1;
                }

                if (pHand2.SubRank(currSubRank) > pHand1.SubRank(currSubRank))
                {
                    return 1;
                }
            }

            return 0;
        }

        // -1 = otherHand is better
        // 0  = hands are equal
        // 1  = this hand is better
        public int Compare(Hand otherHand)
        {
            return CompareHands(otherHand, this);
        }


        // Given two pocket cards and five board cards,
        // determine the best poker hand that can be formed using any 5 of the 7 cards
        //public static Hand FindPlayersBestHand(Card[] pocketCards, Card[] board)
        //{
        //    var cards = new Card[7];

        //    // Put all cards together
        //    cards[0] = pocketCards[0];
        //    cards[1] = pocketCards[1];
        //    cards[2] = board[0];
        //    cards[3] = board[1];
        //    cards[4] = board[2];
        //    cards[5] = board[3];
        //    cards[6] = board[4];

        //    return FindPlayersBestHand(cards);
        //}

        public static Hand FindPlayersBestHand(IReadOnlyList<Card> pocketCards, IReadOnlyList<Card> board)
        {
//            var cards = new Card[7];
            List<Card> cards = new List<Card>();

            if (pocketCards.Count != 2)
            {
                throw new Exception("must supply exactly 2 pocket cards");
            }

            if (board.Count < 3)
            {
                throw new Exception("not enough board cards");
            }

            if (board.Count > 5)
            {
                throw new Exception("too many board cards");
            }

            // Put all cards together
            cards.Add(pocketCards[0]);
            cards.Add(pocketCards[1]);

            foreach(Card c in board)
            {
                cards.Add(c);
            }

            return FindPlayersBestHand(cards);
        }

        public static Hand FindPlayersBestHand(IReadOnlyList<Card> cards)
        {
            if (cards.Count < 5)
            {
                throw new Exception("not enough cards to find best hand");
            }

            if (cards.Count > 7)
            {
                throw new Exception("too many cards to find best hand");
            }

            var currHandCards = new Card[5];
            var bestHand = new Hand(cards);   // if 5 cards just use them

            if (cards.Count == 6)
            {
                for (var i = 0; i < 6; i++)
                {
                    // exclude cards at indices i, make poker hand
                    // with the other 5 cards
                    var currCard = 0;
                    int k;
                    for (k = 0; k < 6; k++)
                    {
                        if (k != i)
                        {
                            currHandCards[currCard] = cards[k];
                            currCard++;
                        }
                    }

                    var currHand = new Hand(currHandCards);

                    // If this is better than current best rank (and sub ranks)
                    // then make this the new best hand
                    if (CompareHands(currHand, bestHand) == -1)
                    {
                        bestHand = currHand;
                    }
                }
            }
            else if (cards.Count == 7)
            {

                for (var i = 0; i < 7; i++)
                {
                    for (var j = i + 1; j < 7; j++)
                    {
                        // exclude cards at indices i and j, make poker hand
                        // with the other 5 cards
                        var currCard = 0;
                        int k;
                        for (k = 0; k < 7; k++)
                        {
                            if ((k != i) && (k != j))
                            {
                                currHandCards[currCard] = cards[k];
                                currCard++;
                            }
                        }

                        var currHand = new Hand(currHandCards);

                        // If this is better than current best rank (and sub ranks)
                        // then make this the new best hand
                        if (CompareHands(currHand, bestHand) == -1)
                        {
                            bestHand = currHand;
                        }
                    }
                }
            }

            return bestHand;
        }


        // Is this the correct place for this function?
        // This function may need to be customisable to accomodate non-standard hands
        // (e.g. round-the-corner straights, tigers, skeets etc.)
        private void _evaluate()
        {
            int i;

            _sortByRank();
            _calcRankCount();
            _calcSuitCount();

            // Is it a straight flush?
            if (IsStraightFlush(ref _numSubRanks, ref _subRank))
            {
                _rank = EHandType.HandStraightFlush;
                return;
            }

            // fours?
            if (IsFours(ref _numSubRanks, ref _subRank))
            {
                _rank = EHandType.HandFours;
                return;
            }

            // full house?
            if (IsFullHouse(ref _numSubRanks, ref _subRank))
            {
                _rank = EHandType.HandFullHouse;
                return;
            }

            // flush?
            if (IsFlush(ref _numSubRanks, ref _subRank))
            {
                _rank = EHandType.HandFlush;
                return;
            }

            // straight?
            if (IsStraight(ref _numSubRanks, ref _subRank))
            {
                _rank = EHandType.HandStraight;
                return;
            }

            // threes?
            if (IsThrees(ref _numSubRanks, ref _subRank))
            {
                _rank = EHandType.HandThrees;
                return;
            }

            // two pair?
            if (IsTwoPair(ref _numSubRanks, ref _subRank))
            {
                _rank = EHandType.HandTwoPair;
                return;
            }

            // one pair?
            if (IsPair(ref _numSubRanks, ref _subRank))
            {
                _rank = EHandType.HandPair;
                return;
            }

            _rank = EHandType.HandRunt;
            _numSubRanks = 5;

            // This works because the hand is already sorted in descending order
            for (i = 0; i < 5; i++)
            {
                _subRank[i] = _cards[i].Rank;
            }
        }


        private bool
        IsStraightFlush(ref int numSubRanks, ref ERankType[] subRank)
        {
            // These are required parameters, but can be ignored
            var flushNumSubRanks = 0;
            var flushSubRank = new ERankType[5];

            return IsStraight(ref numSubRanks, ref subRank) && IsFlush(ref flushNumSubRanks, ref flushSubRank);
        }


        private bool IsFours(ref int numSubRanks, ref ERankType[] subRank)
        {
            int currRank;

            numSubRanks = 2;

            for (currRank = 0; currRank < 13; currRank++)
            {
                if (_rankCount[currRank] == 4)
                {
                    subRank[0] = (ERankType)Enum.ToObject(typeof(ERankType), currRank);

                    int currCard;
                    for (currCard = 0; currCard < 5; currCard++)
                    {
                        if (_cards[currCard].Rank != subRank[0])
                        {
                            subRank[1] = _cards[currCard].Rank;
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        private bool
        IsFullHouse(ref int numSubRanks, ref ERankType[] subRank)
        {
            var threesNumSubRanks = 0;
            var threesSubRank = new ERankType[5];

            var pairNumSubRanks = 0;
            var pairSubRank = new ERankType[5];


            if (IsThrees(ref threesNumSubRanks, ref threesSubRank) &&
                IsPair(ref pairNumSubRanks, ref pairSubRank))
            {
                numSubRanks = 2;
                subRank[0] = threesSubRank[0];
                subRank[1] = pairSubRank[0];

                return true;
            }

            return false;
        }


        private bool IsFlush(ref int numSubRanks, ref ERankType[] subRank)
        {
            int i;

            numSubRanks = 5;

            for (i = 0; i < 4; i++)
            {
                if ((_suitCount[i] != 0) &&
                    (_suitCount[i] != 5))
                {
                    return false;
                }
            }

            for (i = 0; i < 5; i++)
            {
                subRank[i] = _cards[i].Rank;
            }

            return true;
        }


        private bool IsStraight(ref int numSubRanks, ref ERankType[] subRank)
        {
            int i;
            var lowRank = ERankType.RankUnknown;
            var highRank = ERankType.RankUnknown;

            numSubRanks = 1;

            // if more than one card of the same rank
            // then it is not a straight
            for (i = 0; i < 13; i++)
            {
                if (_rankCount[i] > 1)
                {
                    return false;
                }

                // Determine highest and lowest ranked cards in hand here
                if (_rankCount[i] == 1)
                {
                    highRank = (ERankType)Enum.ToObject(typeof(ERankType), i);

                    if (lowRank == ERankType.RankUnknown)
                    {
                        lowRank = (ERankType)Enum.ToObject(typeof(ERankType), i);
                    }
                }
            }

            subRank[0] = highRank;

            // If the highest and lowest rank cards are within a spread of 5 cards, 
            // and there are no pairs, then it must be straight
            if (highRank - lowRank == 4)
            {
                return true;
            }

            // Check for Ace low straight here (special case as my code usually treats Ace as high)
            if ((lowRank == ERankType.RankTwo) && (highRank == ERankType.RankAce))
            {
                // Got Ace and Two

                // Check for any 6's -> K's. (If any then no straight)
                for (i = (int)ERankType.RankSix; i <= (int)ERankType.RankKing; i++)
                {
                    if (_rankCount[i] > 0)
                    {
                        return false;
                    }
                }

                // This is a 5 high straight
                subRank[0] = ERankType.RankFive;

                return true;
            }

            return false;
        }


        private bool IsThrees(ref int numSubRanks, ref ERankType[] subRank)
        {
            ERankType currRank;

            numSubRanks = 3;

            for (currRank = 0; (int)currRank < 13; currRank++)
            {
                if (_rankCount[(int)currRank] == 3)
                {
                    subRank[0] = currRank;

                    var currSubRank = 1;

                    int currCard;
                    for (currCard = 0; currCard < 5; currCard++)
                    {
                        if (_cards[currCard].Rank != subRank[0])
                        {
                            subRank[currSubRank] = _cards[currCard].Rank;
                            currSubRank++;
                        }
                    }
                    return true;
                }
            }

            return false;
        }


        private bool IsTwoPair(ref int numSubRanks, ref ERankType[] subRank)
        {
            ERankType currRank;
            var highPairRank = ERankType.RankUnknown;
            var lowPairRank = ERankType.RankUnknown;
            var oddCardRank = ERankType.RankUnknown;
            var pairCount = 0;

            numSubRanks = 3;

            for (currRank = 0; (int)currRank < 13; currRank++)
            {
                if (_rankCount[(int)currRank] == 2)
                {
                    pairCount++;

                    if (highPairRank != ERankType.RankUnknown)
                    {
                        lowPairRank = highPairRank;
                    }

                    highPairRank = currRank;

                }
                else if (_rankCount[(int)currRank] == 1)
                {
                    oddCardRank = currRank;
                }
            }

            if (pairCount == 2)
            {
                subRank[0] = highPairRank;
                subRank[1] = lowPairRank;
                subRank[2] = oddCardRank;

                return true;
            }

            return false;
        }


        private bool IsPair(ref int numSubRanks, ref ERankType[] subRank)
        {
            ERankType currRank;

            numSubRanks = 4;

            for (currRank = 0; (int)currRank < 13; currRank++)
            {
                if (_rankCount[(int)currRank] == 2)
                {
                    subRank[0] = currRank;

                    var currSubRank = 1;

                    int currCard;
                    for (currCard = 0; currCard < 5; currCard++)
                    {
                        if (_cards[currCard].Rank != subRank[0])
                        {
                            subRank[currSubRank] = _cards[currCard].Rank;
                            currSubRank++;
                        }
                    }
                    return true;
                }
            }

            return false;
        }



        // Uses an insertion sort
        private void _sortByRank()
        {
            var sortedCards = new Card[5];
            int i, j;

            sortedCards[0] = _cards[0];

            for (j = 1; j < 5; j++)
            {
                var key = _cards[j];

                i = j - 1;

                while ((i >= 0) && (sortedCards[i].Rank < key.Rank))
                {
                    sortedCards[i + 1] = sortedCards[i];
                    i--;
                }

                sortedCards[i + 1] = key;
            }

            for (i = 0; i < 5; i++)
            {
                _cards[i] = sortedCards[i];
            }
        }


        private void _calcRankCount()
        {
            int i;

            // reset all values
            for (i = 0; i < 13; i++)
            {
                _rankCount[i] = 0;
            }

            // count how many of each rank
            for (i = 0; i < 5; i++)
            {
                _rankCount[(int)_cards[i].Rank]++;
            }
        }


        private void _calcSuitCount()
        {
            int i;

            // reset all values
            for (i = 0; i < 4; i++)
            {
                _suitCount[i] = 0;
            }

            // count how many of each suit
            for (i = 0; i < 5; i++)
            {
                _suitCount[(int)_cards[i].Suit]++;
            }
        }
    }
}
