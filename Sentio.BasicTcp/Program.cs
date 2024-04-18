using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedVariable
// ReSharper disable UnusedParameter.Local
// ReSharper disable IdentifierTypo

namespace SentioSteppingTcpIp
{
    class Program
    {
        private static readonly TcpClient _tcpClient = new TcpClient();

        private static StreamReader _reader;

        public static void Connect(string host, int port)
        {
            Console.WriteLine("Establishing Connection to {0}", host);
            _tcpClient.Connect(host, port);
            _reader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8);
            Console.WriteLine("Connection established");
        }

        /// <summary>
        /// Send a command to SENTIO, do not collect a response
        /// You should use this command only for the commands which do not send a response (very few *XXX commands)
        /// </summary>
        /// <param name="cmd"></param>
        public static void Send(string cmd)
        {
            if (_tcpClient == null || !_tcpClient.Connected)
            {
                throw new InvalidOperationException("Tcp client not connected!");
            }

            Console.WriteLine($"Sending remote command: {cmd}");

            // Make sure the command is terminated properly
            if (!cmd.EndsWith('\n'))
                cmd = cmd + '\n';

            var tcpStream = _tcpClient.GetStream();
            byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);

            tcpStream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        public static string Readline()
        {
            if (_tcpClient == null || !_tcpClient.Connected)
                throw new InvalidOperationException("Tcp client not connected!");

            var resp = _reader.ReadLine();
            if (string.IsNullOrEmpty(resp))
                throw new InvalidOperationException("Invalid Response!");

            return resp;
        }

        public static void Send(string cmd, out string msg)
        {
            Send(cmd);
            msg = Readline();
        }

        /// <summary>
        /// Send a command to SENTIO, collect the response.
        /// Use this function for SENTIO remote commands
        /// </summary>
        /// <param name="cmd">The command to send</param>
        /// <param name="errc">Error code</param>
        /// <param name="stat">A status code (i.e. last die, last sub site)</param>
        /// <param name="cmdId">Command ID. Only used by asynchronous remote commands</param>
        /// <param name="msg">remote command return string</param>
        public static void Send(string cmd, out RemoteCommandResult errc, out RemoteCommandStatus stat, out int cmdId, out string msg)
        {
            errc = RemoteCommandResult.NoError;
            stat = RemoteCommandStatus.None;
            cmdId = -1;
            msg = "";

            Send(cmd);
            var resp = Readline();
            var tok = resp.Split(",");
            if (tok.Length < 3)
                throw new InvalidOperationException("Invalid Response Format!");

            // SENTIO's error code consists of an error code and a status code!
            var errcOrig = uint.Parse(tok[0]);
            errc = (RemoteCommandResult)(errcOrig & 0b1111111111);  // the lowermost 10 bits cvontain the error code
            stat = (RemoteCommandStatus)(errcOrig >> 10);           // the uppermost bits contain status codes
            cmdId = int.Parse(tok[1]);

            // Collect the remaining arguments and join them back together
            msg = string.Join(",", tok.Skip(2).ToArray());
            Console.WriteLine($"Remote command Response: errc={errc},stat={stat},cmdId={cmdId}: resp=\"{msg}\"");
        }

        static void CheckSentioResp(RemoteCommandResult errc, string msg)
        {
            if (errc != RemoteCommandResult.NoError)
                throw new Exception($"Remote command error {errc}: {msg}");
        }

        static void Main(string[] args)
        {
            try
            {
                // Connect to SENTIO. Make sure SENTIO is running on the local PC and
                // is set up to listen at port 35555 (default port)
                Connect("127.0.0.1", 35555);

                // Ask SENTIO for self identification.
                // err, stat and cmdID will only be set when sending native SENTIO remote commands!
                // "*IDN?" is not a SENTIO remote command but a low level command.
                Send("*IDN?", out var msg);
                Console.WriteLine($"Remote command Response: {msg}");

                // Switch remote command set to SENTIO's native command set
                Send("*RCS 1"); // this command does not have a response!

                // select the wafermap module
                Send("select_module wafermap", out var err, out var stat, out var cmdId, out msg);
                CheckSentioResp(err, msg);

                // Set grid parameters
                Send("map:set_grid_params 40000, 40000, 0, 0, 4000", out err, out stat, out cmdId, out msg);
                CheckSentioResp(err, msg);

                // Step to first die
                // Note: This will only work if you have set a contact height and home position on the prober.
                Send("map:step_first_die", out err, out stat, out cmdId, out msg);
                CheckSentioResp(err, msg);

                // Step until last die state is signalled
                while (!stat.HasFlag(RemoteCommandStatus.LastDie))
                {
                    Send("map:step_next_die", out err, out stat, out cmdId, out msg);
                    CheckSentioResp(err, msg);
                }

                // Download image from the active camera (requires Sentio 23.0.6 or above)
                Send("vis:snap_image **download**, 0", out err, out stat, out cmdId, out msg);
                CheckSentioResp(err, msg);

                // decode jpeg data from base 64 string
                byte[] jpegData = Convert.FromBase64String(msg);
                File.WriteAllBytes("./image.jpg", jpegData);

                Console.WriteLine("Script finished!");
            }
            catch (Exception exc)
            {
                Console.WriteLine("\nError:");
                Console.WriteLine("------");
                Console.WriteLine(exc.Message);
            }
        }
    }
}
