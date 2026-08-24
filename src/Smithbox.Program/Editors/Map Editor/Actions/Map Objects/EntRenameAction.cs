using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class EntRenameAction : ViewportAction
{
    private readonly MapEditorView View;
    private readonly List<MsbEntity> Entities;
    private readonly List<string> NewNames;
    private readonly List<string> OldNames;

    public EntRenameAction(MapEditorView view, List<MsbEntity> entities, List<string> newNames, bool reference)
    {
        View = view;
        Entities = entities;
        OldNames = entities.Select(e => e.Name).ToList();
        NewNames = newNames;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var (entity, name) in Entities.Zip(NewNames))
        {
            Rename(entity, name);
        }
        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach (var (entity, name) in Entities.Zip(OldNames))
        {
            Rename(entity, name);
        }
        return ActionEvent.ObjectAddedRemoved;
    }

    private void Rename(MsbEntity entity, string name)
    {
        var oldName = entity.Name;
        entity.Name = name;

        View.MapGroupsView.UpdateMapGroupEntry(oldName, name);
    }
}