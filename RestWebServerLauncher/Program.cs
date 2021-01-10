using RestWebServer;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace RestWebServerLauncher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener() { TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ThreadId });
            new MessageServer(new WebServer(new IPEndPoint(IPAddress.Any, 2200))).Start();
            Thread.CurrentThread.Join();
        }
    }
}
