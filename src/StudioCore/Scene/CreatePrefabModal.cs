using Hexa.NET.ImGui;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Scene.Interfaces;
using System.Numerics;

namespace StudioCore.Scene;

internal class CreatePrefabModal : IModal
{
    private bool _open;
    private string _prefabCategory = "";

    private string _prefabName = "";
    private Entity _referenceEntity;

    public CreatePrefabModal(Entity reference)
    {
        _referenceEntity = reference;
    }

    public bool IsClosed => !_open;

    public void OpenModal()
    {
        ImGui.OpenPopup("Create Prefab");
        _open = true;
    }

    public void OnGui()
    {
        var scale = DPI.Scale;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 7.0f * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14.0f, 8.0f) * scale);
        if (ImGui.BeginPopupModal("Create Prefab", ref _open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Prefab Name:      ");
            ImGui.SameLine();
            var pname = _prefabName;
            if (ImGui.InputText("##pname", ref pname, 255))
            {
                _prefabName = pname;
            }

            ImGui.NewLine();

            if (ImGui.Button("Create", new Vector2(120, 0) * scale))
            {
                var validated = true;

                if (validated)
                {
                    ImGui.CloseCurrentPopup();
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0) * scale))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        ImGui.PopStyleVar(3);
    }
}
