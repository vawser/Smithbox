using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Editors.ModelEditor;

/// <summary>A material oracle set: the meshes whose materials are compared with the game's own shaders</summary>
public class MaterialOracleSet
{
    public string Game { get; set; } = "";
    public int Size { get; set; } = 256;
    public List<MaterialOracleEntry> Entries { get; set; } = new();
}

public class MaterialOracleEntry
{
    /// <summary>The model container, as the Model Editor lists it (aeg217_012)</summary>
    public string Container { get; set; } = "";

    /// <summary>The model in it; the first when empty</summary>
    public string Model { get; set; }

    /// <summary>The MATBIN / MTD name: the meshes that use it are drawn, and it names the output folder</summary>
    public string Material { get; set; } = "";

    /// <summary>The mesh indices to draw; all that use <see cref="Material" /> when null</summary>
    public int[] Meshes { get; set; }
}

/// <summary>
/// The Smithbox side of the material oracle (Claude/Documentation/Material Fidelity/Oracle.md, O2), run unattended by
/// <c>--material-oracle</c>. For each entry it loads the model, shows only the material's meshes, and captures what the
/// flver shader computes for them in UV space (<see cref="MaterialOracleCapture" />): albedo, metallic, normal, gloss,
/// emissive and coverage, as float files and PNGs in &lt;root&gt;/&lt;GAME&gt;/smithbox/&lt;material&gt;/. Claude/Tools/
/// MaterialOracle draws the game's side and compares them.
/// </summary>
public class MaterialOracleTool : IAutomatedCapture
{
    private enum Stage { Idle, Loading, Settling, Capturing }

    private const int IdleFramesToLoad = 30;
    private const int SettleFrames = 20;
    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CaptureTimeout = TimeSpan.FromSeconds(10);

    /// <summary>Where the run writes, and the set it reads (default &lt;root&gt;/sets/&lt;GAME&gt;.json); set by the automation</summary>
    public static string Root { get; set; }
    public static string SetPath { get; set; }

    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    private Stage CurrentStage = Stage.Idle;
    private MaterialOracleSet Set;
    private string RunFolder;
    private int EntryIndex;
    private ModelWrapper Target;
    private int IdleFrames;
    private int SettledFrames;
    private readonly Stopwatch StageTime = new();
    private MaterialOracleImages Images;
    private bool ImagesArrived;
    private readonly List<(string Name, string Outcome)> Results = new();
    private List<int> DrawnMeshes = new();
    private bool SavedTaa;
    private CaptureDisplay SavedDisplay;
    private string Status = "";

    public MaterialOracleTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private string GameName => Project.Descriptor.ProjectType.ToString();

    public bool IsRunning => CurrentStage != Stage.Idle;
    public string LastStatus => Status;
    public (int Ok, int Total) LastRunCounts => (Results.Count(r => r.Outcome == "ok"), Results.Count);
    public IEnumerable<string> LastRunFailures => Results.Where(r => r.Outcome != "ok").Select(r => $"{r.Name}: {r.Outcome}");

    public bool Start(out string reason)
    {
        reason = null;

        if (string.IsNullOrEmpty(Root))
        {
            reason = "no oracle root";
            return false;
        }

        var setPath = SetPath ?? Path.Join(Root, "sets", $"{GameName}.json");

        try
        {
            Set = JsonSerializer.Deserialize<MaterialOracleSet>(File.ReadAllText(setPath));
        }
        catch (Exception e)
        {
            reason = $"can't read the set {setPath}: {e.Message}";
            return false;
        }

        if (Set == null || Set.Entries.Count == 0)
        {
            reason = $"the set {setPath} has no entries";
            return false;
        }

        if (View.ViewportWindow?.Viewport is not VulkanViewport viewport)
        {
            reason = "no viewport";
            return false;
        }

        RunFolder = Path.Join(Root, GameName, "smithbox");
        Directory.CreateDirectory(RunFolder);
        Results.Clear();

        SavedTaa = CFG.Current.Viewport.Viewport_Post_Taa;
        CFG.Current.Viewport.Viewport_Post_Taa = false;

        // Only the material's meshes may draw: markers, labels, collision and the grid would land on the captured pixels
        SavedDisplay = CaptureDisplay.Hide(View.RenderScene, hideGrid: true);
        MaterialClock.FixedTime = 0;
        viewport.CaptureSize = (Set.Size, Set.Size);

        EntryIndex = 0;
        StartEntry();
        return true;
    }

    public void Cancel() => Finish("cancelled");

    /// <summary>Advances a run. Called every frame by the view.</summary>
    public void Update()
    {
        if (CurrentStage == Stage.Idle)
            return;

        if (View.ViewportWindow?.Viewport is not VulkanViewport viewport)
        {
            Finish("the viewport closed");
            return;
        }

        switch (CurrentStage)
        {
            case Stage.Loading: UpdateLoading(viewport); break;
            case Stage.Settling: UpdateSettling(viewport); break;
            case Stage.Capturing: UpdateCapturing(viewport); break;
        }
    }

