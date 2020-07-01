using System;
using System.Net.Sockets;
// ReSharper disable IdentifierTypo

namespace SentioSteppingTcpIp
{
    class Program
    {
        public static void Connect(string host, int port)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Establishing Connection to {0}", host);
            s.Connect(host, port);
            Console.WriteLine("Connection established");
        }

        /// <summary>
        /// Send a command to SENTIO, do not collect a response
        /// You should use this command only for the commands which do not send a response (very few *XXX commands)
        /// </summary>
        /// <param name="cmd"></param>
        public static void Send(string cmd)
        { }

        /// <summary>
        /// Send a command to SENTIO, collect the response.
        /// Use this function for SENTIO remote commands
        /// </summary>
        /// <param name="cmd">The command to send</param>
        /// <param name="errc">Error code</param>
        /// <param name="stat">A status code (i.e. last die, last sub site)</param>
        /// <param name="cmdId">Command ID. Only used by asynchronous remote commands</param>
        /// <param name="msg">remote command return string</param>
        public static void Send(string cmd, out int errc, out int stat, out int cmdId, out string msg)
        {
            errc = -1;
            stat = -1;
            cmdId = -1;
            msg = "";

            Console.WriteLine($"Sending remote command: {cmd}");

            Console.WriteLine($"Remote command Response: {errc}, {stat}, {cmdId}: {msg}");
        }

        static void Main(string[] args)
        {
            try
            {
                // Connect to SENTIO. Make sure SENTIO is running on the local PC and
                // is set up to listen at port 35555 (default port)
                Connect("127.0.0.1", 35555);

                int errc, stat, cmdId;
                string msg;

                // Ask SENTIO for self identification.
                // errc, stat and cmdID will only be set when sending native SENTIO remote commands!
                // "*IDN?" is not a SENTIO remote command but a low level command.
                Send("*IDN?", out errc, out stat, out cmdId, out msg);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error:");
                Console.WriteLine("------");
                Console.WriteLine(exc.Message);
            }
        }
    }
}
