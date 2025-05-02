using Hexa.NET.ImGui;
using StudioCore.Editors.HavokEditor.Enums;
using StudioCore.Editors.HavokEditor.Framework;
using StudioCore.HavokEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Core;

public class HavokFileSelectionView
{
    private HavokEditorScreen Screen;

    public HavokFileSelectionView(HavokEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnGui()
    {
        var container = Screen.Selection.GetContainer();
        var fileKey = Screen.Selection.GetFileKey();
        var fileCount = Screen.Selection.GetBinderFileCount();

        // File List
        ImGui.Begin("Files##HavokFileList");

        if (fileCount > 0)
        {
            foreach (var file in container.InternalFileList)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var name_segments = file.Split("export");

                var name = "";
                var internalTypeKey = HavokInternalType.None;

                if (name_segments.Length > 1)
                {
                    name = name_segments[1];
                    internalTypeKey = Screen.Selection.GetFileType(name);
                }
                else
                {
                    name = name_segments[0];
                    internalTypeKey = Screen.Selection.GetFileType(name);
                }

                var presentationName = $"{internalTypeKey.GetDisplayName()}: {fileName}";

                if (ImGui.Selectable($@" {presentationName}", name == fileKey))
                {
                    Screen.Selection.SelectNewFile(name, internalTypeKey);
                }
            }
        }

        ImGui.End();
    }
}
