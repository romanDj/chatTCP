using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace chatServer
{
    class ClientObj
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        string username;
        TcpClient client;
        ServerObj server;

        Dictionary<string, string> asks = new Dictionary<string, string>()
        {
            { "Как дела?", "хорошо" },
            { "Сколько тебе лет?", "21" },
            { "Где ты живешь?", "Казань" }
        };

        public ClientObj(TcpClient tcpClient, ServerObj serverObj)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObj;
            serverObj.AddConnection(this);
        }


        /// <summary>
        /// func for go in thread
        /// </summary>
        public void Life()
        {
            try
            {
                Stream = client.GetStream();
                string message = GetMessage();
                username = message;

                message = username + " вошел в чат";

                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);
                SendMessage("Перечень возомжных вопросов: \n\t" 
                    + String.Join("\n\t", asks.Keys)
                    + "\nКоманды:\n\tучастники\n\tпока");

                while (true)
                {
                    try
                    {
                        message = GetMessage();

                        //server.BroadcastMessage(message, this.Id);

                        if (message.Trim().ToUpper() == "ПОКА")
                        {
                            message = String.Format("{0}: покинул чат", username);
                            Console.WriteLine(message);
                            server.BroadcastMessage(message, this.Id);
                            break;
                        }
                        else if (message.Trim().ToUpper() == "УЧАСТНИКИ")
                        {
                            SendMessage("Список участников чата: \n\t"
                                + String.Join("\n\t", server.clients.Select(x => x.username)));
                        }
                        else
                        {
                            message = FindAnswer(message);
                            Console.WriteLine(message);
                            SendMessage(message);
                        }

                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", username);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.Id);
                Close();
            }
        }


        /// <summary>
        /// read input messages
        /// </summary>
        /// <returns>string message</returns>
        private string GetMessage()
        {
            byte[] data = new byte[64]; // for get data
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }


        protected internal void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            Stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// close connections
        /// </summary>
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }


        public string FindAnswer(string question)
        {
            var ask = asks.FirstOrDefault(
                x=> x.Key.Trim().ToUpper() == question.Trim().ToUpper());
            return ask.Value != null ? ask.Value : "не знаю что и сказать";
        }

    }
}
