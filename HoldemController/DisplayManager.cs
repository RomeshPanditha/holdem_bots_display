using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HoldemPlayerContract;

namespace HoldemController
{

    public class DisplayManager
    {
        private int _boardHeight;
        private int _minBoardWidth;
        private int _maxBoardWidth;
        private readonly int _width;
        private readonly int _height;
        private readonly List<ServerHoldemPlayer> _players;
        private List<PlayerPosition> _availablePositions;
        private Dictionary<int, PlayerPosition> _playerPositions;

        public class PlayerPosition
        {
            public int X { get; set; }
            public int Y { get; set; }

            public PositionType Type { get; set; }
        }

        public enum PositionType
        {
            Top,
            Right,
            Bottom,
            Left
        }

        internal DisplayManager(int width, int height, IEnumerable<ServerHoldemPlayer> players)
        {
            Console.SetWindowSize(width, height);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            _width = width;
            _height = height;
            _players = players.Where(s => s.IsActive).ToList();
            if (_players.Count > 8)
            {
                throw new ArgumentOutOfRangeException("numPlayers");
            }
        }

        public void DrawTable()
        {
            DrawLines(PokerTable(), ConsoleColor.DarkGreen);
            BuildAvailablePositions();
            AssignPlayerPositions();
        }

        private void BuildAvailablePositions()
        {
            var midPointX = _width / 2;
            var yPos = (_height - _boardHeight) / 2 / 4;
            _availablePositions = new List<PlayerPosition>();
            _availablePositions.Add(new PlayerPosition { X = (int)(midPointX - _minBoardWidth * .35), Y = yPos, Type = PositionType.Top});
            _availablePositions.Add(new PlayerPosition { X = midPointX - 5, Y = yPos, Type = PositionType.Top });
            _availablePositions.Add(new PlayerPosition { X = (int)(midPointX + _minBoardWidth * .25 ), Y = yPos, Type = PositionType.Top });
            _availablePositions.Add(new PlayerPosition { X = _maxBoardWidth + (_width - _maxBoardWidth) / 8, Y = _height / 2 - (_height - _boardHeight) / 4, Type = PositionType.Right });
            _availablePositions.Add(new PlayerPosition { X = (int)(midPointX - _minBoardWidth * .35), Y = _boardHeight, Type = PositionType.Bottom });
            _availablePositions.Add(new PlayerPosition { X = midPointX - 5, Y = _boardHeight, Type = PositionType.Bottom });
            _availablePositions.Add(new PlayerPosition { X = (int)(midPointX + _minBoardWidth * .25 ), Y = _boardHeight, Type = PositionType.Bottom });
            _availablePositions.Add(new PlayerPosition { X = (_width - _maxBoardWidth) / 4, Y = _height / 2 - (_height - _boardHeight) / 4, Type = PositionType.Left });
        }

        private void AssignPlayerPositions()
        {
            _playerPositions = _players.ToDictionary(s => s.PlayerNum, s => _availablePositions[s.PlayerNum]);
        }

        public void UpdateCommunityCards(Card[] cards)
        {
            var startingX = _width / 2 - 9;
            var startingY = _height / 2 - 1;
            DrawLines(BuildCards(cards, startingX, startingY, true));
        }

        internal void UpdatePots(List<Pot> pots)
        {
            var x = _width / 2 - 30;
            var y = _height / 2 - 1;
            var potDisplay = new List<ConsoleLine>();
            var potNum = 1;
            foreach (var pot in pots)
            {
                potDisplay.Add(new ConsoleLine(x, y, "Pot " + potNum + ": " + pot.Size(), ConsoleColor.White, ConsoleColor.DarkGreen));
                potNum++;
                y++;
            }
            for (var i = 0; i < 4 - pots.Count; i++)
            {
                potDisplay.Add(new ConsoleLine(x, y, "                ", ConsoleColor.White, ConsoleColor.DarkGreen));
                potNum++;
                y++;
            }
            DrawLines(potDisplay);
        }

