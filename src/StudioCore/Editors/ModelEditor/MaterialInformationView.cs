using ImGuiNET;
using Org.BouncyCastle.Utilities;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    /// <summary>
    /// Used to show MATBIN information (does not allow editing).
    /// </summary>
    public class MaterialInformationView
    {
        private ModelEditorScreen Screen;
        private MATBIN CurrentMatbin;

        public MaterialInformationView(ModelEditorScreen editor)
        {
            Screen = editor;
        }

        public void DisplayMatbinInformation(string mtdPath)
        {
            var matname = Path.GetFileNameWithoutExtension(mtdPath);

            if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                if (Smithbox.BankHandler.MaterialBank.Matbins.ContainsKey(matname))
                {
                    CurrentMatbin = Smithbox.BankHandler.MaterialBank.Matbins[matname].Matbin;
                }

                if(CurrentMatbin != null)
                {
                    if (ImGui.CollapsingHeader("MATBIN", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        DisplayMatbin();
                    }
                    ImguiUtils.ShowHoverTooltip("Read-only. Displays MATBIN information for the MATBIN this material references.");
                }
            }
        }

        private void DisplayMatbin()
        {
            string shaderPath = CurrentMatbin.ShaderPath;
            string sourcePath = CurrentMatbin.SourcePath;
            string key = CurrentMatbin.Key.ToString();

            // Display
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Shader Path");
            ImguiUtils.ShowHoverTooltip("Network path to the shader source file.");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Source Path");
            ImguiUtils.ShowHoverTooltip("Network path to the material source file, either a matxml or an mtd.");

            //ImGui.AlignTextToFramePadding();
            //ImGui.Text("Key");
            //ImguiUtils.ShowHoverTooltip("Unknown, presumed to be an identifier for documentation.");

            ImGui.NextColumn();

            var colWidth = ImGui.GetColumnWidth();

            ImGui.AlignTextToFramePadding();
            ImGui.SetNextItemWidth(colWidth);
            ImGui.InputText("##ShaderPath", ref shaderPath, 255, ImGuiInputTextFlags.ReadOnly);

            ImGui.AlignTextToFramePadding();
            ImGui.SetNextItemWidth(colWidth);
            ImGui.InputText("##SourcePath", ref sourcePath, 255, ImGuiInputTextFlags.ReadOnly);

            ImGui.AlignTextToFramePadding();
            //ImGui.SetNextItemWidth(colWidth);
            //ImGui.InputText("##Key", ref key, 255, ImGuiInputTextFlags.ReadOnly);

            ImGui.Columns(1);

            // Samplers
            if (ImGui.CollapsingHeader("MATBIN: Samplers", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < CurrentMatbin.Samplers.Count; i++)
                {
                    DisplayMatbinSampler(CurrentMatbin.Samplers[i], i);
                }
            }

            // Params
            if (ImGui.CollapsingHeader("MATBIN: Parameters", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < CurrentMatbin.Params.Count; i++)
                {
                    DisplayMatbinParam(CurrentMatbin.Params[i], i);
                }
            }
        }

        private void DisplayMatbinParam(MATBIN.Param param, int index)
        {
            string name = param.Name;
            string key = CurrentMatbin.Key.ToString();

            object Value = param.Value;

            ImGui.Separator();
            ImGui.Text("Param");
            ImGui.Separator();

            // Display
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Name");
            ImguiUtils.ShowHoverTooltip("The name of the parameter.");

            //ImGui.AlignTextToFramePadding();
            //ImGui.Text("Key");
            //ImguiUtils.ShowHoverTooltip("Unknown, presumed to be an identifier for documentation.");

            if (param.Type == MATBIN.ParamType.Bool)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Boolean");
                ImguiUtils.ShowHoverTooltip("The value to be used.");
            }
            if (param.Type == MATBIN.ParamType.Int)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Integer");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

            }
            if (param.Type == MATBIN.ParamType.Int2)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Integer [1]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Integer [2]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");
            }
            if (param.Type == MATBIN.ParamType.Float)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float");
                ImguiUtils.ShowHoverTooltip("The value to be used.");
            }
            if (param.Type == MATBIN.ParamType.Float2)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [1]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [2]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");
            }
            if (param.Type == MATBIN.ParamType.Float3)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [1]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [2]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [3]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");
            }
            if (param.Type == MATBIN.ParamType.Float4)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [1]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [2]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [3]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [4]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");
            }
            if (param.Type == MATBIN.ParamType.Float5)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [1]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [2]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [3]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [4]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Float [5]");
                ImguiUtils.ShowHoverTooltip("The value to be used.");
            }

            ImGui.NextColumn();

            var colWidth = ImGui.GetColumnWidth();

            ImGui.AlignTextToFramePadding();
            ImGui.SetNextItemWidth(colWidth);
            ImGui.InputText("##Name", ref name, 255, ImGuiInputTextFlags.ReadOnly);

            //ImGui.AlignTextToFramePadding();
            //ImGui.SetNextItemWidth(colWidth);
            //ImGui.InputText("##Key", ref key, 255, ImGuiInputTextFlags.ReadOnly);

            if (param.Type == MATBIN.ParamType.Bool)
            {
                bool boolValue = (bool)param.Value;

                ImGui.AlignTextToFramePadding();
                ImGui.Checkbox($"##Value_Bool{index}", ref boolValue);
            }
            if (param.Type == MATBIN.ParamType.Int)
            {
                int intValue = (int)param.Value;
                string strValue = intValue.ToString();

                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Int{index}", ref strValue, 255, ImGuiInputTextFlags.ReadOnly);
            }
            if (param.Type == MATBIN.ParamType.Int2)
            {
                int[] intValueArr = (int[])param.Value;
                int intValue1 = intValueArr[0];
                int intValue2 = intValueArr[1];

                string strValue1 = intValue1.ToString();
                string strValue2 = intValue2.ToString();

                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Int2_1{index}", ref strValue1, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Int2_2{index}", ref strValue2, 255, ImGuiInputTextFlags.ReadOnly);
            }
            if (param.Type == MATBIN.ParamType.Float)
            {
                float intValue = (float)param.Value;
                string strValue = intValue.ToString();

                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float{index}", ref strValue, 255, ImGuiInputTextFlags.ReadOnly);
            }
            if (param.Type == MATBIN.ParamType.Float2)
            {
                float[] floatValueArr = (float[])param.Value;
                float floatValue1 = floatValueArr[0];
                float floatValue2 = floatValueArr[1];

                string strValue1 = floatValue1.ToString();
                string strValue2 = floatValue2.ToString();

                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float2_1{index}", ref strValue1, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float2_2{index}", ref strValue2, 255, ImGuiInputTextFlags.ReadOnly);
            }
            if (param.Type == MATBIN.ParamType.Float3)
            {
                float[] floatValueArr = (float[])param.Value;
                float floatValue1 = floatValueArr[0];
                float floatValue2 = floatValueArr[1];
                float floatValue3 = floatValueArr[2];

                string strValue1 = floatValue1.ToString();
                string strValue2 = floatValue2.ToString();
                string strValue3 = floatValue3.ToString();

                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float3_1{index}", ref strValue1, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float3_2{index}", ref strValue2, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float3_3{index}", ref strValue3, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
            }
            if (param.Type == MATBIN.ParamType.Float4)
            {
                float[] floatValueArr = (float[])param.Value;
                float floatValue1 = floatValueArr[0];
                float floatValue2 = floatValueArr[1];
                float floatValue3 = floatValueArr[2];
                float floatValue4 = floatValueArr[3];

                string strValue1 = floatValue1.ToString();
                string strValue2 = floatValue2.ToString();
                string strValue3 = floatValue3.ToString();
                string strValue4 = floatValue4.ToString();

                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float4_1{index}", ref strValue1, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float4_2{index}", ref strValue2, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float4_3{index}", ref strValue3, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float4_4{index}", ref strValue4, 255, ImGuiInputTextFlags.ReadOnly);
            }
            if (param.Type == MATBIN.ParamType.Float5)
            {
                float[] floatValueArr = (float[])param.Value;
                float floatValue1 = floatValueArr[0];
                float floatValue2 = floatValueArr[1];
                float floatValue3 = floatValueArr[2];
                float floatValue4 = floatValueArr[3];
                float floatValue5 = floatValueArr[4];

                string strValue1 = floatValue1.ToString();
                string strValue2 = floatValue2.ToString();
                string strValue3 = floatValue3.ToString();
                string strValue4 = floatValue4.ToString();
                string strValue5 = floatValue5.ToString();

                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float5_1{index}", ref strValue1, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float5_2{index}", ref strValue2, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float5_3{index}", ref strValue3, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float5_4{index}", ref strValue4, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##Value_Float5_5{index}", ref strValue5, 255, ImGuiInputTextFlags.ReadOnly);
            }

            ImGui.Columns(1);
        }

        private void DisplayMatbinSampler(MATBIN.Sampler sampler, int index)
        {
            string path = sampler.Path;
            string type = sampler.Type;
            string key = CurrentMatbin.Key.ToString();
            Vector2 scale = sampler.Unk14;

            ImGui.Separator();
            ImGui.Text("Sampler");
            ImGui.Separator();

            // Display
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Path");
            ImguiUtils.ShowHoverTooltip("An optional network path to the texture, if not specified in the FLVER.");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Type");
            ImguiUtils.ShowHoverTooltip(" The type of the sampler.");

            //ImGui.AlignTextToFramePadding();
            //ImGui.Text("Key");
            //ImguiUtils.ShowHoverTooltip("Unknown, presumed to be an identifier for documentation.");

            //ImGui.AlignTextToFramePadding();
            //ImGui.Text("Scale");
            //ImguiUtils.ShowHoverTooltip("Unknown; most likely to be the scale, but typically 0, 0.");

            ImGui.NextColumn();

            var colWidth = ImGui.GetColumnWidth();

            ImGui.AlignTextToFramePadding();
            ImGui.SetNextItemWidth(colWidth);
            ImGui.InputText($"##Name_{index}", ref path, 255, ImGuiInputTextFlags.ReadOnly);

            ImGui.AlignTextToFramePadding();
            ImGui.SetNextItemWidth(colWidth);
            ImGui.InputText($"##Type_{index}", ref type, 255, ImGuiInputTextFlags.ReadOnly);

            //ImGui.AlignTextToFramePadding();
            //ImGui.SetNextItemWidth(colWidth);
            //ImGui.InputText($"##Key_{index}", ref key, 255, ImGuiInputTextFlags.ReadOnly);

            //ImGui.AlignTextToFramePadding();
            //ImGui.SetNextItemWidth(colWidth);
            //ImGui.InputFloat2($"##Scale_{index}", ref scale, "", ImGuiInputTextFlags.ReadOnly);

            ImGui.Columns(1);
        }
    }
}
