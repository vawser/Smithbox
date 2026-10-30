using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

public class LightAtlasResolver
{
    private static readonly Regex PartSuffixRegex = new(@"_\d+$");

    private MapEditorView View;
    private ProjectEntry Project;
    private MapContainer Parent;

    // Answers to the default entry prompt, kept for a frame so a selection edited
    // as several actions only asks once for each model
    private readonly Dictionary<string, bool> DefaultEntryAnswers = new();
    private int DefaultEntryAnswersFrame = -1;

    public LightAtlasResolver(MapEditorView view, ProjectEntry project, MapContainer parent)
    {
        View = view;
        Project = project;
        Parent = parent;
    }

    // Automatically adjust existing references when a Part's name changes
    public void AutomaticAdjustment(Entity ent, string oldName, string newName)
    {
        if (!CFG.Current.MapEditor_LightAtlas_AutomaticAdjust)
            return;

        if (!View.LightAtlasBank.CanUse() || !EntityHelper.IsPart(ent))
            return;

        foreach (var entryEnt in GetEntries(oldName))
        {
            ((BTAB.Entry)entryEnt.WrappedObject).PartName = newName;
            entryEnt.CachedAliasName = null;

            MapContainer.MarkEntityDirty(Parent, entryEnt);
        }
    }

    // Automatically add new entries for new parts, returning the entities created.
    // May prompt the user, so this should only be called on the first execute of an action
    public List<MsbEntity> AutomaticAddition(IEnumerable<MsbEntity> parts)
    {
        return AddEntries(parts
            .Where(e => IsAtlasPart(e) && GetModelName(e.Name) != null)
            .Select(e => (e, GetModelName(e.Name)))
            .ToList());
    }

    // As AutomaticAddition, for existing parts that have changed model, which can differ from their name
    public List<MsbEntity> AutomaticModelAddition(IEnumerable<MsbEntity> parts)
    {
        return AddEntries(parts
            .Where(e => IsAtlasPart(e) && !string.IsNullOrEmpty(e.GetPropertyValue<string>("ModelName")))
            .Select(e => (e, e.GetPropertyValue<string>("ModelName")))
            .ToList());
    }

    // Automatically remove entries when part is deleted, returning the entries to remove
    public List<Entity> AutomaticDeletion(Entity ent)
    {
        if (!CFG.Current.MapEditor_LightAtlas_AutomaticDelete)
            return new();

        if (!View.LightAtlasBank.CanUse() || !EntityHelper.IsPart(ent))
            return new();

        return GetEntries(ent.Name);
    }

    // Build the reference map for the PartNames that the atlas entries point to
    // Since we can't easily use MSBReference
    public void BuildReferenceMaps()
    {
        if (!View.LightAtlasBank.CanUse())
            return;

        foreach(var parent in Parent.LightAtlasParents)
        {
            foreach(var ent in parent.Children)
            {
                if(ent.WrappedObject.GetType() == typeof(BTAB.Entry))
                {
                    BuildReferenceMap(ent);
                }
            }
        }
    }

    public void BuildReferenceMap(Entity ent)
    {

    }

    // The model name is the part name without the trailing ID, e.g. aeg001_930_1000 is aeg001_930
    // Returns null for names without one, such as the placeholder name of a newly created part
    private static string GetModelName(string partName)
    {
        if (string.IsNullOrEmpty(partName) || !PartSuffixRegex.IsMatch(partName))
            return null;

        return PartSuffixRegex.Replace(partName, "");
    }

    private static bool IsAtlasPart(MsbEntity ent)
    {
        return EntityHelper.IsPartMapPiece(ent) || EntityHelper.IsPartAsset(ent);
    }

    public List<Entity> GetEntries(string partName)
    {
        var entries = new List<Entity>();

        foreach (var atlasParent in Parent.LightAtlasParents)
        {
            entries.AddRange(atlasParent.Children.Where(e =>
                e.WrappedObject is BTAB.Entry entry && entry.PartName == partName));
        }

        return entries;
    }

