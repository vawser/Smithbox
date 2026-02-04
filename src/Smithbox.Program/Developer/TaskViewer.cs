using Hexa.NET.ImGui;
using SoapstoneLib;
using System;
using System.Linq;

namespace StudioCore.Application;

public static class TaskViewer
{
    public static void Display(ProjectEntry project)
    {
        ImGui.Text("Currently running tasks:");
        ImGui.Text("");

        if (TaskManager.GetLiveThreads().Count > 0)
        {
            foreach ((string taskName, TaskManager.LiveTask task) in TaskManager.GetTasks())
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
