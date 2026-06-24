using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Application;

public static class ThumbnailCapture
{
    private static string SaveDir = @"C:\Users\benja\Programming\C#\Smithbox\src\Smithbox.Data\Assets\MSB";

    public static void CaptureThumbnail(ModelEditorView view, string modelName)
    {
        var curProject = view.Project;
        var saveDir = Path.Join(SaveDir, ProjectUtils.GetGameDirectory(curProject), "Thumbnails");

        if(!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }

        var filePath = Path.Join(saveDir, $"{modelName}.png");

        if (view.ViewportWindow.Viewport is VulkanViewport vulkanViewport)
        {
            //CaptureViewport(vulkanViewport, filePath);
        }

        Smithbox.Log(typeof(ThumbnailCapture), $"Saved thumbnail for {modelName}");
    }

}
