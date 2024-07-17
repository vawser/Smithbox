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
    public partial class FLVER0
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Material : IFlverMaterial
        {
            public string Name { get; set; }

            public string MTD { get; set; }

            public List<Texture> Textures { get; set; }
            IReadOnlyList<IFlverTexture> IFlverMaterial.Textures => Textures;

            public List<BufferLayout> Layouts { get; set; }

            internal Material(BinaryReaderEx br, FLVER0 flv)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                int texturesOffset = br.ReadInt32();
                int layoutsOffset = br.ReadInt32();
                br.ReadInt32(); // Data length from name offset to end of buffer layouts
                int layoutHeaderOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = flv.Unicode ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
                MTD = flv.Unicode ? br.GetUTF16(mtdOffset) : br.GetShiftJIS(mtdOffset);

                br.StepIn(texturesOffset);
                {
                    byte textureCount = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    Textures = new List<Texture>(textureCount);
                    for (int i = 0; i < textureCount; i++)
                        Textures.Add(new Texture(br, flv));
                }
                br.StepOut();

                if (layoutHeaderOffset != 0)
                {
                    br.StepIn(layoutHeaderOffset);
                    {
                        int layoutCount = br.ReadInt32();
                        br.AssertInt32((int)br.Position + 0xC);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        Layouts = new List<BufferLayout>(layoutCount);
                        for (int i = 0; i < layoutCount; i++)
                        {
                            int layoutOffset = br.ReadInt32();
                            br.StepIn(layoutOffset);
                            {
                                Layouts.Add(new BufferLayout(br));
                            }
                            br.StepOut();
                        }
                    }
                    br.StepOut();
                }
                else
                {
                    Layouts = new List<BufferLayout>(1);
                    br.StepIn(layoutsOffset);
                    {
                        Layouts.Add(new BufferLayout(br));
                    }
                    br.StepOut();
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
