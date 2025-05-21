using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class QuickView
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private bool InitTargetProperties = false;
    private List<string> curTargetProperties = new();

    public QuickView(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The tooltip handling for Quick View
    /// </summary>
    public void HandleQuickViewTooltip()
    {
        if (Editor.FocusManager.CurrentWindowContext is not MapEditorContext.MapViewport)
            return;

        var curSel = Editor.ViewportSelection.GetSelection();

        if (curSel.Count > 0)
        {
            var firstEnt = (Entity)curSel.First();

            ImGui.BeginTooltip();

            var properties = CFG.Current.QuickView_TargetProperties;

            foreach (var property in properties)
            {
                var propValue = firstEnt.GetPropertyValue(property);

                if (propValue != null)
                {
                    ImGui.Text($"{property}: {propValue}");
                }
            }

            ImGui.EndTooltip();
        }
    }

    /// <summary>
    /// The property settings for Quick View
    /// </summary>
    public void HandleQuickViewProperties()
    {
        if (!InitTargetProperties)
        {
            curTargetProperties = CFG.Current.QuickView_TargetProperties;
            InitTargetProperties = true;
        }

        // Add
        if (ImGui.Button($"{Icons.Plus}##quickViewPropAdd"))
        {
            curTargetProperties.Add("");
            CFG.Current.QuickView_TargetProperties = curTargetProperties;
        }
        UIHelper.Tooltip("Add property input.");

        ImGui.SameLine();

        // Remove
        if (curTargetProperties.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##quickViewPropRemove"))
            {
                curTargetProperties.RemoveAt(curTargetProperties.Count - 1);
                CFG.Current.QuickView_TargetProperties = curTargetProperties;
            }
            UIHelper.Tooltip("Remove last property input.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##quickViewPropRemove"))
            {
                curTargetProperties.RemoveAt(curTargetProperties.Count - 1);
                CFG.Current.QuickView_TargetProperties = curTargetProperties;
            }
            UIHelper.Tooltip("Remove last property input.");
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##quickViewPropReset"))
        {
            curTargetProperties = new List<string>() { "" };
            CFG.Current.QuickView_TargetProperties = curTargetProperties;
        }
        UIHelper.Tooltip("Reset property inputs.");

        for (int i = 0; i < curTargetProperties.Count; i++)
        {
            var curProperty = curTargetProperties[i];

            ImGui.SetNextItemWidth(400f);
            ImGui.InputText($"##propInput{i}", ref curProperty, 255);
            UIHelper.Tooltip("The internal name of the property you want to appear in the Quick View tooltip.");
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                curTargetProperties[i] = curProperty;
                CFG.Current.QuickView_TargetProperties = curTargetProperties;
            }
        }

        if (curTargetProperties.Count == 0)
        {
            if (ImGui.Button("Add Property"))
            {
                curTargetProperties.Add("");
                CFG.Current.QuickView_TargetProperties = curTargetProperties;
            }
            UIHelper.Tooltip("Add property input.");
        }
    }
}
