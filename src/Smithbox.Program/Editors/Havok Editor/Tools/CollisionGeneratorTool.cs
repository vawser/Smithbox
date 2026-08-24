using Hexa.NET.ImGui;
using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class CollisionGeneratorTool
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public CollisionGeneratorTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (!CFG.Current.HavokEditor_ToolVisibility_CollisionGenerator)
            return;

        if(ImGui.CollapsingHeader($"{LOC.Get("HAVOK_Tools_Collision_Generator_Tool")}##collisionGeneratorTool"))
        {
            ImGui.BeginChild("CollisionGeneratorSection", ImGuiChildFlags.Borders);

            if (View.Selection.BinderFileEntry == null)
            {
                GUI.WrappedText(LOC.Get("HAVOK_ColGen_No_Binder_Entry"));
            }
            else if (View.Selection.FilePath == null)
            {
                GUI.WrappedText(LOC.Get("HAVOK_ColGen_No_File_Entry"));
            }
            else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision or HavokCategoryMode.Asset_Collision)
            {
                DisplayGeneratorPanel();
            }
            else
            {
                GUI.WrappedText(LOC.Get("HAVOK_ColGen_Invalid_Category"));
            }

            ImGui.EndChild();
        }
    }

    public void DisplayGeneratorPanel()
    {

    }
}

