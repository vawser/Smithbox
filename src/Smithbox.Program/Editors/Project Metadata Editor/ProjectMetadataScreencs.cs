using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Editors.MetadataEditor;

public class ProjectMetadataScreen
{
    public ActionManager EditorActionManager = new();

    public ProjectEntry SelectedLoadedEntry = null;
    public ProjectEntry SelectedAvaliableEntry = null;

    public string LoadedListFilter = "";
    public bool ExactLoadedListFilter = false;

    public string AvailableListFilter = "";
    public bool ExactAvailableListFilter = false;

    public bool RequestFocus = false;

    public MetadataSelection Selection;

    public ModeSelectionMenu ModeMenu;

    // Project
    public ProjectEnumMenu EnumMenu;
    public ProjectAliasMenu AliasMenu;

    // Param Data
    public ParamDefMenu ParamDefMenu;
    public ParamMetaMenu ParamMetaMenu;

    public ProjectMetadataScreen()
    {
        Selection = new(this);

        ModeMenu = new(this);
        EnumMenu = new();
        AliasMenu = new();

        ParamDefMenu = new(this);
        ParamMetaMenu = new(this);
    }

    public unsafe void OnGUI(uint mainDockspaceID)
    {
        if (RequestFocus)
        {
            ImGui.SetNextWindowFocus();
            RequestFocus = false;
        }

        if (Smithbox.Instance._context.Device == null)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        ImGui.SetNextWindowDockID(mainDockspaceID, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_EditorView);
        if (ImGui.Begin($"{LOC.Get("PROJECT_Window_Project_Metadata_Editor")}###ProjectMetadataEditor", GUI.GetInnerWindowFlags()))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            Shortcuts();

            var dsid = ImGui.GetID("DockSpace_ProjectMetadataEditor");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_ProjectMetadataEditor);

            ImGui.SetNextWindowDockID(dsid, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectMetadataEditor);
            if (ImGui.Begin($"{LOC.Get("PROJECT_Window_Project_Metadata_View")}###ProjectMetadataView", GUI.GetInnerWindowFlags()))
            {
            }

            var viewDockId = ImGui.GetID($"DockSpace_ProjectMetadataEditorView");
            ImGui.DockSpace(viewDockId, new Vector2(0, 0), ref GUI.DockGroup_ProjectMetadataEditorView);

            Display(viewDockId);

            ImGui.End();

            ImGui.End();
        }
        else
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    public void Display(uint editorDockspaceId)
    {
        // Mode Selection
        ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectMetadataEditorView);
        if (ImGui.Begin($@"{LOC.Get("META_Window_Mode_Selection")}###projectMetadataEditor_ModeSelection", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.Metadata_ModeSelection);
            }

            ModeMenu.Display();
        }

        ImGui.End();

        // Project
        if (Selection.EditorMode is MetadataEditorMode.Project)
        {
            // Project Enums
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectMetadataEditorView);
            if (ImGui.Begin($@"{LOC.Get("META_Window_Project_Enums")}###projectMetadataEditor_ProjectEnums", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Metadata_EnumEditor);
                }

                EnumMenu.Display();
            }

            ImGui.End();

            // Project Aliases
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectMetadataEditorView);
            if (ImGui.Begin($@"{LOC.Get("META_Window_Project_Aliases")}###projectMetadataEditor_ProjectAliases", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Metadata_AliasEditor);
                }

                AliasMenu.Display();
            }

            ImGui.End();
        }

        // Project
        if (Selection.EditorMode is MetadataEditorMode.ParamEditor)
        {
            // PARAMDEF Editor

            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectMetadataEditorView);
            if (ImGui.Begin($@"{LOC.Get("META_Window_Param_Def_Editor")}###projectMetadataEditor_ParamDef", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Metadata_ParamDefEditor);
                }

                ParamDefMenu.Display();
            }

            ImGui.End();

            // PARAM Meta Editor
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectMetadataEditorView);
            if (ImGui.Begin($@"{LOC.Get("META_Window_Param_Meta_Editor")}###projectMetadataEditor_ParamMeta", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Metadata_ParamMetaEditor);
                }

                ParamMetaMenu.Display();
            }

            ImGui.End();
        }
    }

    public void Shortcuts()
    {
        if (!FocusManager.IsInProjectMetadataEditor())
            return;

        AliasMenu.Shortcuts();
        EnumMenu.Shortcuts();
    }
}
