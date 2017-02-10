using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using System.IO;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Runtime.Remoting;

using HoldemPlayerContract;

namespace HoldemController
{
    internal class ServerHoldemPlayer : IHoldemPlayer
    {
        private readonly IHoldemPlayer _player;
        public int StackSize { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlive { get; set; }

        public int PlayerNum { get; set; }

        private readonly Card[] _holeCards;

        private string _sName = null;
        private bool? _bIsObserver = null;

        // for launching GetAction in separate thread
        private EActionType _playersAction;
        private int _playersBetAmount;
        private int _botTimeOutMilliSeconds;
        private Task _task;
        private bool _bIsBotBusy = false; // while a task for the bot is running don't start another task. instead don't send it anymore messages and fold until it is not busy at the start of a hand
        private AppDomain _newDomain;

        public ServerHoldemPlayer(int pPlayerNum, Dictionary<string, string> playerConfigSettings)
        {
            string botDir = "bots\\";
            string dllFile = botDir + playerConfigSettings["dll"];
            PlayerNum = pPlayerNum;
            StackSize = Convert.ToInt32(playerConfigSettings["startingStack"]);
            _botTimeOutMilliSeconds = Convert.ToInt32(playerConfigSettings["botTimeOutMilliSeconds"]);

            IsActive = true;
            IsAlive = true;
            _holeCards = new Card[2];

            var an = AssemblyName.GetAssemblyName(dllFile);
            var assembly = Assembly.Load(an);

            var interfaceType = typeof(IHoldemPlayer);
            var types = assembly.GetTypes();
            Type botType = null;

            foreach (var type in types.Where(type => !type.IsInterface
                                                        && !type.IsAbstract
                                                        && type.GetInterface(interfaceType.FullName) != null))
            {
                botType = type;
                break;
            }

            if(botType == null)
            {
                throw new Exception(String.Format("A class that implements the IHoldemPlayer interface was not found in {0}.", dllFile));
            }

            if(dllFile == "bots\\ObserverBot.dll")
            {
                // If bots is trusted then run inside the current app domain
                // !!! need a better way to identify trusted bots !!! use signing?
                _player = (IHoldemPlayer) Activator.CreateInstance(botType);
            }
            else
            {
                // Untrusted bot - create a new sandbox to run it in with limited permissions
                //Setting the AppDomainSetup. It is very important to set the ApplicationBase to a folder 
                //other than the one in which the sandboxer resides.
                AppDomainSetup adSetup = new AppDomainSetup();
                adSetup.ApplicationBase = Path.GetFullPath(botDir);

                //Setting the permissions for the AppDomain. We give the permission to execute and to 
                //read/discover the location where the untrusted code is loaded.
                PermissionSet permSet = new PermissionSet(PermissionState.None);
                permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

                //Now we have everything we need to create the AppDomain, so let's create it.
                _newDomain = AppDomain.CreateDomain("SandBox" + pPlayerNum, null, adSetup, permSet); 

                // Now create an instance of the bot class inside the new appdomain
                _player = (IHoldemPlayer)_newDomain.CreateInstanceAndUnwrap(an.FullName, botType.FullName);
            }    

            InitPlayer(pPlayerNum, playerConfigSettings);
        }

        public void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
            if (_botTimeOutMilliSeconds > 0)
            {
                if (!IsBotBusy())
                {
                    _task = Task.Run(() => { RunInitPlayer(playerNum, playerConfigSettings); });

                    // wait X amount of time for task to complete
                    if (!_task.Wait(_botTimeOutMilliSeconds))
                    {
                        // Note that the task is still running in the background
                        _bIsBotBusy = true;
                        Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                    }
                }
                else
                {
                    // bot is busy still running the previous task
                    Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                }
            }
            else
            {
                // timeout code disabled - just called method directly
                RunInitPlayer(playerNum, playerConfigSettings);
            }

            if (IsObserver)
            {
                IsAlive = false;
                IsActive = false;
                StackSize = 0;
            }
        }

