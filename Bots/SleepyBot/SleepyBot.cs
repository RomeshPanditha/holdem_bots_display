using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HoldemPlayerContract;

namespace SleepyBot
{
    // this bot will sleep for X seconds before calling. This is used to test that the controller can timeout bots if they are taking too long
    // to respond, rather than having one bot blocking the game for an indefinite amount of time
    public class SleepyBot : MarshalByRefObject, IHoldemPlayer
    {
        private int _sleepMilliSeconds = 5000;

        public void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
            // This is called once at the start of the game. playerNum is your unique identifer for the game
//            System.Threading.Thread.Sleep(_sleepMilliSeconds);
            if (playerConfigSettings.ContainsKey("sleepMilliSeconds"))
            {
                _sleepMilliSeconds = Convert.ToInt32(playerConfigSettings["sleepMilliSeconds"]);
            }
        }

        public string Name
        {
            // return the name of your player
            get
            {
//                System.Threading.Thread.Sleep(_sleepMilliSeconds);
                return "SleepyBot";
            }
        }

        public bool IsObserver
        {
            get
            {
//                System.Threading.Thread.Sleep(_sleepMilliSeconds);
                return false;
            }
        }

        public void InitHand(int numPlayers, PlayerInfo[] players)
        {
            // this is called at the start of every hand and tells you the current status of all players (e.g. if is alive and stack size and who is dealer)
            System.Threading.Thread.Sleep(_sleepMilliSeconds);
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
            // receive your hole cards for this hand
            System.Threading.Thread.Sleep(_sleepMilliSeconds);
        }

        public void SeeAction(EStage stage, int playerNum, EActionType action, int amount)
        {
            // this is called to inform you when any player (including yourself) makes an action (eg puts in blinds, checks, folds, calls, raises, or wins hand)
            System.Threading.Thread.Sleep(_sleepMilliSeconds);
        }

        public void GetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out EActionType yourAction, out int amount)
        {
            // This is the bit where you need to put the AI (mostly likely based on info you receive in other methods)

            System.Threading.Thread.Sleep(_sleepMilliSeconds);

            yourAction = EActionType.ActionCall;
            amount = callAmount;
        }

        public void SeeBoardCard(EBoardCardType cardType, Card boardCard)
        {
            // this is called to inform you of the board cards (3 flop cards, turn and river)
            System.Threading.Thread.Sleep(_sleepMilliSeconds);
        }

        public void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand)
        {
            // this is called to inform you of another players hand during the show down. 
            // bestHand is the best hand that they can form with their hole cards and the five board cards
            System.Threading.Thread.Sleep(_sleepMilliSeconds);
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
            System.Threading.Thread.Sleep(_sleepMilliSeconds);
        }
    }
}
