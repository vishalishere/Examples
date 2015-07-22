using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FTPClientCameraSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] data = new byte[1024];
            int sent;
            Console.WriteLine("Enter ip: ");
            string ip = Console.ReadLine();
            Console.WriteLine("Enter port: ");
            int port = Int32.Parse(Console.ReadLine());
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), port);
            
            try{
            TcpClient client = new TcpClient();
            client.Connect(ipep);
            Console.WriteLine("Enter bitmap directory: ");
            string bitmapDir= Console.ReadLine();
            Console.WriteLine("Enter bit map name (ie desert.jpg): ");
            string bitMapName = Console.ReadLine();
            Bitmap bmp = new Bitmap(bitmapDir+"\\"+bitMapName);
            MemoryStream ms = new MemoryStream();
            //Save to memory using the Jpeg format
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //read to end
            byte[] bmpBytes = ms.ToArray();
            bmp.Dispose();
            ms.Close();
            NetworkStream ns = client.GetStream();
            StreamWriter sw = new StreamWriter(ns);
            StreamReader sr = new StreamReader(ns);
            Console.WriteLine("Response: " + sr.ReadLine());
            Console.WriteLine("Sending Username Command");
            sw.WriteLine("USER Guest");
            sw.Flush();
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Response: " + sr.ReadLine());
            Console.WriteLine("Sending Password Command");
            sw.WriteLine("PASS ");
            sw.Flush();
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Response: " + sr.ReadLine());
            Console.WriteLine("Sending PWD command");
            sw.WriteLine("PWD");
            sw.Flush();
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Response: " + sr.ReadLine());
            Console.WriteLine("Sending EPRT- Will Listen on port 300");
            IPEndPoint imageServe = new IPEndPoint(IPAddress.Parse(ip), 300);
            Socket serveImage=new Socket(AddressFamily.InterNetwork,
                            SocketType.Stream, ProtocolType.Tcp);
            serveImage.Bind(imageServe);
            sw.WriteLine("EPRT |1|127.0.0.1|300|");
            sw.Flush();
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Response: " + sr.ReadLine());
            Console.WriteLine("Sending TYPE I");
            sw.WriteLine("TYPE I");
            sw.Flush();
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Response: " + sr.ReadLine());
            Console.WriteLine("Sending STOR");
            serveImage.Listen(10);
            sw.WriteLine("STOR "+bitMapName);
            sw.Flush();
            Socket newClient = serveImage.Accept();
            IPEndPoint newClientIP = (IPEndPoint)newClient.RemoteEndPoint;
            System.Threading.Thread.Sleep(1000);
            sent = SendVarData(newClient, bmpBytes);
            newClient.Shutdown(SocketShutdown.Both);
            newClient.Close();
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Response: " + sr.ReadLine());
           // Console.WriteLine("Sending STOR (again)");
            //sw.WriteLine("STOR 226");
           // sw.Flush();
           //Console.WriteLine("Response: " + sr.ReadLine());
            Console.WriteLine("Sending Quit: ");
            sw.WriteLine("QUIT 221");
            sw.Flush();
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Response: " + sr.ReadLine());
            Console.WriteLine("Disconnecting from server...");
            //server.Shutdown(SocketShutdown.Both);
            //server.Close();
            Console.ReadLine();

    
            }
            catch (SocketException e)
            {
                Console.WriteLine("Unable to connect to server.");
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }

            
        }

        private static int SendVarData(Socket s, byte[] data)
        {
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;

            byte[] datasize = new byte[4];
            datasize = BitConverter.GetBytes(size);
            //sent = s.Send(datasize);

            while (total < size)
            {
                sent = s.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft -= sent;
            }
            return total;
        }
    }

}
