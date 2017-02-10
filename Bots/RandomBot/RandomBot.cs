using System;
using System.Collections.Generic;
using HoldemPlayerContract;

namespace RandomBot
{
    // this bot will randomly choose between fold, call or minimum raise.
    public class RandomBot : MarshalByRefObject, IHoldemPlayer
    {
        private int _playerNum;
        private Random _rnd;

        public void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
            // This is called once at the start of the game. playerNum is your unique identifer for the game
            _playerNum = playerNum;
            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            seed += _playerNum; // to ensure each instance of a random bot gets a different seed
            _rnd = new Random(seed);
        }

        public string Name
        {
            // return the name of your player
            get
            {
                return "RandomBot";
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
            // this is called at the start of every hand and tells you the current status of all players (e.g. if is alive and stack size and who is dealer)
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
            // receive your hole cards for this hand
        }

        public void SeeAction(EStage stage, int playerNum, EActionType action, int amount)
        {
            // this is called to inform you when any player (including yourself) makes an action (eg puts in blinds, checks, folds, calls, raises, or wins hand)
        }

        public void GetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out EActionType yourAction, out int amount)
        {
            yourAction = EActionType.ActionFold;
            amount = 0;

            // This is the bit where you need to put the AI (mostly likely based on info you receive in other methods)

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
                // Side pots are now implemented so you can go all in and call or raise even if you have less than minimum
                int actionNum = _rnd.Next(100);

                if (actionNum < 20)
                {
                    yourAction = EActionType.ActionFold;
                    amount = 0;
                }
                else if (actionNum < 60)
                {
                    yourAction = EActionType.ActionCall;
                    amount = callAmount;
                }
                else
                {
                    yourAction = EActionType.ActionRaise;
                    amount = minRaise;
                }
            }
        }

        public void SeeBoardCard(EBoardCardType cardType, Card boardCard)
        {
            // this is called to inform you of the board cards (3 flop cards, turn and river)
        }

        public void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand)
        {
            // this is called to inform you of another players hand during the show down. 
            // bestHand is the best hand that they can form with their hole cards and the five board cards
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
        }

    }
}
