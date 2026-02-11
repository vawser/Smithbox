#nullable enable

using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Logger.GUI
{
    public class LoggerUI : IDisposable
    {
        private static int _nextId = 0;
        /// <summary>
        /// The unique id of this instance.
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// The prefix name used for ImGui string ids.
        /// </summary>
        public readonly string ImguiName;
        /// <summary>
        /// The list of log entries to display.
        ///
        /// This must only be modified and read on the UI thread. It is not thread-safe.
        /// </summary>
        public List<LogEntry> LogEntries = [];
        /// <summary>
        /// Whether the log window is to be rendered.
        /// </summary>
        public bool IsDisplayed = false;
        /// <summary>
        /// Whether we need to set up the ImGui window.
        /// </summary>
        private bool InitialLayout = false;
        /// <summary>
        /// Whether to hide the last log entry after ShowTime expires.
        /// </summary>
        public bool UseShowTimer = false;
        /// <summary>
        /// Whether we should scroll to the end of the log entries on next draw.
        /// </summary>
        public bool ScrollToEnd = false;
        public LogEntry? LastLogEntry = null;
        private LogSubscription _subscription;
        public string Name;
        private Func<(bool enabled, int fadeTime, bool fadeColor)> _getSettings;

        private string _windowName;
        
        public LoggerUI(string loggerName, Func<LogEvent, bool> filter, Func<(bool enabled, int fadeTime, bool fadeColor)> getSettings)
        {
            Name = loggerName;
            _getSettings = getSettings;
            _subscription = TaskLogs.Subscribe(filter);
            Id = Interlocked.Increment(ref _nextId);
            ImguiName = $"Logger_{Id}_{Name}";
            _windowName = $"{Name}##loggerWindow_{ImguiName}";
        }
        
        /// <summary>
        /// Drains new log messages from the subscription queue and adds them to the log entries.
        /// Should be called on the UI thread.
        /// </summary>
        public void DrainMessages()
        {
            //drain new log entries
            while (_subscription.TryDequeue(out var logEvent))
            {
                if (LastLogEntry?.LogEvent.Message == logEvent.Message)
                {
                    var newEntry = LogEntry.Create(logEvent, LastLogEntry.Count + 1);
                    LogEntries[^1] = newEntry;
                    LastLogEntry = newEntry;
                }
                else
                {
                    var logEntry = LogEntry.Create(logEvent);
                    LogEntries.Add(logEntry);
                    LastLogEntry = logEntry;
                }

                ScrollToEnd = true;
            }
        }

        /// <summary>
        /// Displays the arrow button (to toggle the log window) and the last log entry.
        /// </summary>
        public void DrawTopBar()
        {
            var cfg = _getSettings();
            UseShowTimer = cfg.fadeTime >= 0;
            
            if (!cfg.enabled)
            {
                return;
            }
            
            ImGui.PushID(ImguiName);
            //draw arrow button and last log entry
            if (ImGui.ArrowButton("##LoggerWindowToggle", IsDisplayed ? ImGuiDir.Down : ImGuiDir.Right))
            {
                IsDisplayed = !IsDisplayed;
            }
            UIHelper.Tooltip($"Toggle the display of the {Name} logger.");

            //fade time was in frames before, but we need milliseconds, so we need to convert it based on 60fps avg
            var last = LastLogEntry;
            if (last != null)
            {
                var timeDiff = DateTime.Now - last.LogEvent.Time;

                if (!UseShowTimer || timeDiff.TotalMilliseconds < cfg.fadeTime)
                {
                    Vector4 color = PickColor(last.LogEvent.Level, timeDiff, cfg.fadeTime, cfg.fadeColor, 0.3f);
                    ImGui.TextColored(color, last.FormattedMessage);
                }
            }
            ImGui.PopID();
        }
        
        public void DrawWindow()
        {
            if (IsDisplayed)
            {
                var cfg = _getSettings();
                if (!InitialLayout)
                {
                    UIHelper.SetupPopupWindow();
                    InitialLayout = true;
                }
                
                ImGui.PushID(ImguiName);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_ChildBg);

                if (ImGui.Begin(_windowName, ref IsDisplayed, UIHelper.GetPopupWindowFlags()))
                {
                    if (ImGui.Button("Clear"))
                    {
                        LogEntries.Clear();
                        LastLogEntry = null;
                    }
                    UIHelper.Tooltip("Clear all log entries.");

                    ImGui.SameLine();

                    if (ImGui.Button("Copy to Clipboard"))
                    {
                        var allLogs = string.Join(Environment.NewLine, LogEntries.ConvertAll(le => le.FormattedMessage));
                        PlatformUtils.Instance.SetClipboardText(allLogs);
                    }
                    UIHelper.Tooltip("Copy all log entries to clipboard.");

                    // Log entries
                    ImGui.Separator();
                    ImGui.BeginChild("##LogItems");
                    ImGui.Spacing();
                    
                    var now = DateTime.Now;
                    foreach (var logEntry in LogEntries)
                    {
                        Vector4 color = PickColor(logEntry.LogEvent.Level, now - logEntry.LogEvent.Time, cfg.fadeTime, cfg.fadeColor, 0.1f);
                        ImGui.TextColored(color, logEntry.FormattedMessage);
                    }
                    
                    if (ScrollToEnd)
                    {
                        ImGui.SetScrollHereY();
                        ScrollToEnd = false;
                    }
                    ImGui.Spacing();
                    ImGui.EndChild();
                }

                ImGui.End();
                ImGui.PopStyleColor();
                ImGui.PopID();
            }
        }
        
        public void Dispose()
        {
            TaskLogs.Unsubscribe(_subscription);
        }
        
        // =============statics=============

        /// <summary>
        /// Picks a color based on the log level and current color multiplier.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="timeDiff">The time difference between now and when the log entry was created.</param>
        /// <param name="fadeTime">The time it takes for the color to fade to the faded color.</param>
        /// <returns>The color in RGBA</returns>
        public static Vector4 PickColor(LogLevel? level, TimeSpan timeDiff, int fadeTime, bool fadeColor, float maxFadeFactor)
        {
            //-1 means no fade
            fadeColor = fadeColor && fadeTime >= 0;
            
            var mult = fadeColor ? 1f : 0f;
            var diffMs = timeDiff.TotalMilliseconds;
            if (fadeColor && diffMs < fadeTime)
            {
                mult = Math.Clamp((float)diffMs / fadeTime, 0f, 1f);
            }
            level ??= LogLevel.Information;

            var alpha = 1.0f - (maxFadeFactor * mult);
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
                    UI.Current.ImGui_Logger_Warning_Color.X - (0.2f * mult),
                    UI.Current.ImGui_Logger_Warning_Color.Y - (0.1f * mult),
                    UI.Current.ImGui_Logger_Warning_Color.Z - (0.1f * mult),
                    alpha);
            }

            if (level is LogLevel.Error or LogLevel.Critical)
            {
                return new Vector4(
                    UI.Current.ImGui_Logger_Error_Color.X - (0.1f * mult),
                    UI.Current.ImGui_Logger_Error_Color.Y - (0.1f * mult),
                    UI.Current.ImGui_Logger_Error_Color.Z - (0.1f * mult),
                    alpha);
            }

            return new Vector4(
                UI.Current.ImGui_Logger_Information_Color.X - (0.1f * mult),
                UI.Current.ImGui_Logger_Information_Color.Y - (0.1f * mult),
                UI.Current.ImGui_Logger_Information_Color.Z - (0.1f * mult),
                alpha);
        }
    }

    public sealed record LogEntry(LogEvent LogEvent, string FormattedMessage, int Count = 1)
    {
        public static LogEntry Create(LogEvent logEvent, int count = 1)
        {
            string msg = logEvent.Message;

            if (logEvent.Exception is Exception ex && msg != ex.Message)
            {
                msg += $"\n{ex.Message}\n{ex.StackTrace}";
            }

            var times = count == 1 ? "" : $" [x{count}]";
            var scopeName = logEvent.Category;
            var formatted =
                $"[{logEvent.Time:HH:mm:ss}]{times} [{scopeName}] {msg}";

            return new(logEvent, formatted, count);
        }
    }
}