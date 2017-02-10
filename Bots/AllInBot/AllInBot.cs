using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HoldemPlayerContract;

namespace AllInBot
{
    // this bot always goes allin at every opportunity.
    // This is especially useful when testing side pots are working correctly. i.e. use multiple allin bots with different starting stack sizes
    public class AllInBot : MarshalByRefObject, IHoldemPlayer
    {
        public void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
        }

        public string Name
        {
            get
            {
                return "AllInBot";
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
            amount = maxRaise;
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
