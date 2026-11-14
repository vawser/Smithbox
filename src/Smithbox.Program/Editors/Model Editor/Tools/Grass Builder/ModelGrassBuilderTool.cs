using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Model Editor's GRASS Builder: gives a map piece or asset a GRASS file built from chosen FLVER meshes, or rebuilds
/// the one it has. Faces can be limited by how steep they are, and every vertex starts with the chosen density in each
/// of the six slots (the part's six GrassTypeParam IDs). The file follows the vanilla rules, the same in SDT, ER, AC6
/// and NR (see <see cref="GrassBuilder"/>, Claude/Documentation/GRASS Builder.md and GRASS Later Games.md).
/// <para>
/// A build is one <see cref="ModelGrassEditAction"/> on the view's history, and the model's save writes the file
/// (adding it to the binder when it is new). Only the models whose parts name grass types are offered
/// (<see cref="ModelWrapper.CanBuildGrass"/>); open-world tile pieces get a warning, as no vanilla one holds a file.
/// </para>
/// </summary>
public class ModelGrassBuilderTool
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    // The model shown, and the meshes ticked for it (all of them when it is first shown)
    private ModelWrapper SelectedWrapper;
    private readonly HashSet<FLVER2.Mesh> SelectedMeshes = new(ReferenceEqualityComparer.Instance);

    // The LOD 0 triangles of each mesh, read once per model and again after an edit of the model
    private readonly Dictionary<FLVER2.Mesh, List<(Vector3 A, Vector3 B, Vector3 C)>> Triangles = new(ReferenceEqualityComparer.Instance);
    private bool TrianglesStale = true;

    // How many triangles the current settings keep, kept until they change
    private (int Key, int Kept, int Total) KeptCache = (0, 0, 0);

    private float MaxAngle = 180;
    private readonly float[] Densities = { 1, 0, 0, 0, 0, 0 };

    // Building runs off the draw loop; the result is turned into an action when the tool is next drawn
    private record BuildResult(ModelWrapper Wrapper, GRASS Grass, string Description);
    private readonly ConcurrentQueue<BuildResult> Completed = new();
    private CancellationTokenSource BuildCancel;
    private volatile bool Building;

    // Set while this tool's own action runs, so it doesn't count as a mesh edit
    private bool ExecutingGrassAction;

    public ModelGrassBuilderTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        // Mesh edits change the triangles a build reads; this tool's own builds don't
        view.ViewportActionManager.ActionPerformed += () =>
        {
            if (!ExecutingGrassAction)
                TrianglesStale = true;
        };
    }

    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.SDT or ProjectType.ER
        or ProjectType.AC6 or ProjectType.NR;

    /// <summary>The loaded model of this view, if any.</summary>
    private ModelWrapper GetWrapper()
    {
        var wrapper = View.Selection.SelectedModelWrapper;

        return wrapper?.Container != null && wrapper.Container.View == View ? wrapper : null;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MODEL_GrassBuilder_Header")}##modelGrassBuilderHeader"))
            return;

        ImGui.BeginChild("modelGrassBuilderSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        // A finished build, picked up whichever model is shown now
        while (Completed.TryDequeue(out var result))
        {
            Execute(result);
        }

        if (!CanUse())
        {
            GUI.WrappedText(LOC.Get("MODEL_GrassBuilder_Unsupported_Game"));
            return;
        }

        GUI.WrappedText(LOC.Get("MODEL_GrassBuilder_Hint"));

        var wrapper = GetWrapper();

        if (wrapper != SelectedWrapper)
        {
            SelectedWrapper = wrapper;
            SelectedMeshes.Clear();
            Triangles.Clear();
            TrianglesStale = true;

            if (wrapper != null)
            {
                foreach (var mesh in GetMeshes(wrapper))
                {
                    SelectedMeshes.Add(mesh);
                }
            }
        }

        // File
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_GrassBuilder_File_Header"), LOC.Get("MODEL_GrassBuilder_File_Header_TT"));

        if (wrapper == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_GrassBuilder_No_Model"));
            return;
        }

        if (!wrapper.CanBuildGrass)
        {
            GUI.WrappedText(LOC.Get(Project.Descriptor.ProjectType switch
            {
                ProjectType.SDT => "MODEL_GrassBuilder_Not_Holder_SDT",
                ProjectType.AC6 => "MODEL_GrassBuilder_Not_Holder_AC6",
                _ => "MODEL_GrassBuilder_Not_Holder"
            }, wrapper.Name));
            return;
        }

        var grass = wrapper.GRASS;

        if (wrapper.IsOpenWorldTilePiece)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_GrassBuilder_Open_World_Tile"));
        }

        if (grass == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_GrassBuilder_No_File", wrapper.Name));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MODEL_GrassBuilder_File_Info", wrapper.GrassFileName,
                grass.Vertices.Count, grass.Faces.Count, grass.BoundingVolumeHierarchy.Count));

            if (wrapper.IsGrassFileNew)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MODEL_GrassBuilder_New_File"));
            }
        }

        var meshes = GetMeshes(wrapper);

        if (TrianglesStale)
        {
            ReadTriangles(meshes);
        }

        DisplayMeshes(wrapper, meshes);
        DisplayFaces(meshes);
        DisplayDensities();
        DisplayActions(wrapper, grass, meshes);
    }

    /// <summary>The meshes of the loaded model, in the order of its objects.</summary>
    private static List<FLVER2.Mesh> GetMeshes(ModelWrapper wrapper)
    {
        return wrapper.Container.Meshes
            .Select(e => e.WrappedObject)
            .OfType<FLVER2.Mesh>()
            .ToList();
    }

    private void ReadTriangles(List<FLVER2.Mesh> meshes)
    {
        Triangles.Clear();

        foreach (var mesh in meshes)
        {
            Triangles[mesh] = GrassBuilder.GetMeshTriangles(mesh).ToList();
        }

        // Meshes deleted since they were ticked
        SelectedMeshes.RemoveWhere(m => !Triangles.ContainsKey(m));

        TrianglesStale = false;
        KeptCache = (0, 0, 0);
    }

    // Whether the mesh list is shown; open by default, as it is the main choice
    private bool ShowMeshes = true;

    private void DisplayMeshes(ModelWrapper wrapper, List<FLVER2.Mesh> meshes)
    {
        // Meshes
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MODEL_GrassBuilder_Meshes_Header"), LOC.Get("MODEL_GrassBuilder_Meshes_Header_TT"), ref ShowMeshes);

        GUI.WrappedText(LOC.Get("MODEL_GrassBuilder_Meshes_Selected", SelectedMeshes.Count, meshes.Count));

        GUI.MultiButtonInput("modelGrassBuilderMeshButtons",
            "modelGrassBuilderSelectAll", LOC.Get("MODEL_GrassBuilder_Select_All"), LOC.Get("MODEL_GrassBuilder_Select_All_TT"),
            () =>
            {
                SelectedMeshes.Clear();
                foreach (var mesh in meshes) SelectedMeshes.Add(mesh);
            },
            "modelGrassBuilderSelectNone", LOC.Get("MODEL_GrassBuilder_Select_None"), LOC.Get("MODEL_GrassBuilder_Select_None_TT"),
            () => SelectedMeshes.Clear());

        if (!ShowMeshes)
            return;

        var materials = wrapper.Container.Materials;

        ImGui.BeginChild("modelGrassBuilderMeshList", new Vector2(0, 200 * DPI.UIScale()), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);

        for (var i = 0; i < meshes.Count; i++)
        {
            var mesh = meshes[i];
            var isSelected = SelectedMeshes.Contains(mesh);

            var material = mesh.MaterialIndex >= 0 && mesh.MaterialIndex < materials.Count
                ? (materials[mesh.MaterialIndex].WrappedObject as FLVER2.Material)?.Name ?? ""
                : "";

            var count = Triangles.TryGetValue(mesh, out var tris) ? tris.Count : 0;

            if (ImGui.Checkbox($"{LOC.Get("MODEL_GrassBuilder_Mesh_Entry", i, material, count)}##modelGrassBuilderMesh{i}", ref isSelected))
            {
                if (isSelected)
                    SelectedMeshes.Add(mesh);
                else
                    SelectedMeshes.Remove(mesh);
            }
            GUI.Tooltip(LOC.Get("MODEL_GrassBuilder_Mesh_Entry_TT"));
        }

        ImGui.EndChild();
    }

    private void DisplayFaces(List<FLVER2.Mesh> meshes)
    {
        // Faces
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_GrassBuilder_Faces_Header"), LOC.Get("MODEL_GrassBuilder_Faces_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##modelGrassBuilderMaxAngle", ref MaxAngle, 0, 180, LOC.Get("MODEL_GrassBuilder_Max_Angle_Format"));
        GUI.Tooltip(LOC.Get("MODEL_GrassBuilder_Max_Angle_TT"));

        var (kept, total) = GetKept(meshes);

        GUI.WrappedText(LOC.Get("MODEL_GrassBuilder_Faces_Kept", kept, total));
    }

    /// <summary>The triangles of the ticked meshes the angle limit keeps, and how many there are in all.</summary>
    private (int Kept, int Total) GetKept(List<FLVER2.Mesh> meshes)
    {
        var key = HashCode.Combine(MaxAngle, SelectedMeshes.Count, Triangles.Count);

        foreach (var mesh in meshes)
        {
            if (SelectedMeshes.Contains(mesh))
                key = HashCode.Combine(key, System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(mesh));
        }

        // Zero is the cleared cache
        if (key == 0) key = 1;

        if (KeptCache.Key == key)
            return (KeptCache.Kept, KeptCache.Total);

        var selected = SelectedTriangles(meshes).ToList();
        var kept = GrassBuilder.CountKept(selected, MaxAngle);

        KeptCache = (key, kept, selected.Count);

        return (kept, selected.Count);
    }

    private IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> SelectedTriangles(List<FLVER2.Mesh> meshes)
    {
        foreach (var mesh in meshes)
        {
            if (!SelectedMeshes.Contains(mesh) || !Triangles.TryGetValue(mesh, out var tris))
                continue;

            foreach (var t in tris)
                yield return t;
        }
    }

    private void DisplayDensities()
    {
        // Densities
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_GrassBuilder_Densities_Header"), LOC.Get("MODEL_GrassBuilder_Densities_Header_TT"));

        for (var s = 0; s < GrassBuilder.SlotCount; s++)
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderFloat($"##modelGrassBuilderDensity{s}", ref Densities[s], 0, 1, LOC.Get("MODEL_GrassBuilder_Density_Format", s));
            GUI.Tooltip(LOC.Get("MODEL_GrassBuilder_Density_TT", s));

            Densities[s] = Math.Max(0, Densities[s]);
        }

        if (Densities.All(d => d <= 0))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_GrassBuilder_No_Density"));
        }
    }

    private void DisplayActions(ModelWrapper wrapper, GRASS grass, List<FLVER2.Mesh> meshes)
    {
        var (kept, _) = GetKept(meshes);
        var canBuild = !Building && kept > 0 && Densities.Any(d => d > 0);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_GrassBuilder_Actions_Header"), LOC.Get("MODEL_GrassBuilder_Actions_Header_TT"));

        if (grass != null && !wrapper.IsGrassFileNew)
        {
            GUI.WrappedText(LOC.Get("MODEL_GrassBuilder_Rebuild_Note"));
        }

        var isRebuild = grass != null;

        GUI.ConditionalMultiButtonInput("modelGrassBuilderActions",
            "modelGrassBuilderBuild",
            LOC.Get(isRebuild ? "MODEL_GrassBuilder_Rebuild" : "MODEL_GrassBuilder_Build"),
            LOC.Get(isRebuild ? "MODEL_GrassBuilder_Rebuild_TT" : "MODEL_GrassBuilder_Build_TT", wrapper.GrassFileName),
            () => StartBuild(wrapper, meshes, isRebuild), canBuild);

        if (Building)
        {
            // Progress
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MODEL_GrassBuilder_Progress_Header"), LOC.Get("MODEL_GrassBuilder_Progress_Header_TT"));

            ImGui.ProgressBar(-1.0f * (float)ImGui.GetTime(), new Vector2(-1, 0), LOC.Get("MODEL_GrassBuilder_Building"));

            GUI.MultiButtonInput("modelGrassBuilderProgress",
                "modelGrassBuilderCancel", LOC.Get("MODEL_GrassBuilder_Cancel"), LOC.Get("MODEL_GrassBuilder_Cancel_TT"),
                () => BuildCancel?.Cancel());
        }
    }

    private void StartBuild(ModelWrapper wrapper, List<FLVER2.Mesh> meshes, bool isRebuild)
    {
        BuildCancel?.Cancel();
        BuildCancel = new CancellationTokenSource();
        var token = BuildCancel.Token;

        // Everything the build reads, taken here: the triangles are copies, so later edits can't change them
        var triangles = SelectedTriangles(meshes).ToList();
        var options = new GrassBuildOptions(MaxAngle, (float[])Densities.Clone());
        var description = LOC.Get(isRebuild ? "MODEL_History_Grass_Rebuild" : "MODEL_History_Grass_Build", wrapper.Name);

        Building = true;

        Task.Run(() =>
        {
            try
            {
                var grass = GrassBuilder.Build(triangles, options);

                if (token.IsCancellationRequested)
                    return;

                var problems = GrassBuilder.Check(grass);

                if (problems.Count > 0)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_GrassBuilder_Check_Failed", wrapper.Name, string.Join("; ", problems)));
                    return;
                }

                Completed.Enqueue(new BuildResult(wrapper, grass, description));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_GrassBuilder_Build_Failed", wrapper.Name), e);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    Building = false;
            }
        });

        // A cancelled build stops showing as running straight away
        token.Register(() => Building = false);
    }

    private void Execute(BuildResult result)
    {
        var wrapper = result.Wrapper;

        // The model was unloaded or moved to another view while it was built
        if (wrapper.Container == null || wrapper.Container.View != View || !wrapper.CanBuildGrass)
            return;

        ExecutingGrassAction = true;

        try
        {
            View.ViewportActionManager.ExecuteAction(new ModelGrassEditAction(wrapper, result.Grass, result.Description));
        }
        finally
        {
            ExecutingGrassAction = false;
        }

        Smithbox.Log(this, LOC.Get("MODEL_GrassBuilder_Built_Log", wrapper.Name, result.Grass.Vertices.Count,
            result.Grass.Faces.Count, result.Grass.BoundingVolumeHierarchy.Count));
    }
}
