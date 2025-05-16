#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.FileBrowserNS;

public class FlverFsEntry : SoulsFileFsEntry
{
    internal bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => false;
    public override bool CanView => true;
    public override List<FsEntry> Children => [];

    internal Func<Memory<byte>> getDataFunc;
    internal Memory<byte>? data = null;
    private FLVER2? flver = null;

    public FlverFsEntry(string name, Func<Memory<byte>> getDataFunc)
    {
        this.name = name;
        this.getDataFunc = getDataFunc;
    }

    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        flver = FLVER2.Read(data.Value);
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        flver = null;
        data = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"FLVER File {name}");

        if (flver == null)
            return;

        PropertyTable("FLVER Data", (row) =>
        {
            row("Compression", flver.Compression.ToString());
            row("Header.BigEndian", flver.Header.BigEndian.ToString());
            row("Header.Version", flver.Header.Version.ToString());
            row("Header.BoundingBoxMin", flver.Header.BoundingBoxMin.ToString());
            row("Header.BoundingBoxMax", flver.Header.BoundingBoxMax.ToString());
            row("Header.Unicode", flver.Header.Unicode.ToString());
            row("Header.Unk4A", flver.Header.Unk4A.ToString());
            row("Header.Unk4C", flver.Header.Unk4C.ToString());
            row("Header.Unk5C", flver.Header.Unk5C.ToString());
            row("Header.Unk5D", flver.Header.Unk5D.ToString());
            row("Header.Unk68", flver.Header.Unk68.ToString());
            row("Header.Unk74", flver.Header.Unk74.ToString());
        });
        if (ImGui.CollapsingHeader("FLVER Dummies"))
        {
            ImGui.TreePush("FLVER Dummies##TreePush");
            foreach (var (i, dummy) in flver.Dummies.Select((d, i) => (i, d)))
            {
                if (ImGui.CollapsingHeader($"Dummy {i}"))
                {
                    ImGui.TreePush($"Dummy {i}##TreePush");
                    PropertyTable($"Dummy {i} Table", (row) =>
                    {
                        row("Position", dummy.Position.ToString());
                        row("Forward", dummy.Forward.ToString());
                        row("Upward", dummy.Upward.ToString());
                        row("ReferenceID", dummy.ReferenceID.ToString());
                        row("ParentBoneIndex", dummy.ParentBoneIndex.ToString());
                        row("AttachBoneIndex", dummy.AttachBoneIndex.ToString());
                        row("Color", dummy.Color.ToString());
                        row("Flag1", dummy.Flag1.ToString());
                        row("UseUpwardVector", dummy.UseUpwardVector.ToString());
                        row("Unk30", dummy.Unk30.ToString());
                        row("Unk34", dummy.Unk34.ToString());
                    });
                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.CollapsingHeader("FLVER Materials"))
        {
            ImGui.TreePush("FLVER Materials##TreePush");
            foreach (var (i, material) in flver.Materials.Select((d, i) => (i, d)))
            {
                if (ImGui.CollapsingHeader($"Material {i} ({material.Name})"))
                {
                    ImGui.TreePush($"Material {i}##TreePush");
                    PropertyTable($"Material {i} Table", (row) =>
                    {
                        row("Name", material.Name);
                        row("MTD", material.MTD);
                        row("GXIndex", material.GXIndex.ToString());
                        row("Index", material.Index.ToString());
                    });
                    foreach (var (j, texture) in material.Textures.Select((t, j) => (j, t)))
                    {
                        if (ImGui.CollapsingHeader($"Texture {j} ({texture.Type}: \"{texture.Path}\")##Material_{i} "))
                        {
                            ImGui.TreePush($"Material {i} Texture {j} ({texture.Path})##TreePush");
                            PropertyTable($"Material {i} Texture {j} Table", (row) =>
                            {
                                row("Type", texture.Type);
                                row("Path", texture.Path);
                                row("Scale", texture.Scale.ToString());
                                row("Unk10", texture.Unk10.ToString());
                                row("Unk11", texture.Unk11.ToString());
                                row("Unk14", texture.Unk14.ToString());
                                row("Unk18", texture.Unk18.ToString());
                                row("Unk1C", texture.Unk1C.ToString());
                            });
                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.CollapsingHeader("FLVER GXLists"))
        {
            ImGui.TreePush("FLVER GXLists##TreePush");
            foreach (var (i, gxlist) in flver.GXLists.Select((d, i) => (i, d)))
            {
                if (ImGui.CollapsingHeader($"GXList {i}"))
                {
                    ImGui.TreePush($"GXList {i}##TreePush");
                    PropertyTable($"GXList {i} Table", (row) =>
                    {
                        row("TerminatorID", gxlist.TerminatorID.ToString());
                        row("TerminatorLength", gxlist.TerminatorLength.ToString());
                    });
                    foreach (var (j, gxitem) in gxlist.Select((g, j) => (j, g)))
                    {
                        if (ImGui.CollapsingHeader($"GXItem {i} ({gxitem.ID})"))
                        {
                            ImGui.TreePush($"GXList {i} GXItem {j}##TreePush");
                            PropertyTable($"GXList {i} GXItem {j} Property Table", (row) =>
                            {
                                row("ID", gxitem.ID);
                                row("Unk04", gxitem.Unk04.ToString());
                            });
                            ImGui.TextWrapped($"Data: {string.Join(", ", gxitem.Data)}");
                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        //TODO: Rest of FLVER
        ImGui.Text("TODO: Rest of FLVER");
    }
}