    private List<MsbEntity> AddEntries(List<(MsbEntity Part, string ModelName)> parts)
    {
        var added = new List<MsbEntity>();

        if (!CFG.Current.MapEditor_LightAtlas_AutomaticAdd)
            return added;

        if (!View.LightAtlasBank.CanUse() || Parent.LightAtlasParents.Count == 0)
            return added;

        var unknownParts = new List<(MsbEntity Part, string ModelName)>();

        foreach (var (part, modelName) in parts)
        {
            var inherited = false;

            // A part can be present in several of the map's atlas files, so each is handled separately
            foreach (var atlasParent in Parent.LightAtlasParents)
            {
                if (AddInheritedEntries(atlasParent, part, modelName, added))
                    inherited = true;
            }

            if (!inherited)
                unknownParts.Add((part, modelName));
        }

        var defaultModels = GetDefaultEntryModels(unknownParts.Select(e => e.ModelName).Distinct().ToList());

        foreach (var (part, modelName) in unknownParts)
        {
            if (!defaultModels.Contains(modelName))
                continue;

            var entry = new BTAB.Entry { PartName = part.Name };

            var atlasParent = Parent.LightAtlasParents[0];

            added.Add(AddEntry(atlasParent, entry, atlasParent.Children.Count));
        }

        return added;
    }

    // Copy the entries of the last part in this atlas file that shares the model of the passed part.
    // A part has one entry per lightmapped material, so all of the entries are copied
    private bool AddInheritedEntries(Entity atlasParent, MsbEntity part, string modelName, List<MsbEntity> added)
    {
        var lastIndex = atlasParent.Children.ToList().FindLastIndex(e =>
            e.WrappedObject is BTAB.Entry entry && GetModelName(entry.PartName) == modelName);

        if (lastIndex < 0)
            return false;

        var sourceName = ((BTAB.Entry)atlasParent.Children[lastIndex].WrappedObject).PartName;

        var sourceEntries = atlasParent.Children
            .Select(e => e.WrappedObject)
            .OfType<BTAB.Entry>()
            .Where(e => e.PartName == sourceName)
            .ToList();

        var index = lastIndex + 1;

        foreach (var source in sourceEntries)
        {
            var entry = source.GetClone();
            entry.PartName = part.Name;

            added.Add(AddEntry(atlasParent, entry, index));
            index++;
        }

        return true;
    }

    private MsbEntity AddEntry(Entity atlasParent, BTAB.Entry entry, int index)
    {
        var newEntity = new MsbEntity(View.Universe, Parent, entry, MsbEntityType.LightAtlas);
        newEntity.SupportsName = false;

        Parent.Objects.Add(newEntity);
        atlasParent.AddChild(newEntity, index);

        MapContainer.MarkEntityDirty(Parent, newEntity);

        return newEntity;
    }

    // Returns the models the user wants default entries for, prompting for any not already answered
    private HashSet<string> GetDefaultEntryModels(List<string> modelNames)
    {
        var frame = ImGui.GetFrameCount();

        if (frame != DefaultEntryAnswersFrame)
        {
            DefaultEntryAnswers.Clear();
            DefaultEntryAnswersFrame = frame;
        }

        var unanswered = modelNames.Where(e => !DefaultEntryAnswers.ContainsKey(e)).ToList();

        if (unanswered.Count > 0)
        {
            var result = PlatformUtils.Instance.MessageBox(
                LOC.Get("MAP_LightAtlas_Dialog_Add_Default", Parent.Name, string.Join("\n", unanswered)),
                LOC.Get("MAP_LightAtlas_Dialog_Add_Default_Title"),
                MessageBoxButtons.YesNo);

            foreach (var modelName in unanswered)
            {
                DefaultEntryAnswers[modelName] = result == DialogResult.Yes;
            }
        }

        return modelNames.Where(e => DefaultEntryAnswers[e]).ToHashSet();
    }
}

