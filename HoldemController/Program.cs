using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using HoldemPlayerContract;

namespace HoldemController
{
    class Program
    {
        private DisplayManager _display;

        private ServerHoldemPlayer[] _players;
        private readonly Deck _deck = new Deck();
        private readonly PotManager _potMan = new PotManager();
        private int _totalMoneyInGame;

        private int _dealerPlayerNum;
        private int _littleBlindPlayerNum;
        private int _bigBlindPlayerNum;

        // Game config - default values
        private int _numPlayers;
        private int _littleBlindSize = 100;
        private int _bigBlindSize = 200;
        private int _startingStack = 5000;
        private int _maxNumRaisesPerBettingRound = 4; 
        private int _maxHands = -1;
        private int _doubleBlindFrequency = -1;
        private int _botTimeOutMilliSeconds = 5000;
        
        private int _width = (int) (Console.LargestWindowWidth * .7);
        private int _height = (int) (Console.LargestWindowHeight * .7);

        static void Main()
        {
            try
            {
                var prog = new Program();
                prog.PlayGame();
            }
            catch (Exception e)
            {
                var sExceptionMessage = "EXCEPTION : " + e.Message + "\nPlease send gamelog.txt to gmcdonald73@gmail.com";
                Logger.Log(sExceptionMessage);
            }

            Logger.Close();

            Console.WriteLine("-- press any key to exit --");
            Console.ReadKey();
        }

        public void PlayGame()
        {
            LoadConfig();

            var bDone = false;
            var handNum = 0;

            _totalMoneyInGame = _players.Sum(p => p.StackSize);
            _dealerPlayerNum = 0;
            _littleBlindPlayerNum = GetNextActivePlayer(_dealerPlayerNum);
            _bigBlindPlayerNum = GetNextActivePlayer(_littleBlindPlayerNum);


            _display = new DisplayManager(_width, _height, _players);
            _display.DrawTable();

            while (!bDone)
            {
                var board = new Card[5];
                int lastToAct;
                
                // init round for each player
                handNum++;
                InitHand(handNum);

                // deal out hole cards to all active players
                DealHoleCards();

                // First betting round - get player actions and broadcast to all players until betting round done
                DoBettingRound(EStage.StagePreflop, out lastToAct);

                if (GetNumActivePlayers() > 1)
                {
                    // deal flop
                    board[0] = _deck.DealCard();
                    BroadcastBoardCard(EBoardCardType.BoardFlop1, board[0]);

                    board[1] = _deck.DealCard();
                    BroadcastBoardCard(EBoardCardType.BoardFlop2, board[1]);

                    board[2] = _deck.DealCard();
                    BroadcastBoardCard(EBoardCardType.BoardFlop3, board[2]);

                    // Second betting round - get player actions and broadcast to all players until betting round done
                    if (IsBettingRoundRequired())
                    {
                        DoBettingRound(EStage.StageFlop, out lastToAct);
                    }
                }

                if (GetNumActivePlayers() > 1)
                {
                    // deal turn
                    board[3] = _deck.DealCard();
                    BroadcastBoardCard(EBoardCardType.BoardTurn, board[3]);

                    // Third betting round - get player actions and broadcast to all players until betting round done
                    if (IsBettingRoundRequired())
                    {
                        DoBettingRound(EStage.StageTurn, out lastToAct);
                    }
                }

                if (GetNumActivePlayers() > 1)
                {
                    // deal river
                    board[4] = _deck.DealCard();
                    BroadcastBoardCard(EBoardCardType.BoardRiver, board[4]);

                    // Fourth betting round - get player actions and broadcast to all players until betting round done
                    if (IsBettingRoundRequired())
                    {
                        DoBettingRound(EStage.StageRiver, out lastToAct);
                    }
                }

                ViewCash();

                var handRanker = new HandRanker();

                if (GetNumActivePlayers() > 1)
                {
                    Showdown(board, ref handRanker, lastToAct);
                    handRanker.ViewHandRanks();
                }

                if (GetNumActivePlayers() > 1)
                {
                    // More than one player has shown cards at showdown. Work out how to allocate the pot(s)
                    DistributeWinnings(handRanker);
                }
                else
                {
                    // all players except 1 have folded. Just give entire pot to last man standing
                    var winningPlayer = _players.First(p => p.IsActive).PlayerNum;

                    _players[winningPlayer].StackSize += _potMan.Size();
                    BroadcastAction(EStage.StageShowdown, winningPlayer, EActionType.ActionWin, _potMan.Size());
                    _potMan.EmptyPot();
                }

                // check that money hasn't disappeared or magically appeared
                ReconcileCash();

                // Kill off broke players & check if only one player left
                KillBrokePlayers();

                if (GetNumLivePlayers() == 1)
                {
                    bDone = true;
                }
                else if (_maxHands > 0 && handNum >= _maxHands)
                {
                    bDone = true;
                }
                else
                {
                    // Move to next dealer 
                    MoveDealerAndBlinds();
                }

/*
                ConsoleKeyInfo cki;
                cki= System.Console.ReadKey();
                bDone = (cki.Key == ConsoleKey.Escape);
*/
            }

            EndOfGame();
        }

//        private static void WriteToSpace(int x, int y, string text, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
//        {
//            if (color != null)
//            {
//                Console.ForegroundColor = color.Value;
//            }
//            if (backgroundColor != null)
//            {
//                Console.BackgroundColor = backgroundColor.Value;
//            }
//            var items = text.Replace(Environment.NewLine, "`").Split('`');
//            foreach (var item in items)
//            {
//                Console.SetCursorPosition(x, y);
//                Console.Write(item);
//                y++;
//            }
//            // this will reset both even if only one is probided, so probably best to store the original value, and reset it afterward
//            if (color != null || backgroundColor != null) 
//            {
//                Console.ResetColor();
//            }
//            Console.SetCursorPosition(0, 0);
//        }

