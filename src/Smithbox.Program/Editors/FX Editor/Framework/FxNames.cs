using SoulsFormats;
using System.Globalization;

namespace StudioCore.Editors.FxEditor;

/// <summary>Display names for FXR structure (node/config types, state conditions, actions).</summary>
public static class FxNames
{
    public static string NodeType(short type) => type switch
    {
        2000 => LOC.Get("FX_Node_Root"),
        2001 => LOC.Get("FX_Node_Proxy"),
        2002 => LOC.Get("FX_Node_LevelsOfDetail"),
        2200 => LOC.Get("FX_Node_Basic"),
        2202 => LOC.Get("FX_Node_NodeEmitter"),
        _ => LOC.Get("FX_Node_Unknown", type),
    };

    public static string ConfigType(short type) => type switch
    {
        1002 => LOC.Get("FX_Config_LevelsOfDetail"),
        1004 => LOC.Get("FX_Config_Basic"),
        1005 => LOC.Get("FX_Config_NodeEmitter"),
        _ => LOC.Get("FX_Config_Unknown", type),
    };

    /// <summary>e.g. "StateTime >= 3 → end".</summary>
    public static string Condition(FxrStateCondition c)
    {
        string op = c.Operator switch { 0 => "==", 1 => "!=", 2 => "<", 3 => "<=", _ => "?" };
        string target = c.TargetState < 0 ? LOC.Get("FX_Condition_End") : LOC.Get("FX_Condition_State", c.TargetState);
        return $"{Operand(c.Left)} {op} {Operand(c.Right)} → {target}";
    }

    public static string Operand(FxrOperand o) => o.Type switch
    {
        -4 => o.HasValue ? o.FloatValue.ToString("G6", CultureInfo.InvariantCulture) : "?",
        -3 => LOC.Get("FX_Operand_External", o.HasValue ? o.IntValue : 0),
        -2 => LOC.Get("FX_Operand_UnkMinus2"),
        -1 => LOC.Get("FX_Operand_StateTime"),
        _ => LOC.Get("FX_Operand_Unknown", o.Type),
    };

    /// <summary>The node's StateConfigMap (action 199): which config each state uses. Null if it has none.</summary>
    public static List<int> StateConfigMap(FxrNode node)
    {
        foreach (var a in node.Actions)
            if (a.Type == 199)
                return a.Section10s.Count > 0 ? a.Section10s[0] : new List<int>();
        return null;
    }

    /// <summary>"Appearance: BillboardEx", or "Action 600 (raw)" when the schema doesn't apply.</summary>
    public static string Action(FxData data, FxEntry entry, FxrAction action)
    {
        var view = data.View(entry, action);
        if (view.Definition == null)
            return LOC.Get("FX_Action_Unknown", action.Type);
        var name = view.Definition.Slot != null ? $"{view.Definition.Slot}: {view.Definition.Name}" : view.Definition.Name;
        return view.IsTyped ? name : LOC.Get("FX_Action_Raw", name);
    }

    // ---------------------------------------------------------------- format-neutral (Model/) labels
    // Same text as the FXR3 versions above for FXR3 effects.

    public static string Node(Model.IFxNode node) => node.Kind switch
    {
        Model.FxNodeKind.Root => LOC.Get("FX_Node_Root"),
        Model.FxNodeKind.Proxy => LOC.Get("FX_Node_Proxy"),
        Model.FxNodeKind.LevelsOfDetail => LOC.Get("FX_Node_LevelsOfDetail"),
        Model.FxNodeKind.Basic => LOC.Get("FX_Node_Basic"),
        Model.FxNodeKind.NodeEmitter => LOC.Get("FX_Node_NodeEmitter"),
        _ => LOC.Get("FX_Node_Unknown", node.TypeId),
    };

    public static string Config(Model.IFxConfig config) => config.Kind switch
    {
        Model.FxConfigKind.LevelsOfDetail => LOC.Get("FX_Config_LevelsOfDetail"),
        Model.FxConfigKind.Basic => LOC.Get("FX_Config_Basic"),
        Model.FxConfigKind.NodeEmitter => LOC.Get("FX_Config_NodeEmitter"),
        _ => LOC.Get("FX_Config_Unknown", config.TypeId),
    };

    /// <summary>e.g. "StateTime &lt;= 3 → end".</summary>
    public static string Condition(Model.IFxTransition t)
    {
        string target = t.TargetState < 0 ? LOC.Get("FX_Condition_End") : LOC.Get("FX_Condition_State", t.TargetState);
        return $"{Expression(t.Condition)} → {target}";
    }

    public static string Expression(Model.IFxExpression e) => e.Kind switch
    {
        Model.FxExpressionKind.Compare when e.Operands.Count == 2 => $"{Expression(e.Operands[0])} {e.Operator} {Expression(e.Operands[1])}",
        Model.FxExpressionKind.And => string.Join(" && ", e.Operands.Select(Grouped)),
        Model.FxExpressionKind.Or => string.Join(" || ", e.Operands.Select(Grouped)),
        Model.FxExpressionKind.Not when e.Operands.Count == 1 => $"!{Grouped(e.Operands[0])}",
        Model.FxExpressionKind.Constant => e.Value is double v ? v.ToString("G6", CultureInfo.InvariantCulture) : "?",
        Model.FxExpressionKind.External => LOC.Get("FX_Operand_External", e.Index),
        Model.FxExpressionKind.StateTime => LOC.Get("FX_Operand_StateTime"),
        Model.FxExpressionKind.Unknown when e.Index == -2 => LOC.Get("FX_Operand_UnkMinus2"),
        _ => LOC.Get("FX_Operand_Unknown", e.Index),
    };

    private static string Grouped(Model.IFxExpression e) =>
        e.Kind is Model.FxExpressionKind.Compare or Model.FxExpressionKind.And or Model.FxExpressionKind.Or ? $"({Expression(e)})" : Expression(e);

    /// <summary>"Appearance: BillboardEx", or "Action 600 (raw)" when the schema doesn't apply.</summary>
    public static string Action(Model.IFxAction action)
    {
        if (action.Name == null)
            return LOC.Get("FX_Action_Unknown", action.TypeId);
        var name = action.Slot != null ? $"{action.Slot}: {action.Name}" : action.Name;
        return action.IsTyped ? name : LOC.Get("FX_Action_Raw", name);
    }

    /// <summary>Splits camelCase schema names for display: particleSizeX → Particle Size X.</summary>
    public static string Pretty(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            char ch = name[i];
            if (i == 0) { sb.Append(char.ToUpperInvariant(ch)); continue; }
            bool boundary = char.IsUpper(ch) && !char.IsUpper(name[i - 1])
                || char.IsDigit(ch) && !char.IsDigit(name[i - 1]) && name[i - 1] != '_';
            if (boundary) sb.Append(' ');
            sb.Append(ch);
        }
        return sb.ToString();
    }
}
