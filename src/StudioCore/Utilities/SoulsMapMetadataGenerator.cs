using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

public static class SoulsMapMetadataGenerator
{
    public static float MCEpsilon = 0.001f;
    private class RoomInfo
    {
        public int roomId = -1; //Id of the NVM itself
        public uint mapId = 0;
        public Vector3 BoundingBoxMax;
        public Vector3 BoundingBoxMin;
        public NVM nvm;
        public List<RoomInfo> adjacentRooms = new List<RoomInfo>();
        public List<Gate> gates = new List<Gate>();
    }

    private class Gate
    {
        public List<NVM.Triangle> tris = new List<NVM.Triangle>();
        public List<int> uniqueVertIds = new List<int>();
        public Vector3 BoundingBoxMax;
        public Vector3 BoundingBoxMin;
        public List<MCG.Node> nodes = new List<MCG.Node>(); //Really should only ever be one, but w/e
        public List<Gate> touchedGates = new List<Gate>();

    }

    public class MCCombo
    {
        public MCP mcp = new MCP();
        public MCG mcg = new MCG();

        public MCCombo(bool bigEndian = true)
        {
            mcp.BigEndian = bigEndian;
            mcg.BigEndian = bigEndian;
        }
    }

    public static void GenerateMCGMCP(List<string> directories, AssetLocator _assetLocator, bool toBigEndian = true)
    {
        string baseDirectory = _assetLocator.GameRootDirectory;
        string modDirectory = _assetLocator.GameModDirectory;
        Dictionary<string, MCCombo> mcCombos = new Dictionary<string, MCCombo>();

        //Gather NVM files and filter by MSB 
        Dictionary<string, List<NVM>> nvmDict = new Dictionary<string, List<NVM>>();
        foreach (var dir in directories)
        {
            //Get the MSB to see what navmeshes are being used
            var name = Path.GetFileName(dir);
            mcCombos.Add(name, new MCCombo());
            var msbPath = Path.GetDirectoryName(dir) + $"\\mapstudio\\{name}.msb";

            List<string> msbNavmeshNames = new List<string>();
            if (File.Exists(msbPath))
            {
                if(_assetLocator.Type == GameType.DemonsSouls)
                {
                    var msb = SoulsFile<MSBD>.Read(msbPath);
                    foreach (var navMesh in msb.Parts.Navmeshes)
                    {
                        msbNavmeshNames.Add(navMesh.ModelName.ToLower());
                    }
                } else if (_assetLocator.Type is GameType.DarkSoulsPTDE or GameType.DarkSoulsRemastered)
                {
                    var msb = SoulsFile<MSB1>.Read(msbPath);
                    foreach (var navMesh in msb.Parts.Navmeshes)
                    {
                        msbNavmeshNames.Add(navMesh.ModelName.ToLower());
                    }
                }
            }

            //Get the nvms from the nvmBnd, fall back to loose .nvms if non-existant
            List<NVM> nvmList = new List<NVM>();
            var nvmBnd = Path.Combine(dir, $"{name}.nvmbnd");
            if (File.Exists(nvmBnd))
            {
                var nvmBndFile = new BND3Reader(nvmBnd);
                foreach (var nvmFile in nvmBndFile.Files)
                {
                    var fname = Path.GetFileNameWithoutExtension(nvmFile.Name).ToLower();
                    if(_assetLocator.Type is GameType.DarkSoulsRemastered or GameType.DarkSoulsPTDE)
                    {
                        fname = fname.Substring(0, 7);
                    }
                    if (Path.GetExtension(nvmFile.Name) == ".nvm" && msbNavmeshNames.Contains(fname))
                    {
                        var nvm = SoulsFile<NVM>.Read(nvmBndFile.ReadFile(nvmFile));
                        nvmList.Add(nvm);
                    }
                }
            }
            else
            {
                var files = Directory.EnumerateFiles(dir, "*.nvm");
                foreach (var file in files)
                {
                    var fname = Path.GetFileNameWithoutExtension(file);
                    if (msbNavmeshNames.Contains(fname))
                    {
                        var nvm = SoulsFile<NVM>.Read(file);
                        nvmList.Add(nvm);
                    }
                }
            }
            nvmDict.Add(dir, nvmList);
        }

        //Process NVM data into what we need for MCG and MCP stuff
        //We need this calculated before we devise the result, hence the separate loop
        Dictionary<string, List<RoomInfo>> triDicts = new Dictionary<string, List<RoomInfo>>();
        foreach (var dir in directories)
        {
            var nvmList = nvmDict[dir];
            List<RoomInfo> nvmTriList = new List<RoomInfo>();

            var roomNums = Path.GetFileName(dir).Substring(1).Split('_');
            uint mapId = BitConverter.ToUInt32(new byte[] { Byte.Parse(roomNums[3]), Byte.Parse(roomNums[2]), Byte.Parse(roomNums[1]), Byte.Parse(roomNums[0]) }, 0);
            foreach (var nvm in nvmList)
            {
                RoomInfo roomInfo = new RoomInfo();
                roomInfo.nvm = nvm;

                //Set bounding from nvm
                roomInfo.BoundingBoxMax = nvm.RootBox.MaxValueCorner;
                roomInfo.BoundingBoxMin = nvm.RootBox.MinValueCorner;

                //Add and subtract 1 from Y bounding for the corners. Demon's Souls does this for w/e reason so I'm doing it
                roomInfo.BoundingBoxMax.Y += 1;
                roomInfo.BoundingBoxMin.Y -= 1;

                roomInfo.roomId = nvmTriList.Count;
                roomInfo.mapId = mapId;
                List<int> usedIndices = new List<int>();
                //Gather Gate triangles
                for (int i = 0; i < nvm.Triangles.Count; i++)
                {
                    var triSet = new List<NVM.Triangle>();
                    CompileGateTriSet(triSet, usedIndices, nvm, i);
                    if (triSet.Count > 0)
                    {
                        Gate gate = new Gate();
                        gate.tris = triSet;
                        gate.uniqueVertIds = GetUniqueVertIds(triSet);
                        Vector3 BoundingBoxMax = nvm.Vertices[gate.uniqueVertIds[0]];
                        Vector3 BoundingBoxMin = nvm.Vertices[gate.uniqueVertIds[0]];
                        foreach (var id in gate.uniqueVertIds)
                        {
                            var vert = nvm.Vertices[id];
                            //Min extents
                            if (BoundingBoxMin.X > vert.X)
                            {
                                BoundingBoxMin.X = vert.X;
                            }
                            if (BoundingBoxMin.Y > vert.Y)
                            {
                                BoundingBoxMin.Y = vert.Y;
                            }
                            if (BoundingBoxMin.Z > vert.Z)
                            {
                                BoundingBoxMin.Z = vert.Z;
                            }

                            //Max extents
                            if (BoundingBoxMax.X < vert.X)
                            {
                                BoundingBoxMax.X = vert.X;
                            }
                            if (BoundingBoxMax.Y < vert.Y)
                            {
                                BoundingBoxMax.Y = vert.Y;
                            }
                            if (BoundingBoxMax.Z < vert.Z)
                            {
                                BoundingBoxMax.Z = vert.Z;
                            }
                        }
                        gate.BoundingBoxMax = BoundingBoxMax;
                        gate.BoundingBoxMin = BoundingBoxMin;
                        roomInfo.gates.Add(gate);
                    }
                }
                nvmTriList.Add(roomInfo);
            }
            triDicts.Add(dir, nvmTriList);
        }

        //Create MCP and MCG, presumably Map Container Portals and Map Container Gates, or something along those lines
        //Bounds in MCP should be the same as the bounds in each individual .nvm
        //Bounds should be used to narrow down NVM comparisons

        //Create nodes
        List<MCG.Node> nodes = new List<MCG.Node>();
        foreach (var dir in directories)
        {
            var nvmTriList = triDicts[dir];
            for (int roomInfoId = 0; roomInfoId < nvmTriList.Count; roomInfoId++)
            {
                var roomInfo = nvmTriList[roomInfoId];
                foreach (var gate in roomInfo.gates)
                {
                    //Loop through all other gathered nvms and create nodes as appropriate
                    foreach (var dir2 in directories)
                    {
                        var nvmTriList2 = triDicts[dir2];
                        for (int roomInfo2Id = 0; roomInfo2Id < nvmTriList2.Count; roomInfo2Id++)
                        {
                            var roomInfo2 = nvmTriList2[roomInfo2Id];
                            //Proceed if the nvms overlap or touch and this isn't the current nvm
                            if (roomInfo != roomInfo2 && StudioMath.BoundsIntersect(gate.BoundingBoxMax, gate.BoundingBoxMin, roomInfo2.BoundingBoxMax, roomInfo2.BoundingBoxMin, MCEpsilon))
                            {
                                foreach (var gate2 in roomInfo2.gates)
                                {
                                    //Proceed if a gate in the nvm overlaps or touches the original gate
                                    if (!gate.touchedGates.Contains(gate2) && StudioMath.BoundsIntersect(gate.BoundingBoxMax, gate.BoundingBoxMin, gate2.BoundingBoxMax, gate2.BoundingBoxMin, MCEpsilon))
                                    {
                                        gate.touchedGates.Add(gate2);
                                        gate2.touchedGates.Add(gate);

                                        Vector3 avg = new Vector3();
                                        foreach (var id in gate.uniqueVertIds)
                                        {
                                            avg += roomInfo.nvm.Vertices[id];
                                        }
                                        foreach (var id in gate2.uniqueVertIds)
                                        {
                                            avg += roomInfo2.nvm.Vertices[id];
                                        }

                                        avg /= (gate.uniqueVertIds.Count + gate2.uniqueVertIds.Count);

                                        var node = new MCG.Node();
                                        node.Position = avg;
                                        node.Unk18 = 0;
                                        node.Unk1C = 0;

                                        //Fill in nodes and 'edge' indices later in process
                                        gate.nodes.Add(node);
                                        gate2.nodes.Add(node);
                                        if (!roomInfo.adjacentRooms.Contains(roomInfo2))
                                        {
                                            roomInfo.adjacentRooms.Add(roomInfo2);
                                            roomInfo2.adjacentRooms.Add(roomInfo);
                                        }
                                        nodes.Add(node);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Add the node sets to the files
        foreach (var dir in directories)
        {
            var name = Path.GetFileName(dir);
            mcCombos[name].mcg.Nodes = GetClonedNodeList(nodes);
        }

        //Set up edges
        Dictionary<string, List<MCG.Edge>> tempEdgeDict = new Dictionary<string, List<MCG.Edge>>();
        foreach (var dir in directories)
        {
            List<MCG.Edge> edgeList = new List<MCG.Edge>();
            var nvmTriList = triDicts[dir];
            foreach (var roomInfo in nvmTriList)
            {
                List<string> gatePairs = new List<string>();
                foreach (var gate in roomInfo.gates)
                {
                    //Iterate through gates of the same room to link paths
                    foreach (var gate2 in roomInfo.gates)
                    {
                        var id0 = roomInfo.gates.IndexOf(gate);
                        var id1 = roomInfo.gates.IndexOf(gate2);
                        if (gate2 != gate && gate.nodes.Count > 0 && gate2.nodes.Count > 0 && gate.nodes?[0] != gate2.nodes?[0] && !gatePairs.Contains($"{id0}_{id1}"))
                        {
                            gatePairs.Add($"{id0}_{id1}");
                            gatePairs.Add($"{id1}_{id0}");
                            MCG.Edge edge = new MCG.Edge();
                            edge.MapID = roomInfo.mapId;
                            edge.MCPRoomIndex = roomInfo.roomId;
                            var node0Id = nodes.IndexOf(gate.nodes[0]);
                            var node1Id = nodes.IndexOf(gate2.nodes[0]);
                            if (node0Id > node1Id)
                            {
                                edge.NodeIndexA = node1Id;
                                edge.NodeIndexB = node0Id;
                            }
                            else
                            {
                                edge.NodeIndexA = node0Id;
                                edge.NodeIndexB = node1Id;
                            }
                            edge.Unk20 = Vector3.Distance(gate.nodes[0].Position, gate2.nodes[0].Position);

                            //Node connection data will be added later per mcg

                            //Apparently these can just be empty and the game is chill with it. Adding bad values here is a crash so best left alone for now.
                            edge.UnkIndicesA = new List<int>();
                            edge.UnkIndicesB = new List<int>();
                            edgeList.Add(edge);
                        }
                    }
                }
            }
            tempEdgeDict.Add(dir, edgeList);
        }

        //Set edges to mcg files (alter per order per file)
        foreach (var dir in directories)
        {
            List<int> usedNodeList = new List<int>();
            var name = Path.GetFileName(dir);
            ApplyEdgeSet(mcCombos[name].mcg, tempEdgeDict[dir], usedNodeList, 0);
            int roomCount = triDicts[dir].Count;
            foreach (var pair in tempEdgeDict)
            {
                if (pair.Key == dir)
                {
                    continue;
                }
                ApplyEdgeSet(mcCombos[name].mcg, pair.Value, usedNodeList, roomCount);
                roomCount += triDicts[pair.Key].Count;
            }
            usedNodeList.Sort();
            var mcgOut = (dir + $"\\{Path.GetFileName(dir)}.mcg").Replace(baseDirectory, modDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(mcgOut));
            mcCombos[name].mcg.BigEndian = toBigEndian;
            mcCombos[name].mcg.Write(mcgOut);
        }

        //Set up mcp files
        for (int i = 0; i < directories.Count; i++)
        {
            var dir = directories[i];
            var name = Path.GetFileName(dir);

            //We want one set of these per directory and each of those should start with only rooms in that directory
            List<RoomInfo> tempRooms = new List<RoomInfo>();
            var nvmTriList = triDicts[dir];
            foreach (var room in nvmTriList)
            {
                tempRooms.Add(room);
            }

            //Go through the other rooms
            for (int j = 0; j < directories.Count; j++)
            {
                if (j == i)
                {
                    continue;
                }
                nvmTriList = triDicts[directories[j]];
                foreach (var room in nvmTriList)
                {
                    tempRooms.Add(room);
                }
            }

            //Process into MCP Rooms
            foreach (var room in tempRooms)
            {
                var mcpRoom = new MCP.Room();
                mcpRoom.LocalIndex = room.roomId;
                mcpRoom.BoundingBoxMax = room.BoundingBoxMax;
                mcpRoom.BoundingBoxMin = room.BoundingBoxMin;
                mcpRoom.MapID = room.mapId;

                foreach (var adjRoom in room.adjacentRooms)
                {
                    mcpRoom.ConnectedRoomIndices.Add(tempRooms.IndexOf(adjRoom));
                }
                mcpRoom.ConnectedRoomIndices.Sort();
                mcCombos[name].mcp.Rooms.Add(mcpRoom);
            }
            var mcpOut = (dir + $"\\{Path.GetFileName(dir)}.mcp").Replace(baseDirectory, modDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(mcpOut));
            mcCombos[name].mcp.BigEndian = toBigEndian;
            mcCombos[name].mcp.Write(mcpOut);
        }


    }

    private static void ApplyEdgeSet(MCG mcg, List<MCG.Edge> edgeList, List<int> usedIndexList, int roomCounter)
    {
        foreach (var edge in edgeList)
        {
            if (!usedIndexList.Contains(edge.NodeIndexA))
            {
                usedIndexList.Add(edge.NodeIndexA);
            }
            if (!usedIndexList.Contains(edge.NodeIndexB))
            {
                usedIndexList.Add(edge.NodeIndexB);
            }
            //Addend node data based on current edge
            mcg.Nodes[edge.NodeIndexA].ConnectedNodeIndices.Add(edge.NodeIndexB);
            mcg.Nodes[edge.NodeIndexB].ConnectedNodeIndices.Add(edge.NodeIndexA);

            mcg.Nodes[edge.NodeIndexA].ConnectedEdgeIndices.Add(mcg.Edges.Count);
            mcg.Nodes[edge.NodeIndexB].ConnectedEdgeIndices.Add(mcg.Edges.Count);

            mcg.Nodes[edge.NodeIndexA].ConnectedNodeIndices.Sort();
            mcg.Nodes[edge.NodeIndexB].ConnectedNodeIndices.Sort();

            //Clone and alter edges
            MCG.Edge newEdge = new MCG.Edge();
            newEdge.MapID = edge.MapID;
            newEdge.MCPRoomIndex = edge.MCPRoomIndex + roomCounter; //Since rooms are ordered differently per mcg, we need to ensure they match those new ids.
            newEdge.NodeIndexA = edge.NodeIndexA;
            newEdge.NodeIndexB = edge.NodeIndexB;
            newEdge.Unk20 = edge.Unk20;

            //May need editing later if these are implemented, depending on how they work
            newEdge.UnkIndicesA = edge.UnkIndicesA;
            newEdge.UnkIndicesB = edge.UnkIndicesB;

            mcg.Edges.Add(newEdge);
        }
    }

    private static List<MCG.Node> GetClonedNodeList(List<MCG.Node> nodes)
    {
        List<MCG.Node> clonedNodes = new List<MCG.Node>();
        foreach (var node in nodes)
        {
            MCG.Node clonedNode = new MCG.Node();
            clonedNode.Position = node.Position;
            clonedNode.Unk18 = node.Unk18;
            clonedNode.Unk1C = node.Unk1C;
            clonedNode.ConnectedEdgeIndices = new List<int>();
            clonedNode.ConnectedNodeIndices = new List<int>();

            clonedNodes.Add(clonedNode);
        }

        return clonedNodes;
    }

    private static List<int> GetUniqueVertIds(List<NVM.Triangle> triSet)
    {
        List<int> uniqueIds = new List<int>();

        foreach (var tri in triSet)
        {
            if (!uniqueIds.Contains(tri.VertexIndex1))
            {
                uniqueIds.Add(tri.VertexIndex1);
            }
            if (!uniqueIds.Contains(tri.VertexIndex2))
            {
                uniqueIds.Add(tri.VertexIndex2);
            }
            if (!uniqueIds.Contains(tri.VertexIndex3))
            {
                uniqueIds.Add(tri.VertexIndex3);
            }
        }

        return uniqueIds;
    }

    private static void CompileGateTriSet(List<NVM.Triangle> triSet, List<int> usedIndices, NVM nvm, int id)
    {
        var tri = nvm.Triangles[id];
        if (!usedIndices.Contains(id) && (tri.Flags & NVM.TriangleFlags.GATE) > 0)
        {
            //If we haven't taken this triangle yet, recursively check its adjacent triangles and add ones which are also gates.
            //Adjacent tris which are gates are considered part of the same gate entity
            usedIndices.Add(id);
            triSet.Add(tri);

            if (tri.EdgeIndex1 >= 0)
            {
                CompileGateTriSet(triSet, usedIndices, nvm, tri.EdgeIndex1);
            }
            if (tri.EdgeIndex2 >= 0)
            {
                CompileGateTriSet(triSet, usedIndices, nvm, tri.EdgeIndex2);
            }
            if (tri.EdgeIndex3 >= 0)
            {
                CompileGateTriSet(triSet, usedIndices, nvm, tri.EdgeIndex3);
            }
        }
    }
}