        private void RunInitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings)
        {
            try
            {
                _player.InitPlayer(playerNum, playerConfigSettings);
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, playerNum, e.Message));
            }
        }



        public string Name
        {
            get
            {
                if(_sName == null)
                {
                    _sName = "????"; // default

                    if (_botTimeOutMilliSeconds > 0)
                    {
                        if (!IsBotBusy())
                        {
                            _task = Task.Run(() => { RunGetName(); });

                            // wait X amount of time for task to complete
                            if (!_task.Wait(_botTimeOutMilliSeconds))
                            {
                                // Note that the task is still running in the background
                                _bIsBotBusy = true;
                                Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                            }
                        }
                        else
                        {
                            // bot is busy still running the previous task
                            Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                        }
                    }
                    else
                    {
                        // timeout code disabled - just called method directly
                        RunGetName();
                    }

                }

                return _sName;
            }
        }

        private void RunGetName()
        {
            try
            {
                _sName = _player.Name;
            }
            catch (Exception e)
            {
                _sName = "????";
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }
        }

        public bool IsObserver
        {
            get
            {
                if (_bIsObserver == null)
                {
                    _bIsObserver = true; // default

                    if (_botTimeOutMilliSeconds > 0)
                    {
                        if (!IsBotBusy())
                        {
                            _task = Task.Run(() => { RunIsObserver(); });

                            // wait X amount of time for task to complete
                            if (!_task.Wait(_botTimeOutMilliSeconds))
                            {
                                // Note that the task is still running in the background
                                _bIsBotBusy = true;
                                Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                            }
                        }
                        else
                        {
                            // bot is busy still running the previous task
                            Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                        }
                    }
                    else
                    {
                        // timeout code disabled - just called method directly
                        RunIsObserver();
                    }
                }

                return (bool)_bIsObserver;
            }
        }

        private void RunIsObserver()
        {
            try
            {
                _bIsObserver = _player.IsObserver;
            }
            catch (Exception e)
            {
                _bIsObserver = true;
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }
        }

        public void InitHand(int numPlayers, PlayerInfo[] players)
        {
            IsActive = IsAlive;

            if (_botTimeOutMilliSeconds > 0)
            {
                // if bot is busy at the start of the hand then don't send any messages for the hand (so it doesn't get messages for half a hand)
                _bIsBotBusy = (_task != null && !_task.IsCompleted);

                if (!IsBotBusy())
                {
                    _task = Task.Run(() => { RunInitHand(numPlayers, players); });

                    // wait X amount of time for task to complete
                    if (!_task.Wait(_botTimeOutMilliSeconds))
                    {
                        // Note that the task is still running in the background
                        _bIsBotBusy = true;
                        Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                    }
                }
                else
                {
                    // bot is busy still running the previous task
                    Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                }
            }
            else
            {
                // timeout code disabled - just called method directly
                RunInitHand(numPlayers, players);
            }
        }

        private void RunInitHand(int numPlayers, PlayerInfo[] players)
        {
            try
            {
                _player.InitHand(numPlayers, players);
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }

        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
            if (_botTimeOutMilliSeconds > 0)
            {
                if (!IsBotBusy())
                {
                    _task = Task.Run(() => { RunReceiveHoleCards(hole1, hole2); });

                    // wait X amount of time for task to complete
                    if (!_task.Wait(_botTimeOutMilliSeconds))
                    {
                        // Note that the task is still running in the background
                        _bIsBotBusy = true;
                        Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                    }
                }
                else
                {
                    // bot is busy still running the previous task
                    Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                }
            }
            else
            {
                // timeout code disabled - just called method directly
                RunReceiveHoleCards(hole1, hole2);
            }
        }

        private void RunReceiveHoleCards(Card hole1, Card hole2)
        {
            _holeCards[0] = hole1;
            _holeCards[1] = hole2;

            try
            {
                _player.ReceiveHoleCards(hole1, hole2);
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }
        }

        public Card [] HoleCards()
        {
            return _holeCards;
        }

        private bool IsBotBusy()
        {
            return _bIsBotBusy;
        }

        public void SeeAction(EStage stage, int playerDoingAction, EActionType action, int amount)
        {
            if (_botTimeOutMilliSeconds > 0)
            {
                if (!IsBotBusy())
                {
                    _task = Task.Run(() => { RunSeeAction(stage, playerDoingAction, action, amount); });

                    // wait X amount of time for task to complete
                    if (!_task.Wait(_botTimeOutMilliSeconds))
                    {
                        // Note that the task is still running in the background
                        _bIsBotBusy = true;
                        Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                    }
                }
                else
                {
                    // bot is busy still running the previous task
                    Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                }
            }
            else
            {
                // timeout code disabled - just called method directly
                RunSeeAction(stage, playerDoingAction, action, amount);
            }
        }

        private void RunSeeAction(EStage stage, int playerDoingAction, EActionType action, int amount)
        {
            try
            {
                _player.SeeAction(stage, playerDoingAction, action, amount);
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }
        }


        public void GetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out EActionType playersAction, out int playersBetAmount)
        {
            // Default to fold if exception or timeout
            playersAction = EActionType.ActionFold;
            playersBetAmount = 0;

            if (_botTimeOutMilliSeconds > 0)
            {
                if (!IsBotBusy())
                {
                    _task = Task.Run(() => { RunGetAction(stage, callAmount, minRaise, maxRaise, raisesRemaining, potSize);});

                    // wait X amount of time for task to complete
                    // if method has not returned in time then use default action
                    if (_task.Wait(_botTimeOutMilliSeconds))
                    {
                        playersAction = _playersAction;
                        playersBetAmount = _playersBetAmount;
                    }
                    else
                    {
                        // Note that the task is still running in the background
                        Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                    }
                }
                else
                {
                    // bot is busy still running the previous task
                    Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                }
            }
            else
            {
                // timeout code disabled - just called method directly
                RunGetAction(stage, callAmount, minRaise, maxRaise, raisesRemaining, potSize);
                playersAction = _playersAction;
                playersBetAmount = _playersBetAmount;
            }

            ValidateAction(stage, callAmount, minRaise, maxRaise, raisesRemaining, potSize, ref playersAction, ref playersBetAmount);
        }

        private void RunGetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize)
        {
            try
            {
                // call player.GetAction - can't put out params in anonymous method above so doing it this way
                _player.GetAction(stage, callAmount, minRaise, maxRaise, raisesRemaining, potSize, out _playersAction, out _playersBetAmount);
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }
        }


        private void ValidateAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, ref EActionType playersAction, ref int playersBetAmount)
        {
            // *** Fix up action
            if(stage == EStage.StageShowdown)
            {
                if(playersAction != EActionType.ActionFold)
                {
                    playersAction = EActionType.ActionShow;
                }

                playersBetAmount = 0;
            }
            else
            {
                var bAllIn = false;

                if (playersBetAmount < 0)
                {
                    playersBetAmount = 0;
                }

                if (playersAction == EActionType.ActionRaise)
                {
/*
                    // We shouldn't increase a players bet - but can reduce it if outside limits
                    if (playersBetAmount < minRaise)
                    {
                        playersBetAmount = minRaise;
                    }
*/
                    if (playersBetAmount > maxRaise)
                    {
                        playersBetAmount = maxRaise;
                    }
                }

                if (playersBetAmount > StackSize)
                {
                    playersBetAmount = StackSize;
                }

                if (playersBetAmount == StackSize)
                {
                    bAllIn = true;
                }

                // -- Validate action - prevent player from doing anything illegal
                if (playersAction != EActionType.ActionFold &&
                    playersAction != EActionType.ActionCheck &&
                    playersAction != EActionType.ActionCall &&
                    playersAction != EActionType.ActionRaise )
                {
                    // invalid action - default to call
                    playersAction = EActionType.ActionCall;
                }

                if (playersAction == EActionType.ActionFold && callAmount == 0)
                {
                    // invalid action - don't fold if they can check
                    playersAction = EActionType.ActionCheck;
                }

                if (playersAction == EActionType.ActionCheck && callAmount > 0)
                {
                    // invalid action - can't check so change to call
                    playersAction = EActionType.ActionCall;
                }

                if (playersAction == EActionType.ActionRaise && playersBetAmount <= callAmount)
                {
                    // not enough chips to raise - just call
                    playersAction = EActionType.ActionCall;
                }

                if (playersAction == EActionType.ActionRaise && playersBetAmount > callAmount && playersBetAmount < minRaise && !bAllIn)
                {
                    // not enough chips to raise - just call unless going allin
                    playersAction = EActionType.ActionCall;
                }

                if (playersAction == EActionType.ActionRaise && (raisesRemaining <= 0))
                {
                    // no more raises allowed
                    playersAction = EActionType.ActionCall;
                }

                if (playersAction == EActionType.ActionCall && callAmount == 0)
                {
                    // change call to check if callAmount = 0
                    playersAction = EActionType.ActionCheck;
                }

                // *** Fix betAmount
                if (playersAction == EActionType.ActionFold || playersAction == EActionType.ActionCheck)
                {
                    playersBetAmount = 0;
                }

                if (playersAction == EActionType.ActionCall)
                {
                    playersBetAmount = callAmount;

                    if (playersBetAmount > StackSize)
                    {
                        playersBetAmount = StackSize;
                    }
                }
            }
        }

        public void SeeBoardCard(EBoardCardType cardType, Card boardCard)
        {
            if (_botTimeOutMilliSeconds > 0)
            {
                if (!IsBotBusy())
                {
                    _task = Task.Run(() => { RunSeeBoardCard(cardType, boardCard); });

                    // wait X amount of time for task to complete
                    if (!_task.Wait(_botTimeOutMilliSeconds))
                    {
                        // Note that the task is still running in the background
                        _bIsBotBusy = true;
                        Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                    }
                }
                else
                {
                    // bot is busy still running the previous task
                    Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                }
            }
            else
            {
                // timeout code disabled - just called method directly
                RunSeeBoardCard(cardType, boardCard);
            }
        }
         
        private void RunSeeBoardCard(EBoardCardType cardType, Card boardCard)
        {
            try
            {
                _player.SeeBoardCard(cardType, boardCard);
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }
        }

        public void SeePlayerHand(int playerShowingHand, Card hole1, Card hole2, Hand bestHand)
        {
            if (_botTimeOutMilliSeconds > 0)
            {
                if (!IsBotBusy())
                {
                    _task = Task.Run(() => { RunSeePlayerHand(playerShowingHand, hole1, hole2, bestHand); });

                    // wait X amount of time for task to complete
                    if (!_task.Wait(_botTimeOutMilliSeconds))
                    {
                        // Note that the task is still running in the background
                        _bIsBotBusy = true;
                        Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                    }
                }
                else
                {
                    // bot is busy still running the previous task
                    Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                }
            }
            else
            {
                // timeout code disabled - just called method directly
                RunSeePlayerHand(playerShowingHand, hole1, hole2, bestHand);
            }
        }


        private void RunSeePlayerHand(int playerShowingHand, Card hole1, Card hole2, Hand bestHand)
        {
            try
            {
                _player.SeePlayerHand(playerShowingHand, hole1, hole2, bestHand);
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
            if (_botTimeOutMilliSeconds > 0)
            {
                if (!IsBotBusy())
                {
                    _task = Task.Run(() => { RunEndOfGame(numPlayers, players); });

                    // wait X amount of time for task to complete
                    if (!_task.Wait(_botTimeOutMilliSeconds))
                    {
                        // Note that the task is still running in the background
                        _bIsBotBusy = true;
                        Logger.Log("TIMEOUT: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                    }
                }
                else
                {
                    // bot is busy still running the previous task
                    Logger.Log("BOT BUSY: {0} Player {1}", MethodBase.GetCurrentMethod().Name, PlayerNum);
                }
            }
            else
            {
                // timeout code disabled - just called method directly
                RunEndOfGame(numPlayers, players);
            }

            // !!! should I be doing this here?
            if(_newDomain != null)
            {
                AppDomain.Unload(_newDomain);
            }
        }

        private void RunEndOfGame(int numPlayers, PlayerInfo[] players)
        {
            try
            {
                _player.EndOfGame(numPlayers, players);
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("EXCEPTION: {0} Player {1} : {2}", MethodBase.GetCurrentMethod().Name, PlayerNum, e.Message));
            }
        }
    }
}
