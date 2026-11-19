using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The HKX files a duplicated time act takes with it, so it has animations of its own: each animation file of the
/// original's category that its animations use is copied to the new category (a033_003000 -> a051_003000).
///
/// - An animation that plays its own file gets a copy of it under its new ID.
/// - An animation that imports another of the same category (ImportsHKX / ImportOtherAnim pointing into a033) is
///   pointed at the copy (a051_...), and that file is copied too, so the duplicate doesn't play the original's files.
/// - An import from another category (a000, the shared base set) is left as it is, still shared.
/// - A file the new category already has is kept, not replaced.
///
/// Only a time act whose animations hold the lower half of their ID (the player's, one time act per category) is
/// affected; an enemy's animations carry their full ID, so a copy of its time act shares the same files.
/// </summary>
public static class TimeActDuplicateAnimationFiles
{
    public class Result
    {
        public List<ViewportAction> Actions = new();
        public int Copied;
        public int Kept;
        public int Missing;
        public int Retargeted;
    }

    /// <summary>
    /// Points the copy's imports of the original's category at its own, and returns the actions that add the
    /// copied HKX files (held by the source until it's saved). Call on a copy that isn't in the source yet.
    /// </summary>
    public static Result Prepare(TimeActContainerWrapper source, TimeActEntry original, TimeActEntry copy)
    {
        var result = new Result();
        var size = source.AnimationCategorySize;

        copy.Load();

        if (copy.TimeAct == null)
            return result;

        long oldCategory = original.BindIndex;
        long newCategory = copy.BindIndex;

        long Move(long id) => newCategory * size + id % size;
        bool InOldCategory(long id) => id >= 0 && id / size == oldCategory;

        // Old full ID -> new full ID of every file the copy's animations use in the old category
        var moves = new Dictionary<long, long>();

        foreach (var animation in copy.TimeAct.Animations)
        {
            // An enemy's animations keep their full ID in any time act
            if (animation.ID >= size)
                continue;

            switch (animation.MiniHeader)
            {
                case TAE.Animation.AnimMiniHeader.Standard { ImportsHKX: true } standard:
                    if (InOldCategory(standard.ImportHKXSourceAnimID))
                    {
                        moves[standard.ImportHKXSourceAnimID] = Move(standard.ImportHKXSourceAnimID);
                        standard.ImportHKXSourceAnimID = (int)Move(standard.ImportHKXSourceAnimID);
                        result.Retargeted++;
                    }
                    break;

                case TAE.Animation.AnimMiniHeader.ImportOtherAnim import:
                    if (InOldCategory(import.ImportFromAnimID))
                    {
                        moves[import.ImportFromAnimID] = Move(import.ImportFromAnimID);
                        import.ImportFromAnimID = (int)Move(import.ImportFromAnimID);
                        result.Retargeted++;
                    }
                    break;

                default:
                    // Plays its own file
                    moves[oldCategory * size + animation.ID] = newCategory * size + animation.ID;
                    break;
            }
        }

        if (moves.Count == 0)
            return result;

        using var files = TimeActAnimationFiles.Open(source, false);

        // A character variant that plays another's animations has none of its own, and giving it some would
        // stop it playing the shared ones (as Import HKX)
        if (source.IsCharacter && !files.HasAnimationFiles)
        {
            Smithbox.Log(typeof(TimeActDuplicateAnimationFiles), LOC.Get("TAE_Duplicate_Files_Shared_Set", source.Name));
            return result;
        }

        foreach (var (oldId, newId) in moves.OrderBy(e => e.Key))
        {
            if (files.Contains(newId))
            {
                result.Kept++;
                continue;
            }

            if (!files.TryRead(oldId, out var bytes))
            {
                result.Missing++;
                continue;
            }

            // In the binders that hold the original, so a split anibnd keeps each category's files together
            var binderPaths = !files.IsStored(oldId) && source.PendingAnimationFiles.TryGetValue(oldId, out var pending)
                ? pending.BinderPaths.ToList()
                : files.GetImportBinderPaths(oldId);

            result.Actions.Add(new TimeActImportAnimationFileAction(source,
                new TimeActPendingAnimationFile(newId, bytes, binderPaths)));

            result.Copied++;
        }

        return result;
    }
}
