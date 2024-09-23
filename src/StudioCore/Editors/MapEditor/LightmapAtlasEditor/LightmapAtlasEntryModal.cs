using ImGuiNET;
using SoulsFormats;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.FFXDLSE;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapAtlasEntryModal
{
    private MapEditorScreen Screen;
    private LightmapAtlasScreen AtlasScreen;

    private bool DisplayModal = false;

    public LightmapAtlasEntryModal(MapEditorScreen screen, LightmapAtlasScreen atlasScreen)
    {
        Screen = screen;
        AtlasScreen = atlasScreen;
    }

    public void Display()
    {
        DisplayModal = !DisplayModal;
    }

    public void OnGui()
    {
        if (DisplayModal)
        {
            ImGui.OpenPopup("Create Lightmap Atlas Entry");
        }

        if (ImGui.BeginPopupModal("Create Lightmap Atlas Entry", ref DisplayModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            CreationMenu();

            ImGui.EndPopup();
        }
    }

    private int NewAtlasID = 0;
    private string NewPartName = "Undefined";
    private string NewMaterialName = "";
    private Vector2 NewUVOffset = new Vector2();
    private Vector2 NewUVScale = new Vector2();

    public void CreationMenu()
    {
        var width = 400;
        var buttonSize = new Vector2(width / 2, 32);

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        ImGui.Text("New Entry");
        ImGui.PopStyleColor(1);
        ImGui.Separator();

        ImGui.Text("Atlas ID     ");
        ImGui.SameLine();
        ImGui.InputInt("##newAtlasId", ref NewAtlasID);

        ImGui.Text("Part Name    ");
        ImGui.SameLine();
        ImGui.InputText("##newPartName", ref NewPartName, 255);

        ImGui.Text("Material Name");
        ImGui.SameLine();
        ImGui.InputText("##NewMaterialName", ref NewMaterialName, 255);

        ImGui.Text("UV Offset    ");
        ImGui.SameLine();
        ImGui.InputFloat2("##newUVOffset", ref NewUVOffset);

        ImGui.Text("UV Scale     ");
        ImGui.SameLine();
        ImGui.InputFloat2("##newUVScale", ref NewUVScale);

        if (ImGui.Button("Create", buttonSize))
        {
            var newEntry = new BTAB.Entry();
            newEntry.PartName = NewPartName;
            newEntry.MaterialName = NewMaterialName;
            newEntry.AtlasID = NewAtlasID;
            newEntry.UVOffset = NewUVOffset;
            newEntry.UVScale = NewUVScale;

            var insertIdx = 0;

            if (AtlasScreen.CurrentParent.LightmapAtlas.Entries != null)
            {
                insertIdx = AtlasScreen.CurrentParent.LightmapAtlas.Entries.Count;
            }

            Screen.EditorActionManager.ExecuteAction(new BtabEntryAdd(newEntry, AtlasScreen.CurrentParent.LightmapAtlas.Entries, insertIdx));

            DisplayModal = false;
        }
        ImGui.SameLine();
        if (ImGui.Button("Close", buttonSize))
        {
            DisplayModal = false;
        }
    }
}
