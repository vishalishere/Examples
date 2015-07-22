using System;

namespace ftpdmin
{
    class Program
    {
        static void Main()
        {
            string direct,ip;
            Console.WriteLine("Enter save Directory: ");
            direct = Console.ReadLine();
            Server server = new Server(direct);
        }
    }
}