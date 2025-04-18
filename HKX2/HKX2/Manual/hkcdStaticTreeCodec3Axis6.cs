using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HKX2
{
    public partial class hkcdStaticTreeCodec3Axis6 : hkcdStaticTreeCodec3Axis
    {
        public Vector3 DecompressMin(Vector3 parentMin, Vector3 parentMax)
        {
            float x = ((float)(m_xyz_0 >> 4) * (float)(m_xyz_0 >> 4)) * (1.0f / 226.0f) * (parentMax.X - parentMin.X) + parentMin.X;
            float y = ((float)(m_xyz_1 >> 4) * (float)(m_xyz_1 >> 4)) * (1.0f / 226.0f) * (parentMax.Y - parentMin.Y) + parentMin.Y;
            float z = ((float)(m_xyz_2 >> 4) * (float)(m_xyz_2 >> 4)) * (1.0f / 226.0f) * (parentMax.Z - parentMin.Z) + parentMin.Z;
            return new Vector3(x, y, z);
        }

        public Vector3 DecompressMax(Vector3 parentMin, Vector3 parentMax)
        {
            float x = -((float)(m_xyz_0 & 0x0F) * (float)(m_xyz_0 & 0x0F)) * (1.0f / 226.0f) * (parentMax.X - parentMin.X) + parentMax.X;
            float y = -((float)(m_xyz_1 & 0x0F) * (float)(m_xyz_1 & 0x0F)) * (1.0f / 226.0f) * (parentMax.Y - parentMin.Y) + parentMax.Y;
            float z = -((float)(m_xyz_2 & 0x0F) * (float)(m_xyz_2 & 0x0F)) * (1.0f / 226.0f) * (parentMax.Z - parentMin.Z) + parentMax.Z;
            return new Vector3(x, y, z);
        }
    }
}
