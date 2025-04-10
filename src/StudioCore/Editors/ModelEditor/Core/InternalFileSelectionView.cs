using Hexa.NET.ImGui;
using StudioCore.Core.Project;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Interface;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class InternalFileSelectionView
{
    public ModelEditorScreen Screen;
    public ModelSelectionManager Selection;
    public ModelResourceManager ResManager;

    public InternalFileSelectionView(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ResManager = screen.ResManager;
    }
    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        var scale = DPI.GetUIScale();

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (!Smithbox.AliasCacheHandler.AliasCache.UpdateCacheComplete)
            return;

        if (!UI.Current.Interface_ModelEditor_AssetBrowser)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Associated Files##InternalFileList"))
        {
            DisplayInternalFileList();
        }

        ImGui.End();

        ImGui.PopStyleColor(1);
    }

    private void DisplayInternalFileList()
    {
        if (ResManager.LoadedFlverContainer != null)
        {
            // Internal Files
            foreach (var entry in ResManager.LoadedFlverContainer.InternalFlvers)
            {
                var currentFlver = ResManager.LoadedFlverContainer.CurrentInternalFlver;

                if (ImGui.Selectable($"{entry.Name}", entry == currentFlver))
                {
                    Selection._selectedFlverGroupType = GroupSelectionType.InternalFile;
                    ResManager.LoadedFlverContainer.CurrentInternalFlver = entry;

                    var name = ResManager.LoadedFlverContainer.ContainerName;
                    var type = ResManager.LoadedFlverContainer.Type;
                    var mapId = ResManager.LoadedFlverContainer.MapID;

                    ResManager.LoadRepresentativeModel(name, entry.Name, type, mapId);
                }
            }
        }
    }

}
