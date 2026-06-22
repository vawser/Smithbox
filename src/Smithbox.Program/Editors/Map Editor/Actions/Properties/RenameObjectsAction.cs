using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class RenameObjectsAction(List<MsbEntity> entities, List<string> newNames, bool reference) : ViewportAction
{
    List<string> oldNames = entities.Select(e => e.Name).ToList();

    void Rename(MsbEntity entity, string name)
    {
        if (reference)
        {
            MapEditorActionHelper.SetNameHandleDuplicate(
                entity.ContainingMap,
                entity.ContainingMap.Objects
                    .Where(e => e.WrappedObject is IMsbEntry)
                    .Select(e => e as MsbEntity),
                entity,
                name
            );
        }
        else
        {
            entity.Name = name;
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var (entity, name) in entities.Zip(newNames))
        {
            Rename(entity, name);
        }
        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach (var (entity, name) in entities.Zip(oldNames))
        {
            Rename(entity, name);
        }
        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}