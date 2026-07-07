using Hexa.NET.ImGui;
using SoapstoneLib;
using System;
using System.Linq;

namespace StudioCore.Application;

public class TaskViewer
{
    public TaskViewer() { }

    public void Display()
    {
        GUI.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Task_Viewer"),
            LOC.Get("DEV_Tool_Header_Task_Viewer_TT"));

        if (TaskManager.GetLiveThreads().Count > 0)
        {
            foreach ((string taskName, TaskManager.LiveTask task) in TaskManager.GetTasks())
            {
                ImGui.Text(LOC.Get("DEV_Tool_Task_Information", 
                    task.TaskId, task.TaskName, task.TaskCompletedMessage, task.TaskFailedMessage));
            }
        }

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Soapstone_Server"),
            LOC.Get("DEV_Tool_Header_Soapstone_Server_TT"));

        var running = SoapstoneServer.GetRunningPort() is int port
                ? LOC.Get("DEV_Tool_Running_On_Port", port)
                : LOC.Get("DEV_Tool_Not_Running");

        ImGui.Text(LOC.Get("DEV_Tool_Server_Running", running));
    }
}
