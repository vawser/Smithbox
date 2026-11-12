using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// What a step of the history changes, so the History tool can name it and an undo or redo can first go to it.
/// A target is null where the step doesn't have one, or spans several (as a mass edit does).
/// </summary>
public interface ITimeActHistoryItem
{
    TimeActContainerWrapper TargetSource { get; }
    TimeActEntry TargetEntry { get; }
    TAE.Animation TargetAnimation { get; }

    /// <summary>
    /// An event the step changes. Its animation is looked up from it when the step doesn't name one.
    /// </summary>
    TAE.Event TargetEvent { get; }

    /// <summary>
    /// What the step does, without where: the History tool adds the source, time act and animation
    /// </summary>
    string Describe();
}

/// <summary>
/// An edit to the time act data. These only hold the entries, animations and events they change, which outlive
/// the model in the viewport, so they stay in the history when the source or model changes. The scope marks
/// them apart from the viewport's own actions (gizmo moves of the model), which are dropped with the model.
/// </summary>
public abstract class TimeActAction : ViewportAction, ITimeActHistoryItem
{
    public const string Scope = "TimeAct";

    protected TimeActAction()
    {
        TouchedScopes.Add(Scope);
    }

    public virtual TimeActContainerWrapper TargetSource => null;
    public virtual TimeActEntry TargetEntry => null;
    public virtual TAE.Animation TargetAnimation => null;
    public virtual TAE.Event TargetEvent => null;

    public abstract string Describe();

    /// <summary>
    /// How a value is shown in the history: bytes as hex, a mini header by its type, and the rest as they print
    /// </summary>
    public static string FormatValue(object value)
    {
        return value switch
        {
            null => "",
            byte[] bytes => string.Join(" ", bytes.Select(e => $"{e:X2}")),
            TAE.Animation.AnimMiniHeader header => header.Type.ToString(),
            float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    /// <summary>
    /// How an event is named in the history: its type, and the name of its template when it has one
    /// </summary>
    public static string NameEvent(TAE.Event ev)
    {
        if (ev == null)
            return "";

        var name = ev.Template?.Name ?? ev.TypeName;

        return name != null ? $"[{ev.Type}] {name}" : $"[{ev.Type}]";
    }
}

/// <summary>
/// Several time act edits taken back by one undo. It goes to what they change when they all change the same
/// time act and animation, and is named by its label, or else by the first edit and how many more there are.
/// </summary>
public class TimeActCompoundAction : ViewportCompoundAction, ITimeActHistoryItem
{
    private readonly List<ITimeActHistoryItem> Items;
    private readonly string Label;

    public TimeActCompoundAction(List<ViewportAction> actions, string label = null) : base(actions)
    {
        Items = actions.OfType<ITimeActHistoryItem>().ToList();
        Label = label;
    }

    public TimeActContainerWrapper TargetSource => Shared(e => e.TargetSource);
    public TimeActEntry TargetEntry => Shared(e => e.TargetEntry);
    public TAE.Animation TargetAnimation => Shared(e => e.TargetAnimation);
    public TAE.Event TargetEvent => Items.Count > 0 && TargetEntry != null ? Items[0].TargetEvent : null;

    private T Shared<T>(System.Func<ITimeActHistoryItem, T> get) where T : class
    {
        if (Items.Count == 0)
            return null;

        var first = get(Items[0]);

        return Items.All(e => get(e) == first) ? first : null;
    }

    public string Describe()
    {
        if (Label != null)
            return Label;

        if (Items.Count == 0)
            return "";

        return Items.Count == 1
            ? Items[0].Describe()
            : LOC.Get("TAE_History_Compound", Items[0].Describe(), Items.Count - 1);
    }
}
