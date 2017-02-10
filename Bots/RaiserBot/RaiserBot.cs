using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HoldemPlayerContract;

namespace RaiserBot
{
    // this bot will always bet the minimum raise at every opportunity
    public class RaiserBot : MarshalByRefObject, IHoldemPlayer
    {
        public void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
        }

        public string Name
        {
            get
            {
                return "RaiserBot";
            }
        }

        public bool IsObserver
        {
            get
            {
                return false;
            }
        }

        public void InitHand(int numPlayers, PlayerInfo[] players)
        {
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
        }

        public void SeeAction(EStage stage, int playerNum, EActionType action, int amount)
        {
        }

        public void GetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out EActionType yourAction, out int amount)
        {
            yourAction = EActionType.ActionRaise;
            amount = minRaise;
        }


        public void SeeBoardCard(EBoardCardType cardType, Card boardCard)
        {
        }

        public void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand)
        {
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
        }

    }
}
