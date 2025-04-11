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
    public static void TranslateMesh(FLVER2.Mesh curMesh, Vector3 changeVector)
    {
        foreach(var vertex in curMesh.Vertices)
        {
            vertex.Position.X += changeVector.X;
            vertex.Position.Y += changeVector.Y;
            vertex.Position.Z += changeVector.Z;
        }
    }

    public static void ScaleMesh(FLVER2.Mesh curMesh, Vector3 scaleVector)
    {
        foreach (var vertex in curMesh.Vertices)
        {
            if (scaleVector.X != 0)
            {
                vertex.Position.X *= scaleVector.X;
            }

            if (scaleVector.Z != 0)
            {
                vertex.Position.Y *= scaleVector.Y;
            }

            if (scaleVector.Y != 0)
            {
                vertex.Position.Z *= scaleVector.Z;
            }
        }
    }

    public static void RotateMesh(FLVER2.Mesh curMesh, float angle, RotationAxis axis)
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
                var newY = vertex.Position.Y * cos - vertex.Position.Z * sin;
                var newZ = vertex.Position.Y * sin + vertex.Position.Z * cos;
                vertex.Position.Y = newY;
                vertex.Position.Z = newZ;

                var newNormalY = vertex.Normal.Y * cos - vertex.Normal.Z * sin;
                var newNormalZ = vertex.Normal.Y * sin + vertex.Normal.Z * cos;
                vertex.Normal.Y = newNormalY;
                vertex.Normal.Z = newNormalZ;

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
                var newX = vertex.Position.X * cos - vertex.Position.Z * sin;
                var newZ = vertex.Position.X * sin + vertex.Position.Z * cos;
                vertex.Position.X = newX;
                vertex.Position.Z = newZ;

                var newNormalX = vertex.Normal.X * cos - vertex.Normal.Z * sin;
                var newNormalZ = -vertex.Normal.X * sin + vertex.Normal.Z * cos;
                vertex.Normal.X = newNormalX;
                vertex.Normal.Z = newNormalZ;

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
                var newX = vertex.Position.X * cos - vertex.Position.Y * sin;
                var newY = vertex.Position.X * sin + vertex.Position.Y * cos;
                vertex.Position.X = newX;
                vertex.Position.Y = newY;

                var newNormalX = vertex.Normal.X * cos - vertex.Normal.Y * sin;
                var newNormalY = vertex.Normal.X * sin + vertex.Normal.Y * cos;
                vertex.Normal.X = newNormalX;
                vertex.Normal.Y = newNormalY;

                if (vertex.Tangents.Count > 0)
                {
                    var tangentX = vertex.Tangents[0].X * cos - vertex.Tangents[0].Y * sin;
                    var tangentY = vertex.Tangents[0].X * sin + vertex.Tangents[0].Y * cos;
                    var tangentZ = vertex.Tangents[0].Z;

                    vertex.Tangents[0] = new Vector4(tangentX, tangentY, tangentZ, tangentW);
                }
            }
        }
    }
}

