using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Credit to RunDevelopment for this code
/// </summary>
public struct Float5
{
    public float Item0;
    public float Item1;
    public float Item2;
    public float Item3;
    public float Item4;

    public Float5(float all)
    {
        Item0 = all;
        Item1 = all;
        Item2 = all;
        Item3 = all;
        Item4 = all;
    }
    public Float5(float item0, float item1, float item2, float item3, float item4)
    {
        Item0 = item0;
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
        Item4 = item4;
    }
}