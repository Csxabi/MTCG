using MtcgServer.Databases.Postgres;
using RestWebServer;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace MtcgLauncher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener() { TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ThreadId });
            var db = new PostgreSqlDatabase("Server=127.0.0.1; Port=5432; Database=mtcg; User Id=mtcg_user; Password=mtcg_pass");
            var web = new MtcgWebServer(new WebServer(new IPEndPoint(IPAddress.Any, 10001)), db, new MtcgServer.BattleHandlers.CardImplHandler(db));
            web.Setup();
            web.Start();
            Thread.CurrentThread.Join();
        }
    }
}