        private void LoadConfig()
        {
            var doc = XDocument.Load("HoldemConfig.xml");

            Logger.Log("--- *** CONFIG *** ---");
            Logger.Log(doc.ToString());

            var holdemConfig = doc.Element("HoldemConfig");

            if(holdemConfig == null)
            {
                throw new Exception("Unable to find HoldemConfig element in HoldemConfig.xml");
            }

            var gameRules = holdemConfig.Element("GameRules");

            if (gameRules == null)
            {
                throw new Exception("Unable to find GameRules element in HoldemConfig.xml");
            }

            // Get game rules

            Dictionary<string, string> gameConfigSettings = new Dictionary<string, string>();
            // add defaults to dictionary
            gameConfigSettings.Add("littleBlind", _littleBlindSize.ToString());
            gameConfigSettings.Add("bigBlind", _bigBlindSize.ToString());
            gameConfigSettings.Add("startingStack", _startingStack.ToString());
            gameConfigSettings.Add("maxNumRaisesPerBettingRound", _maxNumRaisesPerBettingRound.ToString());
            gameConfigSettings.Add("maxHands", _maxHands.ToString());
            gameConfigSettings.Add("doubleBlindFrequency", _doubleBlindFrequency.ToString());
            gameConfigSettings.Add("botTimeOutMilliSeconds", _botTimeOutMilliSeconds.ToString());

            // add or update values in dictionary from values in xml
            foreach(XAttribute attr in gameRules.Attributes())
            {
                if (gameConfigSettings.ContainsKey(attr.Name.ToString()))
                {
                    gameConfigSettings[attr.Name.ToString()] = attr.Value;
                }
                else
                {
                    gameConfigSettings.Add(attr.Name.ToString(), attr.Value);
                }
            }

            // read values from dictionary
            _littleBlindSize = Convert.ToInt32(gameConfigSettings["littleBlind"]);
            _bigBlindSize = Convert.ToInt32(gameConfigSettings["bigBlind"]);
            _startingStack = Convert.ToInt32(gameConfigSettings["startingStack"]);
            _maxNumRaisesPerBettingRound = Convert.ToInt32(gameConfigSettings["maxNumRaisesPerBettingRound"]);
            _maxHands = Convert.ToInt32(gameConfigSettings["maxHands"]);
            _doubleBlindFrequency = Convert.ToInt32(gameConfigSettings["doubleBlindFrequency"]);
            _botTimeOutMilliSeconds = Convert.ToInt32(gameConfigSettings["botTimeOutMilliSeconds"]);

            // Create players
            var xplayers = doc.Descendants("Player");
            int i = 0;
            int numLivePlayers = 0;

            _numPlayers = xplayers.Count();

            if(_numPlayers == 0)
            {
                throw new Exception("No Player elements found in HoldemConfig.xml");
            }
            _players = new ServerHoldemPlayer[_numPlayers];


            foreach (var player in xplayers)
            {
                // copy game config settings to player config
                Dictionary<string, string> playerConfigSettings = new Dictionary<string, string>(gameConfigSettings);

                // read player attributes, add to player config or override game settings
                foreach (XAttribute attr in player.Attributes())
                {
                    if(playerConfigSettings.ContainsKey(attr.Name.ToString()))
                    {
                        playerConfigSettings[attr.Name.ToString()] = attr.Value;
                    }
                    else
                    {
                        playerConfigSettings.Add(attr.Name.ToString(), attr.Value);
                    }
                }

                _players[i] = new ServerHoldemPlayer(i, playerConfigSettings);

                if(_players[i].IsAlive)
                {
                    numLivePlayers++;
                }

                i++;
            }

            if(numLivePlayers < 2 || numLivePlayers > 23)
            {
                throw new Exception(String.Format("The number of live (non observer) players found is {0}. It must be between 2 and 23", numLivePlayers));
            }
        }

