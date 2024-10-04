using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.TalkEditor;
using StudioCore.TalkEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the script selection, viewing and editing.
/// </summary>
public class EsdScriptView
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;

    public EsdScriptView(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Scripts##EsdScriptList");

        var info = Selection._selectedFileInfo;
        var scriptKey = Selection._selectedEsdScriptKey;

        if (info != null)
        {
            ImGui.Text($"Scripts");
            ImGui.Separator();

            for (int i = 0; i < info.EsdFiles.Count; i++)
            {
                ESD entry = info.EsdFiles[i];

                if (ImGui.Selectable($@" {entry.Name}", i == scriptKey))
                {
                    Selection.ResetStateGroup();
                    Selection.ResetStateGroupNode();

                    Selection.SetScript(i, entry);
                }
            }
        }

        ImGui.End();
    }
}
