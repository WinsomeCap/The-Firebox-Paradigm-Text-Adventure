using System.Text.Json.Nodes;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using static The_Firebox_Paradigm_Revamped.Program.Game;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace The_Firebox_Paradigm_Revamped
{
    internal class Program
    {

        /* TODO:
     	* [x] - NON-TERMINATING NONPUNITIVE INPUT
     	* [ ] - INVENTORY SYSTEM
     	* [ ] - LOOT COMMAND
     	* --- PLAYER-DRIVEN SUGGESTIONS ---
     	* [ ] - EAT STUFF
     	*   [ ] - BODIES/CORPSES
     	*   [ ] - ANIMALS E.G. BIRDS
     	* [x] - SUICIDE BY BURNING
     	*/
        static void Main(string[] args)
        {
            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            GUI.Text.Header("∫arħen daħ Mørlaħ.");
            /*Console.WriteLine();
            PrettyPrint.WriteObscured("It seems we have a problem", 'e');
            Console.WriteLine();
            PrettyPrint.ReadLine();
            Console.WriteLine();
            PrettyPrint.WriteObscured("It seems we have a problem", 'e');
            Console.WriteLine();*/
            GUI.Text.WriteLine("Loading game files.");
            string[] files = Directory.GetFiles("Scenarios");
            List<string> scenarios = new List<string>();
            foreach (string file in files)
            {
                if (file.Split("\\")[file.Split("\\").Length - 1].EndsWith(".json"))
                {
                    scenarios.Add(file.Split("\\")[file.Split("\\").Length - 1].Split(".")[0].ToUpper());
                }
            }
            GUI.Text.WriteLine("Loading complete.");
            GUI.Text.typeWriter("Greetings.");
            GUI.Text.typeWriter($"In this game, there are {scenarios.Count} different scenarios you may choose to play.");
            GUI.Text.WriteLine();
            string scenarioChoice = GUI.Text.sentenceCase(GUI.displayCommands(scenarios.ToArray(), 0));
            GUI.Text.typeWriter($"Initialising {scenarioChoice} Scenario.");
            Game.load(scenarioChoice);
            GUI.Maximize();

            Console.Clear();
            GUI.Text.Header(scenarioChoice.ToUpper());
            Thread.Sleep(1000);
            Game.play();
        }
        public class Game
        {
            static string scenarioRaw;
            static Scenario scenario;
            static bool alive = true;
            public static void load(string scenarioName)
            {
                string scenarioPath = $"Scenarios/{scenarioName}.json";
                scenarioRaw = File.ReadAllText(scenarioPath);
                var scenarioNode = JsonNode.Parse(scenarioRaw);
                scenario = scenarioNode.Deserialize<Scenario>();
                if (scenario.savedata.playerName == "" || scenario.savedata.playerName == null)
                {
                    GUI.Text.WriteLine($"The default name for this scenario is {scenario.defaults.playerName}");
                    GUI.Text.WriteLine("Do you wish to change this name or continue?");
                    string nameChoice = GUI.displayCommands(new string[] { "CHANGE", "CONTINUE" }, 0, false, true, "");
                    if (nameChoice == "CHANGE")
                    {
                        GUI.Text.WriteLine("Please enter a name.");
                        scenario.savedata.playerName = GUI.Text.ReadLine();
                    }
                    GUI.Text.WriteLine($"Name {scenario.savedata.playerName} selected.");
                    GUI.Text.typeWriter("Saving...");
                    save(scenario, scenarioName);
                }
                GUI.Text.WriteLine($"{scenarioName} Scenario ready.");
                GUI.Text.colour("Please press a key", GUI.ClosestConsoleColor("00a9e9"));
                Console.ReadKey();
            }
            public static void save(Scenario scenarioData, string scenarioName)
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                File.WriteAllText($"Scenarios/{scenarioName}.json", JsonSerializer.Serialize(scenarioData, options));
            }
            public static void play()
            {
                if (scenario.savedata.room == -1)
                {
                    scenario.savedata.room = scenario.defaults.room;
                }
                while (alive)
                {
                    int currentRoom = scenario.savedata.room;
                    GUI.Text.WriteLine();
                    //GUI.Text.WriteLine(scenario.savedata.curEvent);
                    scenario.rooms[currentRoom].describe();
                    //GUI.Text.WriteLine(scenario.savedata.curEvent);
                    DrawMap();
                    scenario.rooms[currentRoom].handleInput();
                }
            }

            #region Game Objects
                public class Event
                {
                    public string type { get; set; }
                    public List<string> description { get; set; }
                    public bool changeRoom { get; set; }
                    public int newRoom { get; set; }
                    public int newEvent { get; set; }
                    public List<Action>? answers { get; set; }
                }
                public class Action
                {
                    public Action()
                    {

                    }
                    public List<string> commands { get; set; }
                    public string type { get; set; }
                    public List<string>? description { get; set; }
                    public int? room { get; set; }
                    public int newEvent { get; set; } = -1;
                    public int curEvent { get; set; } = -1;
                    public List<string>? options { get; set; }
                    public List<Event>? answers { get; set; }
                    public bool changeRoom { get; set; } = false;
                }

                public class Room
                {
                    public static Room LastRoom { get; set; }
                    public string name { get; set; }
                    public List<int> connections { get; set; }
                    public List<string> description { get; set; }
                    public List<Action> actions { get; set; }
                    public List<Event> events { get; set; }
                    public static List<int> curEvents = new List<int>();
                    public void printContext(string input)
                    {
                        bool youDied = false;
                        if (input.EndsWith("/DIE/"))
                        {
                            youDied = true;
                            input = input.Replace("/DIE/", "");
                        }
                        if (input.StartsWith("/t="))
                        {
                            input = input.Replace("/t=", "");
                            bool valid = false;
                            int n = 0;
                            while (!valid)
                            {
                                if (n < input.Length - 1)
                                {
                                    if (input[n + 1] == '/')
                                    {
                                        input = input.Remove(n + 1);
                                        valid = false;
                                    }
                                    n++;
                                }
                                else
                                {
                                    valid = true;
                                }
                            }
                            int overallTime = Convert.ToInt32(input);
                            Thread.Sleep(overallTime);
                        }
                        else if (input.StartsWith("/BACKGROUND/"))
                        {
                            List<string> inputList = input.Split('/').ToList();
                            inputList.RemoveAt(0);
                            string colour = inputList[1].Replace("#", "");
                            byte[] colours = Convert.FromHexString(colour);
                            Console.BackgroundColor = GUI.ClosestConsoleColor(colours[0], colours[1], colours[2]);
                        }
                        else if (input.StartsWith("/HEADER/"))
                        {
                            List<string> inputList = input.Split('/').ToList();
                            inputList.RemoveAt(0);
                            inputList.RemoveAt(0);
                            GUI.Text.Header(inputList[0]);
                        }
                        else if (input.StartsWith("/DIVIDER/"))
                        {
                            for (int i = 0; i < Console.WindowWidth; i++)
                            {
                                Console.Write($"{GUI.Text.UNDERLINE} {GUI.Text.RESET}");
                            }
                        }
                        else if (input == "/CLEAR/")
                        {
                            Console.Clear();
                        }
                        else if (input == "/HANG/")
                        {
                            printContext("PLEASE PRESS ANY KEY:");
                            Console.ReadLine();
                        }
                        else
                        {
                            if (LastRoom != null)
                            {
                                input = input.Replace("/PREV/", LastRoom.name.ToUpper());
                            }
                            input = input.Replace("/NAME/", scenario.savedata.playerName);
                            bool write = input.StartsWith("/WRITE/");
                            if (write)
                            {
                                input = input.Replace("/WRITE/", "");
                            }
                            string[] lines = input.Split(Environment.NewLine);
                            for (int i = 0; i < lines.Length; i++)
                            {
                                string[] words = lines[i].Split(" ");
                                for (int j = 0; j < words.Length; j++)
                                {
                                    if (words[j] == words[j].ToUpper() && words[j].Length > 1)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                    }
                                    Console.Write(words[j]);
                                    Console.Write(" ");
                                    Console.ResetColor();
                                }
                                if (!write)
                                {
                                    GUI.Text.WriteLine();
                                }
                            }
                        }
                        Game.alive = !youDied;
                        if (youDied)
                        {
                            GUI.PrintErr("======================YOU HAVE PERISHED======================", false);
                        }
                    }
                    void showEvents(string type)
                    {
                        curEvents.Clear();
                        if (scenario.savedata.curEvent >= 0)
                        {
                            curEvents.Add(scenario.savedata.curEvent);
                            Event curEvent = events[scenario.savedata.curEvent];
                            if (curEvent.type == type)
                            {
                                for (int i = 0; i < curEvent.description.Count; i++)
                                {
                                    printContext(curEvent.description[i]);
                                    if (curEvent.changeRoom)
                                    {
                                        scenario.savedata.room = curEvent.newRoom;
                                    }
                                    scenario.savedata.curEvent = curEvent.newEvent;
                                }
                            }
                        }
                    }
                    public void describe()
                    {
                        if (scenario.savedata.curEvent == -2)
                        {
                            scenario.savedata.curEvent = scenario.defaults.curEvent;
                        }
                        showEvents("pre");
                        for (int i = 0; i < description.Count; i++)
                        {
                            printContext(description[i]);
                        }
                        showEvents("post");
                    }
                    public void handleInput()
                    {
                        string userInput = "";
                        List<string> availableActions = new List<string>();
                        List<string> universalActions = new List<string>();
                        bool valid = false;
                        bool universal = false;
                        while (!valid)
                        {
                            for (int i = 0; i < actions.Count; i++)
                            {
                                foreach (string command in actions[i].commands)
                                {
                                    availableActions.Add(command);
                                }
                            }
                            for (int i = 0; i < scenario.universalCommands.Count; i++)
                            {
                                foreach (string command in scenario.universalCommands[i].commands)
                                {
                                    universalActions.Add(command);
                                }
                            }
                            GUI.Text.WriteLine("What do you do?");
                            userInput = GUI.Text.ReadLine().ToUpper();
                            valid = KeyTools.String.Search.linear(userInput, availableActions);
                            universal = KeyTools.String.Search.linear(userInput, universalActions);
                            valid = (universal) ? true : valid;
                            if (!valid)
                            {
                                GUI.PrintErr("Command unknown or not implemented. Please try again.", false);
                                //describe();
                            }
                        }
                        //nested linear search to find the action with an appropriate command
                        //standard linear search wouldn't work as the command is a sub-attribute of any action, and we are searching for an index rather than a boolean value
                        int index = -1;
                        List<Action> ActionsList = (universal) ? scenario.universalCommands : actions;
                        for (int i = 0; i < ActionsList.Count; i++)
                        {
                            if (KeyTools.String.Search.linear(userInput, ActionsList[i].commands))
                            {
                                index = i;
                                break;
                            }
                        }

                        if (!KeyTools.Int.Search.linear(ActionsList[index].curEvent, curEvents)  && ActionsList[index].curEvent != -1)
                        {
                            GUI.PrintErr("Command unknown or not implemented. Please try again.", false);
                            handleInput();

                        }
                        switch (actions[index].type)
                        {
                            case "describe":
                                for (int i = 0; i < ActionsList[index].description.Count; i++)
                                {
                                    printContext(ActionsList[index].description[i]);
                                }
                                break;
                            case "warp":
                                LastRoom = scenario.rooms[scenario.savedata.room];
                                scenario.savedata.room = Convert.ToInt32(actions[index].room);
                                break;
                            case "question":
                                //GUI.PrintErr("Question triggered", false);
                                for (int i = 0; i < ActionsList[index].description.Count; i++)
                                {
                                    printContext(ActionsList[index].description[i]);
                                }
                                List<string> options = ActionsList[index].options;
                                if (KeyTools.String.Search.linear("/INPUT/", options))
                                {
                                    options.Remove("/INPUT/");
                                    options.Add("SOMETHING ELSE?");
                                }
                                string answer = GUI.displayCommands(options.ToArray(), 0, false, true, "");
                                Event current = ActionsList[index].answers[KeyTools.String.Search.indexOf(answer, actions[index].options)];
                                for (int i = 0; i < current.description.Count; i++)
                                {
                                    printContext(current.description[i]);
                                }
                                if (answer == "SOMETHING ELSE?")
                                {
                                    bool contained = false;
                                    Action selected = new Action();
                                    while (!contained)
                                    {
                                        string response = GUI.Text.ReadLine();
                                        for (int i = 0; i < current.answers.Count; i++)
                                        {
                                            for (int j = 0; j < current.answers[i].commands.Count; j++)
                                            {
                                                if (response.ToUpper().Contains(current.answers[i].commands[j]))
                                                {
                                                    try
                                                    {
                                                        selected = current.answers[i];
                                                        string[] throwTest = selected.description.ToArray();
                                                        contained = true;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        contained = false;
                                                    }
                                                }
                                            }
                                        }
                                        if (!contained)
                                        {
                                            GUI.PrintErr("I'm sorry, I don't understand what you said.", false);
                                        }
                                    }
                                    if (selected.changeRoom)
                                    {
                                        scenario.savedata.room = Convert.ToInt32(selected.room);
                                    }
                                    current.newEvent = selected.newEvent;
                                    for (int i = 0; i < selected.description.Count; i++)
                                    {
                                        printContext(selected.description[i]);
                                    }
                                }

                                if (current.changeRoom)
                                {
                                    scenario.savedata.room = current.newRoom;
                                }

                                /*
                                 * debug print block
                                 * GUI.Text.WriteLine(current.newEvent);
                                 * GUI.Text.WriteLine(scenario.savedata.curEvent);
                                 * GUI.Text.WriteLine(scenario.rooms[scenario.savedata.room].events[current.newEvent].description[0]);
                                 * GUI.Text.WriteLine(scenario.savedata.curEvent);
                                 */
                                scenario.savedata.curEvent = current.newEvent;
                                break;
                            default:
                                break;
                        }
                        if (actions[index].type != "question")
                        {
                            scenario.savedata.curEvent = ActionsList[index].newEvent;
                        }
                    }
                }
                public class Scenario
                {
                    public Savedata defaults { get; set; }
                    public Savedata savedata { get; set; }
                    public List<Room> rooms { get; set; }
                    public List<Action>? universalCommands { get; set; }
                }
                public class Savedata
                {
                    public string playerName { get; set; }
                    public int room { get; set; }
                    public int curEvent { get; set; }
                }
            #endregion

            public static void DrawMap()
            {
                int Xratio = 10;
                int Yratio = 5;
                int[][] directions = new int[3][] {
                    new int[]{ -1, 1, -1 },
                    new int[]{ 4, 0, 2 },
                    new int[]{ -1, 3, -1 }
                };
                GUI.Text.move((Xratio - 1) * (Console.WindowWidth / Xratio),-6);
                GUI.Text.Header("MAP:","",ConsoleColor.White,8,false);
                //Console.SetCursorPosition((Xratio - 1) * (Console.WindowWidth / Xratio),(Yratio - 1) * (Console.WindowHeight / Yratio));
                //Console.WriteLine(scenario.rooms[scenario.savedata.room].name[0]);
                //GUI.Text.move(-2,-2);
                //Console.BackgroundColor = ConsoleColor.DarkBlue;
                //Console.ForegroundColor = ConsoleColor.White;
                for (int y = 0; y < directions.Length; y++)
                {
                    for (int x = 0; x < directions.Length; x++)
                    {
                        try
                        {
                            if (directions[x][y] == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                            }
                            Console.Write($"{scenario.rooms[scenario.rooms[scenario.savedata.room].connections[directions[x][y]]].name[0]} ");
                            Console.ResetColor();
                        }
                        catch (Exception)
                        {
                            Console.Write("  ");
                        }
                    }
                    GUI.Text.move(-6, 2);
                }
                Console.WriteLine();
            }
        }
    }

    //Libraries
    //These are not intended to be instanceable classes, just designed as libraries so that code can be reused in other projects.

    public class KeyTools
    {
        public static class String
        {
            public static class Search
            {
                public static bool linear(string value, List<string> list)
                {
                    bool found = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].StartsWith("/CONTAINS/"))
                        {
                            string[] broken = list[i].Split("/");
                            for (int j = 0; j < broken.Length; j++)
                            {
                                if (value.Contains(broken[j]))
                                {
                                    found = true;
                                }
                            }
                        }
                        if (list[i] == value)
                        {
                            found = true;
                        }
                    }
                    return found;
                }
                public static int indexOf(string value, List<string> list)
                {
                    int found = -1;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == value)
                        {
                            found = i;
                        }
                    }
                    return found;
                }
            }
        }
        public static class Int
        {
            public static class Search
            {
                public static bool linear(int value, List<int> list)
                {
                    bool found = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == value)
                        {
                            found = true;
                        }
                    }
                    return found;
                }
            }
        }
    }
    public class GUI
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]

        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;

        public static void Maximize()
        {
            ShowWindow(ThisConsole, MAXIMIZE);
        }

        public class Text
        {
            public const string CLS = "\u001bc\x1b[3J";         //clear screen
            public const string RESET = "\x1B[0m";             //set format to default
            public const string BOLD = "\x1B[1m";              //bold text
            public const string DISABLED = "\x1B[2m";          //darkens, based on color
            public const string ITALICS = "\x1B[3m";           //italics text
            public const string UNDERLINE = "\x1B[4m";         //underline text
            public const string BLINK = "\x1B[5m";             //blinks from set color to darker set color.
            public const string INVERT = "\x1B[7m";            //inverts background with foreground.
            public const string STRIKE = "\x1B[9m";            //strikethrough the text
            public const string FGCOLOR = "\u001b[38;5;3m";    //38 - forground, 3m - yellow (256 color palette)
            public const string BGCOLOR = "\u001b[48;5;1m";    //48 - background, 1m - red (256 color palette)

            static bool typeWrite = true;

            public static void typeWriter(string message, bool skippable = true, int time = 100)
            {
                int initLeft = Console.GetCursorPosition().Left;
                int initTop = Console.GetCursorPosition().Top;
                if (typeWrite)
                {
                    for (int i = 0; i < message.Length; i++)
                    {
                        if (skippable)
                        {
                            if (Console.KeyAvailable)
                            {
                                bool skipped = false;
                                switch (Console.ReadKey().Key)
                                {
                                    default:
                                        skipped = true;
                                        break;
                                }
                                if (skipped)
                                {
                                    break;
                                }
                            }
                        }
                        Console.Write(message[i]);
                        Thread.Sleep(time);
                    }
                }
                Console.SetCursorPosition(initLeft, initTop);
                GUI.Text.WriteLine(message);
                if (typeWrite)
                {
                    Thread.Sleep(500);
                }
            }
            public static void colour(string message, ConsoleColor colour, bool reset = true)
            {
                Console.ForegroundColor = colour;
                GUI.Text.WriteLine(message);
                if (reset)
                {
                    Console.ResetColor();
                }
            }
            public static string sentenceCase(string sentence)
            {
                sentence = sentence.ToLower();
                char[] components = sentence.ToCharArray();
                string[] manageableComponents = new string[components.Length];
                for (int i = 0; i < components.Length; i++)
                {
                    manageableComponents[i] = Convert.ToString(components[i]);
                }
                manageableComponents[0] = manageableComponents[0].ToUpper();
                string output = "";
                for (int i = 0; i < components.Length; i++)
                {
                    output += manageableComponents[i];
                }
                return output;
            }
            public static void back()
            {
                Console.SetCursorPosition(Console.GetCursorPosition().Left - 1, Console.GetCursorPosition().Top);
            }
            public static void Header(string title, string subtitle = "", ConsoleColor foreGroundColor = ConsoleColor.White, int windowWidthSize = -1, bool newLine = true)
            {
                if (windowWidthSize == -1)
                {
                    windowWidthSize = Console.WindowWidth;
                }
                Console.Title = title + (subtitle != "" ? " - " + subtitle : "");
                string titleContent = CenterText(title, "║",windowWidthSize);
                string subtitleContent = CenterText(subtitle, "║",windowWidthSize);
                string borderLine = new String('═', windowWidthSize - 2);

                Console.ForegroundColor = foreGroundColor;
                (int Left, int Top) initPos = Console.GetCursorPosition();
                Console.WriteLine($"╔{borderLine}╗");
                Console.SetCursorPosition(initPos.Left, initPos.Top+1);
                Console.WriteLine(titleContent);
                Console.SetCursorPosition(initPos.Left, initPos.Top+2);
                if (!string.IsNullOrEmpty(subtitle))
                {
                    Console.WriteLine(subtitleContent);
                    Console.SetCursorPosition(initPos.Left, initPos.Top+3);
                }
                Console.WriteLine($"╚{borderLine}╝");
                if (!newLine)
                {
                    Console.SetCursorPosition(initPos.Left, initPos.Top+4);
                }
                Console.ResetColor();
            }
            public static string CenterText(string content, string decorationString = "", int windowWidthSize = -1)
            {
                if (windowWidthSize == -1)
                {
                    windowWidthSize = Console.WindowWidth;
                }
                int windowWidth = windowWidthSize - (2 * decorationString.Length);
                return String.Format(decorationString + "{0," + ((windowWidth / 2) + (content.Length / 2)) + "}{1," + (windowWidth - (windowWidth / 2) - (content.Length / 2) + decorationString.Length) + "}", content, decorationString);
            }
            static int lastStringStart = 0;
            public static void WriteLine(string? content = null)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    lastStringStart = CenterText(content).IndexOf(content);
                    Console.WriteLine(CenterText(content));
                }
                else
                {
                    Console.WriteLine();
                }
            }
            public static string ReadLine()
            {
                Console.SetCursorPosition(lastStringStart, Console.GetCursorPosition().Top);
                return Console.ReadLine();
            }
            public static (int x, int y) move(int x = 0, int y = 0)
            {
                int newX, newY;
                newX = Console.GetCursorPosition().Left + x;
                newY = Console.GetCursorPosition().Top + y;
                Console.SetCursorPosition(newX, newY);
                return (newX, newY);
            }

        }
        public static string displayCommands(string[] commands, int selection, bool pause = false, bool clear = true, string message = "Please choose an option:")
        {
            GUI.Text.WriteLine();
            bool selected = false;
            while (!selected)
            {
                if (pause)
                {
                    Console.ReadKey();
                }
                if (clear)
                {
                    Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 2);
                }
                GUI.Text.WriteLine(message);
                for (int i = 0; i < commands.Length; i++)
                {
                    if (i == selection)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write($"[[{commands[i]}]]");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(commands[i]);
                    }
                    Console.Write(" ");
                }
                GUI.Text.WriteLine();
                ConsoleKeyInfo Input = Console.ReadKey();
                switch (Input.Key)
                {
                    case ConsoleKey.RightArrow:
                        if (selection < commands.Length - 1)
                        {
                            selection++;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (selection > 0)
                        {
                            selection--;
                        }
                        break;
                    case ConsoleKey.Enter:
                        selected = true;
                        break;
                    default:
                        break;
                }
            }
            return commands[selection];
        }
        public static void PrintErr(string msg, bool shout = true)
        {
            if (shout)
            {
                GUI.Text.colour("Exception Caught", ConsoleColor.Red);
            }
            GUI.Text.colour(msg, ConsoleColor.Red);
            Console.ResetColor();
        }
        //ConsoleColor approximation of Hex codes
        //https://stackoverflow.com/a/12340136/10315352
        public static ConsoleColor ClosestConsoleColor(byte r, byte g, byte b)
        {
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
        public static ConsoleColor ClosestConsoleColor(string hex)
        {
            byte r, g, b;
            byte[] hexBytes = Convert.FromHexString(hex);
            try
            {
            r = hexBytes[0];
            g = hexBytes[1];
            b = hexBytes[2];
            }
            catch (IndexOutOfRangeException)
            {
                Exception e = new Exception("Invalid hex colour code");
                throw(e);
            }
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
    }

    class PrettyPrint
    {
        public static uint delay = 20;

        private static Random random = new Random();
        private static List<(int, int)> obscuredPositions = new List<(int, int)>();

        private static readonly string POS = "\x1b[{1};{0}H";
        private static readonly string SAVE = "\x1b[s";
        private static readonly string RESTORE = "\x1b[u";

        public static void WriteObscured(string text, char obfuscated)
        {
            foreach (char chr in text)
            {
                if (chr == obfuscated)
                    obscuredPositions.Add((++Console.CursorLeft, Console.CursorTop + 1));
                else
                    Console.Write(chr);
            }
            Console.WriteLine();
        }

        public static string ReadLine()
        {
            using (var reader = new StreamReader(Console.OpenStandardInput()))
            {
                Task<string> readlineTask = reader.ReadLineAsync();

                while (!readlineTask.IsCompleted)
                {
                    using (var writer = new DisposableWrite())
                        foreach ((int x, int y) in obscuredPositions)
                            writer.WriteChar(RandomChar(), x, y);
                    Task.Delay((int)delay).Wait();
                }

                return readlineTask.Result;
            }
        }

        private static char RandomChar()
            => (char)(33 + random.Next(94));

        private class DisposableWrite : IDisposable
        {
            public DisposableWrite()
                => Console.CursorVisible = false;

            public void Dispose()
                => Console.CursorVisible = true;

            public void WriteChar(char chr, int x, int y)
                => Console.Write(SAVE + POS + "{2}" + RESTORE, x, y, chr);
        }
    }
}