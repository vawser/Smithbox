using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Settings;
using StudioCore.Tasks;
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
    private static volatile List<LogEntry> _actionLog = new();
    private static volatile List<LogEntry> _warningLog = new();

    private static volatile HashSet<string> _warningList = new();

    private static volatile LogEntry _lastActionLogEntry;
    private static volatile LogEntry _lastWarningLogEntry;

    private static float _timerColorMult = 1.0f;

    private static bool _actionLog_ScrollToEnd;
    private static bool _warningLog_ScrollToEnd;

    private static SpinLock _spinLock = new(false);

    private static int _actionShowTime = 0;
    private static int _warningShowTime = 0;

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

    private static ImGuiDir ActionLogger_CurrentDir = ImGuiDir.Right;
    private static bool ActionLogger_WindowOpen = false;

    /// <summary>
    /// Top Bar Logger: Actions
    /// </summary>
    public static void DisplayActionLoggerBar()
    {
        if (CFG.Current.System_ShowActionLogger)
        {
            if (ImGui.ArrowButton("##actionLoggerToggle", ActionLogger_CurrentDir))
            {
                if (ActionLogger_CurrentDir == ImGuiDir.Right)
                {
                    ActionLogger_CurrentDir = ImGuiDir.Down;
                    ActionLogger_WindowOpen = true;
                }
                else
                {
                    ActionLogger_CurrentDir = ImGuiDir.Right;
                    ActionLogger_WindowOpen = false;
                }
            }
            UIHelper.Tooltip("Toggle the display of the action logger.");

            // Only show the warning for X frames in the menu bar
            if (_lastActionLogEntry != null)
            {
                if (_actionShowTime > 0 || CFG.Current.System_ActionLogger_FadeTime < 0)
                {
                    if (CFG.Current.System_ActionLogger_FadeTime > 0)
                    {
                        _actionShowTime--;
                    }

                    Vector4 color = PickColor(_lastActionLogEntry.Level);
                    ImGui.TextColored(color, _lastActionLogEntry.FormattedMessage);
                }
            }
        }
    }

    /// <summary>
    /// Action Logger window
    /// </summary>
    public static void DisplayActionLoggerWindow()
    {
        if (ActionLogger_WindowOpen)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
            ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            if (ImGui.Begin("Action Logs##actionTaskLogger", ref ActionLogger_WindowOpen, ImGuiWindowFlags.NoDocking))
            {
                if (ImGui.Button("Clear##actionTaskLogger"))
                {
                    _actionLog.Clear();
                    _lastActionLogEntry = null;
                }

                ImGui.SameLine();
                if (ImGui.Button("Copy to Clipboard##actionTaskLogger"))
                {
                    string contents = "";
                    foreach (var entry in _actionLog)
                    {
                        contents = contents + $"{entry.FormattedMessage}\n";
                    }

                    PlatformUtils.Instance.SetClipboardText($"{contents}");
                }

                ImGui.BeginChild("##actionLogItems");
                ImGui.Spacing();
                for (var i = 0; i < _actionLog.Count; i++)
                {
                    ImGui.Indent();
                    ImGui.TextColored(PickColor(_actionLog[i].Level), _actionLog[i].FormattedMessage);
                    ImGui.Unindent();
                }

                if (_actionLog_ScrollToEnd)
                {
                    ImGui.SetScrollHereY();
                    _actionLog_ScrollToEnd = false;
                }

                ImGui.Spacing();
                ImGui.EndChild();
            }

            ImGui.End();
            ImGui.PopStyleColor(5);
        }
    }

    private static ImGuiDir WarningLogger_CurrentDir = ImGuiDir.Right;
    private static bool WarningLogger_WindowOpen = false;

    /// <summary>
    /// Top Bar Logger: Warnings
    /// </summary>
    public static void DisplayWarningLoggerBar()
    {
        if (CFG.Current.System_ShowWarningLogger)
        {
            if (ImGui.ArrowButton("##warningLoggerToggle", WarningLogger_CurrentDir))
            {
                if (WarningLogger_CurrentDir == ImGuiDir.Right)
                {
                    WarningLogger_CurrentDir = ImGuiDir.Down;
                    WarningLogger_WindowOpen = true;
                }
                else
                {
                    WarningLogger_CurrentDir = ImGuiDir.Right;
                    WarningLogger_WindowOpen = false;
                }
            }
            UIHelper.Tooltip("Toggle the display of the warning logger.");

            if (_lastWarningLogEntry != null)
            {
                // Only show the warning for X frames in the menu bar
                if (_warningShowTime > 0 || CFG.Current.System_WarningLogger_FadeTime < 0)
                {
                    if (CFG.Current.System_WarningLogger_FadeTime > 0)
                    {
                        _warningShowTime--;
                    }

                    Vector4 color = PickColor(_lastWarningLogEntry.Level);
                    ImGui.TextColored(color, _lastWarningLogEntry.FormattedMessage);
                }
            }
        }
    }

    /// <summary>
    /// Action Logger window
    /// </summary>
    public static void DisplayWarningLoggerWindow()
    {
        if (WarningLogger_WindowOpen)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
            ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            if (ImGui.Begin("Warning Logs##warningTaskLogger", ref WarningLogger_WindowOpen, ImGuiWindowFlags.NoDocking))
            {
                if (ImGui.Button("Clear##warningTaskLogger"))
                {
                    _warningLog.Clear();
                    _lastWarningLogEntry = null;
                }

                ImGui.SameLine();
                if (ImGui.Button("Copy to Clipboard##warningTaskLogger"))
                {
                    string contents = "";
                    foreach (var entry in _warningLog)
                    {
                        contents = contents + $"{entry.FormattedMessage}\n";
                    }

                    PlatformUtils.Instance.SetClipboardText($"{contents}");
                }

                ImGui.BeginChild("##warningLogItems");
                ImGui.Spacing();
                for (var i = 0; i < _warningLog.Count; i++)
                {
                    ImGui.Indent();
                    ImGui.TextColored(PickColor(_warningLog[i].Level), _warningLog[i].FormattedMessage);
                    ImGui.Unindent();
                }

                if (_warningLog_ScrollToEnd)
                {
                    ImGui.SetScrollHereY();
                    _warningLog_ScrollToEnd = false;
                }

                ImGui.Spacing();
                ImGui.EndChild();
            }

            ImGui.End();
            ImGui.PopStyleColor(5);
        }
    }

    private static Vector4 PickColor(LogLevel? level)
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
