using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.HavokEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Core;

public class HavokClassEntriesView
{
    private HavokEditorScreen Screen;

    public HavokClassEntriesView(HavokEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnGui()
    {
        ImGui.Begin("Class Entries##HavokClassEntries");

        HandleClassEntries();

        ImGui.End();
    }

    private void HandleClassEntries()
    {
        var objectHierarchy = Screen.Selection.ObjectHierarchy;
        var selectedObjectClass = Screen.Selection.SelectedObjectClass;

        if (objectHierarchy == null)
            return;

        if (selectedObjectClass == null)
            return;

        if(objectHierarchy.ContainsKey(selectedObjectClass))
        {
            var curEntries = objectHierarchy[selectedObjectClass];

            for (int i = 0; i < curEntries.Count; i++)
            {
                // TODO: the name here should be more useful than just the index
                if (ImGui.Selectable($"Entry {i}", i == Screen.Selection.SelectedObjectClassEntryIndex))
                {
                    Screen.Selection.SelectNewClassEntry(i);
                }
            }
        }
    }
}
