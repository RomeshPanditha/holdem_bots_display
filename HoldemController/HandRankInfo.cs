using System.Collections.Generic;
using HoldemPlayerContract;

namespace HoldemController
{
    internal class HandRankInfo
    {
        public Hand Hand;
        public List<int> Players;

        public HandRankInfo(int playerId, Hand playerHand)
        {
            Hand = playerHand;
            Players = new List<int> {playerId};
        }

        public void AddPlayer(int playerId)
        {
            if (!Players.Contains(playerId))
            {
                Players.Add(playerId);
            }
        }
    }
}