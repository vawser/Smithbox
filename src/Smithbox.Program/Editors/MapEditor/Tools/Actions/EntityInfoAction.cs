using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class EntityInfoAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public EntityInfoAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {

    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext(Entity ent)
    {
        if (ImGui.Selectable("Copy Name"))
        {
            CopyEntityNameToClipboard(ent);
        }
        UIHelper.Tooltip($"Copy the current selection's name to the clipboard. For multi-selections, each name is separated by a comma and space.");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Not shown here
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void CopyEntityNameToClipboard(Entity ent)
    {
        if (Editor.ViewportSelection.IsMultiSelection())
        {
            var fullStr = "";

            foreach (var entry in Editor.ViewportSelection.GetSelection())
            {
                var curEnt = (MsbEntity)entry;

                if (fullStr != "")
                    fullStr = $"{fullStr}, {curEnt.Name}";
                else
                    fullStr = $"{curEnt.Name}";
            }

            PlatformUtils.Instance.SetClipboardText(fullStr);
        }
        else
        {

            PlatformUtils.Instance.SetClipboardText(ent.Name);
        }
    }
}