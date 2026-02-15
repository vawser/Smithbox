using Hexa.NET.ImGui;
using HKX2;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorContents_HKX2
{
    public BehaviorView View;
    public ProjectEntry Project;

    public string ImguiID = "BehaviorContents";

    public BehaviorContentsSelection Selection = new();

    public ChosenObjectType TargetObjectType = ChosenObjectType.hkbClipGenerator;

    private List<hkbClipGenerator> CachedClipGenerators = null;

    public BehaviorContents_HKX2(BehaviorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (View.Selection.SelectedFile == null)
            return;

        if (View.Selection.SelectedFile.Havok.HKX2_Object == null)
            return;

        var root = View.Selection.SelectedFile.Havok.HKX2_Object;

        UIHelper.SimpleHeader("Chosen Type", "Select which havok type to display.");

        if (ImGui.BeginCombo("##inputValue", TargetObjectType.GetDisplayName()))
        {
            foreach (var entry in System.Enum.GetValues(typeof(ChosenObjectType)))
            {
                var type = (ChosenObjectType)entry;

                if (ImGui.Selectable(type.GetDisplayName()))
                {
                    TargetObjectType = (ChosenObjectType)entry;
                }
            }
            ImGui.EndCombo();
        }

        if (TargetObjectType is ChosenObjectType.hkbClipGenerator)
        {
            if (CachedClipGenerators == null)
            {
                CachedClipGenerators = root.FindAllOfType<hkbClipGenerator>();
            }
            else
            {
                ImGui.BeginChild("section_hkbClipGenerator");

                for (int i = 0; i < CachedClipGenerators.Count; i++)
                {
                    var anim = CachedClipGenerators[i];

                    var selected = Selection.SelectedClipGenerator == anim;

                    if (ImGui.Selectable($"{anim.m_name}##entry{i}", selected))
                    {
                        Selection.SelectedClipGenerator = anim;
                    }
                }
                ImGui.EndChild();
            }
        }
    }

    public void Invalidate()
    {
        CachedClipGenerators = null;
    }

    public class BehaviorContentsSelection()
    {
        public hkbClipGenerator SelectedClipGenerator = null;
    }
}

public enum ChosenObjectType
{
    [Display(Name ="Clip Generators")]
    hkbClipGenerator
}