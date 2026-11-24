#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Linq;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// What is simulated and how, drawn inside the Playback tool (<see cref="ProjectilePlaybackTool"/>): the spell's bullets
/// to fire, the setup around the shooter (its height, the ground, a wall and a target), what the simulation couldn't
/// follow, and the bullets it fired.
/// </summary>
public class ProjectileSimulationSettings
{
    private readonly ProjectileEditorView View;

    public ProjectileSimulationSettings(ProjectileEditorView view)
    {
        View = view;
    }

    private ProjectileSimulator Simulator => View.Simulator;

    public void DisplayTarget()
    {
        GUI.SimpleHeader(LOC.Get("PROJECTILE_Header_Bullet"), LOC.Get("PROJECTILE_Header_Bullet_TT"));

        var row = Simulator.Row;

        if (row == null)
        {
            GUI.WrappedText(LOC.Get("PROJECTILE_No_Bullet"));
        }
        else if (Simulator.IsSpell)
        {
            GUI.WrappedText(ProjectileParams.RowLabel(row.ID, row.Name));

            DisplaySpellBullets(row);
        }
        else if (!BulletDefinition.IsReadable(row))
        {
            GUI.WrappedText(LOC.Get("PROJECTILE_Unsupported_Layout"));
        }
        else
        {
            GUI.WrappedText(ProjectileParams.RowLabel(row.ID, row.Name));
        }

        GUI.Spacer();
    }

    private void DisplaySpellBullets(Param.Row row)
    {
        var bullets = BulletDefinition.GetSpellBullets(row);

        if (bullets.Length == 0)
        {
            GUI.WrappedText(LOC.Get("PROJECTILE_Spell_No_Bullets"));
            return;
        }

        GUI.WrappedText(LOC.Get("PROJECTILE_Spell_Bullets"));

        var param = ProjectileParams.GetSpellBulletParam(View.Project);

        foreach (var bullet in bullets)
        {
            var enabled = !Simulator.DisabledBullets.Contains(bullet.Id);

            if (ImGui.Checkbox($"{GetSpellBulletLabel(param?[bullet.Id], bullet)}##spellBullet{bullet.Id}", ref enabled))
            {
                if (enabled)
                {
                    Simulator.DisabledBullets.Remove(bullet.Id);

                    Simulator.Time = 0;
                    Simulator.Playing = true;
                }
                else
                {
                    Simulator.DisabledBullets.Add(bullet.Id);
                }

                Simulator.Invalidate();
            }
        }
    }

    private static string GetSpellBulletLabel(Param.Row? row, SpellBullet bullet)
    {
        var label = ProjectileParams.RowLabel(bullet.Id, row?.Name);

        return bullet.Slot switch
        {
            SpellSlotType.Charged => $"{label} ({LOC.Get("PROJECTILE_Slot_Charged")})",
            SpellSlotType.None => $"{label} ({LOC.Get("PROJECTILE_Slot_None")})",
            _ => label
        };
    }

