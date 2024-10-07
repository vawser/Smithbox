using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Credit to RunDevelopment for this code
/// </summary>
public class GX00ItemDescriptor
{
    // E.g. "GX00", "GX80"
    public string ID { get; set; } = "Unknown";
    // The Unk04 value of this GX00.
    // All GX00 item lists seem to have a unique Unk04 value.
    public int Unk04 { get; set; }
    // If the category is not null, then all items are grouped together.
    // Note: Multiple item lists can have the same category.
    public string? Category { get; set; }
    public List<GX00ItemValueDescriptor> Items { get; set; } = new List<GX00ItemValueDescriptor>();

    public bool Fits(FLVER2.GXItem item)
    {
        if (item.ID != ID || item.Unk04 != Unk04) return false;
        return Fits(item.Data);
    }
    public bool Fits(byte[] data)
    {
        if (data.Length % 4 != 0) return false;
        return Fits(data.ToGxValues());
    }
    public bool Fits(GXValue[] values)
    {
        if (values.Length != Items.Count) return false;
        for (int i = 0; i < values.Length; i++)
        {
            if (!Items[i].Fits(values[i]))
                return false;
        }
        return true;
    }
}