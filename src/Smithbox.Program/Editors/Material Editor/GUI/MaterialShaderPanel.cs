using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The Shader Information tool's content (<see cref="MatShaderTool"/>): what the shader table says the shader does
/// with each texture slot, and the params and samplers the material's shader's other materials all have and it
/// lacks. Also the shader labels the Parameters and Textures windows put on each entry.
/// </summary>
public class MaterialShaderPanel
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    public MaterialShaderPanel(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MaterialBank Bank => Project.Handler.MaterialData.PrimaryBank;

    /// <summary>
    /// The shader info of a material: its shader, in its static switch permutation
    /// </summary>
    public MaterialShaderInfo InfoFor(object material) => material switch
    {
        MTD m => Bank.GetShaderInfo(m.ShaderPath, 0),
        MATBIN m => Bank.GetShaderInfo(m.ShaderPath,
            m.Params.FirstOrDefault(p => p.Name == "GXFT_SATStaticSwitch")?.Value is int sw ? sw : 0),
        _ => null
    };

    /// <summary>
    /// The Shader Information tool's groups: the shader, its texture slots, and the entries the material lacks
    /// </summary>
    public void Draw(MaterialSourceType type, object material)
    {
        var info = InfoFor(material);
        if (info == null)
        {
            GUI.WrappedText(LOC.Get("MAT_ShaderTool_No_Shader"));
            return;
        }

        // Shader
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_Shader_Header", info.ShaderName), LOC.Get("MAT_ShaderTool_Shader_Header_TT"));

        if (info.Known)
        {
            GUI.WrappedText(info.Deformer != MaterialDeformer.None
                ? LOC.Get("MAT_Shader_Known_Deformer", info.Deformer)
                : LOC.Get("MAT_Shader_Known"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAT_Shader_Unknown"));
        }

        // Texture Slots
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_ShaderTool_Slots_Header"), LOC.Get("MAT_ShaderTool_Slots_Header_TT"));

        if (!DisplaySlots(material, info))
            GUI.WrappedText(LOC.Get("MAT_ShaderTool_No_Slots"));

        // Missing Entries
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_ShaderTool_Missing_Header"), LOC.Get("MAT_ShaderTool_Missing_Header_TT"));

        DisplayMissing(type, material);
    }

    #region Slots
    /// <summary>
    /// The slot table; false when there are no slots to show
    /// </summary>
    private bool DisplaySlots(object material, MaterialShaderInfo info)
    {
        var samplers = material switch
        {
            MTD m => m.Textures.Select(t => t.Type).ToList(),
            MATBIN m => m.Samplers.Select(s => s.Type).ToList(),
            _ => new List<string>()
        };

        var bySlot = samplers.Where(s => MaterialShaderInfo.SlotOf(s) >= 0)
            .GroupBy(MaterialShaderInfo.SlotOf).ToDictionary(g => g.Key, g => g.First());

        var slots = info.SlotGroups.Keys.Concat(info.SlotUses.Keys).Concat(bySlot.Keys).Distinct().OrderBy(x => x).ToList();

        if (slots.Count == 0)
            return false;

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp;

        if (!ImGui.BeginTable("materialShaderSlots", 5, flags))
            return true;

        ImGui.TableSetupColumn(LOC.Get("MAT_Shader_Slot"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("MAT_Shader_Sampler"));
        ImGui.TableSetupColumn(LOC.Get("MAT_Shader_Group"));
        ImGui.TableSetupColumn(LOC.Get("MAT_Shader_UvSet"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("MAT_Shader_Uses"));
        ImGui.TableHeadersRow();

        foreach (var slot in slots)
        {
            ImGui.TableNextRow();

            // Slot
            ImGui.TableNextColumn();
            ImGui.Text($"{slot}");

            // Sampler, or a note that the shader samples a slot the material has nothing for
            ImGui.TableNextColumn();
            if (bySlot.TryGetValue(slot, out var sampler))
            {
                ImGui.Text(sampler);
            }
            else
            {
                ImGui.TextDisabled(LOC.Get("MAT_Shader_No_Sampler"));
                GUI.Tooltip(LOC.Get("MAT_Shader_No_Sampler_TT"));
            }

            // UV group, with its tiling
            ImGui.TableNextColumn();
            if (info.SlotGroups.TryGetValue(slot, out var group))
            {
                var tiling = ParamValue(material, $"{group}_CommonUV-UVParam");
                ImGui.Text(tiling == null ? group : $"{group} ({tiling})");
                GUI.Tooltip(LOC.Get("MAT_Shader_Group_TT", $"{group}_CommonUV-UVParam"));
            }

            // UV set
            ImGui.TableNextColumn();
            if (info.SlotUvSets.TryGetValue(slot, out var set))
                ImGui.Text(set);

            // Uses: the slot's own, then the params that act on it
            ImGui.TableNextColumn();
            var uses = info.UsesOfSlot(slot).Select(u => Describe(info, u))
                .Concat(info.ParamUses.SelectMany(kv => kv.Value.Where(u => u.Slot == slot).Select(u => $"{Describe(info, u)}: {kv.Key}")))
                .Distinct().ToList();

            if (uses.Count > 0)
            {
                ImGui.TextWrapped(string.Join("; ", uses));
            }
        }

        ImGui.EndTable();

        return true;
    }

    private static string ParamValue(object material, string name)
    {
        object value = material switch
        {
            MTD m => m.Params.FirstOrDefault(p => p.Name == name)?.Value,
            MATBIN m => m.Params.FirstOrDefault(p => p.Name == name)?.Value,
            _ => null
        };

        return value switch
        {
            null => null,
            float[] f => string.Join(", ", f.Select(x => $"{x:0.###}")),
            int[] i => string.Join(", ", i),
            _ => $"{value}"
        };
    }
    #endregion

    #region Add Missing
    private void DisplayMissing(MaterialSourceType type, object material)
    {
        var templates = Bank.Templates;
        var (parms, samplers) = templates.Missing(type, material);

        if (!templates.IsReady)
        {
            GUI.WrappedText(LOC.Get("MAT_Shader_Templates_Building"));
            return;
        }

        if (parms.Count + samplers.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAT_ShaderTool_None_Missing"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAT_Shader_Missing", parms.Count, samplers.Count));

        var names = parms.Select(p => p.Name).Concat(samplers.Select(s => s.Type)).ToList();
        GUI.Tooltip(string.Join("\n", names));

        GUI.MultiButtonInput("materialAddMissingActions",
            "materialAddMissing", LOC.Get("MAT_Shader_Add_Missing"), LOC.Get("MAT_Shader_Add_Missing_TT"),
            () => AddMissing(material, parms, samplers));
    }

    /// <summary>
    /// Adds the missing entries with the commonest type and value among the shader's materials, as one undo step
    /// </summary>
    private void AddMissing(object material, List<TemplateParam> parms, List<TemplateSampler> samplers)
    {
        var file = View.Selection.SelectedFileWrapper;
        var actions = new List<EditorAction>();

        switch (material)
        {
            case MATBIN matbin:
            {
                var paramsProp = typeof(MATBIN).GetProperty(nameof(MATBIN.Params));
                var samplersProp = typeof(MATBIN).GetProperty(nameof(MATBIN.Samplers));
                var paramIndex = matbin.Params.Count;
                var samplerIndex = matbin.Samplers.Count;

                foreach (var p in parms)
                {
                    var param = new MATBIN.Param { Name = p.Name, Type = (MATBIN.ParamType)p.Type };
                    param.Value = MaterialTemplates.CopyValue(p.Value);
                    MaterialKeys.Apply(param);
                    actions.Add(new MatAddListEntry(file, paramsProp, matbin, param, paramIndex++));
                }

                foreach (var s in samplers)
                {
                    var sampler = new MATBIN.Sampler { Type = s.Type, Path = "", Unk14 = s.Unk14 };
                    MaterialKeys.Apply(sampler);
                    actions.Add(new MatAddListEntry(file, samplersProp, matbin, sampler, samplerIndex++));
                }
                break;
            }

            case MTD mtd:
            {
                var paramsProp = typeof(MTD).GetProperty(nameof(MTD.Params));
                var texturesProp = typeof(MTD).GetProperty(nameof(MTD.Textures));
                var paramIndex = mtd.Params.Count;
                var textureIndex = mtd.Textures.Count;

                foreach (var p in parms)
                {
                    var param = new MTD.Param(p.Name, (MTD.ParamType)p.Type, MaterialTemplates.CopyValue(p.Value));
                    actions.Add(new MatAddListEntry(file, paramsProp, mtd, param, paramIndex++));
                }

                foreach (var s in samplers)
                {
                    // The commonest form of the texture among the shader's materials: its UV number and shader data
                    // index matter to the game, so a bare texture of the type wouldn't do
                    var texture = s.Texture != null ? MaterialTemplates.CopyTexture(s.Texture) : new MTD.Texture { Type = s.Type };
                    actions.Add(new MatAddListEntry(file, texturesProp, mtd, texture, textureIndex++));
                }
                break;
            }
        }

        if (actions.Count > 0)
            View.ActionManager.ExecuteAction(new CompoundAction(actions));
    }
    #endregion

    #region Labels
    /// <summary>
    /// A use as a short line: "Blend · Layers[0].Sharpness", "Texture slot 3 · Color", "Engine · Blend mode"
    /// </summary>
    public static string Describe(MaterialShaderInfo info, MaterialShaderUse use)
    {
        var section = use.Section switch
        {
            "Deformer" => LOC.Get("MAT_Shader_Section_Deformer", info.Deformer),
            "TextureInput" => use.Slot != null
                ? LOC.Get("MAT_Shader_Section_TextureInput_Slot", use.Slot)
                : LOC.Get("MAT_Shader_Section_TextureInput"),
            "Engine" => LOC.Get($"MAT_Shader_Engine_{use.Detail}"),
            "Tiling" => LOC.Get("MAT_Shader_Section_Tiling", use.Detail),
            _ => LOC.Get($"MAT_Shader_Section_{use.Section}")
        };

        if (use.Section is "Engine" or "Tiling" || use.Detail == "")
            return section;

        return $"{section} · {use.Detail}";
    }

    /// <summary>
    /// The uses of a param, for the Properties list: the first as a label, all of them as a tooltip
    /// </summary>
    public static void DisplayUses(MaterialShaderInfo info, List<MaterialShaderUse> uses)
    {
        if (info == null || uses.Count == 0)
            return;

        ImGui.SameLine();
        ImGui.TextDisabled(uses.Count == 1 ? Describe(info, uses[0]) : $"{Describe(info, uses[0])} (+{uses.Count - 1})");
        GUI.Tooltip(string.Join("\n", uses.Select(u => Describe(info, u))));
    }
    #endregion
}
