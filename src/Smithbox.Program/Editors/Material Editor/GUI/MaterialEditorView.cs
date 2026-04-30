using Hexa.NET.ImGui;
using HKLib.hk2018.hkaiCollisionAvoidance;
using Silk.NET.SDL;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialEditorView
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public MaterialPropertyCache MaterialPropertyCache = new();

    public MaterialSelection Selection;
    public MaterialFilters Filters;
    public MaterialPropertyInput PropertyInput;

    public MaterialContainerList ContainerList;
    public MaterialFileList FileList;
    public MaterialProperties Properties;

    public int ViewIndex;

    public MaterialEditorView(MaterialEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new(this, project);
        Filters = new(this, project);
        PropertyInput = new(this, project);

        ContainerList = new(this, project);
        FileList = new(this, project);
        Properties = new(this, project);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        if (Project.Handler.MaterialData.PrimaryBank == null)
            return;

        DisplayMenubar();

        var columnCount = 2;
        var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (ImGui.BeginTable("materialTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##FileList", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##Properties", ImGuiTableColumnFlags.WidthStretch, 0.5f);

            // --- Column 1 ---
            ImGui.TableNextColumn();

            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y * CFG.Current.Interace_Editor_Display_Inner_Height_Percent;

            ImGui.BeginChild("##FileListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MaterialEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Draw(width, height * CFG.Current.MaterialEditor_Display_ContainerList_Percentage);
            FileList.Draw(width, height * CFG.Current.MaterialEditor_Display_FileList_Percentage);

            ImGui.EndChild();

            // --- Column 2 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##PropertiesArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MaterialEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            Properties.Draw();

            ImGui.EndChild();

            ImGui.EndTable();
        }
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.BeginMenu("Display"))
                {
                    ImGui.SliderFloat("Containers##containerListDisplayPercentage", ref CFG.Current.MaterialEditor_Display_ContainerList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        // Auto-adjust the other var so the ratio remains 100%
                        CFG.Current.MaterialEditor_Display_FileList_Percentage = 1 - CFG.Current.MaterialEditor_Display_ContainerList_Percentage;
                    }
                    UIHelper.Tooltip("The percentage of the window the Containers section occupies.");

                    ImGui.SliderFloat("Files##fileListDisplayPercentage", ref CFG.Current.MaterialEditor_Display_FileList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        // Auto-adjust the other var so the ratio remains 100%
                        CFG.Current.MaterialEditor_Display_ContainerList_Percentage = 1 - CFG.Current.MaterialEditor_Display_FileList_Percentage;
                    }
                    UIHelper.Tooltip("The percentage of the window the Files section occupies.");

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }
}