        private void InitHand(int handNum)
        {

            // Double the blinds if required. Do this here because later on we may want to include this in info to players
            if (_doubleBlindFrequency > 0 && handNum % _doubleBlindFrequency == 0)
            {
                _littleBlindSize *= 2;
                _bigBlindSize *= 2;
            }
            
            PlayerInfo[] playerInfo = new PlayerInfo[_numPlayers];

            Logger.Log("");
            Logger.Log("---------*** HAND {0} ***----------", handNum);
            Logger.Log("Num\tName\tIsAlive\tStackSize\tIsDealer");

            for (var i = 0; i < _players.Length; i++)
            {
                Logger.Log("{0}\t{1}\t{2}\t{3}\t{4}", i, _players[i].Name, _players[i].IsAlive, _players[i].StackSize, i == _dealerPlayerNum);
                var pInfo = new PlayerInfo(i, _players[i].Name.PadRight(20), _players[i].IsAlive, _players[i].StackSize, i == _dealerPlayerNum, _players[i].IsObserver);
                playerInfo[i] = pInfo;
                if (_players[i].IsActive)
                {
                    _display.UpdatePlayer(_players[i]);
                }
            }

            Logger.Log("---------------");

            // broadcast player info to all players
            foreach (var player in _players)
            {
                player.InitHand(_players.Length, playerInfo);
            }

            // shuffle deck
            _deck.Shuffle();
        }

        private void EndOfGame()
        {
            var playerInfo = new PlayerInfo[_numPlayers];

            Logger.Log("");
            Logger.Log("---------*** GAME OVER ***----------");
            Logger.Log("Num\tName\tIsAlive\tStackSize\tIsDealer");

            for (var i = 0; i < _players.Length; i++)
            {
                Logger.Log("{0}\t{1}\t{2}\t{3}\t{4}", i, _players[i].Name, _players[i].IsAlive, _players[i].StackSize, i == _dealerPlayerNum);
                var pInfo = new PlayerInfo(i, _players[i].Name.PadRight(20), _players[i].IsAlive, _players[i].StackSize, i == _dealerPlayerNum, _players[i].IsObserver);
                playerInfo[i] = pInfo;
            }

            Logger.Log("---------------");

            // broadcast player info to all players
            foreach (var player in _players)
            {
                player.EndOfGame(_players.Length, playerInfo);
            }
        }

        private void TakeBlinds()
        {
            // take blinds - inform all players of blinds
            int bigBlindBet;

            // little blind might not be live (if big blind was eliminated last hand)
            if (_players[_littleBlindPlayerNum].IsAlive)
            {
                // check player has enough chips - if not go all in
                var littleBlindBet = _littleBlindSize > _players[_littleBlindPlayerNum].StackSize
                                         ? _players[_littleBlindPlayerNum].StackSize
                                         : _littleBlindSize;

                TransferMoneyToPot(_littleBlindPlayerNum, littleBlindBet);
                BroadcastAction(EStage.StagePreflop, _littleBlindPlayerNum, EActionType.ActionBlind, littleBlindBet);
            }

            // check player has enough chips - if not go all in
            bigBlindBet = _bigBlindSize > _players[_bigBlindPlayerNum].StackSize
                              ? _players[_bigBlindPlayerNum].StackSize
                              : _bigBlindSize;

            TransferMoneyToPot(_bigBlindPlayerNum, bigBlindBet);
            BroadcastAction(EStage.StagePreflop, _bigBlindPlayerNum, EActionType.ActionBlind, bigBlindBet);
        }

