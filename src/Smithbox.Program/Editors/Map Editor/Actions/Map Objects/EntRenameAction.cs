using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class EntRenameAction : ViewportAction
{
    private readonly List<MsbEntity> Entities;
    private readonly List<string> NewNames;
    private readonly List<string> OldNames;

    private readonly bool ApplyDuplicateHandling;

    public EntRenameAction(List<MsbEntity> entities, List<string> newNames, bool reference)
    {
        Entities = entities;
        OldNames = entities.Select(e => e.Name).ToList();
        NewNames = newNames;
        ApplyDuplicateHandling = reference;
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
        foreach (var (entity, name) in Entities.Zip(NewNames))
        {
            Rename(entity, name);
        }
        return ActionEvent.ObjectAddedRemoved;
    }

    private void Rename(MsbEntity entity, string name)
    {
        entity.Name = name;

        //if (reference)
        //{
        //    MapEditorActionHelper.SetNameHandleDuplicate(
        //        entity.ContainingMap,
        //        entity.ContainingMap.Objects
        //            .Where(e => e.WrappedObject is IMsbEntry)
        //            .Select(e => e as MsbEntity),
        //        entity,
        //        name
        //    );
        //}
        //else
        //{
        //    entity.Name = name;
        //}
    }

    public override string GetEditMessage()
    {
        return "";
    }
}