using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Application;

public class ProjectMetadataScreen
{
    public ActionManager EditorActionManager = new();

    public ProjectEnumMenu EnumMenu;
    public ProjectAliasMenu AliasMenu;

    public ProjectEntry SelectedLoadedEntry = null;
    public ProjectEntry SelectedAvaliableEntry = null;

    public string LoadedListFilter = "";
    public bool ExactLoadedListFilter = false;

    public string AvailableListFilter = "";
    public bool ExactAvailableListFilter = false;

    public bool RequestFocus = false;

    public ProjectMetadataScreen()
    {
        EnumMenu = new();
        AliasMenu = new();
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
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_EditorView);
        if (ImGui.Begin($"{LOC.Get("PRJ_Window_Project_Metadata_Editor")}###ProjectMetadataEditor", UIHelper.GetMainWindowFlags()))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            Shortcuts();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            var dsid = ImGui.GetID("DockSpace_ProjectMetadataEditor");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref UIHelper.DockGroup_ProjectMetadataEditor);

            ImGui.SetNextWindowDockID(dsid, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectMetadataEditor);
            if (ImGui.Begin($"{LOC.Get("PRJ_Window_Project_Metadata_View")}###ProjectMetadataView", UIHelper.GetInnerWindowFlags()))
            {
            }

            var viewDockId = ImGui.GetID($"DockSpace_ProjectMetadataEditorView");
            ImGui.DockSpace(viewDockId, new Vector2(0, 0), ref UIHelper.DockGroup_ProjectMetadataEditorView);

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
        // TODO: paramdef XML editor here?

        if (CFG.Current.Interface_ProjectMetadataEditor_ProjectEnums)
        {
            // Project Enums
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PRJ_Window_Project_Enums")}###projectMetadataEditor_ProjectEnums", UIHelper.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Metadata_EnumEditor);
                }

                DisplayEnumEditor();
            }

            ImGui.End();
        }

        if (CFG.Current.Interface_ProjectMetadataEditor_ProjectAliases)
        {
            // Project Aliases
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PRJ_Window_Project_Aliases")}###projectMetadataEditor_ProjectAliases", UIHelper.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Metadata_AliasEditor);
                }

                DisplayAliasEditor();
            }

            ImGui.End();
        }
    }

    public void DisplayEnumEditor()
    {
        EnumMenu.Display();
    }

    public void DisplayAliasEditor()
    {
        AliasMenu.Display();
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_PCM_Header_Edit")}##editMenuHeader"))
        {
            AliasMenu.EditMenu();
            EnumMenu.EditMenu();

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_PCM_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_View_Project_Aliases")}##projectAliasesToggle"))
            {
                CFG.Current.Interface_ProjectMetadataEditor_ProjectAliases = !CFG.Current.Interface_ProjectMetadataEditor_ProjectAliases;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ProjectMetadataEditor_ProjectAliases);

            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_View_Project_Enums")}##projectEnumsToggle"))
            {
                CFG.Current.Interface_ProjectMetadataEditor_ProjectEnums = !CFG.Current.Interface_ProjectMetadataEditor_ProjectEnums;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ProjectMetadataEditor_ProjectEnums);

            ImGui.EndMenu();
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
