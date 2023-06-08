using System;

namespace GodsBlessing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Terminal terminal = new Terminal();

                terminal.Run();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.Write("While starting the app an exception occured. Is the app situated in the right directory (root_of_your_project/GodsBlessing)?");

                Console.ReadKey();
            }
        }
    }
}
