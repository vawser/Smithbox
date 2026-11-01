using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
///     Plays back what the bullet row selected in the param editor does when it is fired.
///     Nothing here touches the edited data.
/// </summary>
public class ProjectileSimulatorTool
{
    private const float SettingsWidth = 300.0f;

    public ParamEditorView View;
    public ProjectEntry Project;

    private readonly ProjectileViewport Viewport = new();

    private string ParamName;
    private Param.Row Row;
    private SimulationResult Result;

    // The bullets of the current spell that have been turned off
    private readonly HashSet<int> DisabledBullets = new();

    // Every edit goes through the action manager, and undoing or redoing changes this too
    private EditorAction LastAction;
    private bool RebuildPending;

    private int LastUpdateFrame = -1;

    private bool Playing;
    private bool Loop = true;
    private float Time;
    private float Speed = 1.0f;

    private float LaunchHeight = 1.5f;
    private CastOrientation Orientation = CastOrientation.Forward;
    private bool GroundCollision = true;
    private bool SimulateWall;
    private float WallDistance = 10.0f;
    private float WallWidth = 12.0f;
    private float WallHeight = 8.0f;
    private ImpactSurface Surface = ImpactSurface.Map;
    private bool SimulateTarget;
    private Vector3 TargetPosition = new(5.0f, 1.5f, 15.0f);
    private float TargetRadius = 0.5f;
    private bool UseEnemyAttacks;
    private int Seed = 1;

    public ProjectileSimulatorTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private SimulationSettings Settings =>
        new(LaunchHeight, Orientation, GroundCollision, SimulateWall, WallDistance, WallWidth, WallHeight, Surface,
            SimulateTarget, TargetPosition, TargetRadius, Seed);

    public void Display()
    {
        Update();

        var scale = DPI.UIScale();
        var width = SettingsWidth * scale;

        if (ImGui.BeginChild("projectileSimulatorSettings", new Vector2(width, 0), ImGuiChildFlags.ResizeX))
        {
            DisplaySettings();
        }

        ImGui.EndChild();

        ImGui.SameLine();

        if (ImGui.BeginChild("projectileSimulatorPreview"))
        {
            DisplayPreview();
        }

        ImGui.EndChild();
    }

    private void DisplaySettings()
    {
        DisplayTarget();
        DisplaySetup();
        DisplayWarnings();
        DisplayProjectiles();
    }

    // The tool can be open in the tool window and its own window at once, which mustn't double its speed
    private void Update()
    {
        var frame = ImGui.GetFrameCount();

        if (frame == LastUpdateFrame)
        {
            return;
        }

        LastUpdateFrame = frame;

        UpdateTarget();
        UpdatePlayback();
    }

    /// <summary>
    ///     The last bullet stays in place while other params are visited, so that its references can be
    ///     followed and edited without losing the preview.
    /// </summary>
    private void UpdateTarget()
    {
        var activeParam = View.Selection.GetActiveParam();
        Param.Row activeRow = View.Selection.GetActiveRow();

        if (activeParam != null && activeRow != null && activeRow != Row &&
            (IsBulletParam(activeParam) || IsSpellParam(activeParam)))
        {
            ParamName = activeParam;
            Row = activeRow;

            ResetSpellBullets();
            Rebuild();

            Time = 0;
            Playing = Result != null;

            return;
        }

        EditorAction lastAction = View.Editor.ActionManager.PeekUndoAction();

        if (lastAction != LastAction)
        {
            LastAction = lastAction;
            RebuildPending = true;
        }

        if (RebuildPending)
        {
            Rebuild();
        }
    }

    /// <summary>
    ///     Starts on the normal cast of a spell. Charged and unused slots are there to be turned on, but
    ///     a spell made up of nothing else starts with all of them so that it doesn't look empty.
    /// </summary>
    private void ResetSpellBullets()
    {
        DisabledBullets.Clear();

        if (!IsSpellParam(ParamName))
        {
            return;
        }

        SpellBullet[] bullets = BulletDefinition.GetSpellBullets(Row);

        if (bullets.Any(bullet => bullet.Slot is SpellSlotType.Default))
        {
            DisabledBullets.UnionWith(bullets.Where(bullet => bullet.Slot is not SpellSlotType.Default)
                .Select(bullet => bullet.Id));
        }
    }

