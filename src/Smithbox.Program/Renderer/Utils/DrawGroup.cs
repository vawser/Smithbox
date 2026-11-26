using System.Collections.Generic;

namespace StudioCore.Renderer;

public class DrawGroup
{
    public DrawGroup()
    {
        AlwaysVisible = true;
    }

    public DrawGroup(uint[] groups)
    {
        AlwaysVisible = false;
        RenderGroups = groups;
    }

    public bool AlwaysVisible { get; set; } = true;
    public uint[] RenderGroups { get; set; }

    /// <summary>
    /// On an object's groups: what it belongs to (its map), for <see cref="ScopeOverrides" />. Null for none.
    /// </summary>
    public object Scope { get; set; }

    /// <summary>
    /// On a scene's display groups: other display groups for objects of a scope, such as a connect collision's groups
    /// for its target map (Display Groups tool, View From Connect). Null when nothing is overridden.
    /// </summary>
    public Dictionary<object, DrawGroup> ScopeOverrides { get; set; }

    public bool IsInDisplayGroup(DrawGroup disp)
    {
        if (Scope != null && disp?.ScopeOverrides != null && disp.ScopeOverrides.TryGetValue(Scope, out var scoped))
        {
            disp = scoped;
        }

        if (AlwaysVisible || disp.AlwaysVisible || RenderGroups == null)
        {
            return true;
        }

        var isAllZero = true;

        for (var i = 0; i < RenderGroups.Length && i < disp.RenderGroups.Length; i++)
        {
            if ((RenderGroups[i] & disp.RenderGroups[i]) != 0)
            {
                return true;
            }

            if (RenderGroups[i] != 0)
            {
                isAllZero = false;
            }
        }

        if (isAllZero)
        {
            return true;
        }

        return false;
    }
}
