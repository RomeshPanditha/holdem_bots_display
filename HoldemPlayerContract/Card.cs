using System;
// using System.Collections.Generic;

namespace HoldemPlayerContract
{
    [Serializable]
    public class Card : IEquatable<Card>
    {
        public Card(ERankType pRank, ESuitType pSuit)
        {
            Rank = pRank;
            Suit = pSuit;
        }
        public readonly ESuitType Suit; 
        public readonly ERankType Rank;

        public string SuitStr()
        {
            return SuitToString(Suit);
        }

        public static string SuitToString(ESuitType Suit)
        {
            switch (Suit)
            {
                case ESuitType.SuitClubs:
                    return "C";
                case ESuitType.SuitHearts:
                    return "H";
                case ESuitType.SuitSpades:
                    return "S";
                case ESuitType.SuitDiamonds:
                    return "D";
                default:
                    return "?";
            }
        }

        public string RankStr()
        {
            return RankToString(Rank);
        }

        public static string RankToString(ERankType Rank)
        {
            switch (Rank)
            {
                case ERankType.RankAce:
                    return "A";
                case ERankType.RankKing:
                    return "K";
                case ERankType.RankQueen:
                    return "Q";
                case ERankType.RankJack:
                    return "J";
                case ERankType.RankTen:
                    return "T";
                case ERankType.RankNine:
                    return "9";
                case ERankType.RankEight:
                    return "8";
                case ERankType.RankSeven:
                    return "7";
                case ERankType.RankSix:
                    return "6";
                case ERankType.RankFive:
                    return "5";
                case ERankType.RankFour:
                    return "4";
                case ERankType.RankThree:
                    return "3";
                case ERankType.RankTwo:
                    return "2";
                default:
                    return "?";
            }
        }

        public string ValueStr()
        {
	        return RankStr() + SuitStr();
        }

        public override string ToString()
        {
            return ValueStr();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Card objAsCard = obj as Card;
            if (objAsCard == null) return false;
            else return Equals(objAsCard);
        }
        public override int GetHashCode()
        {
            return ((int)Suit * 13) + (int)Rank;
        }
        public bool Equals(Card other)
        {
            if (other == null) return false;
            return (this.Suit == other.Suit && this.Rank == other.Rank);
        }
        // Should also override == and != operators.

    }
}

