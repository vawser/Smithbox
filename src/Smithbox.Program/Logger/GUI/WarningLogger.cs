using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class WarningLogger
{
    public bool IsDisplayed = false;
    private ImGuiDir CurrentDir = ImGuiDir.Right;
    private bool InitialLayout = false;

    public WarningLogger() { }

    public void DisplayTopbarToggle()
    {
        if (CFG.Current.Logger_Enable_Warning_Log)
        {
            if (ImGui.ArrowButton("##WarningLoggerToggle", CurrentDir))
            {
                if (CurrentDir == ImGuiDir.Right)
                {
                    CurrentDir = ImGuiDir.Down;
                    IsDisplayed = true;
                }
                else
                {
                    CurrentDir = ImGuiDir.Right;
                    IsDisplayed = false;
                }
            }
            UIHelper.Tooltip("Toggle the display of the Warning logger.");

            // Only show the warning for X frames in the menu bar
            if (TaskLogs._lastWarningLogEntry != null)
            {
                if (TaskLogs._warningShowTime > 0 || CFG.Current.Logger_Warning_Fade_Time < 0)
                {
                    if (CFG.Current.Logger_Warning_Fade_Time > 0)
                    {
                        TaskLogs._warningShowTime--;
                    }

                    Vector4 color = TaskLogs.PickColor(TaskLogs._lastWarningLogEntry.Level);
                    ImGui.TextColored(color, TaskLogs._lastWarningLogEntry.FormattedMessage);
                }
            }
        }
    }

    public void Draw()
    {
        if (IsDisplayed)
        {
            if (!InitialLayout)
            {
                UIHelper.SetupPopupWindow();
                InitialLayout = true;
            }

            ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_ChildBg);

            if (ImGui.Begin("Warning Logs##WarningTaskLogger", ref IsDisplayed, UIHelper.GetPopupWindowFlags()))
            {
                DisplayWarningLoggerContents();

                ImGui.End();
            }
            else
            {
                UIHelper.SetupPopupWindow();
            }

            ImGui.PopStyleColor(1);
        }
    }

    public void DisplayWarningLoggerContents()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.Button("Clear##WarningTaskLogger", DPI.HalfWidthButton(windowWidth, 24)))
        {
            TaskLogs._warningLog.Clear();
            TaskLogs._lastWarningLogEntry = null;
        }

        ImGui.SameLine();
        if (ImGui.Button("Copy to Clipboard##WarningTaskLogger", DPI.HalfWidthButton(windowWidth, 24)))
        {
            string contents = "";
            foreach (var entry in TaskLogs._warningLog)
            {
                contents = contents + $"{entry.FormattedMessage}\n";
            }

            PlatformUtils.Instance.SetClipboardText($"{contents}");
        }

        ImGui.BeginChild("##WarningLogItems");
        ImGui.Spacing();
        for (var i = 0; i < TaskLogs._warningLog.Count; i++)
        {
            ImGui.Indent();
            ImGui.TextColored(TaskLogs.PickColor(TaskLogs._warningLog[i].Level), TaskLogs._warningLog[i].FormattedMessage);
            ImGui.Unindent();
        }

        if (TaskLogs._warningLog_ScrollToEnd)
        {
            ImGui.SetScrollHereY();
            TaskLogs._warningLog_ScrollToEnd = false;
        }

        ImGui.Spacing();
        ImGui.EndChild();
    }
}