        public void UpdatePlayerAction(bool isAlive, int playerNum, EActionType act, int amount)
        {
            var pos = _playerPositions[playerNum];
            var x = pos.X;
            var y = pos.Y;
            var action = act.ToString().Replace("Action", "") + "  ";
            var bet = amount + "    ";
            ConsoleLine playerAction;
            ConsoleLine betAmount;
            switch (pos.Type)
            {
                case PositionType.Top:
                    playerAction = new ConsoleLine(x, y + 2, action);
                    betAmount = new ConsoleLine(x, y + 3, bet, ConsoleColor.Green);
                    break;
                case PositionType.Right:
                    playerAction = new ConsoleLine(x + 15, y + 2, action);
                    betAmount = new ConsoleLine(x + 15, y + 3, bet, ConsoleColor.Green);
                    break;
                case PositionType.Bottom:
                    playerAction = new ConsoleLine(x, y + 6, action);
                    betAmount = new ConsoleLine(x, y + 5, bet, ConsoleColor.Green);
                    break;
                case PositionType.Left:
                    playerAction = new ConsoleLine(x, y + 2, action);
                    betAmount = new ConsoleLine(x, y + 3, bet, ConsoleColor.Green);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            DrawLines(new List<ConsoleLine> { playerAction, betAmount }, ConsoleColor.Black, isAlive ? ConsoleColor.Cyan : ConsoleColor.Red);
        }

        internal void UpdatePlayer(ServerHoldemPlayer player)
        {
            var pos = _playerPositions[player.PlayerNum];
            var x = pos.X;
            var y = pos.Y;
            var stack = player.StackSize + "     ";
            ConsoleLine playerName;
            ConsoleLine stackSize;
            var holeCards = player.HoleCards();
            List<ConsoleLine> cards;
            
            switch (pos.Type)
            {
                case PositionType.Top:
                    playerName = new ConsoleLine(x, y, player.Name);
                    stackSize = new ConsoleLine(x, y + 1, stack);
                    cards = BuildCards(holeCards, x, y + 5, player.IsAlive);
                    break;
                case PositionType.Right:
                    playerName = new ConsoleLine(x + 15, y, player.Name);
                    stackSize = new ConsoleLine(x + 15, y + 1, stack);
                    cards = BuildCards(holeCards, x, y, player.IsAlive);
                    break;
                case PositionType.Bottom:
                    playerName = new ConsoleLine(x, y + 7, player.Name);
                    stackSize = new ConsoleLine(x, y + 8, stack);
                    cards = BuildCards(holeCards, x, y, player.IsAlive);
                    break;
                case PositionType.Left:
                    playerName = new ConsoleLine(x, y, player.Name);
                    stackSize = new ConsoleLine(x, y + 1, stack);
                    cards = BuildCards(holeCards, x + 12, y, player.IsAlive);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            DrawLines(new List<ConsoleLine> { playerName, stackSize }, ConsoleColor.Black, ConsoleColor.White);
            DrawLines(cards, ConsoleColor.White, ConsoleColor.Black);
        }

        private static List<ConsoleLine> BuildCards(Card[] holeCards, int x, int y, bool isAlive)
        {
            var cards = new List<ConsoleLine>();
            var xVal = x;
            if (!isAlive)
            {
                cards.AddRange(BlankCard(x, y, ConsoleColor.DarkGreen));
                cards.AddRange(BlankCard(x + 4, y, ConsoleColor.DarkGreen));
            }
            else
            {
                foreach (var card in holeCards)
                {
                    cards.AddRange(card != null ? Card(card.Rank, card.Suit, xVal, y) : BlankCard(xVal, y, ConsoleColor.DarkMagenta));
                    xVal += 4;
                }
            }
            return cards;
        }


        private static void DrawLines(IEnumerable<ConsoleLine> lines, ConsoleColor? bgColor = null, ConsoleColor? fgColor = null)
        {
            var bg = Console.BackgroundColor;
            var fg = Console.ForegroundColor;
            foreach (var line in lines)
            {
                Console.ForegroundColor = line.Color ?? fgColor ?? fg;
                Console.BackgroundColor = line.BackgroundColor ?? bgColor ?? fg;

                Console.SetCursorPosition(line.X, line.Y);
                Console.Write(line.Text);

                Console.BackgroundColor = bgColor ?? fg;
                Console.ForegroundColor = fgColor ?? fg;
            }
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }

        private static IEnumerable<ConsoleLine> Card(ERankType rank, ESuitType suit, int startingX, int startingY)
        {
            var rankSymbol = GetRankSymbol(rank);
            var isDouble = rankSymbol.Length > 1;
            var cardColor = GetSuitColor(suit);
            return new List<ConsoleLine>
            {
                new ConsoleLine(startingX, startingY, rankSymbol + (isDouble ? " " : "  "), cardColor, ConsoleColor.White),
                new ConsoleLine(startingX, startingY + 1, " " + GetSuitSymbol(suit) + " ", cardColor, ConsoleColor.White),
                new ConsoleLine(startingX, startingY + 2, (isDouble ? " " : "  ") + rankSymbol, cardColor, ConsoleColor.White)
            };
        }

        private static IEnumerable<ConsoleLine> BlankCard(int startingX, int startingY, ConsoleColor color)
        {
            return new List<ConsoleLine>
            {
                new ConsoleLine(startingX, startingY, "   ", backgroundColor: color),
                new ConsoleLine(startingX, startingY + 1, "   ", backgroundColor: color),
                new ConsoleLine(startingX, startingY + 2, "   ", backgroundColor: color)
            };
        }

        private static ConsoleColor GetSuitColor(ESuitType suit)
        {
            if (suit == ESuitType.SuitDiamonds || suit == ESuitType.SuitHearts)
            {
                return ConsoleColor.Red;
            }
            return ConsoleColor.Black;
        }

        private static string GetSuitSymbol(ESuitType suit)
        {
            switch (suit)
            {
                case ESuitType.SuitClubs:
                    return "\u2663";
                case ESuitType.SuitHearts:
                    return "\u2665";
                case ESuitType.SuitSpades:
                    return "\u2660";
                case ESuitType.SuitDiamonds:
                    return "\u2666";
                case ESuitType.SuitUnknown:
                    return " ";
                default:
                    throw new ArgumentOutOfRangeException("suit", suit, null);
            }
        }

        private static string GetRankSymbol(ERankType rank)
        {
            switch (rank)
            {
                case ERankType.RankTwo:
                    return "2";
                case ERankType.RankThree:
                    return "3";
                case ERankType.RankFour:
                    return "4";
                case ERankType.RankFive:
                    return "5";
                case ERankType.RankSix:
                    return "6";
                case ERankType.RankSeven:
                    return "7";
                case ERankType.RankEight:
                    return "8";
                case ERankType.RankNine:
                    return "9";
                case ERankType.RankTen:
                    return "10";
                case ERankType.RankJack:
                    return "J";
                case ERankType.RankQueen:
                    return "Q";
                case ERankType.RankKing:
                    return "K";
                case ERankType.RankAce:
                    return "A";
                case ERankType.RankUnknown:
                    return " ";
                default:
                    throw new ArgumentOutOfRangeException("rank", rank, null);
            }
        }

        private IEnumerable<ConsoleLine> PokerTable()
        {
            var lines = new List<ConsoleLine>();
            _boardHeight = Convert.ToInt32(_height * .7);
            var y = _height / 2 - (int)(_boardHeight / 2);
            var linePercent = .7;

            for (var i = 0; i < _boardHeight; i++)
            {
                if (i == 0)
                {
                    _minBoardWidth = Convert.ToInt32(_width  * linePercent);
                }
                lines.Add(new ConsoleLine(_width / 2 - (int)(_width * linePercent / 2), y, RepeatChar(" ", (int)(_width * linePercent))));
                y++;
                if (i < _boardHeight * .15)
                {
                    linePercent += 0.02;
                } else if (i >= _boardHeight * .85)
                {
                    linePercent -= 0.02;
                }
                else
                {
                    _maxBoardWidth = Convert.ToInt32(_width * linePercent);
                }
                
            }
            return lines;
        }

        private static string RepeatChar(string chr, int numChars)
        {
            var str = "";
            for (var i = 0; i < numChars; i++)
            {
                str += chr;
            }
            return str;
        }
    }

    public class ConsoleLine
    {
        public ConsoleLine(int x, int y, string text, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            X = x;
            Y = y;
            Text = text;
            Color = color;
            BackgroundColor = backgroundColor;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        public ConsoleColor? Color { get; set; }
        public ConsoleColor? BackgroundColor { get; set; }
    }
}