    public void DisplaySetup(ProjectType projectType)
    {
        var changed = false;
        var cfg = CFG.Current;

        GUI.SimpleHeader(LOC.Get("PROJECTILE_Header_Setup"), "");

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.SliderFloat("##launchHeight", ref cfg.ProjectileEditor.ProjectileEditor_LaunchHeight, 0, 20,
            $"{LOC.Get("PROJECTILE_Launch_Height")}: %.1f m");

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.SliderFloat("##emitHeight", ref cfg.ProjectileEditor.ProjectileEditor_EmitHeight, 0, 5,
            $"{LOC.Get("PROJECTILE_Emit_Height")}: %.1f m");
        GUI.Tooltip(LOC.Get("PROJECTILE_Emit_Height_TT"));

        // Later games have a field for this
        if (projectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB)
        {
            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##fixedEmitRadius", ref cfg.ProjectileEditor.ProjectileEditor_FixedEmitRadius, 0, 30,
                $"{LOC.Get("PROJECTILE_Fixed_Emit_Radius")}: %.1f m");
            GUI.Tooltip(LOC.Get("PROJECTILE_Fixed_Emit_Radius_TT"));
        }

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##castOrientation", $"{LOC.Get("PROJECTILE_Cast_Orientation")}: {ProjectileFormat.GetOrientationName(cfg.ProjectileEditor.ProjectileEditor_Orientation)}"))
        {
            foreach (var orientation in Enum.GetValues<CastOrientation>())
            {
                if (ImGui.Selectable(ProjectileFormat.GetOrientationName(orientation), orientation == cfg.ProjectileEditor.ProjectileEditor_Orientation))
                {
                    cfg.ProjectileEditor.ProjectileEditor_Orientation = orientation;
                    changed = true;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PROJECTILE_Cast_Orientation_TT"));

        // What the bullet rows leave to the animation and the shooter, each shown when the param has its field
        var bulletParam = View.GetBulletParam();

        if (bulletParam?["shootInterval"] != null)
        {
            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##emitterDuration", ref cfg.ProjectileEditor.ProjectileEditor_EmitterDuration, 0, 10,
                $"{LOC.Get("PROJECTILE_Emitter_Duration")}: %.2f s");
            GUI.Tooltip(LOC.Get("PROJECTILE_Emitter_Duration_TT"));
        }

        if (bulletParam?["dist"] != null)
        {
            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##distanceScale", ref cfg.ProjectileEditor.ProjectileEditor_DistanceScale, 0.1f, 3,
                $"{LOC.Get("PROJECTILE_Distance_Scale")}: %.2f");
            GUI.Tooltip(LOC.Get("PROJECTILE_Distance_Scale_TT"));
        }

        if (bulletParam?["spBulletDistUpRate"] != null)
        {
            changed |= ImGui.Checkbox($"{LOC.Get("PROJECTILE_Distance_Up_Effect")}##distanceUpEffect", ref cfg.ProjectileEditor.ProjectileEditor_DistanceUpEffect);
            GUI.Tooltip(LOC.Get("PROJECTILE_Distance_Up_Effect_TT"));
        }

        if (ImGui.Button($"{LOC.Get("PROJECTILE_Reroll")}##reroll"))
        {
            cfg.ProjectileEditor.ProjectileEditor_Seed++;
            changed = true;
        }
        GUI.Tooltip(LOC.Get("PROJECTILE_Reroll_TT"));

        changed |= ImGui.Checkbox($"{LOC.Get("PROJECTILE_Enemy_Attacks")}##useEnemyAttacks", ref cfg.ProjectileEditor.ProjectileEditor_UseEnemyAttacks);
        GUI.Tooltip(LOC.Get("PROJECTILE_Enemy_Attacks_TT"));

        changed |= ImGui.Checkbox($"{LOC.Get("PROJECTILE_Ground_Collision")}##groundCollision", ref cfg.ProjectileEditor.ProjectileEditor_GroundCollision);
        GUI.Tooltip(LOC.Get("PROJECTILE_Ground_Collision_TT"));

        changed |= ImGui.Checkbox($"{LOC.Get("PROJECTILE_Simulate_Wall")}##simulateWall", ref cfg.ProjectileEditor.ProjectileEditor_SimulateWall);
        GUI.Tooltip(LOC.Get("PROJECTILE_Simulate_Wall_TT"));

        if (cfg.ProjectileEditor.ProjectileEditor_SimulateWall)
        {
            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##wallDistance", ref cfg.ProjectileEditor.ProjectileEditor_WallDistance, 0.5f, 100,
                $"{LOC.Get("PROJECTILE_Wall_Distance")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##wallWidth", ref cfg.ProjectileEditor.ProjectileEditor_WallWidth, 1, 50,
                $"{LOC.Get("PROJECTILE_Wall_Width")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##wallHeight", ref cfg.ProjectileEditor.ProjectileEditor_WallHeight, 1, 50,
                $"{LOC.Get("PROJECTILE_Wall_Height")}: %.1f m");
        }