    private MaterialOracleEntry Entry => Set.Entries[EntryIndex];

    private static string FolderName(MaterialOracleEntry entry) => string.Join("_",
        (entry.Material.Length > 0 ? entry.Material : $"{entry.Container}_{entry.Model}").Split(Path.GetInvalidFileNameChars()));

    private void StartEntry()
    {
        while (EntryIndex < Set.Entries.Count)
        {
            Status = $"Loading {Entry.Container} ({EntryIndex + 1} of {Set.Entries.Count})";
            var outcome = LoadEntry(Entry);

            if (outcome == null)
            {
                CurrentStage = Stage.Loading;
                IdleFrames = 0;
                StageTime.Restart();
                return;
            }

            Results.Add((FolderName(Entry), outcome));
            EntryIndex++;
        }

        Finish(null);
    }

    private string LoadEntry(MaterialOracleEntry entry)
    {
        var container = Project.Handler.ModelData.PrimaryBank.Models.FirstOrDefault(e =>
            e.Key.Filename.Equals(entry.Container, StringComparison.OrdinalIgnoreCase)).Value;

        if (container == null)
            return "container not found";

        container.PopulateModelList();

        var model = string.IsNullOrEmpty(entry.Model)
            ? container.Models.FirstOrDefault()
            : container.Models.FirstOrDefault(m => m.Name.Equals(entry.Model, StringComparison.OrdinalIgnoreCase));

        if (model == null)
            return "model not found in container";

        View.Selection.SelectedModelContainerWrapper = container;

        if (!View.LoadModel(model))
            return "model open in another view";

        Target = model;
        return null;
    }

    private static bool ResourcesIdle =>
        ResourceManager.ActiveJobProgress.IsEmpty && !ResourceLoadTracker.HasActiveSession;

    private void UpdateLoading(VulkanViewport viewport)
    {
        if (StageTime.Elapsed > LoadTimeout)
        {
            NextEntry("load timed out");
            return;
        }

        var container = Target?.Container;

        if (View.Selection.SelectedModelWrapper != Target || container == null || !container.TryGetMeshBounds(out var bounds))
        {
            IdleFrames = 0;
            return;
        }

        IdleFrames = ResourcesIdle ? IdleFrames + 1 : 0;

        if (IdleFrames < IdleFramesToLoad)
            return;

        // The camera only has to keep the model in view, so it isn't culled: the oracle draws it in UV space
        viewport.FitBox(bounds);
        View.ViewportSelection.ClearSelection();

        var outcome = Isolate(container, Entry);

        if (outcome != null)
        {
            NextEntry(outcome);
            return;
        }

        CurrentStage = Stage.Settling;
        SettledFrames = 0;
        StageTime.Restart();
        Status = $"Settling {Entry.Container} ({EntryIndex + 1} of {Set.Entries.Count})";
    }

