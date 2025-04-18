using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

// FLVER implementation for Model Editor usage
// Credit to The12thAvenger
namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// An abstract node consisting of a transform and hierarchy information. Its type is determined by its <see cref="NodeFlags"/>
        /// </summary>
        public class Node
        {
            /// <summary>
            /// A set of flags denoting the properties of a node
            /// </summary>
            [Flags]
            public enum NodeFlags
            {
                /// <summary>
                /// Is disconnected from the node hierarchy (should not appear in combination with other flags)
                /// </summary>
                Disabled = 1,
                /// <summary>
                /// A dummy poly references this node using either <see cref="FLVER.Dummy.AttachBoneIndex"/> or <see cref="FLVER.Dummy.ParentBoneIndex"/>
                /// </summary>
                DummyOwner = 1 << 1,
                /// <summary>
                /// This node represents a mesh
                /// </summary>
                Mesh = 1 << 2,
                /// <summary>
                /// This node represents a bone, i.e. it can be used to transform vertices using
                /// </summary>
                Bone = 1 << 3
            }

            /// <summary>
            /// The name of this node
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Index of this node's parent, or -1 for none.
            /// </summary>
            public short ParentIndex { get; set; }

            /// <summary>
            /// Index of this node's first child, or -1 for none.
            /// </summary>
            public short FirstChildIndex { get; set; }

            /// <summary>
            /// Index of the next child of this node's parent, or -1 for none.
            /// </summary>
            public short NextSiblingIndex { get; set; }

            /// <summary>
            /// Index of the previous child of this node's parent, or -1 for none.
            /// </summary>
            public short PreviousSiblingIndex { get; set; }

            /// <summary>
            /// Translation of this bone.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of this bone; euler radians in XZY order.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of this bone.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Minimum extent of the vertices weighted to this bone.
            /// </summary>
            public Vector3 BoundingBoxMin { get; set; }

            /// <summary>
            /// Maximum extent of the vertices weighted to this bone.
            /// </summary>
            public Vector3 BoundingBoxMax { get; set; }

            /// <inheritdoc cref="NodeFlags"/>
            public NodeFlags Flags { get; set; }

            /// <summary>
            /// Creates a Bone with default values.
            /// </summary>
            public Node()
            {
                Name = "";
                ParentIndex = -1;
                FirstChildIndex = -1;
                NextSiblingIndex = -1;
                PreviousSiblingIndex = -1;
                Scale = Vector3.One;
            }

            /// <summary>
            /// Creates a transformation matrix from the scale, rotation, and translation of the bone.
            /// </summary>
            public Matrix4x4 ComputeLocalTransform()
            {
                return Matrix4x4.CreateScale(Scale)
                    * Matrix4x4.CreateRotationX(Rotation.X)
                    * Matrix4x4.CreateRotationZ(Rotation.Z)
                    * Matrix4x4.CreateRotationY(Rotation.Y)
                    * Matrix4x4.CreateTranslation(Position);
            }

            /// <summary>
            /// Returns a string representation of the bone.
            /// </summary>
            public override string ToString() => Name;

            internal Node(BinaryReaderEx br, bool unicode)
            {
                Position = br.ReadVector3();
                int nameOffset = br.ReadInt32();
                Rotation = br.ReadVector3();
                ParentIndex = br.ReadInt16();
                FirstChildIndex = br.ReadInt16();
                Scale = br.ReadVector3();
                NextSiblingIndex = br.ReadInt16();
                PreviousSiblingIndex = br.ReadInt16();
                BoundingBoxMin = br.ReadVector3();
                Flags = (NodeFlags)br.ReadInt32();
                BoundingBoxMax = br.ReadVector3();
                br.AssertPattern(0x34, 0x00);

                if (unicode)
                    Name = br.GetUTF16(nameOffset);
                else
                    Name = br.GetShiftJIS(nameOffset);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteVector3(Position);
                bw.ReserveInt32($"BoneNameOffset{index}");
                bw.WriteVector3(Rotation);
                bw.WriteInt16(ParentIndex);
                bw.WriteInt16(FirstChildIndex);
                bw.WriteVector3(Scale);
                bw.WriteInt16(NextSiblingIndex);
                bw.WriteInt16(PreviousSiblingIndex);
                bw.WriteVector3(BoundingBoxMin);
                bw.WriteInt32((int)Flags);
                bw.WriteVector3(BoundingBoxMax);
                bw.WritePattern(0x34, 0x00);
            }

            internal void WriteStrings(BinaryWriterEx bw, bool unicode, int index)
            {
                bw.FillInt32($"BoneNameOffset{index}", (int)bw.Position);
                if (unicode)
                    bw.WriteUTF16(Name, true);
                else
                    bw.WriteShiftJIS(Name, true);
            }

            public Node Clone()
            {
                return (Node)MemberwiseClone();
            }
        }
    }
}
