using System.Collections.Generic;

namespace HoldemPlayerContract
{
    public interface IHoldemPlayer
    {
        void InitPlayer(int playerNum, Dictionary<string, string> playerConfigSettings);
        string Name { get; }
        bool IsObserver { get; }
        void InitHand(int numPlayers, PlayerInfo []  players);
        void ReceiveHoleCards(Card hole1, Card hole2);
        void SeeAction(EStage stage, int playerNum, EActionType action, int amount);
        void GetAction(EStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out EActionType yourAction, out int amount);
        void SeeBoardCard(EBoardCardType cardType, Card boardCard);
        void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand);
        void EndOfGame(int numPlayers, PlayerInfo[] players);
    }
}