        private void DealHoleCards()
        {
            foreach (var player in _players.Where(p => p.IsActive))
            {
                var hole1 = _deck.DealCard();
                var hole2 = _deck.DealCard();
                Logger.Log("Player {0} hole cards {1} {2}", player.PlayerNum, hole1.ValueStr(), hole2.ValueStr());
                player.ReceiveHoleCards(hole1, hole2);
            }
        }

        private void BroadcastBoardCard(EBoardCardType cardType, Card boardCard)
        {
            Logger.Log("{0} {1}", cardType, boardCard.ValueStr());

            foreach (var player in _players)
            {
                player.SeeBoardCard(cardType, boardCard);
            }
        }

        // broadcast action of a player to all players (including themselves)
        private void BroadcastAction(EStage stage, int playerNumDoingAction, EActionType action, int amount)
        {
            var sLogMsg = string.Format("Player {0} {1} {2}", playerNumDoingAction, action, amount);

            if (_players[playerNumDoingAction].StackSize <= 0)
            {
                sLogMsg += " *ALL IN*";
            }

            Logger.Log(sLogMsg);

            foreach (var player in _players)
            {
                player.SeeAction(stage, playerNumDoingAction, action, amount);
            }
        }

        private void BroadcastPlayerHand(int playerNum, Hand playerBestHand)
        {
            Logger.Log("Player {0} Best Hand =  {1} {2}", playerNum, playerBestHand.HandValueStr(), playerBestHand.HandRankStr());
            var card1 = _players[playerNum].HoleCards()[0];
            var card2 = _players[playerNum].HoleCards()[1];

            foreach (var player in _players)
            {
                player.SeePlayerHand(playerNum, card1, card2, playerBestHand);
            }
        }

        private bool IsBettingRoundRequired()
        {
            // Don't do betting if all players or all but one are all in.
            var numActivePlayersWithChips = _players.Count(p => p.IsActive && (p.StackSize > 0));

            return numActivePlayersWithChips > 1;
        }

        private void DoBettingRound(EStage stage, out int lastToAct)
        {
            var bDone = false;
            var raisesRemaining = _maxNumRaisesPerBettingRound;
            int firstBettorPlayerNum;

            // calc call /raise amounts req
            var lastFullPureRaise = _bigBlindSize;
            int callLevel;

            if (stage == EStage.StagePreflop)
            {
                TakeBlinds();
                firstBettorPlayerNum = GetNextActivePlayer(_bigBlindPlayerNum);
                callLevel = _bigBlindSize; //set this explicitly in case the big blind is short
            }
            else
            {
                firstBettorPlayerNum = GetNextActivePlayer(_dealerPlayerNum);
                callLevel = _potMan.MaxContributions();
            }

            var currBettor = firstBettorPlayerNum;
            lastToAct = GetPrevActivePlayer(currBettor);

            while (!bDone)
            {
                // dont call GetAction if player is already all in
                if (_players[currBettor].StackSize > 0)
                {
                    int callAmount;
                    int minRaise;
                    int maxRaise;
                    CalcRequiredBetAmounts(currBettor, callLevel, lastFullPureRaise, out callAmount, out minRaise, out maxRaise);

                    // get the players action
                    EActionType playersAction;
                    int playersBetAmount;
                    _players[currBettor].GetAction(stage, callAmount, minRaise, maxRaise, raisesRemaining, _potMan.Size(), out playersAction, out playersBetAmount);

                    // *** DO ACTION ***
                    if (playersAction == EActionType.ActionFold)
                    {
                        // if fold then mark player as inactive
                        _players[currBettor].IsActive = false;
                    }
                    else if ((playersAction == EActionType.ActionCall) || (playersAction == EActionType.ActionRaise))
                    {
                        // if call or raise the take $ from players stack and put in pot
                        TransferMoneyToPot(currBettor, playersBetAmount);

						if (playersAction == EActionType.ActionRaise)
						{
                            // if raise then update lastToAct to the preceding active player
                            lastToAct = GetPrevActivePlayer(currBettor);
                            raisesRemaining--;

                            // if this raise is less than the minimum (because all in) then we shouldn't count it as a proper raise and shouldn't allow the original raiser to reraise
                            if (playersBetAmount - callAmount > lastFullPureRaise)
                            {
                                lastFullPureRaise = playersBetAmount - callAmount;
                            }

                            if (_potMan.PlayerContributions(currBettor) > callLevel)
							{
								callLevel = _potMan.PlayerContributions(currBettor);
							}
						}
                    }

                    BroadcastAction(stage, currBettor, playersAction, playersBetAmount);

                    if (_players[currBettor].IsActive)
                    {
                        _display.UpdatePlayer(_players[currBettor]);
                    }
                }

                // if this player is last to act or only one active player left then bDone = true
                if ((currBettor == lastToAct) || (GetNumActivePlayers() == 1))
                {
                    bDone = true;
                }
                else
                {
                    currBettor = GetNextActivePlayer(currBettor);
                }
            }
        }

