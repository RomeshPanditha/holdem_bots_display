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
            _availablePositions.Add(new PlayerPosition { X = Convert.ToInt32(midPointX - _minBoardWidth * .35), Y = yPos, Type = PositionType.Top});
            _availablePositions.Add(new PlayerPosition { X = Convert.ToInt32(midPointX), Y = yPos, Type = PositionType.Top });
            _availablePositions.Add(new PlayerPosition { X = Convert.ToInt32(midPointX + _minBoardWidth * .25 ), Y = yPos, Type = PositionType.Top });
            _availablePositions.Add(new PlayerPosition { X = Convert.ToInt32(_maxBoardWidth + (_width - _maxBoardWidth) / 8), Y = _height / 2 - (_height - _boardHeight) / 4, Type = PositionType.Right });
            _availablePositions.Add(new PlayerPosition { X = Convert.ToInt32(midPointX - _minBoardWidth * .35), Y = _boardHeight, Type = PositionType.Bottom });
            _availablePositions.Add(new PlayerPosition { X = Convert.ToInt32(midPointX), Y = _boardHeight, Type = PositionType.Bottom });
            _availablePositions.Add(new PlayerPosition { X = Convert.ToInt32(midPointX + _minBoardWidth * .25 ), Y = _boardHeight, Type = PositionType.Bottom });
            _availablePositions.Add(new PlayerPosition { X = Convert.ToInt32((_width - _maxBoardWidth) / 4), Y = _height / 2 - (_height - _boardHeight) / 4, Type = PositionType.Left });
        }
        private void AssignPlayerPositions()
        {
            _playerPositions = _players.ToDictionary(s => s.PlayerNum, s => _availablePositions[s.PlayerNum]);
        }

        internal void UpdatePlayer(ServerHoldemPlayer player)
        {
            var pos = _playerPositions[player.PlayerNum];
            var x = pos.X;
            var y = pos.Y;
            ConsoleLine playerName;
            ConsoleLine stackSize;
            ConsoleLine currentBet;
            switch (pos.Type)
            {
                case PositionType.Top:
                    playerName = new ConsoleLine(x, y, player.Name);
                    y++;
                    stackSize = new ConsoleLine(x, y, player.StackSize.ToString());
                    break;
                case PositionType.Right:
                    x += 12;
                    playerName = new ConsoleLine(x, y, player.Name);
                    y++;
                    stackSize = new ConsoleLine(x, y, player.StackSize.ToString());
                    break;
                case PositionType.Bottom:
                    y += 7;
                    playerName = new ConsoleLine(x, y, player.Name);
                    y++;
                    stackSize = new ConsoleLine(x, y, player.StackSize.ToString());
                    break;
                case PositionType.Left:
                    playerName = new ConsoleLine(x, y, player.Name);
                    y++;
                    stackSize = new ConsoleLine(x, y, player.StackSize.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DrawLines(new List<ConsoleLine> { playerName, stackSize }, ConsoleColor.Black, ConsoleColor.White);
        }

        private static void DrawLines(IEnumerable<ConsoleLine> lines, ConsoleColor? bgColor = null, ConsoleColor? fgColor = null)
        {
            var bg = Console.BackgroundColor;
            var fg = Console.ForegroundColor;
            Console.BackgroundColor = bgColor ?? fg;
            Console.ForegroundColor = fgColor ?? fg;
            foreach (var line in lines)
            {
                Console.SetCursorPosition(line.X, line.Y);
                Console.Write(line.Text);
            }
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
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
        public ConsoleLine(int x, int y, string text)
        {
            X = x;
            Y = y;
            Text = text;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
    }
}