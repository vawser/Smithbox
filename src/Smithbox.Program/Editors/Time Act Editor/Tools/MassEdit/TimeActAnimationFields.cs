using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// A property of an animation that the mass edit can change, as the Properties window shows it. Get gives null
/// when the animation doesn't have the property, as the mini header decides which ones it has.
/// ChangesMotion marks the properties that decide which motion plays, so the playback looks it up again.
/// </summary>
public record TimeActAnimationField(string Name, string LabelKey, TimeActMassEditFieldKind Kind, bool ChangesMotion,
    Func<TAE.Animation, object> Get, Action<TAE.Animation, object> Set);

/// <summary>
/// The animation properties the mass edit can change. The ID is left out: the animations are kept in ID order,
/// and the name and motion come from it, so changing many at once would clash with the IDs already in use.
/// Duplicating under a new ID is done by Create instead.
/// </summary>
public static class TimeActAnimationFields
{
    private static TAE.Animation.AnimMiniHeader.Standard Standard(TAE.Animation animation) =>
        animation.MiniHeader as TAE.Animation.AnimMiniHeader.Standard;

    private static TAE.Animation.AnimMiniHeader.ImportOtherAnim Import(TAE.Animation animation) =>
        animation.MiniHeader as TAE.Animation.AnimMiniHeader.ImportOtherAnim;

    public static readonly IReadOnlyList<TimeActAnimationField> All =
    [
        new("AnimFileName", "TAE_PropertiesWindow_Animation_File_Name", TimeActMassEditFieldKind.Text, false,
            a => a.AnimFileName ?? "",
            (a, v) => a.AnimFileName = (string)v),

        new("IsLoopByDefault", "TAE_PropertiesWindow_Animation_Loop_By_Default", TimeActMassEditFieldKind.Bool, false,
            a => Standard(a)?.IsLoopByDefault,
            (a, v) => Standard(a).IsLoopByDefault = (bool)v),

        new("ImportsHKX", "TAE_PropertiesWindow_Animation_Imports_HKX", TimeActMassEditFieldKind.Bool, true,
            a => Standard(a)?.ImportsHKX,
            (a, v) => Standard(a).ImportsHKX = (bool)v),

        new("ImportHKXSourceAnimID", "TAE_PropertiesWindow_Animation_Import_HKX_Source", TimeActMassEditFieldKind.Number, true,
            a => Standard(a)?.ImportHKXSourceAnimID,
            (a, v) => Standard(a).ImportHKXSourceAnimID = (int)v),

        new("AllowDelayLoad", "TAE_PropertiesWindow_Animation_Allow_Delay_Load", TimeActMassEditFieldKind.Bool, false,
            a => Standard(a)?.AllowDelayLoad,
            (a, v) => Standard(a).AllowDelayLoad = (bool)v),

        new("ImportFromAnimID", "TAE_PropertiesWindow_Animation_Import_From", TimeActMassEditFieldKind.Number, true,
            a => Import(a)?.ImportFromAnimID,
            (a, v) => Import(a).ImportFromAnimID = (int)v),

        new("ImportUnknown", "TAE_PropertiesWindow_Animation_Import_Unknown", TimeActMassEditFieldKind.Number, false,
            a => Import(a)?.Unknown,
            (a, v) => Import(a).Unknown = (int)v)
    ];

    public static TimeActAnimationField Find(string name) => All.FirstOrDefault(e => e.Name == name);
}