        private void CalcRequiredBetAmounts(int currBettor, int callLevel, int lastFullPureRaise, out int callAmount, out int minRaise, out int maxRaise)
        {
            callAmount = callLevel - _potMan.PlayerContributions(currBettor);

            maxRaise = _players[currBettor].StackSize; //  if no limit - change this if limit game

            minRaise = callAmount + lastFullPureRaise; 
        }

        private int GetNumActivePlayers()
        {
            return _players.Count(p => p.IsActive);
        }

        private int GetNumLivePlayers()
        {
            return _players.Count(p => p.IsAlive);
        }

        private int GetNextActivePlayer(int player)
        {
            var i = 0;
            while (i < _numPlayers)
            {
                var playerNum = (i + player + 1) % _numPlayers;

                if (_players[playerNum].IsActive)
                {
                    return playerNum;
                }

                i++;
            }

            // no active players
            return -1;
        }

        private int GetNextLivePlayer(int player)
        {
            var i = 0;
            while (i < _numPlayers)
            {
                var playerNum = (i + player + 1) % _numPlayers;

                if (_players[playerNum].IsAlive)
                {
                    return playerNum;
                }

                i++;
            }

            // no live players
            return -1;
        }


        private int GetPrevActivePlayer(int player)
        {
            var i = 0;
            while (i < _numPlayers)
            {
                var playerNum = (_numPlayers + player - i - 1) % _numPlayers;

                if (_players[playerNum].IsActive)
                {
                    return playerNum;
                }

                i++;
            }

            // no active players
            return -1;
        }

        private void TransferMoneyToPot(int playerNum, int amount)
        {
            bool isAllIn;

            if (amount > _players[playerNum].StackSize)
            {
                throw new Exception("insufficient chips");
            }

            if (amount > 0)
            {
                _players[playerNum].StackSize -= amount;

                isAllIn = _players[playerNum].StackSize == 0;

                _potMan.AddPlayersBet(playerNum, amount, isAllIn);

                ReconcileCash();
            }
        }


        private void ReconcileCash()
        {
            // Check money still adds up
            var totalPlayersStacks = _players.Sum(p => p.StackSize);
            var potSize = _potMan.Size();

            if (_totalMoneyInGame != potSize + totalPlayersStacks)
            {
                ViewCash();
                throw new Exception(string.Format("money doesn't add up! Money in game = {0}, Stacks = {1}, Pots = {2}", _totalMoneyInGame, totalPlayersStacks, potSize));
            }
        }

        private void Showdown(Card[] board, ref HandRanker handRanker, int lastToAct)
        {
            var firstToAct = GetNextActivePlayer(lastToAct);
            Hand playerBestHand;
            var uncontestedPots = new List<int>();

            for (var potNum = 0; potNum < _potMan.Pots.Count; potNum++)
            {
                uncontestedPots.Add(potNum);
            }

            // evaluate and show hand for first player to act - flag them as winning for now
            playerBestHand = Hand.FindPlayersBestHand(_players[firstToAct].HoleCards(), board);
            BroadcastPlayerHand(firstToAct, playerBestHand);

            handRanker.AddHand(firstToAct, playerBestHand);
            uncontestedPots = uncontestedPots.Except( _potMan.GetPotsInvolvedIn(firstToAct)).ToList();

            // Loop through other active players 
            var currPlayer = GetNextActivePlayer(firstToAct);

            do
            {
                var player = _players[currPlayer];
                EActionType playersAction;
                int playersAmount;

                // if not first to act then player may fold without showing cards (unless all in)
                // Also don't allow a player to fold if they are involved in a currently uncontested (side) pot
                var potsInvolvedIn = _potMan.GetPotsInvolvedIn(currPlayer);
                if (player.StackSize > 0 && (uncontestedPots.Intersect(potsInvolvedIn).Count() == 0))
                {
                    player.GetAction(EStage.StageShowdown, 0, 0, 0, 0, _potMan.Size(), out playersAction, out playersAmount);
                }
                else
                {
                    playersAction = EActionType.ActionShow;
                }

                if (playersAction == EActionType.ActionFold)
                {
                    _players[currPlayer].IsActive = false;
                    BroadcastAction(EStage.StageShowdown, currPlayer, playersAction, 0);
                }
                else
                {
                    playerBestHand = Hand.FindPlayersBestHand(player.HoleCards(), board);
                    handRanker.AddHand(currPlayer, playerBestHand);
                    uncontestedPots = uncontestedPots.Except(potsInvolvedIn).ToList();

                    BroadcastPlayerHand(currPlayer, playerBestHand);
                }

                currPlayer = GetNextActivePlayer(currPlayer);

            } while (currPlayer != firstToAct);
        }

