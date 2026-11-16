using Hexa.NET.ImGui;

namespace StudioCore.Editors.Common;

/// <summary>
/// Shows whether an event flag ID is one the game stores, and what its range is used for, wherever an editor shows a flag.
/// Only ER has an allocation model (ProjectData.EventFlags); for other games everything here does nothing.
/// </summary>
public static class EventFlagDisplay
{
    public static EventFlagInfo Info(ProjectEntry project, long flag) =>
        project?.Handler?.ProjectData?.EventFlags?.Describe(flag);

    /// <summary>A one-line summary: "Map m60_43_36_00 (open world tile) · item pickups"; null when there's no model</summary>
    public static string Summary(ProjectEntry project, long flag)
    {
        var info = Info(project, flag);
        if (info == null || info.Validity == EventFlagValidity.Zero)
            return null;

        var text = info.Usage != null ? $"{info.Range} · {info.Usage}" : info.Range;
        return info.IsValid ? text : LOC.Get("EVENTFLAG_Summary_Unallocated", text);
    }

    /// <summary>The full tooltip text: validity, range, typical use and, for an unallocated flag, why</summary>
    public static string Tooltip(EventFlagInfo info)
    {
        if (info.Validity == EventFlagValidity.Zero)
            return info.Reason;

        var lines = new List<string>
        {
            info.IsValid ? LOC.Get("EVENTFLAG_Tooltip_Valid") : LOC.Get("EVENTFLAG_Tooltip_Unallocated"),
            LOC.Get("EVENTFLAG_Tooltip_Range", info.Range)
        };
        if (info.Usage != null)
            lines.Add(LOC.Get("EVENTFLAG_Tooltip_Usage", info.Usage));
        if (!info.IsValid && info.Reason != null)
            lines.Add(info.Reason);
        return string.Join("\n", lines);
    }

    /// <summary>
    /// A small warning icon (on the same line) when the flag isn't stored by the game, with the reason as its tooltip.
    /// Draws nothing for a valid flag, for 0 (the usual "no flag"), or without a model.
    /// </summary>
    public static void Marker(ProjectEntry project, long flag)
    {
        if (flag <= 0)
            return;

        var info = Info(project, flag);
        if (info == null || info.IsValid)
            return;

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);
        ImGui.TextUnformatted(Icons.ExclamationTriangle);
        ImGui.PopStyleColor();
        GUI.Tooltip(Tooltip(info));
    }
}
