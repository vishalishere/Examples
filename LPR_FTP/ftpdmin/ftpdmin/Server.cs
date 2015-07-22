using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Drawing;
using Microsoft.VisualBasic;
//Creates a TcpServer that is used to transfer images from the Axis Camera to a directory on a computer
//ftpdmin was chosen as the name because it was supposed to be a replica of a program in c just made in csharp
//However, it was really hard to transfer from c because all of the struct names and functions were nonintuitive
//This program is completely made from internet sources, and there are definitely bugs. For instance it does not implement
// many FTP calls. 


namespace ftpdmin
{
    class Server
    {
        //TCPListener listens to a given IP and port to wait for a connection with the camera
        
        //Download Listener listens to the port given by the camera in the PORT command, which is the
        //port at which the files needed to be downloaded are stored.

        //Listen thread implements tcpListener. We do not want to be stuck in an infinite loop, but
        //we always want to be listening to the camera. That is why we use another thread.

        //Downlaod thread implements the downloadlistener for the same reason as above

        //File name is the download files name given by the camera in the STOR command.

        //direct is the directory to save the files at on the local computer. It was given in the main
        // method of the console program. See Program.cs

        private TcpListener tcpListener;
        private TcpListener downloadListener;
        private Thread listenThread;
        private Thread downloadThread;
        private string fileName;

        private string direct;

