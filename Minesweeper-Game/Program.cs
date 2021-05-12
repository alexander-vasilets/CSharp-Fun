using System;
using System.Collections.Generic;

namespace Minesweeper_Game
{
    struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    class Cell
    {
        public char Graph { get; set; }
        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }
        public Cell(char graph, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Graph = graph;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }
    }
    class Program
    {
        private const int WND_WIDTH = 100, WND_HEIGHT = 30;
        private static int width = 0, height = 0, mineCnt = 0;
        private static Position fieldPos = new Position();
        private static char[,] field = null;
        private static Cell[,] currField = null;
        static void Main(string[] args)
        {
            Console.SetWindowSize(WND_WIDTH, WND_HEIGHT);
            Console.SetBufferSize(WND_WIDTH, WND_HEIGHT);
            Console.CursorVisible = false;

            CreateGameField();
            SetMines();
            CalculateDanger();
            PlayGame();
        }
        static void CreateGameField()
        {
            string greeting = "Welcome to \"Minesweeper\"! Choose the size of minefield\n\n";
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.SetCursorPosition((WND_WIDTH - greeting.Length - 2) / 2, 1);
            Console.WriteLine(greeting);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Width: ");
            while (!int.TryParse(Console.ReadLine(), out width) | width < 2 | width > WND_WIDTH - 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"The width should be a number greater than 1 and less than {WND_WIDTH - 2}!");
                Console.WriteLine("Press any key to continue...");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey(true);
                for (int i = 4; i < 7; ++i)
                {
                    Console.SetCursorPosition(0, i);
                    Console.WriteLine(new string(' ', WND_WIDTH));
                }
                Console.SetCursorPosition(0, 4);
                Console.Write("Width: ");
            }
            Console.Write("Height: ");
            while (!int.TryParse(Console.ReadLine(), out height) | height < 2 | height > WND_HEIGHT - 6)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"The height should be a number greater than 1 and less than {WND_HEIGHT - 5}!");
                Console.WriteLine("Press any key to continue...");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey(true);
                Console.SetCursorPosition(0, 5);
                for (int i = 5; i < 8; ++i)
                {
                    Console.SetCursorPosition(0, i);
                    Console.WriteLine(new string(' ', WND_WIDTH));
                }
                Console.SetCursorPosition(0, 5);
                Console.Write("Height: ");
            }
            Console.Clear();
            field = new char[height, width];
        }
        static void DrawField()
        {
            for(int r = 0; r < height; ++r)
            {
                Console.SetCursorPosition(fieldPos.X, fieldPos.Y + r);
                for (int c = 0; c < width; ++c)
                    Console.Write(currField[r, c].Graph);
                Console.WriteLine();
            }
        }
        static void DrawFullField()
        {
            for (int r = 0; r < height; ++r)
            {
                Console.SetCursorPosition(fieldPos.X, fieldPos.Y + r);
                for (int c = 0; c < width; ++c)
                    Console.Write(field[r, c]);
                Console.WriteLine();
            }
        }
        static void DrawCell(Position pos)
        {
            Console.ForegroundColor = currField[pos.Y, pos.X].ForegroundColor;
            Console.BackgroundColor = currField[pos.Y, pos.X].BackgroundColor;
            Console.SetCursorPosition(fieldPos.X + pos.X, fieldPos.Y + pos.Y);
            Console.Write(currField[pos.Y, pos.X].Graph);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
        static void SetMines()
        {
            string title = "How many mines will be there on the field?\n\n";
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.SetCursorPosition((WND_WIDTH - title.Length - 2) / 2, 1);
            Console.WriteLine(title);
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("Number of mines: ");
            while (!int.TryParse(Console.ReadLine(), out mineCnt) | mineCnt < 1 | mineCnt > field.Length)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"The number of mines should be no less than 1 and no more than {field.Length}!");
                Console.WriteLine("Press any key to continue...");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey(true);
                for (int i = 4; i < 7; ++i)
                {
                    Console.SetCursorPosition(0, i);
                    Console.WriteLine(new string(' ', WND_WIDTH));
                }
                Console.SetCursorPosition(0, 4);
                Console.Write("Number of mines: ");
            }
            Console.Clear();

            Random rand = new Random();
            int rowCnt = height;
            int colCnt = width;
            for (int i = 0; i < mineCnt; ++i)
            {
                int r = 0, c = 0;
                do
                {
                    r = rand.Next(0, rowCnt);
                    c = rand.Next(0, colCnt);
                } while (field[r, c] == '*');
                field[r, c] = '*';
            }
        }
        static void CalculateDanger()
        {
            for (int r = 0; r < height; ++r)
            {
                for (int c = 0; c < width; ++c)
                {
                    if (field[r, c] == '*')
                        continue;
                    int mineCnt = 0;
                    if (r > 0)
                    {
                        if (c > 0)
                            if (field[r - 1, c - 1] == '*')
                                ++mineCnt;
                        if (field[r - 1, c] == '*')
                            ++mineCnt;
                        if (c < width - 1)
                            if (field[r - 1, c + 1] == '*')
                                ++mineCnt;
                    }
                    if (r < height - 1)
                    {
                        if (c > 0)
                            if (field[r + 1, c - 1] == '*')
                                ++mineCnt;
                        if (field[r + 1, c] == '*')
                            ++mineCnt;
                        if (c < width - 1)
                            if (field[r + 1, c + 1] == '*')
                                ++mineCnt;
                    }
                    if (c > 0)
                        if (field[r, c - 1] == '*')
                            ++mineCnt;
                    if (c < width - 1)
                        if (field[r, c + 1] == '*')
                            ++mineCnt;
                    if (mineCnt > 0)
                        field[r, c] = mineCnt.ToString()[0];
                    else
                        field[r, c] = '·';
                }
            }
        }
        static void PlayGame()
        {
            string title = "Can you find all the mines?";
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.SetCursorPosition((WND_WIDTH - title.Length - 2) / 2, 1);
            Console.WriteLine(title);

            string tip = "Arrow keys - move, Spacebar - set/delete mark, Enter - reveal";
            Console.SetCursorPosition((WND_WIDTH - tip.Length - 2) / 2, height + 4);
            Console.WriteLine(tip);
            Console.ForegroundColor = ConsoleColor.White;

            currField = new Cell[height, width];
            for (int r = 0; r < height; ++r)
                for (int c = 0; c < width; ++c)
                    currField[r, c] = new Cell('█', ConsoleColor.White, ConsoleColor.Black);

            fieldPos = new Position((WND_WIDTH - width - 2) / 2, 3);
            DrawField();
            Position cursorPos = new Position(0, 0);
            currField[0, 0].ForegroundColor = ConsoleColor.DarkGreen;
            DrawCell(cursorPos);

            bool lost = false, won = false;
            int openedCnt = 0;

            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while(key.Key != ConsoleKey.Escape)
            {
                Cell currCell = null;
                switch(key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (cursorPos.Y > 0)
                            MoveCursor(new Position(cursorPos.X, cursorPos.Y - 1));
                        break;
                    case ConsoleKey.DownArrow:
                        if (cursorPos.Y < height - 1)
                            MoveCursor(new Position(cursorPos.X, cursorPos.Y + 1));
                        break;
                    case ConsoleKey.LeftArrow:
                        if (cursorPos.X > 0)
                            MoveCursor(new Position(cursorPos.X - 1, cursorPos.Y));
                        break;
                    case ConsoleKey.RightArrow:
                        if (cursorPos.X < width - 1)
                            MoveCursor(new Position(cursorPos.X + 1, cursorPos.Y));
                        break;
                    case ConsoleKey.Spacebar:
                        currCell = currField[cursorPos.Y, cursorPos.X];
                        if (currCell.Graph == '█')
                        {
                            currCell.Graph = 'x';
                            currCell.ForegroundColor = ConsoleColor.DarkBlue;
                            currCell.BackgroundColor = ConsoleColor.White;
                        }
                        else if (currCell.Graph == 'x')
                        {
                            currCell.Graph = '█';
                            currCell.ForegroundColor = ConsoleColor.White;
                            currCell.BackgroundColor = ConsoleColor.Black;
                        }
                        break;
                    case ConsoleKey.Enter:                    
                        currCell = currField[cursorPos.Y, cursorPos.X];
                        if (currCell.Graph != 'x')
                        {
                            currCell.Graph = field[cursorPos.Y, cursorPos.X];
                            if (currCell.Graph == '*')
                            {
                                currCell.ForegroundColor = ConsoleColor.Red;
                                DrawCell(cursorPos);
                                lost = true;
                                break;
                            }
                            if (currCell.Graph == '·')
                            {
                                Queue<Position> q = new Queue<Position>();
                                q.Enqueue(cursorPos);
                                List<Position> l = new List<Position>();
                                RevealAllNeighbours(q, l);
                            }
                            currCell.ForegroundColor = ConsoleColor.DarkGreen;
                            DrawCell(cursorPos);
                            ++openedCnt;
                        }
                        break;
                }
                if (openedCnt == currField.Length - mineCnt)
                    won = true;
                if (lost || won)
                    break;
                key = Console.ReadKey(true);

                void MoveCursor(Position newPos)
                {
                    if (currField[cursorPos.Y, cursorPos.X].Graph != 'x')
                        currField[cursorPos.Y, cursorPos.X].ForegroundColor = ConsoleColor.White;
                    else
                        currField[cursorPos.Y, cursorPos.X].ForegroundColor = ConsoleColor.DarkBlue;
                    DrawCell(cursorPos);
                    cursorPos = newPos;
                    currField[cursorPos.Y, cursorPos.X].ForegroundColor = ConsoleColor.DarkGreen;
                    DrawCell(cursorPos);
                }
                void RevealAllNeighbours(Queue<Position> queue, List<Position> list)
                {
                    while (queue.Count != 0)
                    {
                        Position pos = queue.Dequeue();
                        currField[pos.Y, pos.X].Graph = field[pos.Y, pos.X];
                        DrawCell(pos);
                        ++openedCnt;
                        list.Add(pos);

                        if (field[pos.Y, pos.X] == '·')
                        {
                            if (pos.Y > 0)
                            {
                                if (pos.X > 0)
                                {
                                    if (!list.Contains(new Position(pos.X - 1, pos.Y - 1)))
                                        queue.Enqueue(new Position(pos.X - 1, pos.Y - 1));                                    
                                }
                                if (!list.Contains(new Position(pos.X, pos.Y - 1)))
                                    queue.Enqueue(new Position(pos.X, pos.Y - 1));
                                if (pos.X < width - 1)   
                                    if (!list.Contains(new Position(pos.X + 1, pos.Y - 1)))
                                        queue.Enqueue(new Position(pos.X + 1, pos.Y - 1));
                            }
                            if (pos.Y < height - 1)
                            {
                                if (pos.X > 0)
                                {
                                    if (!list.Contains(new Position(pos.X - 1, pos.Y + 1)))
                                        queue.Enqueue(new Position(pos.X - 1, pos.Y + 1));                                   
                                }
                                if (!list.Contains(new Position(pos.X, pos.Y + 1)))
                                    queue.Enqueue(new Position(pos.X, pos.Y + 1));
                                if (pos.X < width - 1)
                                    if (!list.Contains(new Position(pos.X + 1, pos.Y + 1)))
                                        queue.Enqueue(new Position(pos.X + 1, pos.Y + 1));
                            }
                            if (pos.X > 0)
                                if (!list.Contains(new Position(pos.X - 1, pos.Y)))
                                    queue.Enqueue(new Position(pos.X - 1, pos.Y));
                            if (pos.X < width - 1)
                                if (!list.Contains(new Position(pos.X + 1, pos.Y)))
                                    queue.Enqueue(new Position(pos.X + 1, pos.Y));
                        }                       
                    }
                }
            }

            DrawFullField();

            if (lost)
            {
                string lostText = "You've stepped on a mine! You've lost!";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(0, height + 4);
                Console.Write(new string(' ', WND_WIDTH));
                Console.SetCursorPosition((WND_WIDTH - lostText.Length - 2) / 2, height + 4);
                Console.WriteLine(lostText);
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (won)
            {
                string wonText = "You've found all the mines! You've won!";
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.SetCursorPosition(0, height + 4);
                Console.Write(new string(' ', WND_WIDTH));
                Console.SetCursorPosition((WND_WIDTH - wonText.Length - 2) / 2, height + 4);
                Console.WriteLine(wonText);
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.ReadKey(true);
        }
    }
}
