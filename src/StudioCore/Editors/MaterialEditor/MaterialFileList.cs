using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.EventScriptEditorNS;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialFileList
{
    public MaterialEditor Editor;
    public Project Project;

    public MaterialFileList(Project curPoject, MaterialEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(MaterialEditorContext.FileList);

        ImGui.BeginChild("materialFileList");

        Editor.Filters.DisplayFileFilterSearch();

        DisplayMaterialList();
        DisplayMaterialBinList();

        ImGui.EndChild();
    }

    public void DisplayMaterialList()
    {
        if (Editor.Selection._selectedSourceType is MaterialSourceType.MTD)
        {
            var entries = Project.MaterialData.MTD_Files.Entries;

            if (entries.Count > 0)
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(entries.Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = entries[i];
                        var filename = curEntry.Filename;
                        var isSelected = Editor.Selection.IsFileSelected(i);
                        var displayName = $"{filename}";

                        if (!Editor.Filters.IsFileFilterMatch(displayName))
                            continue;

                        if (ImGui.Selectable($"{displayName}##fileEntry{i}", isSelected))
                        {
                            Editor.Selection.SelectFile(i, curEntry.Filename);
                            Project.MaterialData.PrimaryBank_MTD.LoadBinder(curEntry.Filename, curEntry.Path);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Editor.Selection.AutoSelectFile)
                        {
                            Editor.Selection.AutoSelectFile = false;
                            Editor.Selection.SelectFile(i, curEntry.Filename);
                            Project.MaterialData.PrimaryBank_MTD.LoadBinder(curEntry.Filename, curEntry.Path);
                        }

                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Editor.Selection.AutoSelectFile = true;
                        }
                    }
                }
            }
        }
    }

    public void DisplayMaterialBinList()
    {
        if (Editor.Selection._selectedSourceType is MaterialSourceType.MATBIN)
        {
            var entries = Project.MaterialData.MATBIN_Files.Entries;

            if (entries.Count > 0)
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(entries.Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = entries[i];
                        var filename = curEntry.Filename;
                        var isSelected = Editor.Selection.IsFileSelected(i);
                        var displayName = $"{filename}";

                        if (!Editor.Filters.IsFileFilterMatch(displayName))
                            continue;

                        if (ImGui.Selectable($"{displayName}##fileEntry{i}", isSelected))
                        {
                            Editor.Selection.SelectFile(i, curEntry.Filename);
                            Project.MaterialData.PrimaryBank_MATBIN.LoadBinder(curEntry.Filename, curEntry.Path);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Editor.Selection.AutoSelectFile)
                        {
                            Editor.Selection.AutoSelectFile = false;
                            Editor.Selection.SelectFile(i, curEntry.Filename);
                            Project.MaterialData.PrimaryBank_MATBIN.LoadBinder(curEntry.Filename, curEntry.Path);
                        }

                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Editor.Selection.AutoSelectFile = true;
                        }
                    }
                }
            }
        }
    }
}



