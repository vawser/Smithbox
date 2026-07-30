using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Keybinds;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ModelEditor;

public class ModelMaskToggler
{
    public ModelEditorView View;
    public ProjectEntry Project;

    private bool SelectEntry = false;
    private int SelectedID = -1;

    public ModelMaskToggler(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Model Mask Toggler
        if (ImGui.CollapsingHeader($"{LOC.Get("MODEL_MaskToggler_Header")}##modelMaskTogglerHeader"))
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

        if (View.Project.Handler.ParamEditor == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_No_Param_Editor"));

            return;
        }

        if(View.Selection.SelectedModelWrapper == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_No_Model"));

            return;
        }

        var filename = View.Selection.SelectedModelWrapper.Name;
        var npcParamKey = "NpcParam";

        if (!View.Project.Handler.ParamData.PrimaryBank.Params.ContainsKey(npcParamKey))
        {
            GUI.WrappedText(LOC.Get("MODEL_MaskToggler_No_Npc_Param"));

            return;
        }

        var npcParam = View.Project.Handler.ParamData.PrimaryBank.Params[npcParamKey];

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
                    ToggleMeshes(entry);
                    SelectedID = entry.ID;
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && SelectEntry)
                {
                    SelectEntry = false;
                    SelectedID = entry.ID;
                    ToggleMeshes(entry);
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

    public void ToggleMeshes(Param.Row row)
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

        var container = View.Selection.SelectedModelWrapper.Container;
        var flver = View.Selection.SelectedModelWrapper.FLVER;

        Dictionary<int, FLVER2.Material> materialDict = new();

        for (int i = 0; i < flver.Materials.Count; i++)
        {
            var material = flver.Materials[i];

            materialDict.Add(i, material);
        }

        foreach (var entry in container.Meshes)
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
