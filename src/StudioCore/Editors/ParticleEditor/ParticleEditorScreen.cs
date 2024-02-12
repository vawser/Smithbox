using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.ParticleEditor;
using StudioCore.Editors.TalkEditor;
using StudioCore.UserProject;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.ParticleEditor.ParticleBank;

namespace StudioCore.ParticleEditor;

public class ParticleEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private ParticleFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private FXR3 _selectedParticle;
    private int _selectedParticleKey;

    public ParticleEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Particle Editor##ParticleEditor";
    public string CommandEndpoint => "particle";
    public string SaveType => "Particle";

    public void DrawEditorMenu()
    {
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        if (Project.Type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S)
        {
            ImGui.Text($"This editor does not support {Project.Type}.");
            ImGui.PopStyleVar();
            return;
        }
        else if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

        if (!ParticleBank.IsLoaded)
        {
            if (!CFG.Current.AutoLoadBank_Particle)
            {
                if (ImGui.Button("Load Particle Editor"))
                {
                    ParticleBank.LoadParticles();
                }
            }
        }

        var dsid = ImGui.GetID("DockSpace_ParticleEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (ParticleBank.IsLoaded)
        {
            ParticleFileView();
        }

        ImGui.PopStyleVar();
    }

    public void ParticleFileView()
    {
        // File List
        ImGui.Begin("Files##ParticleFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in ParticleBank.FileBank)
        {
            // Ignore the resource ffxbnd files
            if (info.ParticleFiles.Count > 0)
            {
                if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
                {
                    _selectedParticleKey = -1; // Clear particle key if file is changed

                    _selectedBinderKey = info.Name;
                    _selectedFileInfo = info;
                    _selectedBinder = binder;
                }
            }
        }

        ImGui.End();

        // File List
        ImGui.Begin("Particles##ParticleList");

        if (_selectedFileInfo.ParticleFiles != null)
        {
            ImGui.Text($"Particles");
            ImGui.Separator();

            for (int i = 0; i < _selectedFileInfo.ParticleFiles.Count; i++)
            {
                FXR3 entry = _selectedFileInfo.ParticleFiles[i];

                if (ImGui.Selectable($@" {entry.ID}", i == _selectedParticleKey))
                {
                    _selectedParticleKey = i;
                    _selectedParticle = entry;
                }
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        // Only support FXR3 for now
        if(Project.Type is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            if (CFG.Current.AutoLoadBank_Particle)
                ParticleBank.LoadParticles();
        }

        ResetActionManager();
    }

    public void Save()
    {
        if (ParticleBank.IsLoaded)
            ParticleBank.SaveParticle(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (ParticleBank.IsLoaded)
            ParticleBank.SaveParticles();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
