using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;
using StudioCore.Editors.ParamEditor;

namespace StudioCore.Editors.MapEditor.CollisionPropertyEditor;

public class CollisionPropertyEditorScreen
{
    private CollisionPropertyEditor PropertyEditor;

    private bool DisplayScreen = false;

    public CollisionPropertyEditorScreen()
    {

    }

    public void ToggleDisplay()
    {
        DisplayScreen = !DisplayScreen;
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        return;

        /*
        if (!Smithbox.BankHandler.CollisionBank.UsesCollisionBank())
            return;

        if (!DisplayScreen)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        var selection = Smithbox.EditorHandler.MapEditor.Universe.Selection;

        if (selection == null)
            return;

        var selectedEntity = (MsbEntity)selection.GetSelection().FirstOrDefault();

        if (selectedEntity == null)
            return;

        if (ImGui.Begin($@"Collision Property Editor##MapEditor_CollisionPropertyEditor"))
        {
            ImGui.BeginTabBar("##SelectedCollision");

            foreach(var entry in Smithbox.BankHandler.CollisionBank.Collisions)
            {
                if(selectedEntity.Parent != null)
                {
                    if (selectedEntity.IsPartCollision())
                    {
                        var mapID = selectedEntity.Parent.Name;
                        if (entry.Key == mapID)
                        {
                            foreach(var cols in entry.Value)
                            {
                                TaskLogs.AddLog($"cols.Filename: {cols.Filename}");

                                var obj = (MSBE.Part.Collision)selectedEntity.WrappedObject;

                                TaskLogs.AddLog($"obj.ModelName: {obj.ModelName}");

                                if (cols.Filename == obj.ModelName)
                                {
                                }
                            }
                        }
                    }
                }
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
        */
    }
}
