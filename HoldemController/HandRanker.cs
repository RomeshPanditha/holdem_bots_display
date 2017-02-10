using System.Collections.Generic;
using HoldemPlayerContract;

namespace HoldemController
{
    internal class HandRanker
    {
        public List<HandRankInfo> HandRanks = new List<HandRankInfo>();

        public void AddHand(int playerId, Hand playerHand)
        {
            var i = 0;
            var bAdded = false;

            while (i < HandRanks.Count && !bAdded)
            {
                var listHand = HandRanks[i].Hand;
                var compareResult = playerHand.Compare(listHand);

                if ( compareResult == 1)
                {
                    // player hand is better than hand in list so insert before
                    HandRanks.Insert(i, new HandRankInfo(playerId, playerHand));
                    bAdded = true;
                }
                else if (compareResult == 0)
                {
                    // Hands are same value so add playerId to existing handRank
                    HandRanks[i].AddPlayer(playerId);
                    bAdded = true;
                }

                i++;
            }

            if (!bAdded)
            {
                // lowest (or first) hand. Add hand to end of list
                HandRanks.Add(new HandRankInfo(playerId, playerHand));
            }
        }

        public void ViewHandRanks()
        {
            Logger.Log("");
            Logger.Log("--- Hand Ranks ---");

            foreach (var hri in HandRanks)
            {
                var hand = hri.Hand;

                var sLogMsg = hand.HandRankStr() + " ";

                for (var i = 0; i < hand.NumSubRanks(); i++)
                {
                    sLogMsg += hand.SubRank(i) + " ";
                }

                sLogMsg += "Players (" + string.Join(",", hri.Players) + ")";
                Logger.Log(sLogMsg);
            }

            Logger.Log("");
        }

    }
}
