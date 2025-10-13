using StudioCore;
using StudioCore.Graphics;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SmithboxApp;

public static class Program
{
    private static string _version = "undefined";

    private static bool IsLowRequirements = false;

    private static Smithbox Instance;

    private static bool IsDebug = false;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        #if DEBUG
        IsDebug = true;
        #endif

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-low-requirements":
                    IsLowRequirements = true;
                    break;
            }
        }

        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += CrashHandler;

        Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException());

        _version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "undefined";

        if(IsLowRequirements)
        {
            Instance = new Smithbox(new OpenGLCompatGraphicsContext(), _version, IsLowRequirements);
        }
        else
        {
            Instance = new Smithbox(new VulkanGraphicsContext(), _version, IsLowRequirements);
        }

        if (Instance != null)
        {
            if (IsDebug)
            {
                try
                {
                    Instance.Run();
                }
                catch
                {
                    Instance.AttemptSaveOnCrash();
                    Instance.CrashShutdown();

                    throw;
                }
            }
            else
            {
                Instance.Run();
            }
        }
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

#if MACOS
    private static readonly string CrashLogPath = Path.GetFullPath(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "../Logs/Smithbox/Crash Logs"));
#else
    private static readonly string CrashLogPath = Path.Join(Directory.GetCurrentDirectory(), "Crash Logs");
#endif
    static void ExportCrashLog(List<string> exceptionInfo)
    {
        var time = $"{DateTime.Now:yyyy-M-dd--HH-mm-ss}";
        exceptionInfo.Insert(0, $"Smithbox - Version {_version}\n");
        Directory.CreateDirectory($"{CrashLogPath}");
        var crashLogPath = Path.Join(CrashLogPath, $"Log {time}.txt");
        File.WriteAllLines(crashLogPath, exceptionInfo);

        if (exceptionInfo.Count > 10)
            PlatformUtils.Instance.MessageBox($"Smithbox has run into an issue.\nCrash log has been generated at \"{crashLogPath}\".",
                $"Smithbox Unhandled Error - {_version}", MessageBoxButtons.OK, MessageBoxIcon.Error);
        else
            PlatformUtils.Instance.MessageBox($"Smithbox has run into an issue.\nCrash log has been generated at \"{crashLogPath}\".\n\nCrash Log:\n{string.Join("\n", exceptionInfo)}",
                $"Smithbox Unhandled Error - {_version}", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