        //Initialize Ip adress and threads
        public Server(string dir)
        {
            direct = dir;
            this.tcpListener = new TcpListener(IPAddress.Parse("172.22.22.104"), 3000);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            //Start listening
            this.tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void ListenForDownloads()
        {
            this.downloadThread.Start();

            while(true)
            {
                TcpClient downloadClient = this.downloadListener.AcceptTcpClient();
                Thread clientDownloadThread = new Thread(new ParameterizedThreadStart(HandleClientDownload));
                clientDownloadThread.Start(downloadClient);
            }
        }

        private void HandleClientDownload(object downloadClient)
        {
            Console.WriteLine("IM HERE");
            TcpClient downloaderClient = (TcpClient) downloadClient;
            NetworkStream downloadStream = downloaderClient.GetStream();
            StreamWriter downloadWriter = new StreamWriter(downloadStream, Encoding.ASCII);
            StreamReader downloadReader = new StreamReader(downloadStream);
            try
            {
                //Sets up the path to store the file
                string path = Path.Combine(direct, fileName);
                FileStream file = File.Create(path);
                //Implements the method to download a file
                CopyStream(file, downloadStream);
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("a socket error has occured:" + e);

            }
        }

        private void HandleClientComm(object client)
        {
            //A Server is TCP has to respond to a bunch of commands from the client. The first thing it
            //does when it connects is send code 220 which says it is good to continue.
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            StreamWriter writer = new StreamWriter(clientStream, Encoding.ASCII);
            StreamReader reader=new StreamReader(clientStream);
            writer.WriteLine("220 Ready!");
            writer.Flush();
            string command=reader.ReadLine().ToUpperInvariant();
            int downloadPort=0;
            string ipOfDownload="";
            Console.WriteLine(command);
          while(!command.Equals("QUIT"))
          {
              //USER comes with the username given to the client. Here I do not check if the cameras username
              //is the same as the username in the program. I just give the command 331 which means continue.
            if(command.Contains("USER"))
            {
                writer.WriteLine("331 Username ok, need password");
                writer.Flush();
            }
            //PASS is the same as username. I do not check the passwords, I just give 230 which continues the FTP.
            else if(command.Contains("PASS"))
            {
                writer.WriteLine("230 User Logged In");
                writer.Flush();
            }
            //PWD is Print working directory. I send 257 to say I have a PWD, and I send / because that is what is saved
            // in the camera. I am not actually going to save files at this directory, I just want to continue.
            else if(command.Contains("PWD"))
            {
                writer.WriteLine("257 \"/\"");
                writer.Flush();
            }
            //This is an important command. The client is sending an IP where it wants to do file transfers. It comes in a 
            //Weird format so all this function is doing is allowing me store Ip as "172.22.22.103" instead of "PORT 172,22,22,103"
            //Also there is a port listed at the end, but it is given in 2 numbers. The conversion to one port number is done by
            //changing the two numbers to hexadecimal, appending them, and then transforming them back to decimal.
            else if(command.Contains("PORT"))
            {
                string portPart1 = "";
                string portPart2 = "";
                Console.WriteLine(command);
                int numberOfCommas=0;
                int i=0;
                bool notPort=true;
                bool isNotPortPart2=true;
                while(i<command.Length && notPort)
                {
                   if(command[i].Equals(','))
                   {
                       if(numberOfCommas==3)
                       {
                           notPort=false;
                       }
                       else
                       {
                           ipOfDownload+=".";
                           numberOfCommas++;
                       }
                   }
                   else if(Information.IsNumeric(command[i]))
                   {
                       ipOfDownload+=command[i];
                   }
                   i++;
               }
               while(i<command.Length && isNotPortPart2)
               {
                   if(Information.IsNumeric(command[i]))
                   {
                       portPart1+=command[i];
                   }
                   else
                   {
                       isNotPortPart2=false;
                   }
                   i++;
               }
             while(i<command.Length)
             {
                 portPart2+=command[i];
                 i++;
             }
                Console.WriteLine("IP=" +ipOfDownload);
                Console.WriteLine("PortPart1="+portPart1);
                Console.WriteLine("PortPart2="+portPart2);
                int portPart1int = int.Parse(portPart1);
                int portPart2int = int.Parse(portPart2);
                string portPart1Hex = portPart1int.ToString("X");
                string portPart2Hex = portPart2int.ToString("X");
                string downloadPortHex = portPart1Hex + portPart2Hex;
                downloadPort = Convert.ToInt32(downloadPortHex, 16);
                Console.WriteLine("PortPart1Hex=" + portPart1Hex);
                Console.WriteLine("PortPart2Hex=" + portPart2Hex);
                Console.WriteLine("FinalPort: " + downloadPort);
                this.downloadListener = new TcpListener(IPAddress.Parse(ipOfDownload), downloadPort);
                this.downloadThread = new Thread(new ThreadStart(ListenForDownloads));
                writer.WriteLine("200 Ready for Transport");
                writer.Flush();
         }
        //The client sends TYPE I for image. usually an ftp would switchto binary mode because that is the only way
        //a file can be transferred cleanly.
         else if(command.Contains("TYPE"))
         {
             writer.WriteLine("200 I understand it is an image file");
             writer.Flush();
         }
       //This command gives the name of the file being transferred. I substring to get rid of
       //The STOR . that comes before the file name
         else if(command.Contains("STOR"))
         {
             fileName = command.Substring(6);
             writer.WriteLine("200 FileSuccessful");
             writer.Flush();
             Console.WriteLine(fileName);
         }
        //For all other commands sent by the client, I send 500 which means I'm not implementing those commands.
         else
         {
                writer.WriteLine("500 IDK");
                writer.Flush();
         }
              command=reader.ReadLine().ToUpperInvariant();
              Console.WriteLine(command);
        }    
          writer.WriteLine("221 BYE");
          writer.Flush();
          tcpClient.Close();
        }
        
        private static long CopyStream(Stream input, Stream output, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int count = 0;
            long total = 0;

            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
                total += count;
            }

            return total;
        }

        private static long CopyStreamAscii(Stream input, Stream output, int bufferSize)
        {
            char[] buffer = new char[bufferSize];
            int count = 0;
            long total = 0;

            using (StreamReader rdr = new StreamReader(input))
            {
                using (StreamWriter wtr = new StreamWriter(output, Encoding.ASCII))
                {
                    while ((count = rdr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        wtr.Write(buffer, 0, count);
                        total += count;
                    }
                }
            }

            return total;
        }

        private long CopyStream(Stream input, Stream output)
        {
            //if (_transferType == "I")
            //{
                return CopyStream(input, output, 4096);
            //}
            //else
            //{
             //   return CopyStreamAscii(input, output, 4096);
            //}
        }
    }
}