    /// <summary>Hides everything in the model but the entry's meshes. Returns why it can't, or null.</summary>
    private string Isolate(ModelContainer container, MaterialOracleEntry entry)
    {
        var meshes = container.Meshes ?? new();
        var materials = container.Materials ?? new();

        DrawnMeshes = entry.Meshes?.ToList() ?? meshes
            .Select((m, i) => (m, i))
            .Where(x => x.m.WrappedObject is FLVER2.Mesh mesh && mesh.MaterialIndex < materials.Count &&
                materials[mesh.MaterialIndex].WrappedObject is FLVER2.Material material &&
                Path.GetFileNameWithoutExtension(material.MTD.Replace('\\', '/')).Equals(entry.Material, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.i)
            .ToList();

        if (DrawnMeshes.Count == 0)
            return $"no mesh uses {entry.Material}";

        foreach (var obj in container.Objects)
        {
            obj.EditorVisible = false;
        }

        foreach (var index in DrawnMeshes.Where(i => i >= 0 && i < meshes.Count))
        {
            meshes[index].EditorVisible = true;
        }

        return null;
    }

    private void UpdateSettling(VulkanViewport viewport)
    {
        // Textures can still arrive after the meshes; each one starts the count again
        SettledFrames = ResourcesIdle ? SettledFrames + 1 : 0;

        if (SettledFrames < SettleFrames && StageTime.Elapsed < SettleTimeout)
            return;

        Images = null;
        ImagesArrived = false;
        viewport.ViewPipeline.Oracle.Request(images =>
        {
            Images = images;
            ImagesArrived = true;
        });

        CurrentStage = Stage.Capturing;
        StageTime.Restart();
    }

    private void UpdateCapturing(VulkanViewport viewport)
    {
        if (!ImagesArrived)
        {
            // The capture is only recorded while the viewport is drawn
            if (StageTime.Elapsed > CaptureTimeout)
            {
                viewport.ViewPipeline.Oracle.Cancel();
                NextEntry("viewport not drawn");
            }

            return;
        }

        if (Images == null)
        {
            NextEntry("readback failed");
            return;
        }

        try
        {
            Save(Path.Join(RunFolder, FolderName(Entry)), Images);
            NextEntry("ok");
        }
        catch (Exception e)
        {
            NextEntry($"save failed: {e.Message}");
        }
    }

    /// <summary>
    /// The raw attachments (RGBA float files, read by the comparison) and a PNG per channel to look at: albedo (sRGB
    /// encoded), metallic, normal, gloss, emissive and coverage
    /// </summary>
    private void Save(string folder, MaterialOracleImages images)
    {
        // The viewport draws with y flipped, so its first row is V = 1; the game's side has V = 0 first, as UV space does
        images = images with
        {
            Color = FlipRows(images.Color, images.Width), Surface = FlipRows(images.Surface, images.Width),
            Indirect = FlipRows(images.Indirect, images.Width), Glow = FlipRows(images.Glow, images.Width)
        };

        Directory.CreateDirectory(folder);
        WriteRaw(Path.Join(folder, "color.f32"), images.Color);
        WriteRaw(Path.Join(folder, "surface.f32"), images.Surface);
        WriteRaw(Path.Join(folder, "indirect.f32"), images.Indirect);
        WriteRaw(Path.Join(folder, "glow.f32"), images.Glow);

        WritePng(Path.Join(folder, "albedo.png"), images, p => (images.Color[p], images.Color[p + 1], images.Color[p + 2]), true);
        WritePng(Path.Join(folder, "metallic.png"), images, p => (images.Color[p + 3], images.Color[p + 3], images.Color[p + 3]), false);
        WritePng(Path.Join(folder, "normal.png"), images, p => (images.Surface[p], images.Surface[p + 1], images.Surface[p + 2]), false);
        WritePng(Path.Join(folder, "gloss.png"), images, p => (images.Surface[p + 3], images.Surface[p + 3], images.Surface[p + 3]), false);
        WritePng(Path.Join(folder, "emissive.png"), images, p => (images.Glow[p], images.Glow[p + 1], images.Glow[p + 2]), true);
        WritePng(Path.Join(folder, "coverage.png"), images, p => (images.Indirect[p], images.Indirect[p], images.Indirect[p]), false);

        var manifest = new
        {
            Game = GameName,
            Entry.Container,
            Model = Target?.Name,
            Entry.Material,
            Meshes = DrawnMeshes,
            images.Width,
            images.Height,
            Layout = "color: albedo rgb + metallic; surface: normal * 0.5 + 0.5 + gloss; indirect.x: coverage; glow: emissive",
            Captured = DateTime.Now.ToString("s")
        };
        File.WriteAllText(Path.Join(folder, "smithbox.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static float[] FlipRows(float[] rgba, int width)
    {
        var rowLength = width * 4;
        var rows = rgba.Length / rowLength;
        var flipped = new float[rgba.Length];
        for (var y = 0; y < rows; y++)
            Array.Copy(rgba, y * rowLength, flipped, (rows - 1 - y) * rowLength, rowLength);
        return flipped;
    }

    private static void WriteRaw(string path, float[] data)
    {
        var bytes = new byte[data.Length * 4];
        Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
        File.WriteAllBytes(path, bytes);
    }

    private static void WritePng(string path, MaterialOracleImages images, Func<int, (float r, float g, float b)> pixel, bool srgb)
    {
        static byte Byte(float v, bool srgb)
        {
            v = Math.Clamp(float.IsFinite(v) ? v : 0, 0, 1);
            if (srgb)
                v = v <= 0.0031308f ? v * 12.92f : 1.055f * MathF.Pow(v, 1 / 2.4f) - 0.055f;
            return (byte)MathF.Round(v * 255);
        }

        var rgba = new byte[images.Width * images.Height * 4];
        for (var i = 0; i < images.Width * images.Height; i++)
        {
            var (r, g, b) = pixel(i * 4);
            rgba[i * 4] = Byte(r, srgb);
            rgba[i * 4 + 1] = Byte(g, srgb);
            rgba[i * 4 + 2] = Byte(b, srgb);
            rgba[i * 4 + 3] = 255;
        }

        using var image = Image.LoadPixelData<Rgba32>(rgba, images.Width, images.Height);
        image.SaveAsPng(path);
    }

    private void NextEntry(string outcome)
    {
        Results.Add((FolderName(Entry), outcome));

        if (outcome != "ok")
            Smithbox.Log(GetType(), $"Material oracle, {Entry.Container} {Entry.Material}: {outcome}", Microsoft.Extensions.Logging.LogLevel.Warning);

        Target = null;
        EntryIndex++;
        StartEntry();
    }

    private void Finish(string status)
    {
        if (CurrentStage == Stage.Idle && status != null)
            return;

        CurrentStage = Stage.Idle;

        if (View.ViewportWindow?.Viewport is VulkanViewport viewport)
        {
            viewport.CaptureSize = null;
            viewport.ViewPipeline.Oracle.Cancel();
        }

        CFG.Current.Viewport.Viewport_Post_Taa = SavedTaa;
        SavedDisplay?.Restore();
        SavedDisplay = null;
        MaterialClock.FixedTime = null;

        var (ok, total) = LastRunCounts;
        Status = status ?? $"{ok} of {total} captured into {RunFolder}";
    }
}
