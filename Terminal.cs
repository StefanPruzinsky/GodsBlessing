using System;
using System.Collections.Generic;
using System.Linq;

namespace GodsBlessing
{
    class Terminal
    {
        public TerminalHelper TerminalHelper { get; private set; }
        public DatabaseManager DatabaseManager { get; private set; }
        
        private List<string> commandsHistory;

        private bool DatabaseInitializationSuccess;
        private string DatabaseInitializationSuccessMessage;

        public Terminal()
        {
            try
            {
                DatabaseManager = new DatabaseManager();

                DatabaseInitializationSuccess = true;
                DatabaseInitializationSuccessMessage = "Database connection succeed.";
            }
            catch (Exception e)
            {
               DatabaseInitializationSuccess = false;
               DatabaseInitializationSuccessMessage = e.Message;

               return;
            }

            TerminalHelper = new TerminalHelper(DatabaseManager);

            commandsHistory = new List<string>();
        }

        public void Run()
        {
            //Database connection checking
            if (!DatabaseInitializationSuccess)
            {
                Console.WriteLine(DatabaseInitializationSuccessMessage);
                Console.ReadKey();

                return;
            }

            //Showing splash screen
            ShowSplashScreen();

            //Introductory initialization
            bool wasInitialized = DatabaseManager.InitializationExecution;

            if (!wasInitialized)
                TerminalHelper.InitializeApp();

            //Terminal loop
            string command = "";

            Console.Write("GodsBlessing> ");

            while ((command = CaptureCommand()) != "exit")
            {
                try
                {
                    if (command.Trim() == "--help")
                        Console.Write("\n{0}", DatabaseManager.TerminalCommandsList);
                    else if (command.Trim() == "--version")
                        Console.Write("\nStuff version - {0}", DatabaseManager.VersionOfStuff);
                    else if (command.Contains("migrate "))
                        TerminalHelper.Migrate(command.Split(' ').Select(s => s.Trim()).ToArray()[1]);
                    else if (command.Contains("create "))
                        TerminalHelper.Create(command.Replace("create ", "").Split(' ').Select(s => s.Trim()).ToArray());
                    //else if (command.Trim() == "deploy")
                        //TerminalHelper.DeployApp();
                    else if (command.Trim() != "")
                        TerminalHelper.WriteNotificationMessage("Invalid command, try again.");
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.Write("While command processing an exception occured.");

                    Console.ResetColor();
                }

                Console.Write("\nGodsBlessing> ");
            }
        }

        private string CaptureCommand()
        {
            ConsoleKeyInfo consoleKeyInfo;
            string command = "";

            int actuallCommandHistoryIndex = commandsHistory.Count + 1;
            bool wasUpListing = false;

            while ((consoleKeyInfo = Console.ReadKey()).Key != ConsoleKey.Enter)
            {
                if (consoleKeyInfo.Key == ConsoleKey.UpArrow && actuallCommandHistoryIndex != -1)
                {
                    if (!wasUpListing)
                        actuallCommandHistoryIndex = actuallCommandHistoryIndex - 2;

                    DeleteCharsInConsole(command.Length + 1);
                    Console.Write(commandsHistory[actuallCommandHistoryIndex]);

                    command = commandsHistory[actuallCommandHistoryIndex];

                    actuallCommandHistoryIndex--;
                    wasUpListing = true;
                }
                else if (consoleKeyInfo.Key == ConsoleKey.DownArrow && actuallCommandHistoryIndex != commandsHistory.Count)
                {
                    if (wasUpListing)
                        actuallCommandHistoryIndex = actuallCommandHistoryIndex + 2;

                    DeleteCharsInConsole(command.Length + 1);
                    Console.Write(commandsHistory[actuallCommandHistoryIndex]);

                    command = commandsHistory[actuallCommandHistoryIndex];

                    actuallCommandHistoryIndex++;
                    wasUpListing = false;
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Backspace)
                {
                    if (Console.CursorLeft + 1 > 14)
                        Console.Write(" \b");
                    else
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);

                    if (command.Length != 0)
                        command = command.Remove(command.Length - 1);
                }
                else if (consoleKeyInfo.Key == ConsoleKey.UpArrow || consoleKeyInfo.Key == ConsoleKey.DownArrow || ((int)consoleKeyInfo.KeyChar >= 0 && (int)consoleKeyInfo.KeyChar <= 31))
                    DeleteCharsInConsole(1);
                else
                    command += consoleKeyInfo.KeyChar;
            }

            if (command.Trim() != "")
                commandsHistory.Add(command);

            return command;
        }

        private void DeleteCharsInConsole(int count)
        {
            for (int i = 0; i < count; i++)
                Console.Write("\b \b");
        }

        public void ShowSplashScreen()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(DatabaseManager.TerminalHeader);
            Console.ResetColor();

            Console.WriteLine("\n{0}\n", DatabaseManager.TerminalCommandsList);
        }
    }
}
