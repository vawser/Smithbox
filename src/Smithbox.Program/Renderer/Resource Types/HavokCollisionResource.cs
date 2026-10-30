using HKX2;
using Org.BouncyCastle.Utilities;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public class HavokCollisionResource : IResource, IDisposable
{
    private static readonly object HKLibReadLock = new();

    public CollisionSubmesh[] GPUMeshes;
    public HKX Hkx;
    public hkRootLevelContainer Hkx2;
    public HKLib.hk2018.hkRootLevelContainer ER_HKX;

    public BoundingBox Bounds { get; set; }

    public VkFrontFace FrontFace { get; private set; }

    public bool IsConnectCollision = false;

    public bool _Load(Memory<byte> bytes, AccessLevel al, ResourceLoadContext ctx)
    {
        if (Smithbox.Orchestrator.SelectedProject == null)
            return false;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (ctx.IsConnectCollision)
        {
            IsConnectCollision = true;
        }

        // HKLib - ER
        if (curProject.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            var success = IsAssetCollision(ctx)
                ? HandleHKLibAssetLoad(bytes)
                : HandleHKLibLoad(curProject, ctx);

            if (!success)
                return false;
        }
        // HKX2 - DS3
        else if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            if(DCX.Is(bytes))
            {
                DCX.Type t;
                Memory<byte> decomp = DCX.Decompress(bytes, out t);
                var br = new BinaryReaderEx(false, decomp);
                var des = new PackFileDeserializer();
                Hkx2 = (hkRootLevelContainer)des.Deserialize(br);
            }
            else
            {
                var br = new BinaryReaderEx(false, bytes);
                var des = new PackFileDeserializer();
                Hkx2 = (hkRootLevelContainer)des.Deserialize(br);
            }
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

    public bool _Load(string relativePath, AccessLevel al, ResourceLoadContext ctx)
    {
        if (Smithbox.Orchestrator.SelectedProject == null)
            return false;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (ctx.IsConnectCollision)
        {
            IsConnectCollision = true;
        }

        // HKLib - ER
        if (curProject.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            var success = HandleHKLibLoad(curProject, ctx);

            if (!success)
                return false;
        }
        // HKX2 - DS3
        else if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            try
            {
                var fileData = curProject.VFS.FS.ReadFile(relativePath);

                if (DCX.Is(fileData.Value))
                {
                    DCX.Type t;
                    Memory<byte> decomp = DCX.Decompress(fileData.Value, out t);
                    var br = new BinaryReaderEx(false, decomp);
                    var des = new PackFileDeserializer();
                    Hkx2 = (hkRootLevelContainer)des.Deserialize(br);
                }
                else
                {
                    var br = new BinaryReaderEx(false, fileData.Value);
                    var des = new PackFileDeserializer();
                    Hkx2 = (hkRootLevelContainer)des.Deserialize(br);
                }
            }
            catch(Exception e)
            {
                Smithbox.LogError(this, 
                    LOC.Get("REND_HavokCollisionResource_Load_Failed", relativePath), e);
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
                Smithbox.LogError(this,
                    LOC.Get("REND_HavokCollisionResource_Load_Failed", relativePath), e);
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
                Smithbox.LogError(this,
                    LOC.Get("REND_HavokCollisionResource_Load_Failed", relativePath), e);
            }
        }

        DetermineFrontFace(curProject);

        return HandleMeshLoad(curProject, al);
    }

    public bool HandleMeshLoad(ProjectEntry curProject, AccessLevel al)
    {
        if (curProject.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
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

    private static bool IsAssetCollision(ResourceLoadContext ctx)
    {
        return ctx.ContainerPath.Contains(".geomhkxbnd", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asset collision isn't tied to a loaded map, so it isn't held by the HavokCollisionBank and is read here.
    /// The HavokBinarySerializer doesn't work with the parallel nature of the Resource Manager, so only one read
    /// is done at a time.
    /// </summary>
    public bool HandleHKLibAssetLoad(Memory<byte> bytes)
    {
        var data = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;

        using var stream = new MemoryStream(data.ToArray());

        lock (HKLibReadLock)
        {
            var serializer = new HKLib.Serialization.hk2018.Binary.HavokBinarySerializer();

            ER_HKX = (HKLib.hk2018.hkRootLevelContainer)serializer.Read(stream);
        }

        return true;
    }

    public bool HandleHKLibLoad(ProjectEntry curProject, ResourceLoadContext ctx)
    {
        if (curProject.Handler.MapEditor == null)
            return false;

        var activeView = curProject.Handler.MapEditor.ViewHandler.ActiveView;

        if(activeView == null) 
            return false;

        // Map collision
        var filename = ctx.FileName;

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

        if (curProject.Descriptor.ProjectType is ProjectType.DS2S 
            or ProjectType.DS2 
            or ProjectType.DS3 
            or ProjectType.BB 
            or ProjectType.ER 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
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