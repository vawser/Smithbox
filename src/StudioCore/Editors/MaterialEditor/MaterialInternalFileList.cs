using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialInternalFileList
{
    public MaterialEditor Editor;
    public Project Project;

    public MaterialInternalFileList(Project curPoject, MaterialEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(MaterialEditorContext.InternalFileList);

        ImGui.BeginChild("materialInternalFileList");

        Editor.Filters.DisplayInternalFileFilterSearch();

        var targetBinder = new KeyValuePair<string, BinderContents>();

        if (Editor.Selection._selectedSourceType is MaterialSourceType.MTD)
        {
            targetBinder = Project.MaterialData.PrimaryBank_MTD.Binders.FirstOrDefault(e => e.Key == Editor.Selection._selectedFileName);
        }
        if (Editor.Selection._selectedSourceType is MaterialSourceType.MATBIN)
        {
            targetBinder = Project.MaterialData.PrimaryBank_MATBIN.Binders.FirstOrDefault(e => e.Key == Editor.Selection._selectedFileName);
        }

        if (targetBinder.Value.Files.Count > 0)
        {
            ImGuiListClipper clipper = new ImGuiListClipper();
            clipper.Begin(targetBinder.Value.Files.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var curEntry = targetBinder.Value.Files.ElementAt(i);

                    var filename = curEntry.Name;

                    var isSelected = Editor.Selection.IsInternalFileSelected(i);
                    var displayName = $"{filename}";

                    if (!Editor.Filters.IsInternalFileFilterMatch(displayName))
                        continue;

                    if (ImGui.Selectable($"{displayName}##internalFileEntry{i}", isSelected))
                    {
                        Editor.Selection.SelectInternalFile(i, curEntry.Name);

                        if (Editor.Selection._selectedSourceType is MaterialSourceType.MTD)
                        {
                            Editor.Selection._selectedMaterial = MTD.Read(curEntry.Bytes);
                        }

                        if (Editor.Selection._selectedSourceType is MaterialSourceType.MATBIN)
                        {
                            Editor.Selection._selectedMatbin = MATBIN.Read(curEntry.Bytes);
                        }
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.AutoSelectInternalFile)
                    {
                        Editor.Selection.AutoSelectInternalFile = false;

                        Editor.Selection.SelectInternalFile(i, curEntry.Name);

                        if (Editor.Selection._selectedSourceType is MaterialSourceType.MTD)
                        {
                            Editor.Selection._selectedMaterial = MTD.Read(curEntry.Bytes);
                        }

                        if (Editor.Selection._selectedSourceType is MaterialSourceType.MATBIN)
                        {
                            Editor.Selection._selectedMatbin = MATBIN.Read(curEntry.Bytes);
                        }
                    }

                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.AutoSelectInternalFile = true;
                    }
                }
            }
        }

        ImGui.EndChild();
    }
}


