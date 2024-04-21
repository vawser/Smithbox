using ImGuiNET;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Gui;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.EntryFileList.EntryFileListBank;

namespace StudioCore.Editors.MapEditor.EntryFileList;

public class EntryFileListEditor
{
    private readonly ViewportActionManager _actionManager;

    private readonly RenderScene _scene;
    private readonly ViewportSelection _selection;

    private MapEditorScreen _msbEditor;

    private IViewport _viewport;

    private Universe _universe;

    private MsbEntity selectedEntity;

    private EntryFileListInfo selectedFile;

    public EntryFileListEditor(Universe universe, RenderScene scene, ViewportSelection sel, ViewportActionManager manager, MapEditorScreen editor, IViewport viewport)
    {
        _scene = scene;
        _selection = sel;
        _actionManager = manager;
        _universe = universe;

        _msbEditor = editor;
        _viewport = viewport;
    }

    /// <summary>
    /// Display the window.
    /// </summary>
    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        // WIP: only a viewer for AC6 at the moment
        // TODO: add ability to change the entries:
        // -> Struct1 and Struct2 need to be changed in-line with the String

        return;

        if (Project.Type == ProjectType.Undefined)
            return;

        if (Project.Type != ProjectType.AC6)
            return;

        if (!EntryFileListBank.IsLoaded)
        {
            EntryFileListBank.LoadEntryFileLists();
        }

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Entry File List##MapEditor_entryFileListEditor"))
        {
            ImGui.Columns(2);

            foreach (var (name, info) in EntryFileListBank.Bank)
            {
                if(ImGui.Selectable($"{name}", selectedFile == info))
                {
                    selectedFile = info;
                }
            }

            ImGui.NextColumn();

            if (selectedFile != null)
            {
                var file = selectedFile.EntryFileList;

                foreach (var name in file.Strings)
                {
                    ImGui.Text($"{name}");
                }
            }
            else
            {
                ImGui.Text("Select an entry.");
            }
        }
        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
