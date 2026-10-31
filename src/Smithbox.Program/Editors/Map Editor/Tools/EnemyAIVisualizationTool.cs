using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Shows the sight, listen and smell radius of the selected enemies, as read from their NpcThinkParam row.
/// The spheres stay until toggled off or cleared, and follow their enemy if it is moved.
/// </summary>
public class EnemyAIVisualizationTool
{
    private const string ThinkParamName = "NpcThinkParam";

    // ThinkParamID, except for the MSB formats that have it as NpcThinkParamId
    private static readonly string[] ThinkPropertyNames = ["ThinkParamID", "NpcThinkParamId"];

    private static readonly DetectionSense Sight = new("Sight", Color.Yellow, ["eye_dist", "eye_dist_normal"]);
    private static readonly DetectionSense Listen = new("Listen", Color.Cyan, ["ear_dist"]);
    private static readonly DetectionSense Smell = new("Smell", Color.LimeGreen, ["nose_dist"]);

    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private readonly Dictionary<(Entity, DetectionSense), DetectionSphere> Spheres = new();

    public EnemyAIVisualizationTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        // These have no NpcThinkParam, no think ID in their MSB, or an undocumented NpcThinkParam layout
        if (Project.Descriptor.ProjectType is ProjectType.DS2
            or ProjectType.DS2S
            or ProjectType.DES
            or ProjectType.DB
            or ProjectType.DB_NT)
        {
            GUI.WrappedText("This tool is not supported for this project type.");
            return;
        }

        GUI.WrappedText("Show the detection radius of the selected enemies, as defined by their NpcThinkParam.");
        GUI.WrappedText("");

        var enemies = View.ViewportSelection.GetFilteredSelection<MsbEntity>(IsEnemy);

        GUI.WrappedText($"{Sight.Name}: {GetRadiusText(enemies, Sight)}");
        GUI.WrappedText($"{Listen.Name}: {GetRadiusText(enemies, Listen)}");
        GUI.WrappedText($"{Smell.Name}: {GetRadiusText(enemies, Smell)}");
        GUI.WrappedText("");

        GUI.MultiButtonInput("enemyAiSenses",
            "showSight", Sight.Name, "Toggle the sight radius of the selected enemies.", () => ToggleSpheres(Sight),
            "showListen", Listen.Name, "Toggle the listen radius of the selected enemies.", () => ToggleSpheres(Listen),
            "showSmell", Smell.Name, "Toggle the smell radius of the selected enemies.", () => ToggleSpheres(Smell));

        GUI.MultiButtonInput("enemyAiClear",
            "clearSpheres", "Clear", "Remove all of the detection radius spheres.", Clear);
    }

    /// <summary>
    /// Keeps the spheres on their enemies. Enemies from unloaded maps lose their spheres, whilst deleted
    /// enemies only have them hidden, as the deletion can be undone.
    /// </summary>
    public void Update()
    {
        foreach (var (key, sphere) in Spheres.ToList())
        {
            var (entity, _) = key;

            if (entity.IsDisposed)
            {
                sphere.Dispose();
                Spheres.Remove(key);
                continue;
            }

            sphere.Update(entity);
        }
    }

    public void Clear()
    {
        foreach (var sphere in Spheres.Values)
        {
            sphere.Dispose();
        }

        Spheres.Clear();
    }

    /// <summary>
    /// A range is shown for a selection of enemies that differ.
    /// </summary>
    private string GetRadiusText(IEnumerable<MsbEntity> enemies, DetectionSense sense)
    {
        var radii = enemies.Select(e => GetRadius(e, sense)).OfType<float>().ToList();

        if (radii.Count == 0)
            return "-";

        var min = radii.Min();
        var max = radii.Max();

        return min == max ? $"{min:0.##}" : $"{min:0.##} - {max:0.##}";
    }

    /// <summary>
    /// Only removes the spheres once all of the selected enemies have one, so that a selection
    /// that has them on some enemies fills in the rest first.
    /// </summary>
    private void ToggleSpheres(DetectionSense sense)
    {
        var scene = View.ViewportHandler.RenderScene;

        if (scene == null)
            return;

        var enemies = View.ViewportSelection.GetFilteredSelection<MsbEntity>(IsEnemy)
            .Select(e => (Entity: (Entity)e, Radius: GetRadius(e, sense)))
            .Where(e => e.Radius > 0.0f)
            .ToList();

        if (enemies.Count == 0)
        {
            Smithbox.LogError<EnemyAIVisualizationTool>(
                $"No enemy with a {sense.Name.ToLowerInvariant()} radius is selected.");
            return;
        }

        if (enemies.All(e => Spheres.ContainsKey((e.Entity, sense))))
        {
            foreach (var (entity, _) in enemies)
            {
                Spheres.Remove((entity, sense), out var sphere);
                sphere.Dispose();
            }

            return;
        }

        foreach (var (entity, radius) in enemies)
        {
            if (!Spheres.ContainsKey((entity, sense)))
            {
                Spheres.Add((entity, sense), new DetectionSphere(scene, entity, radius.Value, sense.Color));
            }
        }
    }

    private static bool IsEnemy(MsbEntity entity)
    {
        return EntityHelper.IsPartEnemy(entity) || EntityHelper.IsPartDummyEnemy(entity);
    }

    /// <summary>
    /// Returns null if the enemy has no usable NpcThinkParam row.
    /// </summary>
    private float? GetRadius(Entity entity, DetectionSense sense)
    {
        var thinkParamId = GetThinkParamId(entity);

        if (thinkParamId == null)
            return null;

        var bank = Project.Handler.ParamData?.PrimaryBank;

        if (bank == null || !bank.Params.TryGetValue(ThinkParamName, out var param))
            return null;

        Param.Row row = param[thinkParamId.Value];

        if (row == null)
            return null;

        foreach (var field in sense.ParamFields)
        {
            var cell = row[field];

            if (cell != null)
            {
                return Convert.ToSingle(cell.Value.Value);
            }
        }

        return null;
    }

    private static int? GetThinkParamId(Entity entity)
    {
        var type = entity.WrappedObject.GetType();

        foreach (var name in ThinkPropertyNames)
        {
            var property = type.GetProperty(name);

            if (property != null)
            {
                return (int)property.GetValue(entity.WrappedObject);
            }
        }

        return null;
    }

    private record DetectionSense(string Name, Color Color, string[] ParamFields);

    private class DetectionSphere : IDisposable
    {
        private readonly float Radius;
        private readonly DebugPrimitiveRenderableProxy Wire;
        private readonly DebugPrimitiveRenderableProxy Fill;

        private Vector3 Position;

        public DetectionSphere(RenderScene scene, Entity entity, float radius, Color color)
        {
            Radius = radius;
            Wire = RenderableHelper.GetDetectionSphereProxy(scene, color);
            Fill = RenderableHelper.GetDetectionSphereFillProxy(scene, color);

            Move(entity.GetWorldMatrix().Translation);
        }

        public void Update(Entity entity)
        {
            var position = entity.GetWorldMatrix().Translation;

            if (position != Position)
            {
                Move(position);
            }

            var visible = entity.EditorVisible && entity.Container?.Objects.Contains(entity) == true;

            Wire.Visible = visible;
            Fill.Visible = visible;
        }

        private void Move(Vector3 position)
        {
            Position = position;

            // The rotation and scale of the enemy have no bearing on the radius
            var world = Matrix4x4.CreateScale(Radius) * Matrix4x4.CreateTranslation(position);

            Wire.World = world;
            Fill.World = world;
        }

        public void Dispose()
        {
            Wire.Dispose();
            Fill.Dispose();
        }
    }
}
