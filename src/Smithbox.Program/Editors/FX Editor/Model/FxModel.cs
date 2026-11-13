using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.Editors.FxEditor.Model;

// The FX Editor's format-neutral effect model (Claude/Documentation/FFX Older Games Plan.md §5.1, §12).
// Each effect format has adapters implementing these interfaces over its own SoulsFormats classes; panels work on the
// interfaces and ask for optional capabilities (IFxConfigured, IFxProxy, IFxCurveValue, IFxListValue) where a feature
// only exists in some formats. This folder uses only SoulsFormats and the BCL, so check tools can link its sources,
// and holds no display text: kinds and ids are turned into localised text by the GUI.

/// <summary>The effect file formats the model covers.</summary>
public enum FxFormat
{
    /// <summary>FXR v4/v5: DS3, SDT, ER, AC6, NR.</summary>
    Fxr3,
    /// <summary>DLsE/DLSE class streams: DES, DS2, DS2S.</summary>
    Dlse,
}

/// <summary>An effect: its state machine and its structure.</summary>
public interface IFxEffect
{
    FxFormat Format { get; }

    int Id { get; }

    IReadOnlyList<IFxState> States { get; }

    /// <summary>The root of the effect's tree.</summary>
    IFxNode Root { get; }

    /// <summary>The underlying file object (FXR, FFXDLSE).</summary>
    object Source { get; }

    /// <summary>The file's bytes (no DCX).</summary>
    byte[] Write();

    /// <summary>
    /// The adapter of an underlying object of this effect (FxrNode, FxrAction, …), the same instance on every call,
    /// or null when the object isn't one this effect adapts.
    /// </summary>
    object Adapt(object source);
}

public interface IFxState
{
    int Index { get; }

    IReadOnlyList<IFxTransition> Transitions { get; }

    /// <summary>Actions run while in this state, for formats that keep them on states (DLSE). Empty for FXR3.</summary>
    IReadOnlyList<IFxAction> Actions { get; }

    object Source { get; }
}

/// <summary>A state exit: when <see cref="Condition"/> holds, the effect goes to <see cref="TargetState"/>.</summary>
public interface IFxTransition
{
    /// <summary>The target state index, -1 for "end".</summary>
    int TargetState { get; }

    IFxExpression Condition { get; }

    object Source { get; }
}

public enum FxExpressionKind
{
    /// <summary>Two operands compared with <see cref="IFxExpression.Operator"/>.</summary>
    Compare,
    And,
    Or,
    Not,
    /// <summary>A number (<see cref="IFxExpression.Value"/>).</summary>
    Constant,
    /// <summary>An external value (<see cref="IFxExpression.Index"/>).</summary>
    External,
    StateTime,
    CurrentTick,
    TotalTick,
    /// <summary>Anything else; <see cref="IFxExpression.Index"/> holds the format's own type id.</summary>
    Unknown,
}

/// <summary>A condition, as a tree: FXR3 conditions are one Compare of two leaves; DLSE nests And/Or/Not.</summary>
public interface IFxExpression
{
    FxExpressionKind Kind { get; }

    /// <summary>For Compare: "==", "!=", "&lt;", "&lt;=", "&gt;", "&gt;=" (or "?" for an unknown operator).</summary>
    string Operator { get; }

    /// <summary>For Constant: the value, or null when the file stores none.</summary>
    double? Value { get; }

    /// <summary>For External: the value index. For Unknown: the format's type id.</summary>
    int Index { get; }

    IReadOnlyList<IFxExpression> Operands { get; }
}

public enum FxNodeKind
{
    Root,
    Proxy,
    LevelsOfDetail,
    Basic,
    NodeEmitter,
    Unknown,
}

/// <summary>An item of the effect tree.</summary>
public interface IFxNode
{
    FxNodeKind Kind { get; }

    /// <summary>The format's own type id (FXR3: 2000 Root, 2001 Proxy, …).</summary>
    int TypeId { get; }

    IReadOnlyList<IFxNode> Children { get; }

