using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Picks the collision material of a compressed mesh, by name rather than by number.
/// <para>
/// A material ID is a HitMtrlParam row (the surface: footstep sounds and effects, swamp and water behaviour) plus,
/// in the hundreds, the OBJ2HKX "property" (Safe1-3, Slide1-2), e.g. 429 is row 29 with Slide1. The surface names
/// come from the project's own HitMtrlParam, so they match the game and any rows the project adds.
/// </para>
/// </summary>
public class CollisionMaterialPicker
{
    private readonly ProjectEntry Project;

    private string Filter = "";
    private bool ExactFilter = false;

    public CollisionMaterialPicker(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// The OBJ2HKX property names, by the hundreds of the material ID. 0 has no property.
    /// </summary>
    private static readonly (uint Value, string Key)[] PropertyOptions =
    [
        (0, "HAVOK_CollisionGen_Material_Property_None"),
        (100, "HAVOK_CollisionGen_Material_Property_Safe1"),
        (200, "HAVOK_CollisionGen_Material_Property_Safe2"),
        (300, "HAVOK_CollisionGen_Material_Property_Safe3"),
        (400, "HAVOK_CollisionGen_Material_Property_Slide1"),
        (500, "HAVOK_CollisionGen_Material_Property_Slide2"),
    ];

    private Param GetHitMaterialParam()
    {
        var bank = Project.Handler.ParamData?.PrimaryBank;

        if (bank?.Params == null)
            return null;

        return bank.Params.TryGetValue("HitMtrlParam", out var param) ? param : null;
    }

    /// <summary>
    /// The name of a material's surface row, or null if the row is unnamed or the params aren't loaded
    /// </summary>
    private string GetSurfaceName(uint surface)
    {
        var name = GetHitMaterialParam()?[(int)surface]?.Name;
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private static string GetPropertyName(uint property)
    {
        foreach (var option in PropertyOptions)
        {
            if (option.Value == property)
                return LOC.Get(option.Key);
        }

        return LOC.Get("HAVOK_CollisionGen_Material_Property_Unknown", property);
    }

    /// <summary>
    /// Describes a material ID in words, e.g. "429: Without, Slide1"
    /// </summary>
    public string Describe(uint id)
    {
        var surface = id % 100;
        var property = id - surface;

        var surfaceName = GetSurfaceName(surface) ?? LOC.Get("HAVOK_CollisionGen_Material_Unnamed");

        if (property == 0)
            return LOC.Get("HAVOK_CollisionGen_Material_Description", id, surfaceName);

        return LOC.Get("HAVOK_CollisionGen_Material_Description_Property", id, surfaceName, GetPropertyName(property));
    }

    /// <summary>
    /// The surface, property and raw ID inputs. Returns the chosen ID through <paramref name="materialId"/>.
    /// </summary>
    public void Display(ref int materialId, IReadOnlyList<uint> currentMaterials)
    {
        var id = (uint)Math.Max(materialId, 0);
        var surface = id % 100;
        var property = id - surface;

        // What will be applied, in words
        GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color,
            LOC.Get("HAVOK_CollisionGen_Material_Applying", Describe(id)));

        var param = GetHitMaterialParam();

        // Surface
        GUI.Spacer();
        GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Material_Surface"));

        if (param == null)
        {
            GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Material_No_Param"));
        }
        else
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##collisionMaterialSurface", SurfaceLabel(surface)))
            {
                EditorFilters.DisplayFramedListFilter("collisionMaterialFilter", ref Filter, ref ExactFilter);

                // The property rows (100 and up) are not surfaces of their own
                foreach (var row in param.Rows.Where(r => r.ID >= 0 && r.ID < 100))
                {
                    var label = SurfaceLabel((uint)row.ID);

                    if (!EditorFilters.IsMatch(Filter, label, ExactFilter))
                        continue;

                    if (ImGui.Selectable($"{label}##collisionMaterialSurface{row.ID}", row.ID == surface))
                    {
                        surface = (uint)row.ID;
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("HAVOK_CollisionGen_Material_Surface_TT"));
        }

        // Property
        GUI.Spacer();
        GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Material_Property"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionMaterialProperty", GetPropertyName(property)))
        {
            foreach (var option in PropertyOptions)
            {
                if (ImGui.Selectable($"{LOC.Get(option.Key)}##collisionMaterialProperty{option.Value}", option.Value == property))
                {
                    property = option.Value;
                }
                GUI.Tooltip(LOC.Get($"{option.Key}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("HAVOK_CollisionGen_Material_Property_TT"));

        id = surface + property;

        // The raw ID, for values the lists don't cover
        GUI.Spacer();
        GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Material_Raw_ID"));

        var rawId = (int)id;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputInt("##collisionMaterialId", ref rawId))
        {
            id = (uint)Math.Max(rawId, 0);
        }
        GUI.Tooltip(LOC.Get("HAVOK_CollisionGen_Material_Input_TT"));

        materialId = (int)id;

        // The materials of the shape being replaced
        GUI.Spacer();
        if (currentMaterials.Count == 0)
        {
            GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Material_Current_None"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Material_Current",
                string.Join(", ", currentMaterials.Select(Describe))));
        }
    }

    private string SurfaceLabel(uint surface)
    {
        return $"{surface}: {GetSurfaceName(surface) ?? LOC.Get("HAVOK_CollisionGen_Material_Unnamed")}";
    }

    /// <summary>
    /// The material IDs the compressed mesh shapes of a collision file use, in the order they're listed
    /// </summary>
    public static List<uint> GetShapeMaterials(object root)
    {
        IEnumerable<uint> materials = root switch
        {
            HKX2.hkRootLevelContainer r => r.m_namedVariants
                .Select(v => v.m_variant).OfType<HKX2.hknpPhysicsSceneData>()
                .SelectMany(s => s.m_systemDatas).SelectMany(s => s.m_bodyCinfos)
                .Select(b => b.m_shape).OfType<HKX2.fsnpCustomParamCompressedMeshShape>()
                .SelectMany(s => s.m_pParam?.m_primitiveDataArray ?? new())
                .Select(p => p.m_materialNameData),
            HKLib.hk2019.hkRootLevelContainer r => r.m_namedVariants
                .Select(v => v.m_variant).OfType<HKLib.hk2019.hknpPhysicsSceneData>()
                .SelectMany(s => s.m_systemDatas).SelectMany(s => s.m_bodyCinfos)
                .Select(b => b.m_shape).OfType<HKLib.hk2019.fsnpCustomParamCompressedMeshShape>()
                .SelectMany(s => s.m_pParam?.m_primitiveDataArray ?? new())
                .Select(p => p.m_materialNameData),
            HKLib.hk2018.hkRootLevelContainer r => r.m_namedVariants
                .Select(v => v.m_variant).OfType<HKLib.hk2018.hknpPhysicsSceneData>()
                .SelectMany(s => s.m_systemDatas).SelectMany(s => s.m_bodyCinfos)
                .Select(b => b.m_shape).OfType<HKLib.hk2018.fsnpCustomParamCompressedMeshShape>()
                .SelectMany(s => s.m_pParam?.m_primitiveDataArray ?? new())
                .Select(p => p.m_materialNameData),
            _ => []
        };

        return materials.Distinct().ToList();
    }
}
