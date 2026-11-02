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
    public HKLib.hk2019.hkRootLevelContainer AC6_HKX;

    public BoundingBox Bounds { get; set; }

    public VkFrontFace FrontFace { get; private set; }

    public bool IsConnectCollision = false;

    /// <summary>
    /// The color the mesh is drawn in, which tells the collision types apart when several are displayed.
    /// </summary>
    public Vector3 Color;

    public bool _Load(Memory<byte> bytes, AccessLevel al, ResourceLoadContext ctx)
    {
        if (Smithbox.Orchestrator.SelectedProject == null)
            return false;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (ctx.IsConnectCollision)
        {
            IsConnectCollision = true;
        }

        DetermineColor(ctx);

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
        // HKLib 2019 - AC6
        else if (curProject.Descriptor.ProjectType is ProjectType.AC6)
        {
            var success = IsAssetCollision(ctx)
                ? HandleHKLib2019AssetLoad(bytes)
                : HandleHKLib2019Load(curProject, ctx);

            if (!success)
                return false;
        }
        // HKX2 tagfile - SDT
        else if (curProject.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (!HandleTagLoad(curProject, ctx, bytes))
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

        DetermineColor(ctx);

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
        // HKLib 2019 - AC6
        else if (curProject.Descriptor.ProjectType is ProjectType.AC6)
        {
            if (!HandleHKLib2019Load(curProject, ctx))
                return false;
        }
        // HKX2 tagfile - SDT
        else if (curProject.Descriptor.ProjectType is ProjectType.SDT)
        {
            try
            {
                var fileData = curProject.VFS.FS.ReadFile(relativePath);

                if (!HandleTagLoad(curProject, ctx, fileData.Value))
                    return false;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this,
                    LOC.Get("REND_HavokCollisionResource_Load_Failed", relativePath), e);
                return false;
            }
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
        else if (curProject.Descriptor.ProjectType is ProjectType.AC6)
        {
            return HKLib2019_Helper.LoadCollisionMesh(this, al);
        }
        else if (curProject.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT)
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

    /// <summary>
    /// AC6 asset collision holds its own types, unlike map collision which refers to a compendium.
    /// </summary>
    public bool HandleHKLib2019AssetLoad(Memory<byte> bytes)
    {
        var data = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;

        using var stream = new MemoryStream(data.ToArray());

        lock (HKLibReadLock)
        {
            var serializer = new HKLib.Serialization.hk2019.Binary.HavokBinarySerializer();

            AC6_HKX = (HKLib.hk2019.hkRootLevelContainer)serializer.Read(stream);
        }

        return true;
    }

    /// <summary>
    /// AC6 map collision refers to a compendium in its binder, so it is read by the HavokCollisionBank when the map
    /// is loaded.
    /// </summary>
    public bool HandleHKLib2019Load(ProjectEntry curProject, ResourceLoadContext ctx)
    {
        var activeView = curProject.Handler.MapEditor?.ViewHandler.ActiveView;

        AC6_HKX = activeView?.HavokCollisionBank.GetAC6Container(ctx.FileName);

        return AC6_HKX != null;
    }

    /// <summary>
    /// Sekiro collision is a tagfile. Map collision refers to a compendium in its binder, so it is read by the
    /// HavokCollisionBank when the map is loaded. Anything else (e.g. object collision) holds its own types.
    /// </summary>
    public bool HandleTagLoad(ProjectEntry curProject, ResourceLoadContext ctx, Memory<byte> bytes)
    {
        var activeView = curProject.Handler.MapEditor?.ViewHandler.ActiveView;

        Hkx2 = activeView?.HavokCollisionBank.GetTagContainer(ctx.FileName);

        if (Hkx2 != null)
            return true;

        var data = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;

        if (!TagFileDeserializer.IsTagFile(data.Span))
            return false;

        try
        {
            Hkx2 = new TagFileDeserializer().Deserialize(data.ToArray()) as hkRootLevelContainer;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("REND_HavokCollisionResource_Load_Failed", ctx.FileName), e);
            return false;
        }

        return Hkx2 != null;
    }

    public bool HandleHKLibLoad(ProjectEntry curProject, ResourceLoadContext ctx)
    {
        if (curProject.Handler.MapEditor == null)
            return false;

        var activeView = curProject.Handler.MapEditor.ViewHandler.ActiveView;

        if(activeView == null) 
            return false;

        // Map collision: each type is read from its own binder, so the
        // file name already carries the prefix of its type (h, l or f)
        var filename = ctx.FileName;

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

    /// <summary>
    /// Map collision files are prefixed by their type (h, l or f). Anything else,
    /// e.g. object and asset collision, uses the base collision color.
    /// </summary>
    private void DetermineColor(ResourceLoadContext ctx)
    {
        Color = CFG.Current.Viewport_Collision_Color;

        if (IsConnectCollision)
        {
            Color = CFG.Current.Viewport_Connect_Collision_Color;
            return;
        }

        if (IsAssetCollision(ctx) || ctx.FileName.Length == 0)
            return;

        switch (char.ToLowerInvariant(ctx.FileName[0]))
        {
            case 'h':
                Color = CFG.Current.Viewport_High_Collision_Color;
                break;
            case 'f':
                Color = CFG.Current.Viewport_FallProtection_Collision_Color;
                break;
        }
    }

    public void DetermineFrontFace(ProjectEntry curProject)
    {

        if (curProject.Descriptor.ProjectType is ProjectType.DS2S 
            or ProjectType.DS2 
            or ProjectType.DS3
            or ProjectType.SDT
            or ProjectType.BB
            or ProjectType.ER
            or ProjectType.AC6
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