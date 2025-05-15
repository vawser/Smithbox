using Hexa.NET.ImGui;
using SoapstoneLib;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editor.TaskManager;

namespace StudioCore.DebugNS;

public static class TaskViewer
{
    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        ImGui.Text("Currently running tasks:");
        ImGui.Text("");

        if (GetLiveThreads().Count > 0)
        {
            foreach ((string taskName, LiveTask task) in GetTasks())
            {
                ImGui.Text("" +
                    $"ID: {task.TaskId}\n" +
                    $"Location: {task.TaskName}\n" +
                    $"Success Message: {task.TaskCompletedMessage}\n" +
                    $"Failure Message: {task.TaskFailedMessage}");
            }
        }

        if (ImGui.CollapsingHeader("Soapstone Server"))
        {
            var running = SoapstoneServer.GetRunningPort() is int port
                ? $"running on port {port}"
                : "not running";
            ImGui.Text(
                $"The server is {running}.\nIt is not accessible over the network, only to other programs on this computer.\nPlease restart the program for changes to take effect.");
        }
    }
}
