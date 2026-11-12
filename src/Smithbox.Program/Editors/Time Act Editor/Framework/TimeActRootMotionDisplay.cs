#nullable enable
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Draws the root motion of the playing clip: the path the character takes over the whole clip, and a
/// marker at where the root is at the current time, pointing the way the character faces.
/// </summary>
public class TimeActRootMotionDisplay
{
    // How long the facing tick of the marker is, and how wide its cross is
    private const float MarkerLength = 0.6f;
    private const float MarkerWidth = 0.15f;

    private static readonly Color PathColor = Color.LimeGreen;
    private static readonly Color MarkerColor = Color.White;

    private readonly TimeActEditorView _view;

    private HavokRootMotion? _pathSource;
    private DebugPrimitiveRenderableProxy? _path;
    private DbgPrimLineBatch? _pathLines;

    private DebugPrimitiveRenderableProxy? _marker;
    private DbgPrimLineBatch? _markerLines;

    public TimeActRootMotionDisplay(TimeActEditorView view)
    {
        _view = view;
    }

    public void Update(ModelAnimator? animator)
    {
        HavokRootMotion? rootMotion = animator?.CurrentClip?.RootMotion;

        if (!CFG.Current.TimeActEditor_Display_Root_Motion || animator == null || rootMotion == null)
        {
            Hide();
            return;
        }

        if (rootMotion != _pathSource)
        {
            BuildPath(rootMotion);
        }

        EnsureMarker();

        // Where the root is now, whether or not the model is being moved by it
        _marker!.World = rootMotion.Sample(animator.Time);
        _marker.Visible = true;

        if (_path != null)
        {
            _path.Visible = true;
        }
    }

    private void BuildPath(HavokRootMotion rootMotion)
    {
        ReleasePath();

        _pathSource = rootMotion;

        var lines = new List<(Vector3 A, Vector3 B)>();

        for (var i = 1; i < rootMotion.SampleCount && lines.Count < DbgPrimLineBatch.MaxLines; i++)
        {
            Vector3 a = rootMotion.GetSamplePosition(i - 1);
            Vector3 b = rootMotion.GetSamplePosition(i);

            if (a != b)
            {
                lines.Add((a, b));
            }
        }

        // A clip that keeps the character in place, such as an idle, has no path to show
        if (lines.Count == 0)
            return;

        _pathLines = new DbgPrimLineBatch(lines);
        _path = CreateProxy(_pathLines, PathColor);
    }

    private void EnsureMarker()
    {
        if (_marker != null)
            return;

        // Built around the root, and moved to the current sample through its world matrix.
        // Characters face along +Z in the space of their skeleton.
        _markerLines = new DbgPrimLineBatch(new List<(Vector3 A, Vector3 B)>
        {
            (Vector3.Zero, new Vector3(0, 0, MarkerLength)),
            (new Vector3(-MarkerWidth, 0, 0), new Vector3(MarkerWidth, 0, 0)),
            (new Vector3(0, 0, MarkerLength), new Vector3(-MarkerWidth, 0, MarkerLength - MarkerWidth)),
            (new Vector3(0, 0, MarkerLength), new Vector3(MarkerWidth, 0, MarkerLength - MarkerWidth))
        });

        _marker = CreateProxy(_markerLines, MarkerColor);
    }

    // Drawn over the model, as the path runs along the ground between its feet
    private DebugPrimitiveRenderableProxy CreateProxy(DbgPrimLineBatch lines, Color color)
    {
        return new DebugPrimitiveRenderableProxy(_view.RenderScene.OpaqueRenderables, lines)
        {
            Pickable = false,
            BaseColor = color,
            HighlightedColor = color,
            RenderOverlay = true,
            Visible = false
        };
    }

    private void Hide()
    {
        if (_path != null)
        {
            _path.Visible = false;
        }

        if (_marker != null)
        {
            _marker.Visible = false;
        }
    }

    private void ReleasePath()
    {
        _path?.Dispose();
        _pathLines?.Release();

        _path = null;
        _pathLines = null;
        _pathSource = null;
    }

    /// <summary>
    /// Frees the lines, for when the view closes
    /// </summary>
    public void Clear()
    {
        ReleasePath();

        _marker?.Dispose();
        _markerLines?.Release();

        _marker = null;
        _markerLines = null;
    }
}