    private bool IsBulletParam(string paramName)
    {
        return GetParam(paramName)?.ParamType is "BULLET_PARAM_ST" or "BULLET_PARAM";
    }

    private bool IsSpellParam(string paramName)
    {
        return GetParam(paramName)?.ParamType is "MAGIC_PARAM_ST" or "SPELL_PARAM";
    }

    /// <summary>
    ///     The param that a spell's bullets are in, as games can have several: the rest are for enemies and the like.
    /// </summary>
    private Param GetSpellBulletParam()
    {
        return GetParam("Bullet") ?? GetParam("BulletParam");
    }

    private AttackDefinition FindAttack(int id)
    {
        Param.Row row = GetAttackParam(UseEnemyAttacks)?[id] ?? GetAttackParam(!UseEnemyAttacks)?[id];

        return row != null ? AttackDefinition.FromRow(row) : null;
    }

    private Param GetAttackParam(bool enemy)
    {
        return GetParam(enemy ? "AtkParam_Npc" : "AtkParam_Pc");
    }

    private Param GetParam(string paramName)
    {
        IReadOnlyDictionary<string, Param> paramList = View.GetPrimaryBank().Params;

        return paramList != null && paramList.TryGetValue(paramName, out Param param) ? param : null;
    }

    private void Rebuild()
    {
        RebuildPending = false;
        Result = null;

        if (ParamName == null || Row == null)
        {
            return;
        }

        var isSpell = IsSpellParam(ParamName);

        Param param = isSpell ? GetSpellBulletParam() : GetParam(ParamName);
        IReadOnlyList<int> roots = isSpell
            ? BulletDefinition.GetSpellBullets(Row).Select(bullet => bullet.Id).Where(id => !DisabledBullets.Contains(id)).ToArray()
            : new[] { Row.ID };

        if (param == null || roots.Count == 0 || !isSpell && !BulletDefinition.IsReadable(Row))
        {
            Playing = false;
            return;
        }

        Result = ProjectileSimulation.Run(roots, id =>
        {
            Param.Row row = param[id];

            return row != null ? BulletDefinition.FromRow(row, FindAttack) : null;
        }, Settings);

        Time = Math.Min(Time, Result.Duration);
    }

    private void UpdatePlayback()
    {
        if (!Playing || Result == null)
        {
            return;
        }

        Time += ImGui.GetIO().DeltaTime * Speed;

        if (Time < Result.Duration)
        {
            return;
        }

        Time = Loop ? 0 : Result.Duration;
        Playing = Loop;
    }

    private void DisplayTarget()
    {
        GUI.SimpleHeader(LOC.Get("PARAM_ProjectileSimulator_Header_Bullet"), LOC.Get("PARAM_ProjectileSimulator_Header_Bullet_TT"));

        if (Row == null)
        {
            GUI.WrappedText(LOC.Get("PARAM_ProjectileSimulator_No_Bullet"));
        }
        else if (IsSpellParam(ParamName))
        {
            GUI.WrappedText(GetRowLabel(Row.ID, Row.Name));

            DisplaySpellBullets();
        }
        else if (!BulletDefinition.IsReadable(Row))
        {
            GUI.WrappedText(LOC.Get("PARAM_ProjectileSimulator_Unsupported_Layout"));
        }
        else
        {
            GUI.WrappedText(GetRowLabel(Row.ID, Row.Name));
        }

        GUI.Spacer();
    }

    private void DisplaySpellBullets()
    {
        SpellBullet[] bullets = BulletDefinition.GetSpellBullets(Row);

        if (bullets.Length == 0)
        {
            GUI.WrappedText(LOC.Get("PARAM_ProjectileSimulator_Spell_No_Bullets"));
            return;
        }

        GUI.WrappedText(LOC.Get("PARAM_ProjectileSimulator_Spell_Bullets"));

        Param param = GetSpellBulletParam();

        foreach (SpellBullet bullet in bullets)
        {
            var enabled = !DisabledBullets.Contains(bullet.Id);

            if (ImGui.Checkbox($"{GetSpellBulletLabel(param?[bullet.Id], bullet)}##spellBullet{bullet.Id}", ref enabled))
            {
                if (enabled)
                {
                    DisabledBullets.Remove(bullet.Id);

                    Time = 0;
                    Playing = true;
                }
                else
                {
                    DisabledBullets.Add(bullet.Id);
                }

                RebuildPending = true;
            }
        }
    }

