using Havok;
using Havok.Files;
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
    public CollisionSubmesh[] GPUMeshes;

    /// <summary>The collision as the unified Havok library read it (every game but DS2).</summary>
    public HavokDocument Document;

    /// <summary>DS2's collision, which only SoulsFormats reads.</summary>
    public HKX Hkx;

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

        return Load(Smithbox.Orchestrator.SelectedProject, bytes, al, ctx);
    }

    public bool _Load(string relativePath, AccessLevel al, ResourceLoadContext ctx)
    {
        if (Smithbox.Orchestrator.SelectedProject == null)
            return false;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        Memory<byte>? fileData;
        try
        {
            fileData = curProject.VFS.FS.ReadFile(relativePath);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("REND_HavokCollisionResource_Load_Failed", relativePath), e);
            return false;
        }

        return fileData != null && Load(curProject, fileData.Value, al, ctx);
    }

    /// <summary>
    /// Every game but DS2 is read by the unified Havok library, whatever the container: the file says which SDK it
    /// is (DS1R's own collision is 2015 tagfiles). Map collision of the tagfile games refers to the compendium of
    /// its binder for its types.
    /// </summary>
    private bool Load(ProjectEntry curProject, Memory<byte> bytes, AccessLevel al, ResourceLoadContext ctx)
    {
        if (ctx.IsConnectCollision)
        {
            IsConnectCollision = true;
        }

        DetermineColor(ctx);
        DetermineFrontFace(curProject);

        // DS2 is hk_2012 32-bit, which only SoulsFormats reads
        if (curProject.Descriptor.ProjectType is ProjectType.DS2)
        {
            try
            {
                Hkx = HKX.Read(bytes);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("REND_HavokCollisionResource_Load_Failed", ctx.FileName), e);
                return false;
            }

            return HKX_Helper.LoadCollisionMesh(this, al);
        }

        // Sekiro's map collision is read with its binder's compendium when the map loads
        if (curProject.Descriptor.ProjectType is ProjectType.SDT)
        {
            Document = curProject.Handler.MapEditor?.ViewHandler.ActiveView?.HavokCollisionBank.GetTagDocument(ctx.FileName);
        }

        if (Document == null)
        {
            try
            {
                HavokProfile profile = HavokProfile.FromGame(curProject.Descriptor.ProjectType.ToString());
                if (profile == null)
                    return false;

                var data = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;
                Document = HavokFile.Read(data.ToArray(), profile, ctx.Compendium?.Unified(profile));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("REND_HavokCollisionResource_Load_Failed", ctx.FileName), e);
                return false;
            }
        }

        return HavokCollisionMeshBuilder.LoadCollisionMesh(this, al);
    }

    private static bool IsAssetCollision(ResourceLoadContext ctx)
    {
        return ctx.ContainerPath.Contains(".geomhkxbnd", StringComparison.OrdinalIgnoreCase);
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