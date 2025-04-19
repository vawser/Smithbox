using StudioCore;
using StudioCore.Graphics;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Smithbox_Program_OpenGL
{
    public static class Program
    {
        private static string Version = "2.0.0";
        private static Smithbox StudioCore;

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CrashHandler;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException());

            Smithbox.LowRequirementsMode = true;
            StudioCore = new Smithbox(new OpenGLCompatGraphicsContext());
            StudioCore.Run();
        }

        static List<string> LogExceptions(Exception ex)
        {
            List<string> log = new();
            do
            {
                if (ex is AggregateException ae)
                {
                    if (ae.InnerExceptions.Count == 1)
                        ex = ae.InnerException;
                    else
                        ex = ae.Flatten();
                }
                log.Add($"{ex.Message}\n");
                log.Add(ex.StackTrace);
                ex = ex.InnerException;
                log.Add("----------------------\n");
            }
            while (ex != null);
            log.RemoveAt(log.Count - 1);
            return log;
        }


        static readonly string CrashLogPath = $"{Directory.GetCurrentDirectory()}\\Crash Logs";
        static void ExportCrashLog(List<string> exceptionInfo)
        {
            var time = $"{DateTime.Now:yyyy-M-dd--HH-mm-ss}";
            exceptionInfo.Insert(0, $"Smithbox Version {Version}\n");
            Directory.CreateDirectory($"{CrashLogPath}");
            var crashLogPath = $"{CrashLogPath}\\Log {time}.txt";
            File.WriteAllLines(crashLogPath, exceptionInfo);

            if (exceptionInfo.Count > 10)
                PlatformUtils.Instance.MessageBox($"Smithbox has run into an issue.\nCrash log has been generated at \"{crashLogPath}\".",
                    $"Smithbox Unhandled Error - {Version}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                PlatformUtils.Instance.MessageBox($"Smithbox has run into an issue.\nCrash log has been generated at \"{crashLogPath}\".\n\nCrash Log:\n{string.Join("\n", exceptionInfo)}",
                    $"Smithbox Unhandled Error - {Version}", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("Crash caught : " + e.Message);
            Console.WriteLine("Stack Trace : " + e.StackTrace);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);

            List<string> log = LogExceptions(e);
            ExportCrashLog(log);
        }
    }
}
