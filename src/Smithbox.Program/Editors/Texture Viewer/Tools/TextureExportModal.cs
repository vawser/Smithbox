using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TextureViewer;

public class TextureExportModal
{
    private TextureExport Export;

    public string ModalName = "Texture Export";

    public DeltaBuildProgress LoadProgress;
    public Action<DeltaBuildProgress> ReportProgress;
    public readonly object _progressLock = new();

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public TextureExportModal(string name, TextureExport export)
    {
        ModalName = name;
        Export = export;

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
