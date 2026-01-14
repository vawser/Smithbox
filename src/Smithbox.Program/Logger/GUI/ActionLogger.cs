using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class ActionLogger
{
    public bool IsDisplayed = false;
    private ImGuiDir CurrentDir = ImGuiDir.Right;
    private bool InitialLayout = false;

    public ActionLogger() { }

    public void DisplayTopbarToggle()
    {
        if (CFG.Current.System_ShowActionLogger)
        {
            if (ImGui.ArrowButton("##actionLoggerToggle", CurrentDir))
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
            UIHelper.Tooltip("Toggle the display of the action logger.");

            // Only show the warning for X frames in the menu bar
            if (TaskLogs._lastActionLogEntry != null)
            {
                if (TaskLogs._actionShowTime > 0 || CFG.Current.System_ActionLogger_FadeTime < 0)
                {
                    if (CFG.Current.System_ActionLogger_FadeTime > 0)
                    {
                        TaskLogs._actionShowTime--;
                    }

                    Vector4 color = TaskLogs.PickColor(TaskLogs._lastActionLogEntry.Level);
                    ImGui.TextColored(color, TaskLogs._lastActionLogEntry.FormattedMessage);
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

            if (ImGui.Begin("Action Logs##actionTaskLogger", ref IsDisplayed, UIHelper.GetPopupWindowFlags()))
            {
                DisplayActionLoggerContents();

                ImGui.End();
            }
            else
            {
                UIHelper.SetupPopupWindow();
            }

            ImGui.PopStyleColor(1);
        }
    }

    public void DisplayActionLoggerContents()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.Button("Clear##actionTaskLogger", DPI.HalfWidthButton(windowWidth, 24)))
        {
            TaskLogs._actionLog.Clear();
            TaskLogs._lastActionLogEntry = null;
        }

        ImGui.SameLine();
        if (ImGui.Button("Copy to Clipboard##actionTaskLogger", DPI.HalfWidthButton(windowWidth, 24)))
        {
            string contents = "";
            foreach (var entry in TaskLogs._actionLog)
            {
                contents = contents + $"{entry.FormattedMessage}\n";
            }

            PlatformUtils.Instance.SetClipboardText($"{contents}");
        }

        ImGui.BeginChild("##actionLogItems");
        ImGui.Spacing();
        for (var i = 0; i < TaskLogs._actionLog.Count; i++)
        {
            ImGui.Indent();
            ImGui.TextColored(TaskLogs.PickColor(TaskLogs._actionLog[i].Level), TaskLogs._actionLog[i].FormattedMessage);
            ImGui.Unindent();
        }

        if (TaskLogs._actionLog_ScrollToEnd)
        {
            ImGui.SetScrollHereY();
            TaskLogs._actionLog_ScrollToEnd = false;
        }

        ImGui.Spacing();
        ImGui.EndChild();
    }
}
