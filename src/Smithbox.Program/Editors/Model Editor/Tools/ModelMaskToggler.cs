using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The model having its masks toggled, as described by the editor hosting the tool.
/// </summary>
/// <param name="ModelName">Name of the loaded model.</param>
/// <param name="Flver">The model, for its materials.</param>
/// <param name="Meshes">The entities of its meshes.</param>
public sealed record ModelMaskTarget(string ModelName, FLVER2 Flver, IReadOnlyList<Entity> Meshes);

public class ModelMaskToggler
{
    public ProjectEntry Project;

    // Null while no model is loaded
    private readonly Func<ModelMaskTarget> _getTarget;

    private bool SelectEntry = false;
    private int SelectedID = -1;

    public ModelMaskToggler(ProjectEntry project, Func<ModelMaskTarget> getTarget)
    {
        Project = project;
        _getTarget = getTarget;
    }

    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Model Mask Toggler
        if (ToolSections.Header($"{LOC.Get("MODEL_MaskToggler_Header")}##modelMaskTogglerHeader"))
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_Hint"));
            GUI.Spacer();

            ImGui.Separator();

            ImGui.BeginChild("ModelMaskToolSection");
            Display();
            ImGui.EndChild();
        }
    }

    public void Display()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_Invalid_Project"));
            return;
        }

        if (Project.Handler.ParamEditor == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_No_Param_Editor"));

            return;
        }

        var target = _getTarget();

        if (target == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_No_Model"));

            return;
        }

        var filename = target.ModelName;
        var npcParamKey = "NpcParam";

        if (!Project.Handler.ParamData.PrimaryBank.Params.ContainsKey(npcParamKey))
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_No_Npc_Param"));

            return;
        }

        var npcParam = Project.Handler.ParamData.PrimaryBank.Params[npcParamKey];

        if (npcParam == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_No_Npc_Param"));

            return;
        }

        foreach (var entry in npcParam.Rows)
        {
            if (IsAssociatedParam($"{entry.ID}", filename))
            {
                if (ImGui.Selectable($"[{entry.ID}]##row{entry.ID}", entry.ID == SelectedID, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    ToggleMeshes(target, entry);
                    SelectedID = entry.ID;
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && SelectEntry)
                {
                    SelectEntry = false;
                    SelectedID = entry.ID;
                    ToggleMeshes(target, entry);
                }

                if (ImGui.IsItemFocused())
                {
                    if (InputManager.HasArrowSelection())
                    {
                        SelectEntry = true;
                    }
                }

                GUI.DisplayAlias($"{entry.Name}");
            }
        }
    }

    public bool IsAssociatedParam(string rowID, string filename)
    {
        if (filename.Length >= 4 && rowID.Length >= 4)
        {
            var model = filename.Substring(1, 4); // Remove the 'c'
            var row = rowID.Substring(0, 4);

            if (row == model)
            {
                return true;
            }
        }

        return false;
    }

    public void ToggleMeshes(ModelMaskTarget target, Param.Row row)
    {
        List<bool> maskList = new List<bool>();

        foreach (var cell in row.Cells)
        {
            var internalName = GetInternalName();

            // Works on the assumption that we iterate top to bottom
            // So the natural add order corresponds to the mask I
            // e.g. first entry is mask 0, at index 0, etc
            if (cell.Def.InternalName.Contains(internalName))
            {
                if ($"{cell.Value}" == "0")
                {
                    maskList.Add(false);
                }
                else
                {
                    maskList.Add(true);
                }
            }
        }

        var flver = target.Flver;

        Dictionary<int, FLVER2.Material> materialDict = new();

        for (int i = 0; i < flver.Materials.Count; i++)
        {
            var material = flver.Materials[i];

            materialDict.Add(i, material);
        }

        foreach (var entry in target.Meshes)
        {
            entry.EditorVisible = false;

            FLVER2.Mesh mesh = (FLVER2.Mesh)entry.WrappedObject;

            if (materialDict.ContainsKey(mesh.MaterialIndex))
            {
                var material = materialDict[mesh.MaterialIndex];

                var regex = @"\#[0-9]*\#";
                var maskIdStr = Regex.Match(material.Name, regex).Value;
                maskIdStr = maskIdStr.Replace("#", ""); // Remove the #s

                // If is a mask entry, default to false.
                if (maskIdStr != "")
                {
                    try
                    {
                        int maskId = int.Parse(maskIdStr);
                        entry.EditorVisible = maskList[maskId];
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, 
                            LOC.Get("MODEL_MaskToggler_Invalid_Mask_ID", maskIdStr), e);
                    }
                }
                else
                {
                    entry.EditorVisible = true;
                }
            }
        }
    }

    private string GetInternalName()
    {
        return "modelDispMask";
    }
}