        if (cfg.ProjectileEditor.ProjectileEditor_GroundCollision || cfg.ProjectileEditor.ProjectileEditor_SimulateWall)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo("##surface", ProjectileFormat.GetSurfaceName(cfg.ProjectileEditor.ProjectileEditor_Surface)))
            {
                foreach (var surface in new[] { ImpactSurface.Map, ImpactSurface.Water })
                {
                    if (ImGui.Selectable(ProjectileFormat.GetSurfaceName(surface), surface == cfg.ProjectileEditor.ProjectileEditor_Surface))
                    {
                        cfg.ProjectileEditor.ProjectileEditor_Surface = surface;
                        changed = true;
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("PROJECTILE_Surface_TT"));
        }

        changed |= ImGui.Checkbox($"{LOC.Get("PROJECTILE_Simulate_Target")}##simulateTarget", ref cfg.ProjectileEditor.ProjectileEditor_SimulateTarget);
        GUI.Tooltip(LOC.Get("PROJECTILE_Simulate_Target_TT"));

        if (cfg.ProjectileEditor.ProjectileEditor_SimulateTarget)
        {
            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.DragFloat("##targetX", ref cfg.ProjectileEditor.ProjectileEditor_TargetPosition.X, 0.1f, -200, 200,
                $"{LOC.Get("PROJECTILE_Target_Right")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.DragFloat("##targetY", ref cfg.ProjectileEditor.ProjectileEditor_TargetPosition.Y, 0.1f, -50, 200,
                $"{LOC.Get("PROJECTILE_Target_Height")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.DragFloat("##targetZ", ref cfg.ProjectileEditor.ProjectileEditor_TargetPosition.Z, 0.1f, -200, 200,
                $"{LOC.Get("PROJECTILE_Target_Forward")}: %.1f m");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##targetRadius", ref cfg.ProjectileEditor.ProjectileEditor_TargetRadius, 0.1f, 10,
                $"{LOC.Get("PROJECTILE_Target_Radius")}: %.1f m");

            changed |= DisplayTargetMotion();

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##behindTargetHeight", ref cfg.ProjectileEditor.ProjectileEditor_BehindTargetHeight, 0, 30,
                $"{LOC.Get("PROJECTILE_Behind_Target_Height")}: %.1f m");
            GUI.Tooltip(LOC.Get("PROJECTILE_Behind_Target_TT"));

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##behindTargetDistance", ref cfg.ProjectileEditor.ProjectileEditor_BehindTargetDistance, 0, 30,
                $"{LOC.Get("PROJECTILE_Behind_Target_Distance")}: %.1f m");
            GUI.Tooltip(LOC.Get("PROJECTILE_Behind_Target_TT"));
        }

        if (changed)
            Simulator.Invalidate();

        GUI.Spacer();
    }

    private static bool DisplayTargetMotion()
    {
        var changed = false;
        var cfg = CFG.Current;

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##targetMotion", $"{LOC.Get("PROJECTILE_Target_Motion")}: {ProjectileFormat.GetMotionName(cfg.ProjectileEditor.ProjectileEditor_TargetMotion)}"))
        {
            foreach (var motion in Enum.GetValues<TargetMotion>())
            {
                if (ImGui.Selectable(ProjectileFormat.GetMotionName(motion), motion == cfg.ProjectileEditor.ProjectileEditor_TargetMotion))
                {
                    cfg.ProjectileEditor.ProjectileEditor_TargetMotion = motion;
                    changed = true;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PROJECTILE_Target_Motion_TT"));

        if (cfg.ProjectileEditor.ProjectileEditor_TargetMotion is not TargetMotion.Still)
        {
            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##targetSpeed", ref cfg.ProjectileEditor.ProjectileEditor_TargetSpeed, 0.5f, 30,
                $"{LOC.Get("PROJECTILE_Target_Speed")}: %.1f m/s");

            ImGui.SetNextItemWidth(-1);
            changed |= ImGui.SliderFloat("##targetRange", ref cfg.ProjectileEditor.ProjectileEditor_TargetRange, 0.5f, 30,
                $"{LOC.Get("PROJECTILE_Target_Range")}: %.1f m");
        }

        return changed;
    }

    public void DisplayWarnings()
    {
        var result = Simulator.Result;

        if (result == null || result.Warnings == SimulationWarning.None && result.Ignored == BulletIgnoredBehavior.None)
            return;

        GUI.SimpleHeader(LOC.Get("PROJECTILE_Header_Warnings"), LOC.Get("PROJECTILE_Header_Warnings_TT"));

        Warning(result, SimulationWarning.ProjectileLimit, "PROJECTILE_Warning_Projectile_Limit", ProjectileSimulation.ProjectileLimit);
        Warning(result, SimulationWarning.TimeLimit, "PROJECTILE_Warning_Time_Limit", ProjectileSimulation.DurationLimit);
        Warning(result, SimulationWarning.MissingRow, "PROJECTILE_Warning_Missing_Row");
        Warning(result, SimulationWarning.SelfReference, "PROJECTILE_Warning_Self_Reference");
        Warning(result, SimulationWarning.AbsorbedCondition, "PROJECTILE_Warning_Absorbed_Condition");

        Ignored(result, BulletIgnoredBehavior.Homing, "PROJECTILE_Ignored_Homing");
        Ignored(result, BulletIgnoredBehavior.EmitPosition, "PROJECTILE_Ignored_Emit_Position");
        Ignored(result, BulletIgnoredBehavior.Following, "PROJECTILE_Ignored_Following");
        Ignored(result, BulletIgnoredBehavior.Arc, "PROJECTILE_Ignored_Arc");

        GUI.Spacer();
    }

    private static void Warning(SimulationResult result, SimulationWarning warning, string key, params object[] args)
    {
        if (result.Warnings.HasFlag(warning))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get(key, args));
    }

    private static void Ignored(SimulationResult result, BulletIgnoredBehavior behavior, string key)
    {
        if (result.Ignored.HasFlag(behavior))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get(key));
    }

    /// <summary>The bullets fired, by when; clicking one shows its row</summary>
    public void DisplayProjectiles(ProjectType projectType)
    {
        if (Simulator.Result is not { } result)
            return;

        GUI.SimpleHeader($"{LOC.Get("PROJECTILE_Header_Projectiles")} ({result.Tracks.Count})",
            LOC.Get("PROJECTILE_Header_Projectiles_TT"));

        if (ImGui.BeginChild("projectileList", default, ImGuiChildFlags.Borders))
        {
            var bulletParam = View.GetBulletParam();
            var paramName = Simulator.IsSpell
                ? ProjectileParams.GetParam(View.Project, "Bullet") != null ? "Bullet" : "BulletParam"
                : Simulator.ParamName;

            for (var i = 0; i < result.Tracks.Count; i++)
            {
                var track = result.Tracks[i];
                var color = ProjectileFormat.GetColor(track.Generation);

                if (!track.IsActive(Simulator.Time))
                    color.W = 0.5f;

                ImGui.PushStyleColor(ImGuiCol.Text, color);

                // Clicking one goes to the time it was fired
                if (ImGui.Selectable($"{track.SpawnTime:F2}s {ProjectileParams.RowLabel(track.RowId, track.Name)}##track{i}"))
                {
                    Simulator.Time = track.SpawnTime;
                    Simulator.Playing = false;
                }

                ImGui.PopStyleColor();

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(ProjectileFormat.DescribeTrack(result, track, projectType));

                if (paramName != null && bulletParam?[track.RowId] is { } row && ImGui.BeginPopupContextItem($"trackContext{i}"))
                {
                    if (ImGui.Selectable($"{LOC.Get("PROJECTILE_Show_Bullet")}##showBullet", false,
                            row != Simulator.Row ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                        View.Select(paramName, row);
                    GUI.Tooltip(LOC.Get("PROJECTILE_Show_Bullet_TT"));

                    if (ImGui.Selectable($"{LOC.Get("PROJECTILE_Open_In_Param_Editor")}##openInParamEditor"))
                        EditorCommandQueue.AddCommand($"param/select/-1/{paramName}/{row.ID}");

                    ImGui.EndPopup();
                }
            }
        }

        ImGui.EndChild();
    }
}
