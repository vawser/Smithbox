using Hexa.NET.ImGui;
using StudioCore.Editors.MapEditorNS;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditorNS;
public class MassEditLog
{
    private MapEditor Editor;

    private List<MapActionGroup> MassEditActions = new List<MapActionGroup>();

    public bool ShowMassEditLog = true;

    public MassEditLog(MapEditor editor)
    {
        Editor = editor;
    }

    public void DisplayButton()
    {
        if (MassEditActions != null)
        {
            if (ImGui.Button($"{ForkAwesome.Eye}##previousEditLog"))
            {
                Editor.MassEditHandler.Tools.ShowToolView = false;
                ShowMassEditLog = true;
            }
            UIHelper.Tooltip("Toggle visibility of the edit log.");
        }
    }

    public void Display()
    {
        if (MassEditActions != null)
        {
            if (ShowMassEditLog)
            {
                ImGui.BeginChild("previousEditLogSection");

                foreach (var entry in MassEditActions)
                {
                    var displayName = entry.MapID;
                    var alias = AliasUtils.GetMapNameAlias(entry.MapID);
                    if (alias != null)
                        displayName = $"{displayName} {alias}";

                    if (ImGui.CollapsingHeader($"{displayName}##mapTab_{entry.MapID}"))
                    {
                        var changes = entry.Actions;

                        foreach (var change in changes)
                        {
                            if (change is PropertiesChangedAction propChange)
                            {
                                UIHelper.WrappedText($"{propChange.GetEditMessage()}");
                            }
                        }
                    }
                }

                ImGui.EndChild();
            }
        }
    }

    public void UpdateLogSource(List<MapActionGroup> actionGroups)
    {
        MassEditActions = actionGroups;
    }

    public void ClearLogSource()
    {
        MassEditActions = new List<MapActionGroup>();
    }
}
