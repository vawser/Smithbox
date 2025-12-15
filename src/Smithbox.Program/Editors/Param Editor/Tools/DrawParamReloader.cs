using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Memory;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.ParamEditor;

public class DrawParamReloader
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    private AOBReader? AOBReader;
    private Dictionary<string, IntPtr> Addresses = new Dictionary<string, IntPtr>();

    public DrawParamReloader(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    private readonly List<ProjectType> SupportedProjectTypes = new()
    {
        ProjectType.DS1,
        ProjectType.DS1R
    };

    private readonly List<string> ApplicableParams = new()
    {
        "DofBank",
        "EnvLightTexBank",
        "FogBank",
        "LensFlareBank",
        "LensFlareExBank",
        "LightBank",
        "LightScatteringBank",
        "PointLightBank",
        "ShadowBank",
        "ToneCorrectBank",
        "ToneMapBank",
        "LightBank"
    };

    public bool SupportsDrawParamReload(ProjectType gameType)
    {
        // Disable for now
        return false;

        return SupportedProjectTypes.Contains(gameType);
    }

    public void DisplayDrawParamReloader()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Param Reloader
        if (SupportsDrawParamReload(Editor.Project.ProjectType))
        {
            if (ImGui.CollapsingHeader("Draw Param Reloader"))
            {
                if (ImGui.Button("Reload Current Draw Param", DPI.WholeWidthButton(windowWidth, 24)))
                {
                    ReloadDrawParam(Editor);
                }
                UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ReloadDrawParam.HintText}");
            }
        }
    }
    public void DisplayDrawParamReloaderMenu()
    {
        if (SupportsDrawParamReload(Project.ProjectType))
        {
            if (ImGui.BeginMenu("Draw Param Reloader"))
            {
                if (ImGui.MenuItem("Current Param"))
                {
                    ReloadDrawParam(Editor);
                }
                UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ReloadDrawParam.HintText}");

                ImGui.EndMenu();
            }
        }
    }

    public void ReloadDrawParam(ParamEditorScreen editor)
    {
        if (SupportsDrawParamReload(Editor.Project.ProjectType))
        {
            if (editor._activeView.Selection.GetActiveParam() != null)
            {
                ReloadCurrentParam(editor.Project.ParamData.PrimaryBank, editor._activeView.Selection.GetActiveParam());
            }
            else
            {
                TaskLogs.AddLog("No param has been selected yet for the Draw Param Reloder.");
            }
        }
        else
        {
            TaskLogs.AddLog("Draw Param Reloader cannot reload for this project.");
        }
    }

    public void ReloadCurrentParam(ParamBank bank, string paramKey)
    {
        var targetExe = "";
        if (Project.ProjectType is ProjectType.DS1)
        {
            targetExe = "DARKSOULS";
        }
        if (Project.ProjectType is ProjectType.DS1R)
        {
            targetExe = "DarkSoulsRemastered";
        }

        bool isApplicableParam = false;
        foreach(var paramSegment in ApplicableParams)
        {
            if(paramKey.Contains(paramSegment))
            {
                isApplicableParam = true;
                break;
            }
        }

        if (!isApplicableParam)
        {
            TaskLogs.AddLog("Currently selected param is not a draw param.");
            return;
        }

        // Get the draw param address in memory
        if (AOBReader == null)
        {
            AOBReader = new AOBReader(targetExe);
        }

        if (!AOBReader.IsProcessValid)
            return;

        IntPtr address = IntPtr.Zero;
        Andre.Formats.Param curParam = null;

        if (string.IsNullOrEmpty(paramKey))
            return;

        // Get the location of the param in memory, use the vanilla param as that is what is loaded since DS1/DS1R mods are placed within the game directory
        if (Project.ParamData.VanillaBank.Params.ContainsKey(paramKey))
        {
            curParam = Project.ParamData.VanillaBank.Params[paramKey];

            if (!Addresses.ContainsKey(paramKey))
            {
                var data = curParam.Write(DCX.Type.None);
                address = AOBReader.AOBScan(data);

                if (address != IntPtr.Zero)
                {
                    Addresses.Add(paramKey, address);
                }
            }
        }

        // Get data from actual mod param
        if (Project.ParamData.PrimaryBank.Params.ContainsKey(paramKey))
        {
            curParam = Project.ParamData.PrimaryBank.Params[paramKey];
        }

        if (curParam == null)
            return;

        if (!Addresses.ContainsKey(paramKey))
            return;

        address = Addresses[paramKey];

        foreach (Param.Row row in curParam.Rows)
        {
            long dataOffset = row.GetDataOffset();
            int typeOffset = 0;

            foreach (Param.Cell cell in row.Cells)
            {
                switch (cell.Def.DisplayType)
                {
                    case PARAMDEF.DefType.s8:
                        HandleSByteField(address, dataOffset, typeOffset, cell, row.ID);
                        ++typeOffset;
                        continue;
                    case PARAMDEF.DefType.u8:
                        HandleByteField(address, dataOffset, typeOffset, cell, row.ID);
                        ++typeOffset;
                        continue;
                    case PARAMDEF.DefType.s16:
                        HandleShortField(address, dataOffset, typeOffset, cell, row.ID);
                        typeOffset += 2;
                        continue;
                    case PARAMDEF.DefType.u16:
                        HandleUShortField(address, dataOffset, typeOffset, cell, row.ID);
                        typeOffset += 2;
                        continue;
                    case PARAMDEF.DefType.s32:
                    case PARAMDEF.DefType.b32:
                        HandleIntField(address, dataOffset, typeOffset, cell, row.ID);
                        typeOffset += 4;
                        continue;
                    case PARAMDEF.DefType.u32:
                        HandleUIntField(address, dataOffset, typeOffset, cell, row.ID);
                        typeOffset += 4;
                        continue;
                    case PARAMDEF.DefType.f32:
                    case PARAMDEF.DefType.angle32:
                        HandleFloatField(address, dataOffset, typeOffset, cell, row.ID);
                        typeOffset += 4;
                        continue;
                    case PARAMDEF.DefType.f64:
                        HandleDoubleField(address, dataOffset, typeOffset, cell, row.ID);
                        typeOffset += 8;
                        continue;
                    case PARAMDEF.DefType.dummy8:
                        ImGui.Text($"Pad:{cell.Def.ArrayLength}");
                        typeOffset += cell.Def.ArrayLength;
                        continue;
                    default:
                        continue;
                }
            }
        }

        TaskLogs.AddLog($"Updated draw param: {paramKey}.");
    }

    private void HandleSByteField(
      IntPtr address,
      long offset,
      int typeOffset,
      Param.Cell cell,
      int paramId)
    {
        int int16 = (int)Convert.ToInt16(cell.Value); 
        AOBReader.WriteMemory(address, offset + (long)typeOffset, (object)(sbyte)int16, cell.Def.DisplayType);
    }

    private void HandleByteField(
      IntPtr address,
      long offset,
      int typeOffset,
      Param.Cell cell,
      int paramId)
    {
        int v = (int)Convert.ToByte(cell.Value);
        AOBReader.WriteMemory(address, offset + (long)typeOffset, (object)(byte)v, cell.Def.DisplayType);
    }

    private void HandleShortField(
      IntPtr address,
      long offset,
      int typeOffset,
      Param.Cell cell,
      int paramId)
    {
        int int16 = (int)Convert.ToInt16(cell.Value);
        AOBReader.WriteMemory(address, offset + (long)typeOffset, (object)(short)int16, cell.Def.DisplayType);
    }

    private void HandleUShortField(
      IntPtr address,
      long offset,
      int typeOffset,
      Param.Cell cell,
      int paramId)
    {
        int uint16 = (int)Convert.ToUInt16(cell.Value);
        AOBReader.WriteMemory(address, offset + (long)typeOffset, (object)(ushort)uint16, cell.Def.DisplayType);
    }

    private void HandleIntField(
      IntPtr address,
      long offset,
      int typeOffset,
      Param.Cell cell,
      int paramId)
    {
        int int32 = Convert.ToInt32(cell.Value);
        AOBReader.WriteMemory(address, offset + (long)typeOffset, (object)int32, cell.Def.DisplayType);
    }

    private void HandleUIntField(
      IntPtr address,
      long offset,
      int typeOffset,
      Param.Cell cell,
      int paramId)
    {
        float single = Convert.ToSingle(cell.Value);
        AOBReader.WriteMemory(address, offset + (long)typeOffset, (object)Convert.ToUInt32(single), cell.Def.DisplayType);
    }

    private void HandleFloatField(
      IntPtr address,
      long offset,
      int typeOffset,
      Param.Cell cell,
      int paramId)
    {
        float v = (float)cell.Value;
        AOBReader.WriteMemory(address, offset + (long)typeOffset, (object)v, cell.Def.DisplayType);
    }

    private void HandleDoubleField(
      IntPtr address,
      long offset,
      int typeOffset,
      Param.Cell cell,
      int paramId)
    {
        float single = Convert.ToSingle(cell.Value);
        AOBReader.WriteMemory(address, offset + (long)typeOffset, (object)(double)single, cell.Def.DisplayType);
    }
}
