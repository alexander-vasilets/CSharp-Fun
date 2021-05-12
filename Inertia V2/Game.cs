using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Threading;
using System.Linq;
using WMPLib;
using Framework;

namespace Inertia_V2
{
    enum ProgramStatus
    {
        Active, Closed
    }
    enum GameStatus
    {
        InProcess, Lost, Won
    }
    enum Window
    {
        Title, LevelSelection, ProfileManager, Game, Exit, Environment
    }
    class Game
    {
        static ProgramStatus Status { get; set; }
        static bool Sound { get; set; }
        static bool Music { get; set; }
        static Size windowSize = new Size(101, 30);      
        static string[] Levels;
        static string CurrLevel { get; set; }
        static List<string> Profiles;
        static string CurrProfile { get; set; }
        static WindowsMediaPlayer music = new WindowsMediaPlayer();
        static WindowsMediaPlayer sound = new WindowsMediaPlayer();
        static string menuChangeSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\menuChange.wav";
        static string menuSelectSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\menuSelect.mp3";
        static string menuBackSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\menuBack.wav";

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        static void Main(string[] args)
        {
            Console.SetWindowSize(windowSize.width, windowSize.height);
            Console.SetBufferSize(windowSize.width, windowSize.height);
            Console.CursorVisible = false;

            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, 0xF030, 0x00000000); 
                DeleteMenu(sysMenu, 0xF000, 0x00000000);
                DeleteMenu(sysMenu, 0xF060, 0x00000000);
                DeleteMenu(sysMenu, 0xF020, 0x00000000);
            }
            
            Status = ProgramStatus.Active;
            Sound = Music = true;                     

            InitializeGameData();

            Window nextWindow = new Window();
           
