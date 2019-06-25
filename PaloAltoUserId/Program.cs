using System;
using System.ServiceProcess;
using org.aha_net.Logging;

namespace org.aha_net.PaloAltoUserId
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnCrash);

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new PaloAltoUserId(args) 
            };
            ServiceBase.Run(ServicesToRun);
        }

        static void OnCrash(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception) e.ExceptionObject;

            if(e.IsTerminating)
            {
                Log.Error(">>>>> CRASHING <<<<<");
                Log.Error("Fatal Unhandled Exception: " + ex.ToString());

                if (PaloAltoUserId.cts != null) PaloAltoUserId.cts.Cancel();

                Log.RemoveAll();

                Console.Error.WriteLine("Fatal Unhandled Exception: " + ex.ToString());
                Console.Error.Flush();
                Console.Out.Flush();

                Environment.Exit(5);
            }
            else
            {
                Log.Error("Non-fatal Unhandled Exception: " + ex.ToString());

                Log.FlushAll();

                Console.Error.WriteLine("Non-fatal Unhandled Exception: " + ex.ToString());
                Console.Error.Flush();
                Console.Out.Flush();
            }
        }
    }
}
