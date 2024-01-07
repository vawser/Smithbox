namespace StudioCore.Scene;

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

    public bool IsInDisplayGroup(DrawGroup disp)
    {
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
