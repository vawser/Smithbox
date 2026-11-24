using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Register in Behavior: asks the Behavior Editor to add the behavior clips that time act animations need, so the
/// game can play them. The Behavior Editor owns the behavior file (its loading, undo and save), so this only sends
/// the character and the full animation IDs: behavior/register/{character}/{id},{id},...
/// The IDs are sent rather than read from disk there, so unsaved time act changes are included.
/// </summary>
public static class TimeActBehaviorRegistration
{
    /// <summary>
    /// Whether the source's character has a behavior the Behavior Editor can open
    /// </summary>
    public static bool CanRegister(ProjectEntry project, TimeActContainerWrapper source, out string reason)
    {
        reason = null;

        if (source == null || !source.IsCharacter)
        {
            reason = LOC.Get("TAE_Register_Not_Character");
            return false;
        }

        if (!project.Handler.UsesHavokBehaviorEditor() || project.Handler.BehaviorEditor?.HavokBehaviors == null)
        {
            reason = LOC.Get("TAE_Register_No_Behavior_Editor");
            return false;
        }

        // Only the games whose behaviors were surveyed (Claude/Documentation/Time Act.md (Features))
        if (project.Descriptor.ProjectType is not (ProjectType.ER or ProjectType.NR or ProjectType.DS3 or ProjectType.SDT))
        {
            reason = LOC.Get("TAE_Register_Unsupported_Game");
            return false;
        }

        var hasBinder = project.Locator.HavokBehaviorFiles.Entries
            .Any(e => e.Filename.Equals(source.Name, StringComparison.OrdinalIgnoreCase));

        if (!hasBinder)
        {
            reason = LOC.Get("TAE_Register_No_Behavior", source.Name);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sends the animations of a time act (all of them, or the ones given) to the Behavior Editor's Register dialog
    /// </summary>
    public static void Register(TimeActContainerWrapper source, TimeActEntry entry, IEnumerable<TAE.Animation> animations = null)
    {
        entry.Load();

        if (entry.TimeAct == null)
        {
            Smithbox.LogError(typeof(TimeActBehaviorRegistration), LOC.Get("TAE_Register_Load_Failed", entry.Name));
            return;
        }

        var size = source.AnimationCategorySize;
        var ids = (animations ?? entry.TimeAct.Animations)
            .Select(a => entry.GetFullAnimationID(a, size))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return;

        EditorCommandQueue.AddCommand($"behavior/register/{source.Name}/{string.Join(",", ids)}");
    }
}
