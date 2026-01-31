using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamDeltaProgressModal
{
    private ParamDeltaPatcher Patcher;

    public string ModalName = "Delta Export";

    public DeltaBuildProgress LoadProgress;
    public Action<DeltaBuildProgress> ReportProgress;
    public readonly object _progressLock = new();

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public ParamDeltaProgressModal(string name, ParamDeltaPatcher patcher)
    {
        ModalName = name;
        Patcher = patcher;

        ReportProgress = SetProgress;
    }

    public void SetProgress(DeltaBuildProgress progress)
    {
        lock (_progressLock)
        {
            LoadProgress = progress;
        }
    }

    public void Draw()
    {
        if (!DisplayModal)
            return;

        var popupName = $"{ModalName}##{ModalName.GetHashCode()}";

        ImGui.OpenPopup(popupName);

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(popupName,
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            DeltaBuildProgress progress;
            lock (_progressLock)
                progress = LoadProgress;

            if (!string.IsNullOrEmpty(progress.PhaseLabel))
            {
                ImGui.Text(progress.PhaseLabel);
                ImGui.Spacing();
            }

            ImGui.ProgressBar(
                Math.Clamp(progress.Percent, 0f, 1f),
                new System.Numerics.Vector2(400, 0),
                $"{(int)(progress.Percent * 100)}%"
            );

            if (!string.IsNullOrEmpty(progress.StepLabel))
            {
                ImGui.Spacing();
                ImGui.TextDisabled(progress.StepLabel);
            }

            ImGui.EndPopup();
        }
    }
}
