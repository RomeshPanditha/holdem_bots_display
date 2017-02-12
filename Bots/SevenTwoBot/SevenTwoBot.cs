using System.Collections.Generic;
using System.Linq;
using HoldemPlayerContract;

namespace SevenTwoBot
{
    public class SevenTwoBot : IHoldemPlayer
    {
        private Card[] _currentHand = new Card[2];
        public void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
        }


        public string Name { get { return "Seven Two Bot"; } }

        public bool IsObserver { get { return false; } } 
        public void InitHand(int numPlayers, PlayerInfo[] players)
        {
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
            _currentHand[0] = hole1;
            _currentHand[1] = hole2;
        }

        public void SeeAction(EStage stage, int playerNum, EActionType action, int amount)
        {
        }

        public void GetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize,
            out EActionType yourAction, out int amount)
        {
            if (stage == EStage.StagePreflop)
            {
                if (_currentHand.Any(c => c.Rank == ERankType.RankSeven) && _currentHand.Any(c => c.Rank == ERankType.RankTwo) &&
                    _currentHand[0].Suit != _currentHand[1].Suit)
                {
                    yourAction = EActionType.ActionRaise;
                    amount = maxRaise;
                }
            }
            yourAction = EActionType.ActionFold;
            amount = 0;
        }

        public void SeeBoardCard(EBoardCardType cardType, Card boardCard)
        {
        }

        public void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand)
        {
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
            _currentHand = new Card[2];
        }
    }
}
