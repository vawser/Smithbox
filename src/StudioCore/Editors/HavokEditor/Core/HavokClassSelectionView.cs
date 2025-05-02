using HKLib.hk2018;
using Hexa.NET.ImGui;
using StudioCore.Core.Project;
using StudioCore.Editors.HavokEditor.Data;
using StudioCore.Editors.HavokEditor.Framework;
using StudioCore.Editors.MapEditor;
using StudioCore.HavokEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Core;

public class HavokClassSelectionView
{
    private HavokEditorScreen Screen;

    public HavokClassSelectionView(HavokEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnGui()
    {
        ImGui.Begin("Class Selection##HavokClassList");

        HandleClassList();

        ImGui.End();
    }

    private void HandleClassList()
    {
        var objectHierarchy = Screen.Selection.ObjectHierarchy;
        var selectedObjectClass = Screen.Selection.SelectedObjectClass;

        int i = 0;

        if (objectHierarchy == null)
            return;

        foreach (var entry in objectHierarchy)
        {
            var curType = entry.Key;
            var objects = entry.Value;

            var meta = Screen.Selection.GetClassMeta(curType);

            var displayName = $"{curType}";
            if(meta != null)
            {
                displayName = meta.Name;
            }
            else
            {
                displayName = displayName.Replace("HKLib.hk2018.", "").Replace("+", "_");
            }

            if (ImGui.Selectable($"{displayName} [{objects.Count}]##entry{i}", curType == selectedObjectClass))
            {
                Screen.Selection.SelectNewClass(curType);
            }
            if (meta != null && meta.Description != "")
            {
                UIHelper.ShowHoverTooltip(meta.Description);
            }

            i++;
        }
    }
}
