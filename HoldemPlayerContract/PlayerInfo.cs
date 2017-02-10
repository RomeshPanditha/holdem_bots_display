
using System;

namespace HoldemPlayerContract
{
    [Serializable]
    public class PlayerInfo
    {
        public PlayerInfo(int pPlayerNum, string pName, bool pIsAlive, int pStackSize, bool pIsDealer, bool pIsObserver)
        {
            PlayerNum = pPlayerNum;
            Name = pName;
            IsAlive = pIsAlive;
            StackSize = pStackSize;
            IsDealer = pIsDealer;
            IsObserver = pIsObserver;
        }

        public readonly int PlayerNum;
        public readonly string Name;
        public readonly bool IsAlive;
        public readonly int StackSize;
        public readonly bool IsDealer;
        public readonly bool IsObserver;
    }
}