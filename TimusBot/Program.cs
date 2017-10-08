using System;
using System.Threading;
using TimusLib;

namespace TimusBot
{
    static class Program
    {
        static void Main()
        {
            Logger.InitLogger();

            while (true)
            {
                try
                {
                    new Bot().Run().Wait();
                }
                catch (Exception e)
                {
                    Logger.Log.Error("FAILED-TO-RUN-BOT", e);
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            }
        }
    }
}
