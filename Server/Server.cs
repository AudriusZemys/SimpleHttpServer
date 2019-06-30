using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Server
    {
        private Socket Socket;
        //Define an available port you'd like to listen HERE.
        private readonly int Port = 80;
        private readonly int Backlog = 10;
        private byte[] Buffer = new byte[1024];
        private List<Socket> Clients = new List<Socket>();

        //Simply create the server by creating a Socket object.
        public Server()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        //Start the server and listen to the socket.
        public void Start()
        {
            Socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            Socket.Listen(Backlog);
        }

        //Ah, we received a request so we must accept it with joy!
        public void Accept()
        {
            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket CurrentClient = Socket.EndAccept(ar);
            //Not much reason to do that here since we ourselves are the clients, but having a list of clients is a must.
            Clients.Add(CurrentClient);
            CurrentClient.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), CurrentClient);
            //After we BeginReceive we need to start Accepting other clients
            Accept();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            //We pass CurrentClient as the state so we could make further actions with it, hardcast is needed.
            var client = (Socket)ar.AsyncState;
            var receivedSize = client.EndReceive(ar);
            var receivedBytes = new byte[receivedSize];
            Array.Copy(Buffer, receivedBytes, receivedSize);
            var data = Encoding.ASCII.GetString(receivedBytes);
            Console.WriteLine("Data received: \n" + data);
            //Looks nasty and unecessary but we need to distinguish GET's from other methods.
            var headers = data.Split(' ');
            if (headers[0].ToUpper() == "GET")
            {
                //All good, return that index file.
                var responseBytes = Encoding.ASCII.GetBytes(GetIndexFile());
                client.BeginSend(responseBytes, 0, responseBytes.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
            }
            else
            {
                //Something is wrong here...
                Console.WriteLine("Only GET method is supported");
                var responseBytes = Encoding.ASCII.GetBytes("Only GET method is supported");
                client.BeginSend(responseBytes, 0, responseBytes.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            //Turn the lights off after leaving the room!
            client.Shutdown(SocketShutdown.Send);
        }

        private string GetIndexFile()
        {
            if (File.Exists(Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "..\\index.html")))
            {
                return File.ReadAllText(Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "..\\index.html"));
            }

            return "<h1>404 Index page somehow not found... ¯\\_(ツ)_/¯<h1>";
        }
    }
}