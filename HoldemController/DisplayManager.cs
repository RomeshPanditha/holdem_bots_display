using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HoldemController
{

    public class DisplayManager
    {
        private int _boardHeight;
        private int _minBoardWidth;
        private int _maxBoardWidth;
        private readonly int _width;
        private readonly int _height;
        private readonly int _numPlayers;
        private readonly List<PlayerPosition> _availablePositions;
        private Dictionary<string, PlayerPosition> _playerPositions;

        public class PlayerPosition
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public DisplayManager(int width, int height, int numPlayers)
        {
            Console.SetWindowSize(width, height);
            _width = width;
            _height = height;
            _numPlayers = numPlayers;
            if (numPlayers > 8)
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
            var topYPos = (_height - _boardHeight) / 2 / 4;
            var position1 = new PlayerPosition { X = Convert.ToInt32(midPointX - _minBoardWidth * .3), Y = topYPos };
            var position2 = new PlayerPosition { X = midPointX, Y = topYPos };
            var position3 = new PlayerPosition { X = Convert.ToInt32(midPointX + _minBoardWidth * .3 ), Y = topYPos };
            var line = new ConsoleLine
            {
                X = position1.X,
                Y = position1.Y,
                Text = "Player 1"
            };
            var line2 = new ConsoleLine
            {
                X = position2.X,
                Y = position2.Y,
                Text = "Player 2"
            };
            var line3 = new ConsoleLine
            {
                X = position3.X,
                Y = position3.Y,
                Text = "Player 3"
            };
            DrawLines(new List<ConsoleLine> {line, line2, line3}, ConsoleColor.White, ConsoleColor.Black);

        }
        private void AssignPlayerPositions()
        {

        }

        private void UpdatePlayer(ServerHoldemPlayer player)
        {

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
                lines.Add(new ConsoleLine
                {
                    Y = y,
                    X = _width / 2 - (int)(_width * linePercent / 2),
                    Text = RepeatChar(" ", (int)(_width * linePercent))
                });
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
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
    }
}