        private void DistributeWinnings(HandRanker handRanker)
        {
            foreach(var pot in _potMan.Pots)
            {
                // get all players who are involved in this pot
                var playersInvolved = pot.ListPlayersInvolved();

                // loop through hand ranks from highest to lowest
                // find highest ranked handRank that includes at least one of these players
                foreach (var hri in handRanker.HandRanks)
                {
                    var playersInRank = hri.Players;

                    var winningPlayers = playersInvolved.Intersect(playersInRank).ToList();

                    if (winningPlayers.Count > 0)
                    {
                        // split pot equally between winning players - then handle remainder, remove cash from pot, add to players stack
                        var amountWon = new Dictionary<int, int>();
                        var potRemaining = pot.Size();
                        var shareOfWinnings = potRemaining / winningPlayers.Count;

                        foreach (var i in winningPlayers)
                        {
                            amountWon[i] = shareOfWinnings;
                            potRemaining -= shareOfWinnings;
                        }

                        // if remainder left in pot then allocate 1 chip at a time starting at player in worst position (closest to little blind)
                        var currPlayer = _dealerPlayerNum;

                        while (potRemaining > 0)
                        {
                            do
                            {
                                currPlayer = GetNextActivePlayer(currPlayer);
                            } while (!winningPlayers.Contains(currPlayer));

                            amountWon[currPlayer]++;
                            potRemaining--;
                        }

                        pot.EmptyPot();

                        // broadcast win
                        foreach (var pair in amountWon)
                        {
                            _players[pair.Key].StackSize += pair.Value;
                            BroadcastAction(EStage.StageShowdown, pair.Key, EActionType.ActionWin, pair.Value);
                        }

                        break;
                    }
                }
            }

            _potMan.EmptyPot();
        }

        private void KillBrokePlayers()
        {
            // Kill off broke players
            foreach (var player in _players)
            {
                if(player.IsAlive && player.StackSize <= 0)
                {
                    Logger.Log("Player {0} has been eliminated", player.PlayerNum);

                    player.IsAlive = false;
                    player.IsActive = false;
                }
            }
        }

        private void MoveDealerAndBlinds()
        {
            _dealerPlayerNum = _littleBlindPlayerNum; // note that this player might not be live if just eliminated
            _littleBlindPlayerNum = _bigBlindPlayerNum; // note that this player might not be live if just eliminated
            _bigBlindPlayerNum = GetNextLivePlayer(_bigBlindPlayerNum);
        }

        public void ViewCash()
        {
            var potNum = 0;

            Logger.Log("");
            Logger.Log("--- View Money ---");

            string sLogMsg;
            // show player numbers
            sLogMsg = "Players\t";

            foreach (var player in _players)
            {
                if (player.IsAlive)
                {
                    sLogMsg += player.PlayerNum + "\t";
                }
            }

            sLogMsg += "Total";
            Logger.Log(sLogMsg);

            // Show stack size
            sLogMsg  = "Stack\t";

            var totalStackSize = 0;
            foreach (var player in _players)
            {
                if (player.IsAlive)
                {
                    sLogMsg += player.StackSize + "\t";
                    totalStackSize += player.StackSize;
                }
            }

            sLogMsg += totalStackSize;
            Logger.Log(sLogMsg);

            // Show breakdown of each pot
            foreach (var p in _potMan.Pots)
            {
                var playerAmount = new int[_players.Length];

                sLogMsg = "Pot " + potNum + "\t";

                foreach (var pair in p.PlayerContributions)
                {
                    playerAmount[pair.Key] = pair.Value;
                }

                foreach (var player in _players)
                {
                    if (player.IsAlive)
                    {
                        sLogMsg += playerAmount[player.PlayerNum] + "\t";
                    }
                }

                sLogMsg += p.Size();
                Logger.Log(sLogMsg);

                potNum++;
            }

            Logger.Log("");
        }
    }
}
