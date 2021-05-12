using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Framework
{
    public enum Direction
    {
        UpLeft, Up, UpRight,
        Left, Right,
        DownLeft, Down, DownRight
    }
    public struct Position
    {
        public int x { get; set; }
        public int y { get; set; }
        public Position(int x, int y)
            { this.x = x; this.y = y; }            
        public Position(Position other)
        { x = other.x; y = other.y; }
    }
    public struct Size
    {
        public int width { get; set; }
        public int height { get; set; }
        public Size(int width, int height)
        { this.width = width; this.height = height; }
        public Size(Size other)
        { width = other.width; height = other.height; }
    }
    public class UserInterfaceElement
    {
        public Position position { get; protected set; }
        public Position prevPosition { get; protected set; }
        public Size size { get; protected set; }
        public Size prevSize { get; protected set; }
        protected Size screenSize = new Size(Console.WindowWidth, Console.WindowHeight);
        protected string[] content;
        public ConsoleColor color { get; protected set; }
        public UserInterfaceElement(Position position, Size size, ConsoleColor color = ConsoleColor.White)
            { this.position = this.prevPosition = position; this.size = this.prevSize = size; this.color = color; }

        public string GetContent()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i != content.Length; ++i)
                sb.Append(content[i]);
            return sb.ToString();
        }
        public void SetColor(ConsoleColor color)
            { this.color = color; }
        public void SetSize(Size newSize)
            { prevSize = new Size(size); size = newSize; }
        public void SetPosition(Position newPosition, bool resetPreviousPosition = false)
            { prevPosition = (resetPreviousPosition) ? newPosition : position; position = newPosition; }
        public void SetContent(params string[] content)
            { this.content = content; CheckContentSize(); }
        public void Move(Direction direction, int step, bool resetPreviousPosition = false)
        {
            if (step < 0)
                step = 0;
            switch (direction)
            {
                case Direction.UpLeft: SetPosition(new Position(position.x - step, position.y - step), resetPreviousPosition); break;
                case Direction.Up: SetPosition(new Position(position.x, position.y - step), resetPreviousPosition); break;
                case Direction.UpRight: SetPosition(new Position(position.x + step, position.y - step)); break;
                case Direction.Left: SetPosition(new Position(position.x - step, position.y), resetPreviousPosition); break;
                case Direction.Right: SetPosition(new Position(position.x + step, position.y), resetPreviousPosition); break;
                case Direction.DownLeft: SetPosition(new Position(position.x - step, position.y + step), resetPreviousPosition); break;
                case Direction.Down: SetPosition(new Position(position.x, position.y + step), resetPreviousPosition); break;
                case Direction.DownRight: SetPosition(new Position(position.x + step, position.y + step), resetPreviousPosition); break;
            }
        }
        public void CenterVertically(Size container, Position originPoint)
        {
            SetPosition(new Position(position.x, originPoint.y + (container.height - size.height) / 2));
            prevPosition = position;
        }
        public void CenterHorizontally(Size container, Position originPoint)
        {
            SetPosition(new Position(originPoint.x + (container.width - size.width) / 2, position.y));
            prevPosition = position;
        }
        public void CenterBoth(Size container, Position originPoint)
        {
            SetPosition(new Position(originPoint.x + (container.width - size.width) / 2, originPoint.y + (container.height - size.height) / 2));
            prevPosition = position;
        }
        public void Clear()
        {
            if (content.Length == 0)
                return;
            Position cursor = new Position(prevPosition);
            Console.SetCursorPosition(cursor.x, cursor.y);
            for (int i = 0; i < prevSize.height; ++i)
            {
                Console.Write(new string(' ', prevSize.width));
                if (cursor.y + 1 < screenSize.height)
                    Console.SetCursorPosition(cursor.x, ++cursor.y);
            }
        }
        public void Draw()
        {
            if (content.Length == 0)
                return;
            Position cursor = new Position(position);
            Console.ForegroundColor = color;
            Console.SetCursorPosition(cursor.x, cursor.y);
            for (int i = 0; i < content.Length; ++i)
            {
                Console.Write(content[i]);
                if (cursor.y + 1 < screenSize.height)
                    Console.SetCursorPosition(cursor.x, ++cursor.y);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        public void CheckContentSize()
        {
            List<string> temp = new List<string>();
            int tempSize = 0, maxSize = size.width * size.height;
            for (int i = 0; i != content.Length; ++i)
            {
                if (content[i].Length <= size.width)
                {
                    temp.Add(content[i]);
                    ++tempSize;
                    if (tempSize == size.height)
                        break;
                }
                else
                {
                    int index = 0; bool maxSizeReached = false;
                    while ((content[i].Length - index) > size.width)
                    {
                        temp.Add(content[i].Substring(index, size.width));
                        ++tempSize;
                        if (tempSize == size.height)
                        {
                            maxSizeReached = true;
                            break;
                        }
                        index += size.width;
                    }
                    if (!maxSizeReached)
                        temp.Add(content[i].Substring(index));
                }
            }
            content = temp.ToArray();
        }
    }
    public class Picture : UserInterfaceElement
    {
        public Picture(Position position, ConsoleColor color = ConsoleColor.White) : base(position, new Size(0, 0), color)
            { }
        public void LoadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                int longest = 0;
                using (var sr = new StreamReader(fileName))
                {
                    List<string> temp = new List<string>();
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.Length > longest)
                            longest = s.Length;
                        temp.Add(s);
                    }
                    content = temp.ToArray();
                }
                SetSize(new Size(longest, content.Length));
                prevSize = new Size(size);
            }
        }
    }
    public class TextBlock : UserInterfaceElement
    {
        public TextBlock(Position position, Size size, params string[] content) : base(position, size)
        {
            this.content = content;
            CheckContentSize();
        }
        public void CenterText()
        {
            for (int i = 0; i != content.Length; ++i)
            {
                int space = (size.width - content[i].Length) / 2;
                content[i] = new string(' ', space) + content[i];
            }
        }
    }
    public class Rectangle : UserInterfaceElement
    {
        public char symbol { get; private set; }
        public Rectangle(Position position, Size size, char symbol, ConsoleColor borderColor = ConsoleColor.White) : base(position, size, borderColor)
        { 
            this.symbol = symbol;
            content = new string[size.height];
            content[0] = new string(symbol, size.width);
            for(int i = 1; i < size.height - 1; ++i)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(symbol);
                sb.Append(new string(' ', size.width - 2));
                sb.Append(symbol);
                content[i] = sb.ToString();
            }
            content[size.height - 1] = new string(symbol, size.width);
        }
        public void Normalize()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < size.width / 2; ++i)
                sb.Append($"{symbol} ");
            if (size.width % 2 == 0)
                sb.Remove(sb.Length - 1, 1);
            sb.Append($"{symbol}");           
            content[0] = sb.ToString();
            content[content.Length - 1] = sb.ToString();

        }
    }
    public class InputForm : UserInterfaceElement
    {
        public InputForm(Position position, Size size) : base(position, size)
            { }
        public string ReceiveInput(int length = Int32.MaxValue, params char[] invalidCharacters)
        {
            content = new string[] { "" };
            StringBuilder sb = new StringBuilder();
            Console.SetCursorPosition(position.x, position.y);
            var key = Console.ReadKey(true);
            bool reachedEnd = false;
            while (true)
            {
                if (Array.IndexOf(invalidCharacters, key.KeyChar) != -1)
                {
                    key = Console.ReadKey(true);
                    continue;
                }
                if (!char.IsControl(key.KeyChar) && !reachedEnd)
                    sb.Append(key.KeyChar);
                else if (key.Key == ConsoleKey.Backspace && sb.Length != 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    reachedEnd = false;
                }
                else if (key.Key == ConsoleKey.Escape)
                    sb = new StringBuilder();
                else if (key.Key == ConsoleKey.Enter)
                {
                    if (sb.Length != 0)
                        break;
                }
                content = new string[] { sb.ToString() };          
                CheckContentSize();
                Clear();
                Draw();
                if (sb.Length == length || (sb.Length == size.width * size.height))
                    reachedEnd = true;
                Console.SetCursorPosition(position.x + (sb.Length % size.width), position.y + (sb.Length / size.width));
                key = Console.ReadKey(true);
            }
            content = new string[] { "" };
            Clear();
            Draw();
            return sb.ToString();
        }
    }

    /*public class GameObject
    {
        public ObjectType Type { get; private set; }
        public ConsoleColor Color { get; private set; }
        public char Graph { get; private set; }
        public GameObject(char graph)
        {
            switch(graph)
            {
                case 'I': Type = ObjectType.Player; Color = ConsoleColor.DarkYellow; break;
                case '#': Type = ObjectType.Wall; Color = ConsoleColor.DarkBlue; break;
                case '%': Type = ObjectType.Trap; Color = ConsoleColor.Red; break;
                case '@': Type = ObjectType.Bonus; Color = ConsoleColor.DarkGreen; break;
                case '.': Type = ObjectType.StopPoint; Color = ConsoleColor.White; break;
                case ' ': Type = ObjectType.Empty; Color = ConsoleColor.White; break;
            }
            Graph = graph;
        }
    }*/
    public enum ObjectType
    {
        Player, Wall, Bonus, Trap, StopPoint
    }
    public abstract class GameObject
    {
        public ObjectType Type { get; protected set; }
        public Position Position { get; protected set; }
        public GameObject(Position position) { Position = position; }
        public virtual void Draw()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(Position.x, Position.y);
            Console.Write(' ');
        }
        public void SetPosition(Position newPos)
        {
            Position = newPos;
        }
    }
    public class Player : GameObject
    {
        public Player(Position position) : base(position) { Type = ObjectType.Player; }
        public override void Draw()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.SetCursorPosition(Position.x, Position.y);
            Console.Write('I');
        }
    }
    public class Wall : GameObject
    {
        public Wall(Position position) : base(position) { Type = ObjectType.Wall; }
        public override void Draw()
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.SetCursorPosition(Position.x, Position.y);
            Console.Write('#');
        }
    }
    public class Bonus : GameObject
    {
        public Bonus(Position position) : base(position) { Type = ObjectType.Bonus; }
        public override void Draw()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.SetCursorPosition(Position.x, Position.y);
            Console.Write('@');
        }
    }
    public class Trap : GameObject
    {
        public Trap(Position position) : base(position) { Type = ObjectType.Trap; }
        public override void Draw()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(Position.x, Position.y);
            Console.Write('%');
        }
    }
    public class StopPoint : GameObject
    {
        public StopPoint(Position position) : base(position) { Type = ObjectType.StopPoint; }
        public override void Draw()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(Position.x, Position.y);
            Console.Write('.');
        }
    }
    public class GameField
    {
        public Size Size { get; private set; }
        public Position Position { get; private set; }
        private GameObject[,] Field { get; set; }
        public int MaxScore { get; private set; }
        public Position InitialPlayerPosition { get; private set; }
        public GameField(Position position) 
            { Position = position; Field = new GameObject[0,0]; }

        public GameObject this[int row, int col]
        { 
            get { return Field[row, col]; }
            set { Field[row, col] = value; }
        }
        public void SetPosition(Position newPos)
        { 
            Position = newPos;
            for (int row = 0; row != Field.GetLength(0); ++row)
            {
                for (int col = 0; col != Field.GetLength(1); ++col)
                {
                    Field[row, col]?.SetPosition(new Position(col * 2 + Position.x, row + Position.y));
                }
            }
        }
        public void LoadFromFile(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line = sr.ReadLine();
                string[] meta = line.Split(' ');
                MaxScore = Convert.ToInt32(meta[0]);
                int width = Convert.ToInt32(meta[1]);
                int height = Convert.ToInt32(meta[2]);
                Size = new Size(width, height);
                Field = new GameObject[height, width];

                int row = 0;
                while ((line = sr.ReadLine()) != null && row < Field.GetLength(0))
                {
                    for (int col = 0; col < line.Length && col < Field.GetLength(1) * 2; col += 2)
                    {
                        char graph = line[col];
                        if (graph == 'I')
                            InitialPlayerPosition = new Position(col / 2, row);
                        GameObject obj = null;
                        switch(graph)
                        {
                            case 'I': obj = new Player(new Position(col + Position.x, row + Position.y)); break;
                            case '#': obj = new Wall(new Position(col + Position.x, row + Position.y)); break;
                            case '@': obj = new Bonus(new Position(col + Position.x, row + Position.y)); break;
                            case '%': obj = new Trap(new Position(col + Position.x, row + Position.y)); break;
                            case '.': obj = new StopPoint(new Position(col + Position.x, row + Position.y)); break;
                        }
                        Field[row, col / 2] = obj;
                    }
                    ++row;
                }
            }
        }
        public void Draw() // Нарисовать игровое поле
        {          
            for (int row = 0; row != Field.GetLength(0); ++row) // Для каждого ряда
            {               
                for (int col = 0; col != Field.GetLength(1); ++col) // Для каждого элемента ряда
                {
                    GameObject obj = Field[row, col]; // Получаем игровой объект в данной ячейке
                    /*if (obj == null) // Если ячейка пуста
                    {
                        Console.ForegroundColor = ConsoleColor.White; // То просто 
                        Console.Write("  ");                           // печатаем пробел
                    }
                    else*/
                        Field[row, col]?.Draw(); // Иначе вызываем метод рисования объекта
                }
            }
            Console.ForegroundColor = ConsoleColor.White; // Возвращаем цвет консоли к стандартному
        }
        public void PutObject(GameObject obj, Position pos)
        {
            Field[pos.y, pos.x] = obj;
            obj?.SetPosition(new Position(pos.x * 2 + Position.x, pos.y + Position.y));
            if (obj == null)
            {
                Console.SetCursorPosition(Position.x + pos.x * 2, Position.y + pos.y);
                Console.Write(' ');
            }
            else
                obj.Draw();      
        }
    }
    public class Counter
    {
        public Position position { get; protected set; }
        public ConsoleColor color { get; protected set; }
        public string name { get; protected set; }
        public int counter { get; protected set; }
        public Counter(Position position, string name, int initialValue = 0, ConsoleColor color = ConsoleColor.White)
        { this.position = position; this.name = name; counter = initialValue; this.color = color; }

        public void Set(int value)
        { Clear(); counter = value; }
        public void Update(int change)
        { counter += change; }
        public int Get()
        { return counter; }
        public void Clear()
        {
            Console.SetCursorPosition(position.x, position.y);
            Console.Write(new string(' ', name.Length + counter.ToString().Length + 3));
        }
        public void Draw()
        {
            Console.SetCursorPosition(position.x, position.y);
            Console.ForegroundColor = color;
            Console.Write(name + ": " + counter);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
