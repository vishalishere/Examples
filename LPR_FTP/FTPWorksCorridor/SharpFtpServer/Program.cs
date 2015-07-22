using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SharpFtpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter IP address to serve on: ");
            string ip = Console.ReadLine();
            Console.WriteLine("Enter port to serve on: ");
            int port = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter directory to save images to: ");
            string direct = Console.ReadLine();
            using (FtpServer server = new FtpServer(IPAddress.Parse(ip), port, direct))
            {
                server.Start();

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
            }
        }
    }
}