    private static string GetSpellBulletLabel(Param.Row row, SpellBullet bullet)
    {
        var label = GetRowLabel(bullet.Id, row?.Name);

        return bullet.Slot switch
        {
            SpellSlotType.Charged => $"{label} ({LOC.Get("PARAM_ProjectileSimulator_Slot_Charged")})",
            SpellSlotType.None => $"{label} ({LOC.Get("PARAM_ProjectileSimulator_Slot_None")})",
            _ => label
        };
    }

    private void DisplaySetup()
    {
        var changed = false;

        GUI.SimpleHeader(LOC.Get("PARAM_ProjectileSimulator_Header_Setup"), "");

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.SliderFloat("##launchHeight", ref LaunchHeight, 0, 20,
            $"{LOC.Get("PARAM_ProjectileSimulator_Launch_Height")}: %.1f m");

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##castOrientation", $"{LOC.Get("PARAM_ProjectileSimulator_Cast_Orientation")}: {GetOrientationName(Orientation)}"))
        {
            foreach (CastOrientation orientation in Enum.GetValues<CastOrientation>())
            {
                if (ImGui.Selectable(GetOrientationName(orientation), orientation == Orientation))
                {
                    Orientation = orientation;
                    changed = true;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectileSimulator_Cast_Orientation_TT"));

        if (ImGui.Button($"{LOC.Get("PARAM_ProjectileSimulator_Reroll")}##reroll"))
        {
            Seed++;
            changed = true;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectileSimulator_Reroll_TT"));

        changed |= ImGui.Checkbox($"{LOC.Get("PARAM_ProjectileSimulator_Enemy_Attacks")}##useEnemyAttacks", ref UseEnemyAttacks);
        GUI.Tooltip(LOC.Get("PARAM_ProjectileSimulator_Enemy_Attacks_TT"));

        changed |= ImGui.Checkbox($"{LOC.Get("PARAM_ProjectileSimulator_Ground_Collision")}##groundCollision", ref GroundCollision);
        GUI.Tooltip(LOC.Get("PARAM_ProjectileSimulator_Ground_Collision_TT"));

        changed |= ImGui.Checkbox($"{LOC.Get("PARAM_ProjectileSimulator_Simulate_Wall")}##simulateWall", ref SimulateWall);
        GUI.Tooltip(LOC.Get("PARAM_ProjectileSimulator_Simulate_Wall_TT"));

        if (SimulateWall)
        {
            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##wallDistance", ref WallDistance, 0.5f, 100,
                $"{LOC.Get("PARAM_ProjectileSimulator_Wall_Distance")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##wallWidth", ref WallWidth, 1, 50,
                $"{LOC.Get("PARAM_ProjectileSimulator_Wall_Width")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##wallHeight", ref WallHeight, 1, 50,
                $"{LOC.Get("PARAM_ProjectileSimulator_Wall_Height")}: %.1f m");
        }

        if (GroundCollision || SimulateWall)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo("##surface", GetSurfaceName(Surface)))
            {
                foreach (ImpactSurface surface in new[] { ImpactSurface.Map, ImpactSurface.Water })
                {
                    if (ImGui.Selectable(GetSurfaceName(surface), surface == Surface))
                    {
                        Surface = surface;
                        changed = true;
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("PARAM_ProjectileSimulator_Surface_TT"));
        }

        changed |= ImGui.Checkbox($"{LOC.Get("PARAM_ProjectileSimulator_Simulate_Target")}##simulateTarget", ref SimulateTarget);
        GUI.Tooltip(LOC.Get("PARAM_ProjectileSimulator_Simulate_Target_TT"));

        if (SimulateTarget)
        {
            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.DragFloat("##targetX", ref TargetPosition.X, 0.1f, -200, 200,
                $"{LOC.Get("PARAM_ProjectileSimulator_Target_Right")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.DragFloat("##targetY", ref TargetPosition.Y, 0.1f, -50, 200,
                $"{LOC.Get("PARAM_ProjectileSimulator_Target_Height")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.DragFloat("##targetZ", ref TargetPosition.Z, 0.1f, -200, 200,
                $"{LOC.Get("PARAM_ProjectileSimulator_Target_Forward")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##targetRadius", ref TargetRadius, 0.1f, 10,
                $"{LOC.Get("PARAM_ProjectileSimulator_Target_Radius")}: %.1f m");
        }

        if (changed)
        {
            RebuildPending = true;
        }

        GUI.Spacer();
    }

    private void DisplayWarnings()
    {
        if (Result == null || (Result.Warnings == SimulationWarning.None && Result.Ignored == BulletIgnoredBehavior.None))
        {
            return;
        }

        GUI.SimpleHeader(LOC.Get("PARAM_ProjectileSimulator_Header_Warnings"), LOC.Get("PARAM_ProjectileSimulator_Header_Warnings_TT"));

        DisplayWarning(SimulationWarning.ProjectileLimit, "PARAM_ProjectileSimulator_Warning_Projectile_Limit", ProjectileSimulation.ProjectileLimit);
        DisplayWarning(SimulationWarning.TimeLimit, "PARAM_ProjectileSimulator_Warning_Time_Limit", ProjectileSimulation.DurationLimit);
        DisplayWarning(SimulationWarning.MissingRow, "PARAM_ProjectileSimulator_Warning_Missing_Row");
        DisplayWarning(SimulationWarning.SelfReference, "PARAM_ProjectileSimulator_Warning_Self_Reference");
        DisplayWarning(SimulationWarning.UndocumentedCondition, "PARAM_ProjectileSimulator_Warning_Undocumented_Condition");
        DisplayWarning(SimulationWarning.AbsorbedCondition, "PARAM_ProjectileSimulator_Warning_Absorbed_Condition");

        DisplayIgnored(BulletIgnoredBehavior.Homing, "PARAM_ProjectileSimulator_Ignored_Homing");
        DisplayIgnored(BulletIgnoredBehavior.EmitPosition, "PARAM_ProjectileSimulator_Ignored_Emit_Position");
        DisplayIgnored(BulletIgnoredBehavior.Following, "PARAM_ProjectileSimulator_Ignored_Following");

        GUI.Spacer();
    }

    private void DisplayWarning(SimulationWarning warning, string key, params object[] args)
    {
        if (Result.Warnings.HasFlag(warning))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get(key, args));
        }
    }

    private void DisplayIgnored(BulletIgnoredBehavior behavior, string key)
    {
        if (Result.Ignored.HasFlag(behavior))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get(key));
        }
    }

    private void DisplayProjectiles()
    {
        if (Result == null)
        {
            return;
        }

        GUI.SimpleHeader($"{LOC.Get("PARAM_ProjectileSimulator_Header_Projectiles")} ({Result.Tracks.Count})",
            LOC.Get("PARAM_ProjectileSimulator_Header_Projectiles_TT"));

        if (ImGui.BeginChild("projectileList", default, ImGuiChildFlags.Borders))
        {
            for (var i = 0; i < Result.Tracks.Count; i++)
            {
                ProjectileTrack track = Result.Tracks[i];

                Vector4 color = ProjectileViewport.GetColor(track.Generation);

                if (!track.IsActive(Time))
                {
                    color.W = 0.5f;
                }

                ImGui.TextColored(color, $"{track.SpawnTime:F2}s {GetRowLabel(track.RowId, track.Name)}");

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetItemTooltip(GetTrackDescription(track));
                }
            }
        }

        ImGui.EndChild();
    }

    private string GetTrackDescription(ProjectileTrack track)
    {
        var origin = track.Origin switch
        {
            ProjectileOrigin.Chain => LOC.Get("PARAM_ProjectileSimulator_Origin_Chain", Result.Tracks[track.Parent].RowId),
            ProjectileOrigin.Interval => LOC.Get("PARAM_ProjectileSimulator_Origin_Interval", Result.Tracks[track.Parent].RowId),
            _ => LOC.Get("PARAM_ProjectileSimulator_Origin_Root")
        };

        var end = track.End switch
        {
            ProjectileEnd.HitWall => LOC.Get("PARAM_ProjectileSimulator_End_Hit_Wall", track.EndTime),
            ProjectileEnd.HitGround => LOC.Get("PARAM_ProjectileSimulator_End_Hit_Ground", track.EndTime),
            ProjectileEnd.HitTarget => LOC.Get("PARAM_ProjectileSimulator_End_Hit_Target", track.EndTime),
            ProjectileEnd.TimeLimit => LOC.Get("PARAM_ProjectileSimulator_End_Time_Limit", track.EndTime),
            _ => LOC.Get("PARAM_ProjectileSimulator_End_Expired", track.EndTime)
        };

        var description = $"{origin}\n{end}";

        if (track.Attack != null)
        {
            description += $"\n{LOC.Get("PARAM_ProjectileSimulator_Attack", track.Attack.RowId, track.Attack.Radius)}" +
                           $"\n{ProjectileViewport.FormatDamage(track.Attack.Flat, track.Attack.Correction)}";
        }

        if (track.ChainWithheld)
        {
            description += $"\n{LOC.Get("PARAM_ProjectileSimulator_Chain_Withheld")}";
        }

        return description;
    }

    private void DisplayPreview()
    {
        Vector2 size = ImGui.GetContentRegionAvail();

        size.Y -= ImGui.GetFrameHeightWithSpacing() * 2;

        if (size.X > 1 && size.Y > 1)
        {
            Viewport.Display("##projectileViewport", size, Result, Settings, Time);

            // The movement keys are on the same keys as the editor's shortcuts, with Ctrl being the slow speed
            if (Viewport.IsUsingCamera)
            {
                ImGui.SetWindowFocus();
                FocusManager.SetFocus(EditorFocusContext.None);
            }

            if (!Viewport.IsLooking)
            {
                GUI.Tooltip(LOC.Get("PARAM_ProjectileSimulator_Viewport_TT"));
            }
        }

        DisplayPlayback();
    }

    private void DisplayPlayback()
    {
        var duration = Result?.Duration ?? 0;

        ImGui.BeginDisabled(Result == null);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.SliderFloat("##projectileTime", ref Time, 0, duration, $"{Time:F2} / {duration:F2} s"))
        {
            Playing = false;
        }

        if (ImGui.Button($"{(Playing ? LOC.Get("PARAM_ProjectileSimulator_Pause") : LOC.Get("PARAM_ProjectileSimulator_Play"))}##projectilePlay"))
        {
            // A finished simulation starts over
            if (!Playing && Time >= duration)
            {
                Time = 0;
            }

            Playing = !Playing;
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PARAM_ProjectileSimulator_Stop")}##projectileStop"))
        {
            Playing = false;
            Time = 0;
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PARAM_ProjectileSimulator_Frame")}##projectileFrame"))
        {
            Viewport.Frame(Result.BoundsMin, Result.BoundsMax);
        }

        ImGui.SameLine();

        ImGui.Checkbox($"{LOC.Get("PARAM_ProjectileSimulator_Loop")}##projectileLoop", ref Loop);

        ImGui.SameLine();

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##projectileSpeed", ref Speed, 0.05f, 2f, $"{LOC.Get("PARAM_ProjectileSimulator_Speed")}: %.2f");

        ImGui.EndDisabled();
    }

    private static string GetRowLabel(int id, string name)
    {
        return string.IsNullOrEmpty(name) ? $"{id}" : $"{id} {name}";
    }

    private static string GetOrientationName(CastOrientation orientation)
    {
        return orientation switch
        {
            CastOrientation.Down => LOC.Get("PARAM_ProjectileSimulator_Orientation_Down"),
            CastOrientation.Up => LOC.Get("PARAM_ProjectileSimulator_Orientation_Up"),
            _ => LOC.Get("PARAM_ProjectileSimulator_Orientation_Forward")
        };
    }

    private static string GetSurfaceName(ImpactSurface surface)
    {
        return surface switch
        {
            ImpactSurface.Water => LOC.Get("PARAM_ProjectileSimulator_Surface_Water"),
            _ => LOC.Get("PARAM_ProjectileSimulator_Surface_Map")
        };
    }
}