            Login();
            while (Status == ProgramStatus.Active)
            {
                nextWindow = Title();
                if (nextWindow == Window.Exit)
                {
                    nextWindow = Exit(Window.Title);
                    if (nextWindow == Window.Environment)
                    {
                        Status = ProgramStatus.Closed;
                    }
                }
                else if (nextWindow == Window.LevelSelection)
                {
                    while (true)
                    {
                        nextWindow = LevelSelection();
                        if (nextWindow == Window.Title)
                            break;
                        else
                        {
                            nextWindow = PlayGame(CurrLevel);
                        }
                    }
                }
                else
                {
                    nextWindow = ProfileManager();
                }
            }
            return;
        }
        static void InitializeGameData()
        {
            Levels = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Levels");
            for (int i = 0; i < Levels.Length; ++i)
            {
                int slash = Levels[i].LastIndexOf('\\') + 1;
                int dot = Levels[i].LastIndexOf('.');
                Levels[i] = Levels[i].Substring(slash, dot - slash);
            }
            if (Levels.Length > 10)
            {
                var temp = Levels;
                Levels = new string[10];
                Array.Copy(temp, Levels, 10);
            }
            string[] profilesArray = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Profiles");
            Profiles = new List<string>(profilesArray);
            for (int i = 0; i < Profiles.Count; ++i)
            {
                int slash = Profiles[i].LastIndexOf('\\') + 1;
                int dot = Profiles[i].LastIndexOf('.');
                Profiles[i] = Profiles[i].Substring(slash, dot - slash);
            }
            while (Profiles.Count > 10)
            {
                Profiles.RemoveAt(Profiles.Count - 1);
            }
            if (Profiles.Count == 0)
                return;
            foreach (string profile in profilesArray)
            {
                List<string> progress = new List<string>();
                using (StreamReader reader = new StreamReader(profile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        progress.Add(line);
                    }                   
                }
                List<string> levelNames = new List<string>();
                foreach(string entry in progress)
                {
                    int space = entry.LastIndexOf(' ');
                    string levelName = entry.Substring(0, space);
                    levelNames.Add(levelName);
                }
                int i = 0;
                while (i != levelNames.Count)
                {
                    if (!Levels.Contains(levelNames[i]))
                    {
                        levelNames.RemoveAt(i);
                        progress.RemoveAt(i);
                        continue;
                    }
                    ++i;
                }
                foreach (string level in Levels)
                {
                    if (!levelNames.Contains(level))
                    {
                        progress.Add(level + " 0");
                    }
                }
                progress.Sort();
                File.Delete(profile);                
                File.WriteAllLines(profile, progress);
            }
            string dir = Directory.GetCurrentDirectory() + "\\Highscores";
            string[] highscoresArray = Directory.GetFiles(dir);
            for (int i = 0; i != highscoresArray.Length; ++i)
            {
                int slash = highscoresArray[i].LastIndexOf('\\');
                int space = highscoresArray[i].LastIndexOf('.');
                if (Array.IndexOf(Levels, highscoresArray[i].Substring(slash + 1, space - slash - 1)) == -1)
                    File.Delete(highscoresArray[i]);
            }
        }
        static void ClearHighscores(string[] levels)
        {
            string dir = Directory.GetCurrentDirectory() + "\\Highscores";
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
            for (int i = 0; i != levels.Length; ++i)
            {
                string fileName = dir + $"\\{levels[i]}" + ".txt";
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    for (int k = 0; k != 10; ++k)
                    {
                        sw.WriteLine("--//--   0");
                    }
                }
            }
        }
        static void DeleteProfile(int index)
        {
            string fileName = Directory.GetCurrentDirectory() + "\\Profiles\\" + $"{Profiles[index]}.txt";
            File.Delete(fileName);
            Profiles.RemoveAt(index);
        }
        static void ClearProgress(string profileName)
        {
            string fileName = Directory.GetCurrentDirectory() + "\\Profiles\\" + $"{profileName}.txt";
            File.Delete(fileName);
            using (StreamWriter sw = File.CreateText(fileName))
            {
                foreach(string level in Levels)
                {
                    sw.WriteLine(level + " 0");
                }
            }
        }
        static void Login()
        {
            Console.Clear();

            string loginText = "Введите имя профиля:";
            TextBlock login = new TextBlock(new Position(0, 5), new Size(loginText.Length, 1), loginText);          

            Rectangle frame = new Rectangle(new Position(0, 4), new Size(24, 3), '*', ConsoleColor.Green);
            frame.CenterHorizontally(windowSize, frame.position);
            login.SetPosition(new Position(frame.position.x - loginText.Length - 1, 5));

            string text = "Существующие профили:";
            TextBlock textBlock = new TextBlock(new Position(0, frame.position.y + frame.size.height + 2), new Size(text.Length, 1), text);
            textBlock.CenterHorizontally(windowSize, textBlock.position);
            textBlock.SetColor(ConsoleColor.DarkGreen);

            TextBlock profileNames = new TextBlock(new Position(0, textBlock.position.y + 2), new Size(20, Profiles.Count), Profiles.ToArray());
            profileNames.CenterText();
            profileNames.CenterHorizontally(windowSize, profileNames.position);

            login.Draw(); frame.Draw(); textBlock.Draw(); profileNames.Draw();

            InputForm loginPrompt = new InputForm(new Position(0, 5), new Size(20, 1));
            loginPrompt.CenterHorizontally(frame.size, frame.position);
            char[] invalidCharacters = new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|', ' '};
            bool profileSelected = false;
            string profileName = "";
            while (!profileSelected)
            {
                profileName = loginPrompt.ReceiveInput(20, invalidCharacters);
                if (Profiles.IndexOf(profileName) == -1 && Profiles.Count >= 10)
                {
                    string limitText = "Количество профилей достигло максимума";
                    string limitText2 = "Выберите уже существующий профиль";
                    TextBlock profileLimit = new TextBlock(new Position(0, profileNames.position.y + profileNames.size.height + 2), new Size(limitText.Length, 2), limitText, limitText2);
                    profileLimit.CenterHorizontally(windowSize, profileLimit.position);
                    profileLimit.CenterText();
                    profileLimit.SetColor(ConsoleColor.Red);
                    profileLimit.Draw();
                    Console.ReadKey(true);
                    profileLimit.Clear();
                    continue;
                }
                string inquiryText = "Вы хотите начать игру как " + profileName;
                string inquiryText2 = "(Enter - да, другая клавиша - нет)";
                string longer = (inquiryText.Length > inquiryText2.Length) ? inquiryText : inquiryText2;
                TextBlock inquiry = new TextBlock(new Position(0, profileNames.position.y + profileNames.size.height + 2), new Size(longer.Length, 2), inquiryText, inquiryText2);
                inquiry.SetColor(ConsoleColor.DarkGreen);
                inquiry.CenterHorizontally(windowSize, inquiry.position);
                inquiry.CenterText();
                inquiry.Draw();
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    if (Profiles.IndexOf(profileName) == -1 && Profiles.Count < 10)
                    {
                        using (StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "\\Profiles\\" + profileName + ".txt"))
                        {
                            for (int i = 0; i != Levels.Length; ++i)
                            {
                                sw.WriteLine(Levels[i] + " " + 0);                                
                            }
                        }
                        bool inserted = false;
                        for (int i = 0; i != Profiles.Count; ++i)
                        {
                            if (string.Compare(Profiles[i], profileName) == 1)
                            {
                                Profiles.Insert(i, profileName);
                                inserted = true;
                                break;
                            }
                        }
                        if (!inserted)
                            Profiles.Insert(Profiles.Count, profileName);
                    }
                    CurrProfile = profileName;
                    profileSelected = true;
                    sound.URL = menuSelectSoundFile;
                }
                inquiry.Clear();
            }           
        }
        static Window Exit(Window prevWindow)
        {
            Console.Clear();
            music.settings.mute = true;
            string menuChangeSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\menuChange.wav";
            string menuSelectSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\menuSelect.mp3";

            Rectangle frame = new Rectangle(new Position(0, 0), new Size(41, 10), '*', ConsoleColor.Red);
            frame.CenterBoth(windowSize, new Position(0, 0));
            frame.Normalize();
            string inquiryText = "Вы действительно хотите выйти?";
            string inquiryText2 = "Все изменения будут сохранены";
            TextBlock inquiry = new TextBlock(frame.position, new Size(inquiryText.Length, 2), inquiryText, inquiryText2);
            inquiry.Move(Direction.DownRight, 1, true);
            inquiry.CenterHorizontally(frame.size, frame.position);
            string yesText = "Да, выйти";
            TextBlock yes = new TextBlock(new Position(inquiry.position), new Size(yesText.Length, 1), yesText);
            yes.Move(Direction.Down, 4, true);
            yes.CenterHorizontally(frame.size, frame.position);
            yes.SetColor(ConsoleColor.Red);
            string noText = "Нет, продолжить";
            TextBlock no = new TextBlock(new Position(yes.position), new Size(noText.Length, 1), noText);
            no.Move(Direction.Down, 2, true);
            no.CenterHorizontally(frame.size, frame.position);

            frame.Draw(); inquiry.Draw(); yes.Draw(); no.Draw();

            UserInterfaceElement[] buttons = new UserInterfaceElement[2] { yes, no };
            int currBtn = 0;
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while (true)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.DownArrow)
                {
                    if (currBtn < 1)
                    {
                        buttons[currBtn].SetColor(ConsoleColor.White);
                        buttons[++currBtn].SetColor(ConsoleColor.Green);
                        sound.URL = menuChangeSoundFile;
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (currBtn > 0)
                    {
                        buttons[currBtn].SetColor(ConsoleColor.White);
                        buttons[--currBtn].SetColor(ConsoleColor.Red);
                        sound.URL = menuChangeSoundFile;
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    sound.URL = menuSelectSoundFile;
                    if (currBtn == 0)
                        return Window.Environment;
                    else
                        return prevWindow;                    
                }
                yes.Draw();
                no.Draw();
            }
        }
        static Window Title()
        {
            Console.Clear();
            if (Music)
                music.settings.mute = false;
            if (!(music.playState == WMPPlayState.wmppsPlaying))
            {
                string musicFile = Directory.GetCurrentDirectory() + "\\Resources\\goldenWind.mp3";
                music.URL = musicFile;
                music.settings.setMode("loop", true);
                music.settings.volume = 50;  
            }
            string menuChangeSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\menuChange.wav";
            string menuSelectSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\menuSelect.mp3";

            Picture logo = new Picture(new Position(0, 3));
            logo.SetColor(ConsoleColor.Green);
            logo.LoadFromFile("Resources\\logo.txt");
            logo.CenterHorizontally(windowSize, logo.position);
            logo.Draw();

            string welcomeText = "Добро пожаловать, " + CurrProfile + "!";
            TextBlock welcome = new TextBlock(new Position(0, 15), new Size(welcomeText.Length, 1), welcomeText);
            welcome.SetColor(ConsoleColor.Green);
            welcome.CenterHorizontally(windowSize, welcome.position);
            welcome.Draw();

            string newGameText = "Начать игру";
            TextBlock newGame = new TextBlock(new Position(0, 18), new Size(newGameText.Length, 1), newGameText);
            newGame.CenterHorizontally(windowSize, newGame.position);
            newGame.SetColor(ConsoleColor.DarkYellow);
            newGame.Draw();

            string profileManagerText = "Управление профилями";
            TextBlock profileManager = new TextBlock(new Position(0, 20), new Size(profileManagerText.Length, 1), profileManagerText);
            profileManager.CenterHorizontally(windowSize, profileManager.position);
            profileManager.Draw();

            string clearHighscoresText = "Очистить рекорды";
            TextBlock clearHighscores = new TextBlock(new Position(0, 22), new Size(clearHighscoresText.Length, 1), clearHighscoresText);
            clearHighscores.CenterHorizontally(windowSize, clearHighscores.position);
            clearHighscores.Draw();

            string enableSoundText = "Вкл/Выкл звук";
            TextBlock enableSound = new TextBlock(new Position(0, 24), new Size(enableSoundText.Length, 1), enableSoundText);
            enableSound.CenterHorizontally(windowSize, enableSound.position);
            enableSound.Draw();

            string enableMusicText = "Вкл/Выкл музыку";
            TextBlock enableMusic = new TextBlock(new Position(0, 26), new Size(enableMusicText.Length, 1), enableMusicText);
            enableMusic.CenterHorizontally(windowSize, enableMusic.position);
            enableMusic.Draw();

            string exitGameText = "Выйти из игры";
            TextBlock exitGame = new TextBlock(new Position(0, 28), new Size(exitGameText.Length, 1), exitGameText);
            exitGame.CenterHorizontally(windowSize, exitGame.position);
            exitGame.Draw();

            TextBlock highscoresCleared = new TextBlock(new Position(clearHighscores.position), new Size(30, 1), "Все рекорды были удалены");
            highscoresCleared.Move(Direction.Right, clearHighscores.size.width + 3, true);
            highscoresCleared.SetColor(ConsoleColor.Red);

            TextBlock soundChanged = new TextBlock(new Position(enableSound.position), new Size(30, 1), "Звук включен");
            soundChanged.Move(Direction.Right, enableSound.size.width + 3, true);
            soundChanged.SetColor(ConsoleColor.DarkGreen);

            TextBlock musicChanged = new TextBlock(new Position(enableMusic.position), new Size(30, 1), "Музыка включена");
            musicChanged.Move(Direction.Right, enableMusic.size.width + 3, true);
            musicChanged.SetColor(ConsoleColor.DarkGreen);


            System.Timers.Timer animationTimer = new System.Timers.Timer(1000);
            animationTimer.Enabled = true;
            animationTimer.AutoReset = true;
            bool up = false;
            bool logoIsDrawing = false;
            animationTimer.Elapsed += Animation;
            animationTimer.Start();

            TextBlock[] buttons = new TextBlock[] { newGame, profileManager, clearHighscores, enableSound, enableMusic, exitGame };
            int currBtn = 0;
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            bool stop = false;
            Window nextWindow = new Window();
            while (!stop)
            {
                key = Console.ReadKey(true);
                highscoresCleared.Clear();
                if (key.Key == ConsoleKey.DownArrow)
                {
                    if (currBtn < 5)
                    {
                        buttons[currBtn].SetColor(ConsoleColor.White);
                        buttons[++currBtn].SetColor(ConsoleColor.DarkYellow);
                        sound.URL = menuChangeSoundFile;
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (currBtn > 0)
                    {
                        buttons[currBtn].SetColor(ConsoleColor.White);
                        buttons[--currBtn].SetColor(ConsoleColor.DarkYellow);
                        sound.URL = menuChangeSoundFile;
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    sound.URL = menuSelectSoundFile;
                    switch (currBtn)
                    {                        
                        case 0: nextWindow = Window.LevelSelection; stop = true; break;
                        case 1: nextWindow = Window.ProfileManager; stop = true; break;
                        case 2: ClearHighscores(Levels); highscoresCleared.Draw(); break;
                        case 3: 
                            Sound = !Sound; 
                            soundChanged.SetContent((Sound) ? "Звук включен" : "Звук выключен");
                            soundChanged.SetColor((Sound) ? ConsoleColor.DarkGreen : ConsoleColor.Red);
                            soundChanged.Clear(); soundChanged.Draw();
                            sound.settings.mute = (Sound) ? false : true;
                            break;
                        case 4:
                            Music = !Music;
                            musicChanged.SetContent((Music) ? "Музыка включена" : "Музыка выключена");
                            musicChanged.SetColor((Music) ? ConsoleColor.DarkGreen : ConsoleColor.Red);
                            musicChanged.Clear(); musicChanged.Draw();
                            music.settings.mute = (Music) ? false : true;
                            break;
                        case 5: sound.URL = menuBackSoundFile; nextWindow = Window.Exit; stop = true; break;
                    }

                }
                while (logoIsDrawing) ;
                foreach (UserInterfaceElement button in buttons)
                    button.Draw();
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
            }

            animationTimer.Stop();
            animationTimer.Dispose();
            return nextWindow;

            void Animation(object obj, ElapsedEventArgs e)
            {
                logoIsDrawing = true;
                if (up)
                {
                    logo.Move(Direction.Up, 1);
                    up = false;
                }
                else
                {
                    logo.Move(Direction.Down, 1);
                    up = true;
                }
                Random rand = new Random();
                logo.SetColor((ConsoleColor)rand.Next(1, 15));
                logo.Clear();
                logo.Draw();
                logoIsDrawing = false;
            }
        }
        static Window LevelSelection()
        {
            Console.Clear();           

            Rectangle frame = new Rectangle(new Position(0, 2), new Size(41, 5), '*', ConsoleColor.DarkYellow);
            frame.CenterHorizontally(windowSize, frame.position);
            frame.Normalize();
            frame.Draw();

            string titleText = "Выберите уровень из списка";
            TextBlock title = new TextBlock(new Position(0, 4), new Size(titleText.Length, 1), titleText);
            title.CenterHorizontally(windowSize, title.position);
            title.SetColor(ConsoleColor.DarkYellow);
            title.Draw();

            List<TextBlock> levelList = new List<TextBlock>();
            List<TextBlock> scoreList = new List<TextBlock>();
            string dir = Directory.GetCurrentDirectory();
            using (StreamReader sr = new StreamReader(dir + "\\Profiles\\" + CurrProfile + ".txt"))
            {
                for (int i = 0; i != Levels.Length; ++i)
                {
                    string line = sr.ReadLine();
                    int space = line.LastIndexOf(' ');
                    TextBlock level = new TextBlock(new Position(38, i * 2 + 9), new Size(20, 1), Levels[i]);
                    levelList.Add(level);
                    level.Draw();
                    string scoreText = line.Substring(space + 1);
                    int currentScore = Convert.ToInt32(scoreText);
                    int maxScore;
                    using (StreamReader reader = new StreamReader(dir + "\\Levels\\" + Levels[i] + ".txt"))
                    {
                        string firstLine = reader.ReadLine();
                        string[] meta = firstLine.Split(' ');
                        maxScore = Convert.ToInt32(meta[0]);
                    }
                    scoreText += "/" + maxScore;
                    TextBlock score = new TextBlock(new Position(59, i * 2 + 9), new Size(10, 1), scoreText);
                    scoreList.Add(score);
                    score.Draw();
                    if (currentScore == maxScore)
                    {
                        TextBlock completed = new TextBlock(new Position(70, i * 2 + 9), new Size(20, 1), "Уровень пройден!");
                        completed.SetColor(ConsoleColor.DarkGreen);
                        completed.Draw();
                    }
                }
                
            }

            int currBtn = 0;
            levelList[0].SetColor(ConsoleColor.DarkGreen);
            scoreList[0].SetColor(ConsoleColor.DarkGreen);
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while (true)
            {
                foreach (TextBlock level in levelList)
                    level.Draw();
                foreach (TextBlock score in scoreList)
                    score.Draw();
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.DownArrow)
                {
                    if (currBtn < levelList.Count - 1)
                    {
                        sound.URL = menuChangeSoundFile;
                        levelList[currBtn].SetColor(ConsoleColor.White);
                        scoreList[currBtn].SetColor(ConsoleColor.White);
                        levelList[++currBtn].SetColor(ConsoleColor.DarkGreen);
                        scoreList[currBtn].SetColor(ConsoleColor.DarkGreen);
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (currBtn > 0)
                    {
                        sound.URL = menuChangeSoundFile;
                        levelList[currBtn].SetColor(ConsoleColor.White);
                        scoreList[currBtn].SetColor(ConsoleColor.White);
                        levelList[--currBtn].SetColor(ConsoleColor.DarkGreen);
                        scoreList[currBtn].SetColor(ConsoleColor.DarkGreen);
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    sound.URL = menuSelectSoundFile;
                    CurrLevel = Levels[currBtn];
                    return Window.Game;
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    sound.URL = menuBackSoundFile;
                    return Window.Title;
                }
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
            }
        }
        static Window PlayGame(string levelName)
        {
            Console.Clear();
            string wallSmashSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\wallSmash.wav";
            string stopSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\stop.wav";
            string bonusPickupSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\bonusPickup.wav";
            string victorySoundFile = Directory.GetCurrentDirectory() + "\\Resources\\victory.wav";
            string failureSoundFile = Directory.GetCurrentDirectory() + "\\Resources\\failure.wav";

            string[] tips = { "Используйте Numpad1-Numpad9 для движения и R - для рестарта", "Условные обозначения:", "I - игрок", "# - стена", ". - точка остановки", "% - ловушка", "@ - бонус" };
            TextBlock help = new TextBlock(new Position(0, 1), new Size(70, tips.Length), tips);
            help.SetColor(ConsoleColor.DarkGreen);
            help.Draw();

            GameField field = new GameField(new Position(10, 10));           
            field.LoadFromFile(Directory.GetCurrentDirectory() + "\\Levels\\" + levelName + ".txt");
            field.SetPosition(new Position((70 - (field.Size.width * 2 - 1)) / 2, (30 - field.Size.height) / 2 + 2));
            field.Draw();

            Rectangle frame = new Rectangle(new Position(70, 2), new Size(27, 25), '*', ConsoleColor.DarkGreen);
            frame.Normalize();
            frame.Draw();

            TextBlock highscores = new TextBlock(new Position(frame.position.x + 1, frame.position.y + 2), new Size(25, 22));

            string fileName = Directory.GetCurrentDirectory() + "\\Highscores\\" + levelName + ".txt";
            if (!File.Exists(fileName))
            {
                using (StreamWriter sr = File.CreateText(fileName))
                {
                    for (int i = 0; i != 10; ++i)
                    {
                        sr.WriteLine("--//--   0");
                    }
                }
            }
            List<string> highscoresText = new List<string>();
            highscoresText.Add("Лучший счет");
            if (File.Exists(fileName))
            {               
                using (StreamReader sr = new StreamReader(fileName))
                {
                    for (int i = 1; i <= 10; ++i)
                    {
                        highscoresText.Add(new string(' ', 25));
                        highscoresText.Add(sr.ReadLine());                        
                    }
                }
            }
            else
            {              
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    for (int i = 1; i < 11; ++i)
                    {
                        sw.WriteLine("--//--   0");                   
                    }
                }
                for (int i = 1; i < 21; ++i)
                {
                    highscoresText[i] = new string(' ', 25);
                    highscoresText[++i] = "--//--   0";
                }
            }
            highscores.SetContent(highscoresText.ToArray());
            highscores.CenterText();
            highscores.Draw();

            Counter score = new Counter(new Position(25, field.Position.y + field.Size.height + 2), "Счет", 0, ConsoleColor.DarkGreen);
            Counter turns = new Counter(new Position(37, field.Position.y + field.Size.height + 2), "Ходы", 0, ConsoleColor.DarkBlue);
            score.Draw(); turns.Draw();

            Position playerPos = field.InitialPlayerPosition;
            GameObject prevObject = null;

            int bestScore = 0;

            GameStatus gameStatus = GameStatus.InProcess;

            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while (true)
            {
                while (true)
                {
                    key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.NumPad7: Move(-1, -1); break;
                        case ConsoleKey.NumPad8: Move(0, -1); break;
                        case ConsoleKey.NumPad9: Move(1, -1); break;
                        case ConsoleKey.NumPad4: Move(-1, 0); break;
                        case ConsoleKey.NumPad6: Move(1, 0); break;
                        case ConsoleKey.NumPad1: Move(-1, 1); break;
                        case ConsoleKey.NumPad2: Move(0, 1); break;
                        case ConsoleKey.NumPad3: Move(1, 1); break;
                        case ConsoleKey.R: UpdateHighscore(); Restart(); break;
                    }
                    if (key.Key == ConsoleKey.Escape)
                    {
                        UpdateHighscore();
                        SaveProgress();
                        sound.URL = menuBackSoundFile;
                        return Window.LevelSelection;
                    }
                    while (Console.KeyAvailable)
                        Console.ReadKey(true);
                    if (gameStatus == GameStatus.Lost)
                    {                      
                        break;
                    }
                    if (score.Get() == field.MaxScore)
                    {
                        gameStatus = GameStatus.Won;
                        break;
                    }
                }
                if (gameStatus == GameStatus.Lost)
                {
                    sound.URL = failureSoundFile;
                    help.Clear();
                    if (bestScore < score.Get())
                        bestScore = score.Get();
                    TextBlock lost = new TextBlock(new Position(0, 0), new Size(42, 2), "Вы проиграли!", "Начать заново? (Enter - да, Escape - нет)");
                    lost.CenterHorizontally(new Size(70, 2), lost.position);
                    lost.SetPosition(new Position(lost.position.x, field.Position.y - 4), true);
                    lost.SetColor(ConsoleColor.Red);
                    lost.CenterText();
                    lost.Draw();
                    UpdateHighscore();
                    key = Console.ReadKey(true);
                    while (key.Key != ConsoleKey.Enter)
                    {
                        if (key.Key == ConsoleKey.Escape)
                        {
                            SaveProgress();
                            return Window.LevelSelection;
                        }
                        key = Console.ReadKey(true);
                    }
                    lost.Clear();
                    Restart();
                }
                else if (gameStatus == GameStatus.Won)
                {
                    sound.URL = victorySoundFile;
                    help.Clear();
                    bestScore = field.MaxScore;
                    TextBlock won = new TextBlock(new Position(0, 0), new Size(33, 2), "Вы выиграли!", "Нажмите любую клавишу чтобы выйти");
                    won.CenterHorizontally(new Size(70, 2), won.position);
                    won.SetPosition(new Position(won.position.x, field.Position.y - 4), true);
                    won.CenterText();
                    won.SetColor(ConsoleColor.DarkGreen);
                    won.Draw();
                    key = Console.ReadKey(true);
                    UpdateHighscore();
                    SaveProgress();
                    return Window.LevelSelection;
                }
            }

            void Move(int moveX, int moveY)
            {
                while (true)
                {
                    Position newPos = new Position(playerPos.x + moveX, playerPos.y + moveY);
                    if (newPos.x >= 0 && newPos.x < field.Size.width && newPos.y >= 0 && newPos.y < field.Size.height)
                    {
                        GameObject currObject = field[newPos.y, newPos.x];
                        if (currObject?.Type == ObjectType.Wall)
                        {
                            sound.URL = wallSmashSoundFile;
                            break;
                        }
                        else if (currObject?.Type == ObjectType.Bonus)
                        {
                            sound.URL = bonusPickupSoundFile;
                            score.Update(1);
                            score.Draw();
                            currObject = null;
                        }
                        else if (currObject?.Type == ObjectType.Trap)
                            gameStatus = GameStatus.Lost;
                        else if (currObject?.Type == ObjectType.StopPoint)
                            sound.URL = stopSoundFile;
                        field.PutObject(prevObject, playerPos);
                        playerPos = newPos;
                        field.PutObject(new Player(new Position(playerPos.x * 2 + field.Position.x * 2, playerPos.y + field.Position.y)), playerPos);
                        prevObject = currObject;                      
                        if (prevObject?.Type == ObjectType.StopPoint || prevObject?.Type == ObjectType.Trap)
                            break;
                    }
                    else
                        break;                   
                    Thread.Sleep(100);
                }
                turns.Update(1);
                turns.Draw();
            }
            void UpdateHighscore()
            {
                if (score.Get() == 0)
                    return;
                for (int i = 0; i != 10; ++i)
                {
                    int index = i * 2 + 2;
                    int space = highscoresText[index].LastIndexOf(' ');
                    int highScore = Convert.ToInt32(highscoresText[index].Substring(space + 1));
                    string record = CurrProfile + "   " + score.Get();
                    if (highscoresText.IndexOf(record) != -1)
                        break;
                    if (score.Get() >= highScore)
                    {
                        highscoresText.Insert(index, new string(' ', 25));
                        highscoresText.Insert(index, record);                       
                        highscoresText.RemoveRange(highscoresText.Count - 2, 2);
                        break;
                    }
                }
            }
            void SaveProgress()
            {
                string highscoreFile = Directory.GetCurrentDirectory() + "\\Highscores\\" + levelName + ".txt";
                using (StreamWriter sw = new StreamWriter(highscoreFile, false))
                {
                    for (int i = 2; i <= 20; i += 2)
                        sw.WriteLine(highscoresText[i]);
                }
                string profileFile = Directory.GetCurrentDirectory() + "\\Profiles\\" + CurrProfile + ".txt";
                string[] levels = new string[Levels.Length];
                using (StreamReader sr = new StreamReader(profileFile))
                {
                    string line;
                    int index = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        levels[index] = line;
                        ++index;
                    }
                }
                for (int i = 0; i != levels.Length; ++i)
                {
                    int space = levels[i].LastIndexOf(' ');
                    if (levels[i].Substring(0, space) == levelName && Convert.ToInt32(levels[i].Substring(space + 1)) < bestScore)
                    {
                        levels[i] = levelName + " " + bestScore;
                        break;
                    }
                }
                using (StreamWriter sw = new StreamWriter(profileFile, false))
                {
                    for (int i = 0; i != levels.Length; ++i)
                        sw.WriteLine(levels[i]);
                }
            }
            void Restart()
            {
                help.Draw();
                field.LoadFromFile(Directory.GetCurrentDirectory() + "\\Levels\\" + levelName + ".txt");
                field.Draw();
                playerPos = field.InitialPlayerPosition;
                prevObject = null;
                gameStatus = GameStatus.InProcess;
                highscores.Clear();
                highscores.SetContent(highscoresText.ToArray());
                highscores.CenterText();
                highscores.Draw();
                score.Set(0);
                score.Draw();
                turns.Set(0);
                turns.Draw();
            }
        }
        static Window ProfileManager()
        {
            Console.Clear();

            Rectangle frame = new Rectangle(new Position(0, 2), new Size(47, 5), '*', ConsoleColor.DarkYellow);
            frame.CenterHorizontally(windowSize, frame.position);
            frame.Normalize();
            frame.Draw();

            string titleText = "Удаление профилей или очистка прогресса";
            TextBlock title = new TextBlock(new Position(0, 4), new Size(titleText.Length, 1), titleText);
            title.CenterHorizontally(windowSize, title.position);
            title.SetColor(ConsoleColor.DarkYellow);
            title.Draw();

            List<TextBlock> profileList = new List<TextBlock>();
            List<TextBlock> clearList = new List<TextBlock>();
            List<TextBlock> deleteList = new List<TextBlock>();

            for (int i = 0; i != Profiles.Count; ++i)
            {
                TextBlock profile = new TextBlock(new Position(20, i * 2 + 9), new Size(20, 1), Profiles[i]);             
                profileList.Add(profile);
                profile.Draw();

                TextBlock clear = new TextBlock(new Position(41, i * 2 + 9), new Size(20, 1), "Очистить прогресс");
                clear.SetColor(ConsoleColor.Black);
                clearList.Add(clear);
                clear.Draw();

                TextBlock delete = new TextBlock(new Position(62, i * 2 + 9), new Size(32, 1), "Удалить профиль");
                delete.SetColor(ConsoleColor.Black);
                deleteList.Add(delete);
                delete.Draw();
            }

            int index = Profiles.IndexOf(CurrProfile);
            deleteList[index].SetContent("Нельзя удалить активный профиль!");

            int currBtn = 0, currCol = 0;
            profileList[0].SetColor(ConsoleColor.DarkGreen);
            profileList[0].Draw();
            clearList[0].SetColor(ConsoleColor.White);
            clearList[0].Draw();
            deleteList[0].SetColor(ConsoleColor.White);
            deleteList[0].Draw();

            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while (true)
            {             
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.DownArrow && currCol == 0)
                {
                    if (currBtn < profileList.Count - 1)
                    {
                        sound.URL = menuChangeSoundFile;
                        profileList[currBtn].SetColor(ConsoleColor.White);
                        clearList[currBtn].SetColor(ConsoleColor.Black);
                        deleteList[currBtn].SetColor(ConsoleColor.Black);
                        profileList[++currBtn].SetColor(ConsoleColor.DarkGreen);
                        clearList[currBtn].SetColor(ConsoleColor.White);
                        deleteList[currBtn].SetColor(ConsoleColor.White);
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow && currCol == 0)
                {
                    if (currBtn > 0)
                    {
                        sound.URL = menuChangeSoundFile;
                        profileList[currBtn].SetColor(ConsoleColor.White);
                        clearList[currBtn].SetColor(ConsoleColor.Black);
                        deleteList[currBtn].SetColor(ConsoleColor.Black);
                        profileList[--currBtn].SetColor(ConsoleColor.DarkGreen);
                        clearList[currBtn].SetColor(ConsoleColor.White);
                        deleteList[currBtn].SetColor(ConsoleColor.White);
                    }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    switch (currCol)
                    {
                        case 0: profileList[currBtn].SetColor(ConsoleColor.White); clearList[currBtn].SetColor(ConsoleColor.Red); ++currCol; break;
                        case 1: clearList[currBtn].SetColor(ConsoleColor.White); deleteList[currBtn].SetColor(ConsoleColor.Red); ++currCol; break;
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    switch (currCol)
                    {
                        case 1: clearList[currBtn].SetColor(ConsoleColor.White); profileList[currBtn].SetColor(ConsoleColor.DarkGreen); --currCol; break;
                        case 2: deleteList[currBtn].SetColor(ConsoleColor.White); clearList[currBtn].SetColor(ConsoleColor.Red); --currCol; break;
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    sound.URL = menuSelectSoundFile;
                    if (currCol == 1 && clearList[currBtn].GetContent() != "Прогресс очищен")
                    {
                        ClearProgress(Profiles[currBtn]);
                        clearList[currBtn].Clear();
                        clearList[currBtn].SetContent("Прогресс очищен");
                        clearList[currBtn].SetColor(ConsoleColor.DarkGreen);
                    }
                    if (currCol == 2 && deleteList[currBtn].GetContent() != "Нельзя удалить активный профиль!")
                    {
                        DeleteProfile(currBtn);
                        Console.Clear();
                        frame.Draw();
                        title.Draw();
                        profileList.RemoveAt(currBtn);
                        clearList.RemoveAt(currBtn);
                        deleteList.RemoveAt(currBtn);
                        profileList[0].SetColor(ConsoleColor.Green);
                        clearList[0].SetColor(ConsoleColor.White);
                        deleteList[0].SetColor(ConsoleColor.White);
                        currBtn = 0; currCol = 0;
                        for (int i = 0; i != profileList.Count; ++i)
                        {
                            profileList[i].SetPosition(new Position(20, i * 2 + 9));
                            clearList[i].SetPosition(new Position(41, i * 2 + 9));
                            deleteList[i].SetPosition(new Position(62, i * 2 + 9));
                        }
                    }
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    sound.URL = menuBackSoundFile;
                    return Window.Title;
                }
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
                for (int i = 0; i != profileList.Count; ++i)
                {
                    profileList[i].Draw();
                    clearList[i].Draw();
                    deleteList[i].Draw();
                }
            }
        }
    }
}
