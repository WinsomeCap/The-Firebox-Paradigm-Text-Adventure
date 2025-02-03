using System.Text.Json.Nodes;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using static The_Firebox_Paradigm_Revamped.Program.Game;
using System.Runtime.InteropServices;

namespace The_Firebox_Paradigm_Revamped
{
    internal class Program
    {

        /* TODO:
     	* [ ] - NON-TERMINATING NONPUNITIVE INPUT
     	* [ ] - INVENTORY SYSTEM
     	* [ ] - LOOT COMMAND
     	*/

        static void Main()
        {
            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            GUI.Text.Header("∫arħen daħ Mørlaħ.");
            Console.WriteLine();
            PrettyPrint.WriteObscured("It seems we have a problem",'e');
            Console.WriteLine();
            PrettyPrint.ReadLine();
            Console.WriteLine();
            PrettyPrint.WriteObscured("It seems we have a problem",'e');
            Console.WriteLine();
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
            Game.play();
        }
        public class Game
        {
            static string scenarioRaw;
            static Scenario scenario;
            public static void load(string scenarioName)
            {
                string scenarioPath = $"Scenarios/{scenarioName}.json";
                scenarioRaw = File.ReadAllText(scenarioPath);
                var scenarioNode = JsonNode.Parse(scenarioRaw);
                scenario = JsonSerializer.Deserialize<Scenario>(scenarioNode);
                if (scenario.savedata.playerName == "")
                {
                    scenario.savedata.playerName = scenario.defaults.playerName;
                    GUI.Text.WriteLine($"The default name for this scenario is {scenario.savedata.playerName}");
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
                while (true)
                {
                    int currentRoom = scenario.savedata.room;
                    GUI.Text.WriteLine();
                    //GUI.Text.WriteLine(scenario.savedata.curEvent);
                    scenario.rooms[currentRoom].describe();
                    //GUI.Text.WriteLine(scenario.savedata.curEvent);
                    scenario.rooms[currentRoom].handleInput();
                }
            }

            //Game objects
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
                public string name { get; set; }
                public List<int> connections { get; set; }
                public List<string> description { get; set; }
                public List<Action> actions { get; set; }
                public List<Event> events { get; set; }
                public static List<int> curEvents = new List<int>();
                public void printContext(string input)
                {
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
                    else
                    {
                        bool write = input.StartsWith("/write/");
                        if (write)
                        {
                            input = input.Replace("/write/", "");
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
                    bool valid = false;
                    while (!valid)
                    {
                        for (int i = 0; i < actions.Count; i++)
                        {
                            foreach (string command in actions[i].commands)
                            {
                                availableActions.Add(command);
                            }
                        }
                        GUI.Text.WriteLine("What do you do?");
                        userInput = GUI.Text.ReadLine().ToUpper();
                        valid = KeyTools.String.Search.linear(userInput, availableActions);
                        if (!valid)
                        {
                            GUI.PrintErr("Command unknown or not implemented. Please try again.", false);
                            //describe();
                        }
                    }
                    //nested linear search to find the action with an appropriate command
                    //standard linear search wouldn't work as the command is a sub-attribute of any action, and we are searching for an index rather than a boolean value
                    int index = -1;
                    for (int i = 0; i < actions.Count; i++)
                    {
                        if (KeyTools.String.Search.linear(userInput, actions[i].commands))
                        {
                            index = i;
                        }
                    }
                    if (!KeyTools.Int.Search.linear(actions[index].curEvent, curEvents) && actions[index].curEvent != -1)
                    {
                        GUI.PrintErr("Command unknown or not implemented. Please try again.", false);
                        handleInput();
                    }
                    switch (actions[index].type)
                    {
                        case "describe":
                            for (int i = 0; i < actions[index].description.Count; i++)
                            {
                                printContext(actions[index].description[i]);
                            }
                            break;
                        case "warp":
                            scenario.savedata.room = Convert.ToInt32(actions[index].room);
                            break;
                        case "question":
                            //GUI.PrintErr("Question triggered", false);
                            for (int i = 0; i < actions[index].description.Count; i++)
                            {
                                printContext(actions[index].description[i]);
                            }
                            List<string> options = actions[index].options;
                            if (KeyTools.String.Search.linear("/INPUT/", options))
                            {
                                options.Remove("/INPUT/");
                                options.Add("SOMETHING ELSE?");
                            }
                            string answer = GUI.displayCommands(options.ToArray(), 0, false, true, "");
                            Event current = actions[index].answers[KeyTools.String.Search.indexOf(answer, actions[index].options)];
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
                        scenario.savedata.curEvent = actions[index].newEvent;
                    }
                }
            }
            public class Scenario
            {
                public Savedata defaults { get; set; }
                public Savedata savedata { get; set; }
                public List<Room> rooms { get; set; }
            }
            public class Savedata
            {
                public string playerName { get; set; }
                public int room { get; set; }
                public int curEvent { get; set; }
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
            public static void Header(string title, string subtitle = "", ConsoleColor foreGroundColor = ConsoleColor.White, int windowWidthSize = 90)
            {
                Console.Title = title + (subtitle != "" ? " - " + subtitle : "");
                string titleContent = CenterText(title, "║");
                string subtitleContent = CenterText(subtitle, "║");
                string borderLine = new String('═', windowWidthSize - 2);

                Console.ForegroundColor = foreGroundColor;
                Console.WriteLine($"╔{borderLine}╗");
                Console.WriteLine(titleContent);
                if (!string.IsNullOrEmpty(subtitle))
                {
                    Console.WriteLine(subtitleContent);
                }
                Console.WriteLine($"╚{borderLine}╝");
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
                Console.SetCursorPosition(lastStringStart,Console.GetCursorPosition().Top);
                return Console.ReadLine();
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


