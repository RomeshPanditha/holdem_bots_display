using System;
using System.Collections.Generic;
using System.Linq;

namespace HoldemController
{
    internal class PotManager
    {
        public List<Pot> Pots = new List<Pot>();

        public void AddPlayersBet(int playerId, int betAmount, bool isAllIn)
        {
            if (betAmount == 0) return;

            if (Pots.Count == 0)
            {
                Pots.Add(new Pot());
            }
            // work out which side pots to call. create new side pots if required

            // loop through pots from most players involved to least
            // call each pot if able (and raise the last if able). if allin and unable to call a pot then split this pot in two
            var betAmountRemaining = betAmount;

            var potNum = 0;
            var numPots = Pots.Count;

            while (potNum < numPots && betAmountRemaining > 0)
            {
                var p = Pots[potNum];

                var amountToCall = p.AmountToCall(playerId);

                if (betAmountRemaining >= amountToCall)
                {
                    //enough chips to call or raise pot
                    if (p.IsCapped())
                    {
                        // call this pot and try to put remainder in next one
                        p.AddPlayersBet(playerId, amountToCall);
                        betAmountRemaining -= amountToCall;
                    }
                    else
                    {
                        // pot is not capped so bet full raise into this one
                        p.AddPlayersBet(playerId, betAmountRemaining);
                        betAmountRemaining = 0;

                        if (isAllIn)
                        {
                            // if this player is allin then cap this pot so no other player may increase it
                            p.CapPot();
                        }
                    }
                }
                else
                {
                    // not enough to call - player better be going all in
                    if(!isAllIn)
                    {
                        throw new Exception("insufficient chips to call");
                    }

                    // split current pot and cap it, add players bet to new pot
                    SplitPotAndAddBet(p, playerId, betAmountRemaining);
                    betAmountRemaining = 0;
                }

                potNum++;
            }

            // all pots capped so need to create a new one to put remaining chips in
            if (betAmountRemaining > 0)
            {
                var newPot = new Pot();
                newPot.AddPlayersBet(playerId, betAmountRemaining);

                if (isAllIn)
                {
                    // if this player is allin then cap this pot so no other player may increase it
                    newPot.CapPot();
                }

                Pots.Add(newPot);
            }
        }

        private void SplitPotAndAddBet(Pot p, int playerId, int betAmount)
        {
            var newPot1 = new Pot();
            var newPot2 = new Pot();

            var pastContribution = 0;
            if (p.PlayerContributions.ContainsKey(playerId))
            {
                pastContribution = p.PlayerContributions[playerId];
            }

            // this is the players total contribution to this pot including the current bet. This is the amount we need to split on
            var totalContribution = betAmount + pastContribution;
            var capAmount = totalContribution;

            foreach (var pair in p.PlayerContributions)
            {
                if (pair.Key != playerId)
                {
                    if (pair.Value > capAmount)
                    {
                        newPot1.AddPlayersBet(pair.Key, capAmount);
                        newPot2.AddPlayersBet(pair.Key, pair.Value - capAmount);
                    }
                    else
                    {
                        newPot1.AddPlayersBet(pair.Key, pair.Value);
                    }
                }
            }

            newPot1.AddPlayersBet(playerId, totalContribution);

            newPot1.CapPot();

            if (p.IsCapped())
            {
                newPot2.CapPot();
            }

            Pots.Remove(p);
            Pots.Add(newPot1);
            Pots.Add(newPot2);

            Pots.Sort((x, y) => x.NumPlayersInvolved <= y.NumPlayersInvolved
                                    ? (x.NumPlayersInvolved < y.NumPlayersInvolved
                                           ? 1
                                           : 0)
                                    : -1);
        }

        public void EmptyPot()
        {
            Pots.Clear();
        }

        public int Size()
        {
            return Pots.Sum(p => p.Size());
        }

        public int AmountToCall(int playerId)
        {
            return Pots.Sum(p => p.AmountToCall(playerId));
        }

        public List<int> GetPotsInvolvedIn(int playerNum)
        {
            var potsInvolvedIn = new List<int>();

            for(var potNum=0; potNum<Pots.Count; potNum++)
            {
                if (Pots[potNum].ListPlayersInvolved().Contains(playerNum))
                {
                    potsInvolvedIn.Add(potNum);
                }
            }

            return potsInvolvedIn;
        }

        public int MaxContributions()
        {
            return Pots.Sum(p => p.MaxContributions());
        }

        public int PlayerContributions(int playerNum)
        {
            return Pots.Where(p => p.PlayerContributions.ContainsKey(playerNum))
                       .Sum(p => p.PlayerContributions[playerNum]);
        }
    }
}
