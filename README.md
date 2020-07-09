# Examples for controlling MPI SENTIO with C#
A basic example for demonstrating how to remotely control SENTIO with TCP/Ip

```
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
        Send("map:step_first_die", out err, out stat, out cmdId, out msg);
        CheckSentioResp(err, msg);

        // Step until last die state is signalled
        while (!stat.HasFlag(RemoteCommandStatus.LastDie))
        {
            Send("map:step_next_die", out err, out stat, out cmdId, out msg);
            CheckSentioResp(err, msg);
        }

        Console.WriteLine("Script finished!");
    }
    catch (Exception exc)
    {
        Console.WriteLine("\nError:");
        Console.WriteLine("------");
        Console.WriteLine(exc.Message);
    }
}
 ```
