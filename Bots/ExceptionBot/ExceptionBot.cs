using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HoldemPlayerContract;

namespace ExceptionBot
{
    // this bot always throws a divide by zero exception when any method is called.
    // this is used to test the exception handling in the controller and make sure that if a bot throws an exception it will
    // not crash the game
    public class ExceptionBot : MarshalByRefObject, IHoldemPlayer
    {
        private int zero = 0;

        public void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
            // This is called once at the start of the game. playerNum is your unique identifer for the game
            int i = 1 / zero;
        }

        public string Name
        {
            // return the name of your player
            get
            {
                int i = 1 / zero;
                return "ExceptionBot";
            }
        }

        public bool IsObserver
        {
            get
            {
                int i = 1 / zero;
                return false;
            }
        }

        public void InitHand(int numPlayers, PlayerInfo[] players)
        {
            // this is called at the start of every hand and tells you the current status of all players (e.g. if is alive and stack size and who is dealer)
            int i = 1 / zero;
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
            // receive your hole cards for this hand
            int i = 1 / zero;
        }

        public void SeeAction(EStage stage, int playerNum, EActionType action, int amount)
        {
            // this is called to inform you when any player (including yourself) makes an action (eg puts in blinds, checks, folds, calls, raises, or wins hand)
            int i = 1 / zero;
        }

        public void GetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out EActionType yourAction, out int amount)
        {
            // This is the bit where you need to put the AI (mostly likely based on info you receive in other methods)
            int i = 1 / zero;

            if (stage == EStage.StageShowdown)
            {
                // if stage is the showdown then choose whether to show your hand or fold
                yourAction = EActionType.ActionShow;
                amount = 0;
            }
            else
            {
                // stage is preflop, flop, turn or river
                // choose whether to fold, check, call or raise
                // the controller will validate your action and try to honour your action if possible but may change it (e.g. it won't let you fold if checking is possible)
                // amount only matters if you are raising (if calling the controller will use the correct amount). 
                // If raising, minRaise and maxRaise are the total amount required to put into the pot (i.e. it includes the call amount)
                // Side pots aren't implemented so if you run out of money you can still call (but not raise) and your stack size may go negative. 
                // If your stack size is still 0 or negative at the end of the hand then you are out of the game.
                yourAction = EActionType.ActionCall;
                amount = callAmount;
            }
        }

        public void SeeBoardCard(EBoardCardType cardType, Card boardCard)
        {
            // this is called to inform you of the board cards (3 flop cards, turn and river)
            int i = 1 / zero;
        }

        public void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand)
        {
            // this is called to inform you of another players hand during the show down. 
            // bestHand is the best hand that they can form with their hole cards and the five board cards
            int i = 1 / zero;
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
            int i = 1 / zero;
        }
    }
}
