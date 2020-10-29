using chatServer;
using System;
using System.Threading;

namespace chat
{
    class Program
    {
        static ServerObj server;
        static Thread listenThread;
        static void Main(string[] args)
        {
            try
            {
                server = new ServerObj();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
