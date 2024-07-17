using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// FLVER implementation for Model Editor usage
// Credit to The12thAvenger
namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// For mapping skeletons and their controls in Sekiro forward.
        /// </summary>
        public class SkeletonSet
        {
            /// <summary>
            /// Contains the standard skeleton hierarchy, which corresponds to the node hierarchy.
            /// </summary>
            public List<Bone> BaseSkeleton { get; set; }

            /// <summary>
            /// Contains all skeleton hierarchies including that of the control rig and the ragdoll bones.
            /// </summary>
            public List<Bone> AllSkeletons { get; set; }

            /// <summary>
            /// Creates an empty BoneMapping.
            /// </summary>
            public SkeletonSet()
            {
                BaseSkeleton = new List<Bone>();
                AllSkeletons = new List<Bone>();
            }

            internal SkeletonSet(BinaryReaderEx br)
            {
                short count1 = br.ReadInt16();
                short count2 = br.ReadInt16();
                uint offset1 = br.ReadUInt32();
                uint offset2 = br.ReadUInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                br.StepIn(offset1);
                {
                    BaseSkeleton = new List<Bone>(count1);
                    for (int i = 0; i < count1; i++)
                        BaseSkeleton.Add(new Bone(br));
                }
                br.StepOut();

                br.StepIn(offset2);
                {
                    AllSkeletons = new List<Bone>(count2);
                    for (int i = 0; i < count2; i++)
                        AllSkeletons.Add(new Bone(br));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16((short)BaseSkeleton.Count);
                bw.WriteInt16((short)AllSkeletons.Count);
                bw.ReserveUInt32("BaseSkeletonOffset");
                bw.ReserveUInt32("ControlSkeletonOffset");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillUInt32("BaseSkeletonOffset", (uint)bw.Position);
                foreach (Bone member in BaseSkeleton)
                    member.Write(bw);

                bw.FillUInt32("ControlSkeletonOffset", (uint)bw.Position);
                foreach (Bone member in AllSkeletons)
                    member.Write(bw);
            }

            public SkeletonSet Clone()
            {
                return (SkeletonSet)MemberwiseClone();
            }

            /// <summary>
            /// A bone in a skeleton.
            /// </summary>
            public class Bone
            {
                /// <summary>
                /// Index of the parent bone, or -1 for none.
                /// </summary>
                public short ParentIndex { get; set; }

                /// <summary>
                /// Index of the bone's first child, or -1 for none.
                /// </summary>
                public short FirstChildIndex { get; set; }

                /// <summary>
                /// Index of the bone's next sibling, or -1 for none.
                /// </summary>
                public short NextSiblingIndex { get; set; }

                /// <summary>
                /// Index of the bone's sibling, or -1 for none.
                /// </summary>
                public short PreviousSiblingIndex { get; set; }

                /// <summary>
                /// Index of the node in the <see cref="FLVER2.Nodes"/> list
                /// </summary>
                public int NodeIndex { get; set; }

                /// <summary>
                /// Creates a Bone with default values.
                /// </summary>
                public Bone(int nodeIndex)
                {
                    NodeIndex = nodeIndex;
                    ParentIndex = -1;
                    FirstChildIndex = -1;
                    PreviousSiblingIndex = -1;
                    NextSiblingIndex = -1;
                }

                internal Bone(BinaryReaderEx br)
                {
                    ParentIndex = br.ReadInt16();
                    FirstChildIndex = br.ReadInt16();
                    NextSiblingIndex = br.ReadInt16();
                    PreviousSiblingIndex = br.ReadInt16();
                    NodeIndex = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(ParentIndex);
                    bw.WriteInt16(FirstChildIndex);
                    bw.WriteInt16(NextSiblingIndex);
                    bw.WriteInt16(PreviousSiblingIndex);
                    bw.WriteInt32(NodeIndex);
                    bw.WriteInt32(0);
                }
                public Bone Clone()
                {
                    return (Bone)MemberwiseClone();
                }
            }
        }
    }
}
