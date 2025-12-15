using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static SoulsFormats.AIP;

namespace StudioCore.Editors.MapEditor;

public class AutoInvadeManager
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, AIP> Files = new();

    public AutoInvadeManager(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Setup();
    }

    public bool CanUse()
    {
        if (Project.ProjectType is ProjectType.ER)
            return true;

        return false;
    }

    public void Setup()
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.MapData.AutoInvadeBinders.Entries)
        {
            try
            {
                var binderData = Project.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);

                try
                {
                    var binder = BND4.Read(binderData);

                    foreach(var file in binder.Files)
                    {
                        try
                        {
                            var aipData = AIP.Read(file.Bytes);

                            Files.Add(Path.GetFileNameWithoutExtension(file.Name), aipData);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {file.Name} as AIP", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} as AIPBND", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} from VFS", LogLevel.Error, LogPriority.High, e);
            }
        }
    }

    public void LoadAIP(MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Files)
        {
            if (entry.Key == map.Name)
            {
                var aip = entry.Value;

                if (aip != null)
                {
                    map.LoadAIP(map.Name, aip);
                }
            }
        }
    }
    public void SaveAIP(MapEditorScreen editor, MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.MapData.AutoInvadeBinders.Entries)
        {
            // File
            try
            {
                var binderData = Project.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);

                try
                {
                    var applyEdit = false;
                    var binder = BND4.Read(binderData);

                    foreach (var file in binder.Files)
                    {
                        var mapID = Path.GetFileNameWithoutExtension(file.Name);

                        if (map.Name == mapID)
                        {
                            var existingAIP = AIP.Read(file.Bytes);

                            existingAIP.Points = new();

                            foreach (var point in map.AutoInvadeParent.Children)
                            {
                                var existingPointInstance = (AutoInvadePointInstance)point.WrappedObject;

                                existingAIP.Points.Add(existingPointInstance);
                            }

                            var newBytes = existingAIP.Write();

                            // Only apply edit if the AIP data has changed
                            // So we don't add the autoinvadepoint.aipbnd.dcx file
                            // pointlessly to a user's mod if they haven't edited anything
                            if (!BytePerfectHelper.Md5Equal(file.Bytes.Span, newBytes))
                            {
                                applyEdit = true;
                                file.Bytes = newBytes;
                            }
                        }
                    }

                    if (applyEdit)
                    {
                        var binderOutput = binder.Write();
                        Project.ProjectFS.WriteFile(entry.Path, binderOutput);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to write {entry.Path} as AIPBND", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} from VFS", LogLevel.Error, LogPriority.High, e);
            }
        }
    }

    public Quaternion FromEulerDegrees(float pitchDeg, float yawDeg, float rollDeg)
    {
        float pitch = MathF.PI / 180f * pitchDeg;
        float yaw = MathF.PI / 180f * yawDeg;
        float roll = MathF.PI / 180f * rollDeg;

        float cy = MathF.Cos(yaw * 0.5f);
        float sy = MathF.Sin(yaw * 0.5f);
        float cp = MathF.Cos(pitch * 0.5f);
        float sp = MathF.Sin(pitch * 0.5f);
        float cr = MathF.Cos(roll * 0.5f);
        float sr = MathF.Sin(roll * 0.5f);

        Quaternion q;
        q.W = cr * cp * cy + sr * sp * sy;
        q.X = sr * cp * cy - cr * sp * sy;
        q.Y = cr * sp * cy + sr * cp * sy;
        q.Z = cr * cp * sy - sr * sp * cy;

        return q;
    }

    public void OnUnloadMap(string mapId)
    {
        if (Project.ProjectType != ProjectType.ER)
            return;
    }

}
