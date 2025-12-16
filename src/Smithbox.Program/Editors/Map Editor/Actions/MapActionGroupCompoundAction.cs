using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

public class MapActionGroupCompoundAction : ViewportAction
{
    private MapEditorScreen Editor;
    private readonly List<MapActionGroup> Actions;

    private Action<bool> PostExecutionAction;

    public MapActionGroupCompoundAction(MapEditorScreen editor, List<MapActionGroup> actions)
    {
        Editor = editor;
        Actions = actions;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var evt = ActionEvent.NoEvent;

        foreach (var group in Actions)
        {
            var name = group.MapID;
            var alias = AliasHelper.GetMapNameAlias(Editor.Project, name);
            if (alias != null)
                name = $"{name} {alias}";

            var tAction = group.Actions;

            foreach (var act in tAction)
            {
                if (act != null)
                {
                    evt |= act.Execute(isRedo);
                }
            }

            TaskLogs.AddLog($"Applied MSB mass edit affecting {tAction.Count} map objects for {name}");
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return evt;
    }

    public override ActionEvent Undo()
    {
        var evt = ActionEvent.NoEvent;

        foreach (var group in Actions)
        {
            var name = group.MapID;
            var alias = AliasHelper.GetMapNameAlias(Editor.Project, name);
            if (alias != null)
                name = $"{name} {alias}";

            var tAction = group.Actions;

            foreach (var act in tAction)
            {
                if (act != null)
                {
                    evt |= act.Undo();
                }
            }

            TaskLogs.AddLog($"Reverted MSB mass edit affecting {tAction.Count} map objects for {name}");
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return evt;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
