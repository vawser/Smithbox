using SoulsFormats;
using StudioCore.DebugPrimitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore;

public class NewAnimSkeleton
{
    public const int MaxBoneCount = 1024;

    public List<FlverBoneInfo> FlverSkeleton = new();
    public List<HkxBoneInfo> HkxSkeleton = new();

    public HKX.HKASkeleton OriginalHavokSkeleton;

    public List<int> RootBoneIndices = new();

    public Matrix4x4[] ShaderMatrices = new Matrix4x4[1024];

    public NewAnimSkeleton(List<FLVER.Bone> flverBones)
    {
        var childCounts = new int[flverBones.Count];

        FlverSkeleton = new List<FlverBoneInfo>();

        for (var i = 0; i < flverBones.Count; i++)
        {
            var newBone = new FlverBoneInfo(flverBones[i], flverBones);
            if (flverBones[i].ParentIndex >= 0)
            {
                childCounts[flverBones[i].ParentIndex]++;
            }

            FlverSkeleton.Add(newBone);
        }

        for (var i = 0; i < FlverSkeleton.Count; i++)
        {
            FlverSkeleton[i].Length = Math.Max(0.1f,
                (flverBones[i].BoundingBoxMax.Z - flverBones[i].BoundingBoxMin.Z) * 0.8f);

            if (childCounts[i] == 1 && flverBones[i].ChildIndex >= 0)
            {
                Vector3 parentChildDifference = Vector3.Transform(Vector3.Zero,
                                                    FlverSkeleton[flverBones[i].ChildIndex].ReferenceMatrix) -
                                                Vector3.Transform(Vector3.Zero, FlverSkeleton[i].ReferenceMatrix);

                Vector3 parentChildDirection = Vector3.Normalize(parentChildDifference);

                Vector3 parentDir = Vector3.TransformNormal(-Vector3.UnitZ,
                    Matrix4x4.CreateRotationX(flverBones[i].Rotation.X) *
                    Matrix4x4.CreateRotationZ(flverBones[i].Rotation.Z) *
                    Matrix4x4.CreateRotationY(flverBones[i].Rotation.Y));

                var dot = Vector3.Dot(parentDir, parentChildDirection);

                FlverSkeleton[i].Length = parentChildDifference.Length() * (float)Math.Cos(dot);
            }
            else
            {
                FlverSkeleton[i].Length = Math.Max(0.1f,
                    (flverBones[i].BoundingBoxMax.Z - flverBones[i].BoundingBoxMin.Z) * 0.8f);
            }
        }

        for (var i = 0; i < 1024; i++)
        {
            ShaderMatrices[i] = Matrix4x4.Identity;
        }
    }

    public bool BoneLimitExceeded => FlverSkeleton.Count > MaxBoneCount;

    public Matrix4x4 this[int boneIndex]
    {
        get => ShaderMatrices[boneIndex];
        set
        {
            FlverSkeleton[boneIndex].CurrentMatrix = FlverSkeleton[boneIndex].ReferenceMatrix * value;
            ShaderMatrices[boneIndex] = value;
        }
    }

    public void LoadHKXSkeleton(HKX.HKASkeleton skeleton)
    {
        OriginalHavokSkeleton = skeleton;
        HkxSkeleton.Clear();
        for (var i = 0; i < skeleton.Bones.Size; i++)
        {
            var newHkxBone = new HkxBoneInfo();
            newHkxBone.Name = skeleton.Bones[i].Name.GetString();
            newHkxBone.ParentIndex = skeleton.ParentIndices[i].data;
            newHkxBone.RelativeReferenceMatrix =
                Matrix4x4.CreateScale(new Vector3(
                    skeleton.Transforms[i].Scale.Vector.X,
                    skeleton.Transforms[i].Scale.Vector.Y,
                    skeleton.Transforms[i].Scale.Vector.Z))
                * Matrix4x4.CreateFromQuaternion(new Quaternion(
                    skeleton.Transforms[i].Rotation.Vector.X,
                    skeleton.Transforms[i].Rotation.Vector.Y,
                    skeleton.Transforms[i].Rotation.Vector.Z,
                    skeleton.Transforms[i].Rotation.Vector.W))
                * Matrix4x4.CreateTranslation(new Vector3(
                    skeleton.Transforms[i].Position.Vector.X,
                    skeleton.Transforms[i].Position.Vector.Y,
                    skeleton.Transforms[i].Position.Vector.Z));

            for (var j = 0; j < FlverSkeleton.Count; j++)
            {
                if (FlverSkeleton[j].Name == newHkxBone.Name)
                {
                    FlverSkeleton[j].HkxBoneIndex = i;
                    newHkxBone.FlverBoneIndex = j;
                    break;
                }
            }

            HkxSkeleton.Add(newHkxBone);
        }

        Matrix4x4 GetAbsoluteReferenceMatrix(int i)
        {
            Matrix4x4 result = Matrix4x4.Identity;

            do
            {
                result *= HkxSkeleton[i].RelativeReferenceMatrix;
                i = HkxSkeleton[i].ParentIndex;
            } while (i >= 0);

            return result;
        }

        for (var i = 0; i < HkxSkeleton.Count; i++)
        {
            HkxSkeleton[i].ReferenceMatrix = GetAbsoluteReferenceMatrix(i);
            for (var j = 0; j < HkxSkeleton.Count; j++)
            {
                if (HkxSkeleton[j].ParentIndex == i)
                {
                    HkxSkeleton[i].ChildIndices.Add(j);
                }
            }

            if (HkxSkeleton[i].ParentIndex < 0)
            {
                RootBoneIndices.Add(i);
            }
        }
    }