/// <summary>
/// The light atlas entries an action added and removed for the parts it changed. The edit is worked out
/// once, as doing so may prompt the user, and is then reversed and reapplied as the action is undone and redone.
/// </summary>
public class LightAtlasEdit
{
    private readonly List<Entity> Added = new();
    private readonly List<Entity> Removed = new();

    private List<(Entity Entity, EntityPlacement Placement)> AddedRecords = new();
    private List<(Entity Entity, EntityPlacement Placement)> RemovedRecords = new();

    private LightAtlasEdit() { }

    /// <param name="parts">The new parts</param>
    /// <param name="maps">The map of each part, by index. Parts with no map are skipped.</param>
    public static LightAtlasEdit ForNewParts(IEnumerable<MsbEntity> parts, IEnumerable<ObjectContainer> maps)
    {
        var edit = new LightAtlasEdit();

        var partsByMap = parts.Zip(maps)
            .Where(e => e.Second is MapContainer)
            .GroupBy(e => (MapContainer)e.Second, e => e.First);

        foreach (var group in partsByMap)
        {
            edit.Added.AddRange(group.Key.LightAtlasResolver.AutomaticAddition(group));
        }

        return edit;
    }

    /// <summary>
    /// The entries of a part describe its old model, so they are replaced as if it was a new part of the new model
    /// </summary>
    public static LightAtlasEdit ForModelChange(IEnumerable<Entity> parts)
    {
        var edit = new LightAtlasEdit();

        var partsByMap = parts.OfType<MsbEntity>()
            .Where(e => e.Container is MapContainer)
            .GroupBy(e => (MapContainer)e.Container)
            .ToList();

        foreach (var group in partsByMap)
        {
            edit.Removed.AddRange(group.SelectMany(group.Key.LightAtlasResolver.AutomaticDeletion));
        }

        edit.RemovedRecords = TakeOut(edit.Removed);

        foreach (var group in partsByMap)
        {
            edit.Added.AddRange(group.Key.LightAtlasResolver.AutomaticModelAddition(group));
        }

        return edit;
    }

    /// <summary>
    /// The entries of a part only apply to its own map, so they are replaced with entries from the target map
    /// </summary>
    public static LightAtlasEdit ForMovedParts(IEnumerable<(MsbEntity Part, MapContainer Source, MapContainer Target)> moves)
    {
        var edit = new LightAtlasEdit();

        var moveList = moves.ToList();

        edit.Removed.AddRange(moveList.SelectMany(e => e.Source.LightAtlasResolver.AutomaticDeletion(e.Part)));

        edit.RemovedRecords = TakeOut(edit.Removed);

        foreach (var group in moveList.GroupBy(e => e.Target, e => e.Part))
        {
            edit.Added.AddRange(group.Key.LightAtlasResolver.AutomaticAddition(group));
        }

        return edit;
    }

    public void Undo()
    {
        AddedRecords = TakeOut(Added);
        PutBack(RemovedRecords);
    }

    public void Redo()
    {
        RemovedRecords = TakeOut(Removed);
        PutBack(AddedRecords);
    }

    private static List<(Entity Entity, EntityPlacement Placement)> TakeOut(List<Entity> entries)
    {
        // Capture every placement before removing anything, so the indices are all relative to the original lists
        var records = entries
            .Select(e => (e, EntityPlacement.Capture(e.Container as MapContainer, e)))
            .ToList();

        foreach (var (ent, placement) in records)
        {
            MapContainer.MarkEntityDirty(placement.Map, ent);
            EntityPlacement.Detach(placement.Map, ent);
        }

        return records;
    }

    private static void PutBack(List<(Entity Entity, EntityPlacement Placement)> records)
    {
        EntityPlacement.RestoreAll(records);

        foreach (var (ent, placement) in records)
        {
            MapContainer.MarkEntityDirty(placement.Map, ent);
        }
    }
}
