using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Makes the event group for an event added to an animation, the way the game's own files have them. Going by
/// Tools/TaeTemplateSurvey groupmap over vanilla ER, NR, AC6, SDT, DS3, DES and DB_NT (2026-11-12):
/// - most groups hold a single event (ER 69%, DS3 74%, SDT 80%), and the events of one type in an animation
///   are usually spread over groups of their own (ER 86%), so each new event gets a new group;
/// - the group type is set by the event type, but is often not the same number (ER type 129 is in group type 128),
///   and over 99% of each type's events use one group type, which TAE/{Game}/Groups.json records;
/// - the group data type is 0 in every game but Demon's Souls, rather than the event type.
/// Animations without groups (the older formats) get none.
/// </summary>
public static class TimeActEventGroups
{
    /// <summary>
    /// A new group for an event of the type, or null if the animation doesn't use groups. Types the game's files
    /// never put in a group fall back to a group of their own type, with data type 0.
    /// </summary>
    public static TAE.EventGroup Create(TAE.Animation animation, int eventType,
        IReadOnlyDictionary<int, TimeActGroupMapEntry> groupMap)
    {
        if (animation.EventGroups.Count == 0)
            return null;

        long groupType = eventType;
        long dataType = 0;

        if (groupMap != null && groupMap.TryGetValue(eventType, out var entry))
        {
            groupType = entry.GroupType;
            dataType = entry.DataType;
        }

        var group = new TAE.EventGroup(groupType);
        group.GroupData.DataType = (TAE.EventGroup.EventGroupDataType)dataType;

        return group;
    }
}
