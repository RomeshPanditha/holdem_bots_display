using System;
using System.IO;
using System.Collections.Generic;
using HoldemPlayerContract;

namespace ObserverBot
{
    // this bot is not an active player, but simply watches the game,  records stats and writes them to a log file -  playerinfo.txt
    public class ObserverBot : MarshalByRefObject, IHoldemPlayer
    {
        private class PlayerStats
        {
            public int PlayerNum;
            public int NumHandsPlayed;
            public int NumPreFlopsFolded;
            public int NumFlopsFolded;
            public int NumTurnsFolded;
            public int NumRiversFolded;
            public int NumShowdowns;
            public int NumHandsWon;
            public bool bWonThisHand;
            public bool bIsObserver;

            public PlayerStats(int pPlayerNum, bool pbIsObserver)
            {
                PlayerNum = pPlayerNum;
                bIsObserver = pbIsObserver;
                NumHandsPlayed = 0;
                NumPreFlopsFolded = 0;
                NumFlopsFolded = 0;
                NumTurnsFolded = 0;
                NumRiversFolded = 0;
                NumShowdowns = 0;
                NumHandsWon = 0;
                bWonThisHand = false;
            }
        }

        private int _playerNum;
        private Card [] _board;
        private int _handNum = 0;
        TextWriter _tw;
        private PlayerStats [] playerStats;

        public void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
            // This is called once at the start of the game. playerNum is your unique identifer for the game
            _playerNum = playerNum;
            _board = new Card[5];
            _tw = new StreamWriter("playerinfo.txt", false);
        }

        public string Name
        {
            // return the name of your player
            get
            {
                return "ObserverBot";
            }
        }

        public bool IsObserver
        {
            get
            {
                return true;
            }
        }

        public void InitHand(int numPlayers, PlayerInfo[] players)
        {
            // this is called at the start of every hand and tells you the current status of all players (e.g. if is alive and stack size and who is dealer)
            // create a writer and open the file
            _handNum++;

            if(_handNum == 1)
            {
                playerStats = new PlayerStats[numPlayers];

                string sHeader = "HandNum\t";

                foreach (PlayerInfo p in players)
                {
                    playerStats[p.PlayerNum] = new PlayerStats(p.PlayerNum, p.IsObserver);
                    if(!p.IsObserver)
                    {
                        sHeader += p.PlayerNum + "-" + p.Name + "\t";
                    }
                }

                // write a line of text to the file
                _tw.WriteLine(sHeader);
            }

            string sStackSizes = _handNum + "\t";

            foreach (PlayerInfo p in players)
            {
                if(!p.IsObserver)
                {
                    sStackSizes += p.StackSize + "\t";
                }

                if (p.IsAlive)
                {
                    playerStats[p.PlayerNum].NumHandsPlayed++;
                    playerStats[p.PlayerNum].bWonThisHand = false;
                }
            }

            // write a line of text to the file
            _tw.WriteLine(sStackSizes);
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
        }

        public void SeeAction(EStage stage, int playerNum, EActionType action, int amount)
        {
            // this is called to inform you when any player (including yourself) makes an action (eg puts in blinds, checks, folds, calls, raises, or wins hand)
            if (action == EActionType.ActionFold)
            {
                switch (stage)
                {
                    case EStage.StagePreflop:
                        playerStats[playerNum].NumPreFlopsFolded++;
                        break;
                    case EStage.StageFlop:
                        playerStats[playerNum].NumFlopsFolded++;
                        break;
                    case EStage.StageTurn:
                        playerStats[playerNum].NumTurnsFolded++;
                        break;
                    case EStage.StageRiver:
                        playerStats[playerNum].NumRiversFolded++;
                        break;
                }
            }
            else if (action == EActionType.ActionWin)
            {
                // be careful not to double count here. This can be call multiple times for the same player for the same hand if they win more than one side pot
                if(!playerStats[playerNum].bWonThisHand)
                {
                    playerStats[playerNum].NumHandsWon++;
                    playerStats[playerNum].bWonThisHand = true;
                }
            }

        }

        public void GetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out EActionType yourAction, out int amount)
        {
            amount = 0;
            yourAction = EActionType.ActionFold;
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
            _handNum++;

            string sStackSizes = _handNum + "\t";

            foreach (PlayerInfo p in players)
            {
                if(!p.IsObserver)
                {
                    sStackSizes += p.StackSize + "\t";
                }
            }

            // write a line of text to the file
            _tw.WriteLine(sStackSizes);

            _tw.WriteLine("");

            _tw.WriteLine("PlayerNum\tNumHandsPlayed\tNumPreFlopsFolded\tNumFlopsFolded\tNumTurnsFolded\tNumRiversFolded\tNumShowdowns\tNumHandsWon");

            foreach (PlayerStats p in playerStats)
            {
                if(!p.bIsObserver)
                {
                    p.NumShowdowns = p.NumHandsPlayed - (p.NumPreFlopsFolded + p.NumFlopsFolded + p.NumTurnsFolded + p.NumRiversFolded);

                    _tw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                            p.PlayerNum,
                            p.NumHandsPlayed,
                            p.NumPreFlopsFolded,
                            p.NumFlopsFolded,
                            p.NumTurnsFolded,
                            p.NumRiversFolded,
                            p.NumShowdowns,
                            p.NumHandsWon
                        );
                }
            }

            // close the stream
            _tw.Close();
        }
    }
}
