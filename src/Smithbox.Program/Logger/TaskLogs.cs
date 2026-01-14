using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

/// <summary>
///     Used to log and display information for the user.
/// </summary>
public static class TaskLogs
{
    public static volatile List<LogEntry> _actionLog = new();
    public static volatile List<LogEntry> _warningLog = new();

    public static volatile HashSet<string> _warningList = new();

    public static volatile LogEntry _lastActionLogEntry;
    public static volatile LogEntry _lastWarningLogEntry;

    public static float _timerColorMult = 1.0f;

    public static bool _actionLog_ScrollToEnd;
    public static bool _warningLog_ScrollToEnd;

    private static SpinLock _spinLock = new(false);

    public static int _actionShowTime = 0;
    public static int _warningShowTime = 0;

    /// <summary>
    /// Adds a new entry to task logger.
    /// </summary>
    public static void AddLog(string text, LogLevel level = LogLevel.Information, LogPriority priority = LogPriority.Normal, Exception ex = null)
    {
        Task.Run(() =>
        {
            var lockTaken = false;
            try
            {
                // Wait until no other threads are using spinlock
                _spinLock.Enter(ref lockTaken);

                if (level is LogLevel.Warning or LogLevel.Error)
                {
                    _warningShowTime = CFG.Current.System_WarningLogger_FadeTime;

                    LogEntry lastLog = _warningLog.LastOrDefault();
                    if (lastLog != null)
                    {
                        if (lastLog.Message == text)
                        {
                            lastLog.MessageCount++;
                            if (priority != LogPriority.Low)
                            {
                                ResetColorTimer();
                            }
                            return;
                        }
                    }

                    LogEntry entry = new(text, level, priority);

                    if (ex != null)
                    {
                        if (text != ex.Message)
                        {
                            entry.Message += $"\n{ex.Message}";
                        }

                        _warningLog.Add(entry);
                        _warningLog.Add(new LogEntry($"{ex.StackTrace}",
                            level, LogPriority.Low));
                    }
                    else
                    {
                        _warningLog.Add(entry);
                    }

                    _warningLog_ScrollToEnd = true;

                    if (priority != LogPriority.Low)
                    {
                        _lastWarningLogEntry = entry;

                        if (priority == LogPriority.High)
                        {
                            var popupMessage = entry.Message;
                            if (ex != null)
                            {
                                popupMessage += $"\n{ex.StackTrace}";
                            }

                            //PlatformUtils.Instance.MessageBox(popupMessage, level.ToString(), MessageBoxButtons.OK);
                        }

                        ResetColorTimer();
                    }
                }
                else
                {
                    _actionShowTime = CFG.Current.System_ActionLogger_FadeTime;

                    LogEntry lastLog = _actionLog.LastOrDefault();
                    if (lastLog != null)
                    {
                        if (lastLog.Message == text)
                        {
                            lastLog.MessageCount++;
                            if (priority != LogPriority.Low)
                            {
                                ResetColorTimer();
                            }
                            return;
                        }
                    }

                    LogEntry entry = new(text, level, priority);

                    if (ex != null)
                    {
                        if (text != ex.Message)
                        {
                            entry.Message += $": {ex.Message}";
                        }

                        _actionLog.Add(entry);
                        _actionLog.Add(new LogEntry($"{ex.StackTrace}",
                            level, LogPriority.Low));
                    }
                    else
                    {
                        _actionLog.Add(entry);
                    }

                    _actionLog_ScrollToEnd = true;

                    if (priority != LogPriority.Low)
                    {
                        _lastActionLogEntry = entry;
                        if (level is LogLevel.Warning or LogLevel.Error)
                        {
                            _warningList.Add(text);
                        }

                        if (priority == LogPriority.High)
                        {
                            var popupMessage = entry.Message;
                            if (ex != null)
                            {
                                popupMessage += $"\n{ex.StackTrace}";
                            }

                            //PlatformUtils.Instance.MessageBox(popupMessage, level.ToString(), MessageBoxButtons.OK);
                        }

                        ResetColorTimer();
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit(false);
                }
            }
        });
    }

    public static void AddError(string text, Exception ex = null)
    {
        if(ex != null)
        {
            AddLog(text, LogLevel.Error, LogPriority.High, ex);
        }
        else
        {
            AddLog(text, LogLevel.Error, LogPriority.High);
        }
    }

    public static void AddInfo(string text, Exception ex = null)
    {
        if (ex != null)
        {
            AddLog(text, LogLevel.Information, LogPriority.High, ex);
        }
        else
        {
            AddLog(text, LogLevel.Information, LogPriority.High);
        }
    }

    public static void AddWarning(string text, Exception ex = null)
    {
        if (ex != null)
        {
            AddLog(text, LogLevel.Warning, LogPriority.High, ex);
        }
        else
        {
            AddLog(text, LogLevel.Warning, LogPriority.High);
        }
    }

    public static Vector4 PickColor(LogLevel? level)
    {
        var mult = 0.0f;
        if (level == null)
        {
            level = LogLevel.Information;
            mult = _timerColorMult;
        }

        var alpha = 1.0f - (0.3f * mult);
        if (level is LogLevel.Information)
        {
            return new Vector4(
                UI.Current.ImGui_Logger_Information_Color.X + (0.1f * mult),
                UI.Current.ImGui_Logger_Information_Color.Y - (0.1f * mult),
                UI.Current.ImGui_Logger_Information_Color.Z + (0.5f * mult),
                alpha);
        }

        if (level is LogLevel.Warning)
        {
            return new Vector4(
                UI.Current.ImGui_Logger_Warning_Color.X - (0.1f * mult),
                UI.Current.ImGui_Logger_Warning_Color.Y - (0.1f * mult),
                UI.Current.ImGui_Logger_Warning_Color.Z + (0.6f * mult),
                alpha);
        }

        if (level is LogLevel.Error or LogLevel.Critical)
        {
            return new Vector4(
                UI.Current.ImGui_Logger_Error_Color.X - (0.1f * mult),
                UI.Current.ImGui_Logger_Error_Color.Y + (0.6f * mult),
                UI.Current.ImGui_Logger_Error_Color.Z + (0.6f * mult),
                alpha);
        }

        return new Vector4(
            UI.Current.ImGui_Logger_Information_Color.X - (0.1f * mult),
            UI.Current.ImGui_Logger_Information_Color.Y - (0.1f * mult),
            UI.Current.ImGui_Logger_Information_Color.Z - (0.1f * mult),
            alpha);
    }

    /// <summary>
    ///     Manages color timer for last log in menu bar.
    /// </summary>
    private static void ResetColorTimer()
    {
        if (_timerColorMult == 1.0f)
        {
            // Color timer is not currently running, start it.
            Task.Run(() =>
            {
                // Time for task text color to transition completely (in miliseconds)
                const float transitionTime = 1000.0f;
                // Time for task text color to start transitioning (in miliseconds)
                const int transitionDelay = 4000;

                _timerColorMult = 0.0f;
                var prevMult = -1.0f;
                while (_timerColorMult < 1.0f)
                {
                    if (_timerColorMult != prevMult)
                    {
                        // Mult was just changed, sleep for initial delay.
                        Thread.Sleep(transitionDelay);
                    }

                    _timerColorMult += 1.0f / transitionTime;
                    prevMult = _timerColorMult;
                    Thread.Sleep(1);
                }

                _timerColorMult = 1.0f;
            });
        }
        else
        {
            // Color timer is currently running, reset time.
            _timerColorMult = 0.0f;
        }
    }

    public class LogEntry
    {
        public LogLevel Level;

        /// <summary>
        ///     Log message.
        /// </summary>
        public string Message;

        /// <summary>
        ///     Number of messages this LogEntry represents.
        /// </summary>
        public uint MessageCount = 1;

        public LogPriority Priority = LogPriority.Normal;

        /// <summary>
        ///     Time which log was created
        /// </summary>
        public DateTime LogTime;

        /// <summary>
        ///     Log message with additional formatting and info.
        /// </summary>
        public string FormattedMessage
        {
            get
            {
                var mes = Message;
                if (MessageCount > 1)
                {
                    mes += $" x{MessageCount}";
                }

                mes = $"[{LogTime.Hour:D2}:{LogTime.Minute:D2}:{LogTime.Second:D2}] {mes}";

                return mes;
            }
        }

        public LogEntry(string message, LogLevel level)
        {
            Message = message;
            Level = level;
            LogTime = DateTime.Now;
        }

        public LogEntry(string message, LogLevel level, LogPriority priority)
        {
            Message = message;
            Level = level;
            Priority = priority;
            LogTime = DateTime.Now;
        }
    }
}
