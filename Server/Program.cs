using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Program
    {
        //Notifies about thread
        public static ManualResetEvent IsInProgress = new ManualResetEvent(false);

        //CLOSE THE CONSOLE IF YOU WANT TO EXIT
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
            Console.WriteLine("Server started...");
            while (true)
            {
                IsInProgress.Reset();
                server.Accept();
                IsInProgress.WaitOne();
            }
        }
    }
}