    public void SetHkxBoneMatrix(int hkxBoneIndex, Matrix4x4 m)
    {
        var flverBoneIndex = HkxSkeleton[hkxBoneIndex].FlverBoneIndex;
        if (flverBoneIndex >= 0)
        {
            this[flverBoneIndex] = FlverSkeleton[flverBoneIndex].ReferenceMatrix.Inverse() * m;
        }
    }

    public void DrawPrimitives()
    {
        //foreach (var f in FlverSkeleton)
        //    f.DrawPrim(MODEL.CurrentTransform.WorldMatrix);
    }

    public void ApplyBakedFlverReferencePose()
    {
        for (var i = 0; i < FlverSkeleton.Count; i++)
        {
            this[i] = FlverSkeleton[i].ReferenceMatrix;
        }
    }

    public void RevertToReferencePose()
    {
        for (var i = 0; i < FlverSkeleton.Count; i++)
        {
            this[i] = Matrix4x4.Identity;
        }
    }

    public class FlverBoneInfo
    {
        public IDbgPrim BonePrim;
        public DbgPrimWireBox BoundingBoxPrim;
        public Matrix4x4 CurrentMatrix = Matrix4x4.Identity;
        public int HkxBoneIndex = -1;

        public float Length = 1.0f;
        public string Name;
        public Matrix4x4 ReferenceMatrix = Matrix4x4.Identity;

        public FlverBoneInfo(FLVER.Bone bone, List<FLVER.Bone> boneList)
        {
            Matrix4x4 GetBoneMatrix(FLVER.Bone b)
            {
                FLVER.Bone parentBone = b;

                Matrix4x4 result = Matrix4x4.Identity;

                do
                {
                    result *= Matrix4x4.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                    result *= Matrix4x4.CreateRotationX(parentBone.Rotation.X);
                    result *= Matrix4x4.CreateRotationZ(parentBone.Rotation.Z);
                    result *= Matrix4x4.CreateRotationY(parentBone.Rotation.Y);
                    result *= Matrix4x4.CreateTranslation(parentBone.Position.X, parentBone.Position.Y,
                        parentBone.Position.Z);

                    if (parentBone.ParentIndex >= 0)
                    {
                        parentBone = boneList[parentBone.ParentIndex];
                    }
                    else
                    {
                        parentBone = null;
                    }
                } while (parentBone != null);

                return result;
            }

            ReferenceMatrix = GetBoneMatrix(bone);
            Name = bone.Name;

            if (bone.Unk3C == 0)
            {
                //BonePrim = new DbgPrimWireBone(bone.Name, new Transform(ReferenceMatrix), DBG.COLOR_FLVER_BONE)
                //{
                //    Category = DbgPrimCategory.FlverBone,
                //};

                /*BoundingBoxPrim = new DbgPrimWireBox(Transform.Default,
                    new Vector3(bone.BoundingBoxMin.X, bone.BoundingBoxMin.Y, bone.BoundingBoxMin.Z),
                    new Vector3(bone.BoundingBoxMax.X, bone.BoundingBoxMax.Y, bone.BoundingBoxMax.Z),
                    DBG.COLOR_FLVER_BONE_BBOX)
                {
                    Category = DbgPrimCategory.FlverBoneBoundingBox,
                };*/
            }
        }

        public void DrawPrim(Matrix4x4 world)
        {
            if (BonePrim != null && BoundingBoxPrim != null)
            {
                //BonePrim.Transform = new Transform(Matrix.CreateScale(Length) * CurrentMatrix);
                //BonePrim.Draw(null, world);
                //BoundingBoxPrim.UpdateTransform(new Transform(CurrentMatrix));
                //BoundingBoxPrim.Draw(null, world);
            }
        }
    }

    public class HkxBoneInfo
    {
        public List<int> ChildIndices = new();
        public int FlverBoneIndex = -1;
        public string Name;
        public short ParentIndex = -1;
        public Matrix4x4 ReferenceMatrix = Matrix4x4.Identity;
        public Matrix4x4 RelativeReferenceMatrix = Matrix4x4.Identity;
    }
}
