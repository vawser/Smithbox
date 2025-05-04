using Hexa.NET.ImGui;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public class MassEditLog
{
    private MapEditorScreen Editor;
    private MassEditHandler Handler;

    private List<MapActionGroup> MassEditActions = new List<MapActionGroup>();

    public bool ShowMassEditLog = true;

    public MassEditLog(MapEditorScreen screen, MassEditHandler handler)
    {
        Editor = screen;
        Handler = handler;
    }

    public void DisplayButton()
    {
        if (MassEditActions != null)
        {
            if (ImGui.Button($"{Icons.Eye}##previousEditLog"))
            {
                Handler.Tools.ShowToolView = false;
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
                    var alias = AliasUtils.GetMapNameAlias(Editor.Project, entry.MapID);
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
