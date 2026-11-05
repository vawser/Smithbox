using StudioCore;
using StudioCore.Application;
using StudioCore.Interface;
using StudioCore.Renderer;

using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Tracy;

namespace SmithboxApp;

public static unsafe class Program
{
    private static string _version = "undefined";

    private static Smithbox Instance;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Registered first, so a failure while reading the startup configuration or localization still leaves a crash log
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += CrashHandler;

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        _version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "undefined";

        Startup.Setup();
        Startup.Load();

        LOC.Setup();
        LOC.Load();

        Profiler.Startup();

        Instance = new Smithbox(_version, RenderingBackend.OpenGL, ProgramType.Editor);

        try
        {
            Instance.Run();
        }
        catch
        {
            // Shutting down can fail as well, which must not hide what caused the crash
            try
            {
                Instance.AttemptSaveOnCrash();
                Instance.CrashShutdown();
            }
            catch (Exception shutdownException)
            {
                Console.WriteLine("Crash shutdown failed : " + shutdownException.Message);
            }

            // Rethrown so the crash handler writes the crash log
            throw;
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

    // The executable's folder, rather than the working directory, which can still be the launching folder here
    private static readonly string CrashLogPath = Path.Join(AppContext.BaseDirectory, "Crash Logs");
    static void ExportCrashLog(List<string> exceptionInfo)
    {
        var time = $"{DateTime.Now:yyyy-M-dd--HH-mm-ss}";
        exceptionInfo.Insert(0, $"Smithbox - Version {_version}\n");
        Directory.CreateDirectory($"{CrashLogPath}");
        var crashLogPath = Path.Join(CrashLogPath, $"Log {time}.txt");
        File.WriteAllLines(crashLogPath, exceptionInfo);

        try
        {
            // A crash before the Smithbox instance exists has no dialog helper yet
            if (PlatformUtils.Instance == null)
                PlatformUtils.InitializeWindows(null);

            if (exceptionInfo.Count > 10)
                PlatformUtils.Instance.MessageBox($"Smithbox has run into an issue.\nCrash log has been generated at \"{crashLogPath}\".",
                    $"Smithbox Unhandled Error - {_version}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                PlatformUtils.Instance.MessageBox($"Smithbox has run into an issue.\nCrash log has been generated at \"{crashLogPath}\".\n\nCrash Log:\n{string.Join("\n", exceptionInfo)}",
                    $"Smithbox Unhandled Error - {_version}", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception dialogException)
        {
            // The crash log is already written; the dialog is only a notice
            Console.WriteLine($"Crash log written to \"{crashLogPath}\", but the dialog failed : " + dialogException.Message);
        }
    }


    static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = args.ExceptionObject as Exception
            ?? new Exception($"Non-exception object thrown: {args.ExceptionObject}");
        Console.WriteLine("Crash caught : " + e.Message);
        Console.WriteLine("Stack Trace : " + e.StackTrace);
        Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);

        try
        {
            List<string> log = LogExceptions(e);
            ExportCrashLog(log);
        }
        catch (Exception logException)
        {
            Console.WriteLine("Failed to write the crash log : " + logException.Message);
        }
    }
}
