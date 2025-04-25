using Hexa.NET.ImGui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Resource.ResourceManager;

namespace StudioCore.Resource;

public static class ResourceLoadWindow
{
    private static bool TaskWindowOpen = true;

    public static void DisplayWindow(float w, float h)
    {
        var scale = DPI.GetUIScale();

        if (GetActiveJobProgress().Count() > 0)
        {
            ImGui.SetNextWindowSize(new Vector2(400, 310) * scale);
            ImGui.SetNextWindowPos(new Vector2(w - (100 * scale), h - (300 * scale)));
            if (!ImGui.Begin("Resource Loading Tasks", ref TaskWindowOpen, ImGuiWindowFlags.NoDecoration))
            {
                ImGui.End();
                return;
            }

            foreach (KeyValuePair<ResourceJob, int> job in GetActiveJobProgress())
            {
                if (!job.Key.Finished)
                {
                    var completed = job.Key.Progress;
                    var size = job.Key.GetEstimateTaskSize();
                    ImGui.Text(job.Key.Name);

                    if (size == 0)
                    {
                        ImGui.ProgressBar(0.0f);
                    }
                    else
                    {
                        ImGui.ProgressBar(completed / (float)size, new Vector2(386.0f, 20.0f) * scale);
                    }
                }
            }

            ImGui.End();
        }
    }
}
