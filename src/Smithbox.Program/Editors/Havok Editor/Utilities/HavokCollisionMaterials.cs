using System;
using System.Collections.Generic;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The collision material IDs a compressed mesh stores per triangle group (fsnpCustomMeshParameter.PrimitiveData
/// m_materialNameData), and the FLVER material naming that selects them.
/// <para>
/// The names follow the OBJ2HKX exporter (Reference/OBJ2HKX/mats.xml): a material called
/// hkm_&lt;Name&gt;_&lt;Property&gt;, e.g. hkm_Cobblestone_Safe1, has the ID of its name plus the ID of its
/// property. The property can be left out. ER map collision uses the same IDs (e.g. 429 is Without + Slide1).
/// </para>
/// </summary>
public static class HavokCollisionMaterials
{
    public static readonly IReadOnlyDictionary<string, uint> Names = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
    {
        ["Dummy"] = 0,
        ["Cobblestone"] = 1,
        ["Stone"] = 2,
        ["Soil"] = 3,
        ["Wood"] = 4,
        ["Grassland"] = 5,
        ["Gravel"] = 6,
        ["Missing"] = 7,
        ["Trees"] = 8,
        ["Swamp"] = 9,
        ["Nest"] = 10,
        ["Iron"] = 11,
        ["Flesh"] = 12,
        ["Sand"] = 13,
        ["Bones"] = 14,
        ["Ash"] = 15,
        ["RoofTiles"] = 16,
        ["Cloth"] = 17,
        ["FallenLeaves"] = 18,
        ["Bell"] = 19,
        ["WaterPuddle"] = 20,
        ["WaterAnkles"] = 21,
        ["WaterKnees"] = 22,
        ["PoisonSwampShallow"] = 23,
        ["PoisonSwampDeep"] = 24,
        ["PoisonSwampKnees"] = 25,
        ["PoisonSwampPuddle"] = 26,
        ["WaterWaist"] = 27,
        ["Carpet"] = 28,
        ["Without"] = 29,
        ["Eclipse"] = 30,
        ["GodGraveNet"] = 31,
    };

    public static readonly IReadOnlyDictionary<string, uint> Properties = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
    {
        ["Safe1"] = 100,
        ["Safe2"] = 200,
        ["Safe3"] = 300,
        ["Slide1"] = 400,
        ["Slide2"] = 500,
    };

    /// <summary>
    /// Reads the collision material ID from a FLVER material name of the form hkm_Name or hkm_Name_Property.
    /// </summary>
    public static bool TryParse(string materialName, out uint id)
    {
        id = 0;

        if (string.IsNullOrEmpty(materialName) || !materialName.StartsWith("hkm_", StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = materialName.Substring(4).Split('_');

        if (!Names.TryGetValue(parts[0], out var nameId))
            return false;

        uint propertyId = 0;
        if (parts.Length > 1 && !Properties.TryGetValue(parts[1], out propertyId))
            return false;

        id = nameId + propertyId;
        return true;
    }
}