    /// <summary>The node's own actions (FXR3: not the ones in its configs).</summary>
    IReadOnlyList<IFxAction> Actions { get; }

    object Source { get; }
}

/// <summary>Capability: a node whose actions come in configs, one used per state (FXR3).</summary>
public interface IFxConfigured
{
    IReadOnlyList<IFxConfig> Configs { get; }

    /// <summary>Which config each state uses (FXR3 action 199), or null when the node has no map.</summary>
    IReadOnlyList<int> StateConfigMap { get; }
}

/// <summary>Capability: a node that plays another effect.</summary>
public interface IFxProxy
{
    /// <summary>The referenced effect id, or -1 when the node stores none.</summary>
    int TargetEffect { get; }
}

public enum FxConfigKind
{
    LevelsOfDetail,
    Basic,
    NodeEmitter,
    Unknown,
}

public interface IFxConfig
{
    FxConfigKind Kind { get; }

    int TypeId { get; }

    IReadOnlyList<IFxAction> Actions { get; }

    /// <summary>The config type's fixed action slots in this game, each with the action types that may fill it; null when it has none.</summary>
    IReadOnlyList<int[]> SlotTypes { get; }

    object Source { get; }
}

public interface IFxAction
{
    int TypeId { get; }

    /// <summary>The schema's name, or null when the schema doesn't know the type.</summary>
    string Name { get; }

    /// <summary>The schema's slot or category (Emitter, Appearance, …), or null.</summary>
    string Slot { get; }

    bool IsAppearance { get; }

    /// <summary>True when the action fits its schema layout; otherwise <see cref="Values"/> are the raw data.</summary>
    bool IsTyped { get; }

    /// <summary>The named values in layout order, or the raw values of an untyped action.</summary>
    IReadOnlyList<IFxValue> Values { get; }

    /// <summary>Actions nested inside this one (DLSE action calls). Empty for FXR3.</summary>
    IReadOnlyList<IFxAction> Calls { get; }

    object Source { get; }
}

public enum FxValueKind
{
    Int,
    Float,
    Bool,
    /// <summary>Several floats (<see cref="IFxValue.Components"/>).</summary>
    Vector,
    /// <summary>A curve; see <see cref="IFxCurveValue"/>.</summary>
    Curve,
    /// <summary>A list of ints; see <see cref="IFxListValue"/>.</summary>
    IntList,
    /// <summary>A raw word of an untyped action.</summary>
    Raw,
    /// <summary>Anything else the format stores (calls, references); read through the action's Source.</summary>
    Other,
}

/// <summary>One value of an action.</summary>
public interface IFxValue
{
    string Name { get; }

    FxValueKind Kind { get; }

    int Components { get; }

    /// <summary>The schema calls it unknown (unk…).</summary>
    bool IsUnknown { get; }

    /// <summary>The file stores it (false: the schema's default is shown).</summary>
    bool IsStored { get; }

    string Description { get; }

    /// <summary>The schema's enum name, or null.</summary>
    string Enum { get; }

    /// <summary>The resource kind it refers to (texture, model, …), or null.</summary>
    string Resource { get; }

    /// <summary>Int/Float/Bool/Vector/Raw values in display units; null for other kinds.</summary>
    double[] Get();

    /// <summary>
    /// Sets an Int/Float/Bool/Vector/Raw value from display units. Components equal to what <see cref="Get"/> returns
    /// keep their stored words, so setting a value to itself never changes the file.
    /// </summary>
    void Set(double[] display);
}

/// <summary>Capability: a value stored as a curve.</summary>
public interface IFxCurveValue
{
    /// <summary>The curve in display units (a new object each call).</summary>
    FxrCurve GetCurve();

    /// <summary>Writes a curve in display units; an unchanged curve leaves the file unchanged.</summary>
    void SetCurve(FxrCurve curve);
}

/// <summary>Capability: a value stored as a list of ints.</summary>
public interface IFxListValue
{
    /// <summary>The stored list, or null when the file stores none.</summary>
    List<int> GetList();

    void SetList(List<int> values);
}
