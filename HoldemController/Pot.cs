using System;
using System.Collections.Generic;
using System.Linq;

namespace HoldemController
{
    internal class Pot
    {
        private int _potSize;  // should match sum off all player contributions - unless we have distributed winnings
        private int _maxContributions;
        private bool _isCapped;

        // need to record contributions per player
        public Dictionary<int, int> PlayerContributions = new Dictionary<int, int>();

        public int NumPlayersInvolved;

        public void EmptyPot()
        {
            _potSize = 0;
            _maxContributions = 0;
            PlayerContributions.Clear();
            NumPlayersInvolved = 0;
            _isCapped = false;
        }

        public void CapPot()
        {
            _isCapped = true;
        }

        public bool IsCapped()
        {
            return _isCapped;
        }

        public void AddPlayersBet(int playerId, int amount)
        {
            var contributions = 0;

            if (PlayerContributions.ContainsKey(playerId))
            {
                contributions = PlayerContributions[playerId];
            }
            else
            {
                NumPlayersInvolved++;
            }

            // Not an error - This could validly occur if splitting a pot and another player has not yet called the pot that is being split
            //if (amount < (_maxContributions - contributions))
            //{
            //    throw new Exception("insufficient amount to call");
            //}

            contributions += amount;

            if (contributions > _maxContributions)
            {
                if (_isCapped)
                {
                    throw new Exception("contribution is too big, pot is capped");
                }

                _maxContributions = contributions;
            }

            PlayerContributions[playerId] = contributions;
            _potSize += amount;
        }

        public int Size()
        {
            return _potSize;
        }
        public int MaxContributions()
        {
            return _maxContributions;
        }

        public int AmountToCall(int playerId)
        {
            // max total contributions by any player - contributions for this player
            int contributions;
            PlayerContributions.TryGetValue(playerId, out contributions);

            return _maxContributions - contributions;
        }

        public List<int> ListPlayersInvolved()
        {
            // get list of all players that have contributed to the pot
            return PlayerContributions.Select(pair => pair.Key).ToList();
        }
    }
}