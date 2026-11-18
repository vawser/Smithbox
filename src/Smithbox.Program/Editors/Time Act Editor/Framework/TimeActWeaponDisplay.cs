#nullable enable
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Puts the weapons of a player where the events say: in one hand or both by the weapon style, moved by the weapon
/// location events, and hidden by the hide weapon events. The places come from the WepAbsorpPosParam row of each
/// weapon, so a game without the param keeps its weapons in hand. Nothing is stored: each frame every weapon is put
/// where the events at the current time say, which is where the part holds it when they say nothing.
/// </summary>
public class TimeActWeaponDisplay
{
    // The change types of the weapon location events (DSAnimStudio's DS3MemeAbsorpTaeType)
    private const int ChangeToDefault = -1;
    private const int ChangeToLeft = 0;
    private const int ChangeToRight = 1;
    private const int ChangeToBoth = 2;
    private const int ChangeToHang = 3;

    private const int StyleLeftBoth = 2;
    private const int StyleRightBoth = 3;

    private readonly TimeActEditorView _view;
    private readonly ProjectEntry _project;
    private readonly AnimationPreviewPanel _panel;

    private readonly Dictionary<(string Part, int EquipParamId, WeaponLocation Location), (string Node, Matrix4x4 Offset)?> _placements = new();
    private readonly Dictionary<int, (string Node, Matrix4x4 Offset)?> _dummyPlacements = new();

    // The weapons this has hidden, so that it only shows again what it hid
    private readonly HashSet<MeshRenderableProxy> _hidden = new();

    public TimeActWeaponDisplay(TimeActEditorView view, ProjectEntry project, AnimationPreviewPanel panel)
    {
        _view = view;
        _project = project;
        _panel = panel;
    }

    /// <summary>
    /// Forgets where the weapons go, for when their params have been edited
    /// </summary>
    public void ClearPlacements()
    {
        _placements.Clear();
        _dummyPlacements.Clear();
    }

    /// <summary>
    /// Forgets the weapons, for when they have been replaced
    /// </summary>
    public void Invalidate()
    {
        _placements.Clear();
        _hidden.Clear();
    }

    public void Update(object? key, TimeActSimulation simulation)
    {
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;

        if (key == null || container?.CharacterMesh is not { } mesh || container.CharacterAppearance?.Parts is not { } parts)
            return;

        for (var i = 0; i < parts.Count && i < mesh.Submeshes.Count; i++)
        {
            CharacterPart part = parts[i];

            if (part.AttachNode == null)
                continue;

            MeshRenderableProxy weapon = mesh.Submeshes[i];
            var left = CharacterComposition.IsLeftHand(part);

            // A dummy poly given outright (Sekiro) takes the place of the weapon's own places
            var placement = (left ? simulation.LeftWeaponDummy : simulation.RightWeaponDummy) is { } dummy
                ? GetDummyPlacement(dummy)
                : GetPlacement(part, GetLocation(part, left, simulation));

            if (placement is var (node, offset))
            {
                // A place on a bone the model doesn't have leaves the weapon where it was
                if (!_panel.SetAttachment(key, weapon, node, offset))
                {
                    _panel.ResetAttachment(key, weapon);
                }
            }
            else
            {
                _panel.ResetAttachment(key, weapon);
            }

            SetHidden(weapon, left ? simulation.HideLeftWeapon : simulation.HideRightWeapon);
        }
    }

    private WeaponLocation GetLocation(CharacterPart part, bool left, TimeActSimulation simulation)
    {
        var style = simulation.WeaponStyle;

        // The weapon held in both hands is in its both-hands place, and the other one hung on the body
        WeaponLocation location = (style, left) switch
        {
            (StyleLeftBoth, true) => WeaponLocation.LeftBoth,
            (StyleLeftBoth, false) => WeaponLocation.RightHang,
            (StyleRightBoth, false) => WeaponLocation.RightBoth,
            (StyleRightBoth, true) => WeaponLocation.LeftHang,
            (_, true) => WeaponLocation.Left,
            _ => WeaponLocation.Right
        };

        var defaultLocation = location;
        var invisibleType = CharacterComposition.GetWeaponInvisibleType(_project, part);
        var prefix = left ? "LHModel0" : "RHModel0";

        foreach (TAE.Event ev in simulation.WeaponLocationEvents)
        {
            if (!IsEnabledForStyle(ev, style))
                continue;

            if (TimeActSimulation.GetFloat(ev, $"{prefix}ChangeType") is not { } changeValue)
                continue;

            // The event only moves weapons whose row has the same wepInvisibleType as its condition
            if (TimeActSimulation.GetFloat(ev, $"{prefix}AbsorpPosParamCondition") is { } condition &&
                (int)condition != invisibleType)
                continue;

            location = (int)changeValue switch
            {
                ChangeToDefault => defaultLocation,
                ChangeToLeft => WeaponLocation.Left,
                ChangeToRight => WeaponLocation.Right,
                ChangeToBoth => left ? WeaponLocation.LeftBoth : WeaponLocation.RightBoth,
                ChangeToHang => left ? WeaponLocation.LeftHang : WeaponLocation.RightHang,

                // Keeps what an earlier event set, and the types nothing is known about do the same
                _ => location
            };
        }

        return location;
    }

    // The event says which of the styles it applies in, where its def has the fields
    private static bool IsEnabledForStyle(TAE.Event ev, int style)
    {
        var field = style switch
        {
            StyleLeftBoth => "IsEnabledWhile2HandingLeftWeapon",
            StyleRightBoth => "IsEnabledWhile2HandingRightWeapon",
            _ => "IsEnabledWhile1Handing"
        };

        return TimeActSimulation.GetFloat(ev, field) is not { } enabled || enabled != 0;
    }

    private (string Node, Matrix4x4 Offset)? GetPlacement(CharacterPart part, WeaponLocation location)
    {
        var key = (part.Name, part.EquipParamId, location);

        if (!_placements.TryGetValue(key, out var placement))
        {
            placement = CharacterComposition.GetWeaponPlacement(_project, part, location);
            _placements[key] = placement;
        }

        return placement;
    }

    private (string Node, Matrix4x4 Offset)? GetDummyPlacement(int dummy)
    {
        if (!_dummyPlacements.TryGetValue(dummy, out var placement))
        {
            placement = CharacterComposition.GetWeaponPlacementAtDummy(_project, dummy);
            _dummyPlacements[dummy] = placement;
        }

        return placement;
    }

    private void SetHidden(MeshRenderableProxy weapon, bool hidden)
    {
        if (hidden)
        {
            if (weapon.Visible)
            {
                weapon.Visible = false;
                _hidden.Add(weapon);
            }
        }
        else if (_hidden.Remove(weapon))
        {
            weapon.Visible = true;
        }
    }
}
