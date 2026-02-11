using HKX2;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public class HavokCollisionResource : IResource, IDisposable
{
    public CollisionSubmesh[] GPUMeshes;
    public HKX Hkx;
    public hkRootLevelContainer Hkx2;
    public HKLib.hk2018.hkRootLevelContainer ER_HKX;

    public BoundingBox Bounds { get; set; }

    public VkFrontFace FrontFace { get; private set; }

    public bool IsConnectCollision = false;

    public bool _Load(Memory<byte> bytes, AccessLevel al, string virtPath)
    {
        if (Smithbox.Orchestrator.SelectedProject == null)
            return false;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if(virtPath.Contains("connect"))
        {
            IsConnectCollision = true;
        }

        // HKLib - ER
        if (curProject.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            var success = HandleHKLibLoad(curProject, virtPath);

            if (!success)
                return false;
        }
        // HKX2 - DS3
        else if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            DCX.Type t;
            Memory<byte> decomp = DCX.Decompress(bytes, out t);
            var br = new BinaryReaderEx(false, decomp);
            var des = new PackFileDeserializer();
            Hkx2 = (hkRootLevelContainer)des.Deserialize(br);
        }
        // HKX - BB
        else if (curProject.Descriptor.ProjectType is ProjectType.BB)
        {
            Hkx = HKX.Read(bytes, HKX.HKXVariation.HKXBloodBorne);
        }
        // HKX - DS2 / DS1
        else
        {
            Hkx = HKX.Read(bytes);
        }

        DetermineFrontFace(curProject);

        return HandleMeshLoad(curProject, al);
    }

    public bool _Load(string relativePath, AccessLevel al, string virtPath)
    {
        if (Smithbox.Orchestrator.SelectedProject == null)
            return false;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (virtPath.Contains("connect"))
        {
            IsConnectCollision = true;
        }

        // HKLib - ER
        if (curProject.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            var success = HandleHKLibLoad(curProject, virtPath);

            if (!success)
                return false;
        }
        // HKX2 - DS3
        else if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            try
            {
                var fileData = curProject.VFS.FS.ReadFile(relativePath);

                DCX.Type t;
                Memory<byte> decomp = DCX.Decompress(fileData.Value, out t);
                var br = new BinaryReaderEx(false, decomp);
                var des = new PackFileDeserializer();
                Hkx2 = (hkRootLevelContainer)des.Deserialize(br);
            }
            catch(Exception e)
            {
                Smithbox.LogError(this, $"[Smithbox] Failed to load {relativePath} during HavokCollisionResource load.", LogPriority.High, e);
            }
        }
        // HKX - BB
        else if (curProject.Descriptor.ProjectType is ProjectType.BB)
        {
            try
            {
                var fileData = curProject.VFS.FS.ReadFile(relativePath);

                Hkx = HKX.Read(fileData.Value, HKX.HKXVariation.HKXBloodBorne);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Smithbox] Failed to load {relativePath} during HavokCollisionResource load.", LogPriority.High, e);
            }
        }
        // HKX - DS2 / DS1
        else
        {
            try
            {
                var fileData = curProject.VFS.FS.ReadFile(relativePath);

                // Intercept and load the collision from PTDE FS for DS1R projects
                if(CFG.Current.MapEditor_Use_PTDE_Collisions_In_DS1R_Projects && curProject.Descriptor.ProjectType is ProjectType.DS1R)
                {
                    fileData = curProject.VFS.PTDE_FS.ReadFile(relativePath);
                }

                Hkx = HKX.Read(fileData.Value);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Smithbox] Failed to load {relativePath} during HavokCollisionResource load.", LogPriority.High, e);
            }
        }

        DetermineFrontFace(curProject);

        return HandleMeshLoad(curProject, al);
    }

    public bool HandleMeshLoad(ProjectEntry curProject, AccessLevel al)
    {
        if (curProject.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            return HKLib_Helper.LoadCollisionMesh(this, al);
        }
        else if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            return HKX2_Helper.LoadCollisionMesh(this, al);
        }
        else
        {
            return HKX_Helper.LoadCollisionMesh(this, al);
        }
    }

    public bool HandleHKLibLoad(ProjectEntry curProject, string virtPath)
    {
        if (curProject.Handler.MapEditor == null)
            return false;

        var activeView = curProject.Handler.MapEditor.ViewHandler.ActiveView;

        if(activeView == null) 
            return false;

        // Map collision
        var pathElements = virtPath.Split('/');
        var filename = pathElements[4];

        if (activeView.HavokCollisionBank.VisibleCollisionType is HavokCollisionType.Low)
        {
            filename = $"l{filename.Substring(1)}";
        }

        if (activeView.HavokCollisionBank.VisibleCollisionType is HavokCollisionType.High)
        {
            filename = $"h{filename.Substring(1)}";
        }

        if (activeView.HavokCollisionBank.VisibleCollisionType is HavokCollisionType.FallProtection)
        {
            filename = $"f{filename.Substring(1)}";
        }

        // HKX for ER is loaded directly in HavokCollisionManager
        // This is required since the parallel nature of the
        // Resource Manager doesn't work with the HavokBinarySerializer
        if (activeView.HavokCollisionBank.HavokContainers.ContainsKey(filename))
        {
            ER_HKX = activeView.HavokCollisionBank.HavokContainers[filename];
        }
        else
        {
            return false;
        }

        return true;
    }

    public void DetermineFrontFace(ProjectEntry curProject)
    {

        if (curProject.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3 or ProjectType.BB or ProjectType.ER or ProjectType.NR)
        {
            FrontFace = VkFrontFace.Clockwise;
        }
        else
        {
            FrontFace = VkFrontFace.CounterClockwise;
        }
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
            }

            if (GPUMeshes != null)
            {
                foreach (CollisionSubmesh m in GPUMeshes)
                {
                    m.GeomBuffer.Dispose();
                }
            }

            disposedValue = true;
        }
    }

    ~HavokCollisionResource()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

public class CollisionSubmesh
{
    public int IndexCount;
    public int[] PickingIndices;

    public Vector3[] PickingVertices;

    public VertexIndexBufferAllocator.VertexIndexBufferHandle GeomBuffer { get; set; }

    public int VertexCount { get; set; }
    public BoundingBox Bounds { get; set; }
}