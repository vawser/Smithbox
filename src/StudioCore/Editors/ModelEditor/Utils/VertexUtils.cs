using SoulsFormats;
using StudioCore.Editors.ModelEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Utils;

public static class VertexUtils
{
    public static void TranslateMesh(FLVER2 curFlver, FLVER2.Mesh curMesh, Vector3 changeVector)
    {
        foreach(var vertex in curMesh.Vertices)
        {
            vertex.Position.X += changeVector.X;
            vertex.Position.Y += changeVector.Y;
            vertex.Position.Z += changeVector.Z;
        }

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);
    }

    public static void ScaleMesh(FLVER2 curFlver, FLVER2.Mesh curMesh, Vector3 scaleVector)
    {
        foreach (var vertex in curMesh.Vertices)
        {
            vertex.Position.X *= scaleVector.X;
            vertex.Position.Y *= scaleVector.Y;
            vertex.Position.Z *= scaleVector.Z;
        }

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);
    }
    public static void RotateMesh(FLVER2 curFlver, FLVER2.Mesh curMesh, float angle, RotationAxis axis)
    {
        float angleRadians = angle * (float)Math.PI / 180.0f;
        float cos = (float)Math.Cos(angleRadians);
        float sin = (float)Math.Sin(angleRadians);

        foreach (var vertex in curMesh.Vertices)
        {
            float tangentW = vertex.Tangents[0].W;
            if(vertex.Tangents[0].W == 0)
            {
                tangentW = -1;
            }

            if (axis is RotationAxis.X)
            {
                vertex.Position.Y = vertex.Position.Y * cos - vertex.Position.Z * sin;
                vertex.Position.Z = vertex.Position.Y * sin + vertex.Position.Z * cos;

                vertex.Normal.Y = vertex.Normal.Y * cos - vertex.Normal.Z * sin;
                vertex.Normal.Z = vertex.Normal.Y * sin + vertex.Normal.Z * cos;

                if (vertex.Tangents.Count > 0)
                {
                    var tangentX = vertex.Tangents[0].X;
                    var tangentY = vertex.Tangents[0].Y * cos - vertex.Tangents[0].Z * sin;
                    var tangentZ = vertex.Tangents[0].Y * sin + vertex.Tangents[0].Z * cos;

                    vertex.Tangents[0] = new Vector4(tangentX, tangentY, tangentZ, tangentW);
                }
            }
            if (axis is RotationAxis.Y)
            {
                vertex.Position.X = vertex.Position.X * cos - vertex.Position.Z * sin;
                vertex.Position.Z = -vertex.Position.X * sin + vertex.Position.Z * cos;

                vertex.Normal.X = vertex.Normal.X * cos - vertex.Normal.Z * sin;
                vertex.Normal.Z = -vertex.Normal.X * sin + vertex.Normal.Z * cos;

                if (vertex.Tangents.Count > 0)
                {
                    var tangentX = vertex.Tangents[0].X * cos - vertex.Tangents[0].Z * sin;
                    var tangentY = vertex.Tangents[0].Y;
                    var tangentZ = vertex.Tangents[0].X * sin + vertex.Tangents[0].Z * cos;

                    vertex.Tangents[0] = new Vector4(tangentX, tangentY, tangentZ, tangentW);
                }
            }
            if (axis is RotationAxis.Z)
            {
                vertex.Position.X = vertex.Position.X * cos - vertex.Position.Y * sin;
                vertex.Position.Y = vertex.Position.X * sin + vertex.Position.Y * cos;

                vertex.Normal.X = vertex.Normal.X * cos - vertex.Normal.Y * sin;
                vertex.Normal.Y = vertex.Normal.X * sin + vertex.Normal.Y * cos;

                if (vertex.Tangents.Count > 0)
                {
                    var tangentX = vertex.Tangents[0].X * cos - vertex.Tangents[0].Y * sin;
                    var tangentY = vertex.Tangents[0].X * sin + vertex.Tangents[0].Y * cos;
                    var tangentZ = vertex.Tangents[0].Z;

                    vertex.Tangents[0] = new Vector4(tangentX, tangentY, tangentZ, tangentW);
                }
            }
        }

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);
    }
}

