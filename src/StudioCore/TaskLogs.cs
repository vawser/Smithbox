using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore;

/// <summary>
///     Used to log and display information for the user.
/// </summary>
public static class TaskLogs
{
    /// <summary>
    ///     Priority of log message. Affects how log is conveyed to the user.
    /// </summary>
    public enum LogPriority
    {
        /// <summary>
        ///     Log will be present in Logger window.
        /// </summary>
        Low,

        /// <summary>
        ///     Log will be present in Menu bar + warning list, logger window.
        /// </summary>
        Normal,

        /// <summary>
        ///     Log will be present in message box, menu bar + warning list, logger window.
        /// </summary>
        High
    }

    private static volatile List<LogEntry> _log = new();
    private static volatile HashSet<string> _warningList = new();

    private static volatile LogEntry _lastLogEntry;
#if DEBUG
    private static bool _showDebugLogs = true;
# else
    private static bool _showDebugLogs = false;
#endif

    /// <summary>
    ///     Multiply text color values. Mult transitions from 0 to 1 during transition timer.
    /// </summary>
    private static float _timerColorMult = 1.0f;

    private static bool _loggerWindowOpen;
    private static bool _scrollToEnd;
    private static SpinLock _spinlock = new(false);

    /// <summary>
    ///     Adds a new entry to task logger.
    /// </summary>
    /// <param name="text">Text to add to log.</param>
    /// <param name="level">Type of entry. Affects text color.</param>
    public static void AddLog(string text, LogLevel level = LogLevel.Information,
        LogPriority priority = LogPriority.Normal, Exception ex = null)
    {
        if (level == LogLevel.Debug && !_showDebugLogs)
        {
            return;
        }

        Task.Run(() =>
        {
            var lockTaken = false;
            try
            {
                // Wait until no other threads are using spinlock
                _spinlock.Enter(ref lockTaken);

                LogEntry lastLog = _log.LastOrDefault();
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

                    _log.Add(entry);
                    _log.Add(new LogEntry($"{ex.StackTrace}",
                        level, LogPriority.Low));
                }
                else
                {
                    _log.Add(entry);
                }

                _scrollToEnd = true;

                if (priority != LogPriority.Low)
                {
                    _lastLogEntry = entry;
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

                        PlatformUtils.Instance.MessageBox(popupMessage, level.ToString(),
                            MessageBoxButtons.OK);
                    }

                    ResetColorTimer();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _spinlock.Exit(false);
                }
            }
        });
    }

    public static void Display()
    {
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Warning List
        if (_warningList.Count > 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0f, 0f, 1.0f));
            if (ImGui.BeginMenu("!! WARNINGS!! "))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                ImGui.Text("Click warnings to remove them from list");
                if (ImGui.Button("Remove All Warnings"))
                {
                    _warningList.Clear();
                }

                ImGui.Separator();
                foreach (var text in _warningList)
                {
                    if (ImGui.Selectable(text, false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        _warningList.Remove(text);
                        break;
                    }
                }

                ImGui.PopStyleColor();
                ImGui.EndMenu();
            }

            ImGui.PopStyleColor();
        }

        // Logger
        var dir = ImGuiDir.Right;
        if (_loggerWindowOpen)
        {
            dir = ImGuiDir.Down;
        }

        if (ImGui.ArrowButton("##ShowLogsBtn", dir))
        {
            _loggerWindowOpen = !_loggerWindowOpen;
        }

        if (_loggerWindowOpen)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 0.98f));
            ImGui.PushStyleColor(ImGuiCol.TitleBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.1f, 0.1f, 0.1f, 0.98f));
            if (ImGui.Begin("Logger##TaskLogger", ref _loggerWindowOpen, ImGuiWindowFlags.NoDocking))
            {
                if (ImGui.Button("Clear##TaskLogger"))
                {
                    _log.Clear();
                    _lastLogEntry = null;
                    AddLog("Log cleared");
                }

                ImGui.SameLine();
                ImGui.Checkbox("Log debug messages", ref _showDebugLogs);

                ImGui.BeginChild("##LogItems");
                ImGui.Spacing();
                for (var i = 0; i < _log.Count; i++)
                {
                    ImGui.Indent();
                    ImGui.TextColored(PickColor(_log[i].Level), _log[i].FormattedMessage);
                    ImGui.Unindent();
                }

                if (_scrollToEnd)
                {
                    ImGui.SetScrollHereY();
                    _scrollToEnd = false;
                }

                ImGui.Spacing();
                ImGui.EndChild();
            }

            ImGui.End();
            ImGui.PopStyleColor(4);
        }

        if (_lastLogEntry != null)
        {
            Vector4 color = PickColor(null);
            ImGui.TextColored(color, _lastLogEntry.FormattedMessage);
        }
    }

    private static Vector4 PickColor(LogLevel? level)
    {
        var mult = 0.0f;
        if (level == null)
        {
            level = _lastLogEntry.Level;
            mult = _timerColorMult;
        }

        var alpha = 1.0f - (0.3f * mult);
        if (level is LogLevel.Information)
        {
            return new Vector4(
                0.8f + (0.1f * mult),
                1.0f - (0.1f * mult),
                0.4f + (0.5f * mult),
                alpha);
        }

        if (level is LogLevel.Warning)
        {
            return new Vector4(
                1.0f - (0.1f * mult),
                1.0f - (0.1f * mult),
                0.3f + (0.6f * mult),
                alpha);
        }

        if (level is LogLevel.Error or LogLevel.Critical)
        {
            return new Vector4(
                1.0f - (0.1f * mult),
                0.3f + (0.6f * mult),
                0.3f + (0.6f * mult),
                alpha);
        }

        return new Vector4(
            1.0f - (0.1f * mult),
            1.0f - (0.1f * mult),
            1.0f - (0.1f * mult),
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
                mes = $"[{LogTime.Hour:D2}:{LogTime.Minute:D2}] {mes}";

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
