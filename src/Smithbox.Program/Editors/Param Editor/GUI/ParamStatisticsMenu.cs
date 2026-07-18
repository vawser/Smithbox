using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ParamStatisticsMenu
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public IEnumerable<(object, int)> _distributionOutput;
    public bool _isStatisticPopupOpen;

    public string _statisticPopupOutput = "";
    public string _statisticPopupParameter = "";

    public ParamStatisticsMenu(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OpenStatisticPopup(string popup)
    {
        ImGui.OpenPopup(popup);
        _isStatisticPopupOpen = true;
    }

    public void Display()
    {
        if (ImGui.BeginPopup("distributionPopup"))
        {
            ImGui.Text(LOC.Get("PARAM_StatisticsMenu_Occurences", _statisticPopupParameter));

            try
            {
                // Sort Value
                if (ImGui.Button($"{LOC.Get("PARAM_StatisticsMenu_Action_Sort_Value")}##sortValueAction"))
                {
                    _distributionOutput = _distributionOutput.OrderBy(g => g.Item1);
                    _statisticPopupOutput = string.Join('\n',
                        _distributionOutput.Select(e =>
                            e.Item1.ToString().PadLeft(9) + ": " + e.Item2.ToParamEditorString() + LOC.Get("PARAM_StatisticsMenu_Times")));
                }

                ImGui.SameLine();

                // Sort Count
                if (ImGui.Button($"{LOC.Get("PARAM_StatisticsMenu_Action_Sort_Count")}##sortCountAction"))
                {
                    _distributionOutput = _distributionOutput.OrderByDescending(g => g.Item2);
                    _statisticPopupOutput = string.Join('\n',
                        _distributionOutput.Select(e =>
                            e.Item1.ToString().PadLeft(9) + ": " + e.Item2.ToParamEditorString() + LOC.Get("PARAM_StatisticsMenu_Times")));
                }
            }
            catch (Exception e)
            {
                // Happily ignore exceptions. This is non-mutating code with no critical use.
                Smithbox.LogError(this, LOC.Get("PARAM_StatisticsMenu_Failed"), e);
            }

            ImGui.Separator();
            ImGui.Text(LOC.Get("PARAM_StatisticsMenu_Value_Count").PadLeft(9) + LOC.Get("PARAM_StatisticsMenu_Count"));
            ImGui.Separator();
            ImGui.Text(_statisticPopupOutput);
            ImGui.EndPopup();
        }
        else
        {
            _isStatisticPopupOpen = false;
            _statisticPopupOutput = "";
            _statisticPopupParameter = "";
            _distributionOutput = null;
        }
    }
}
