namespace Veldrid.SPIRV
{
    internal struct Bool32
    {
        public readonly uint Value;

        public Bool32(bool value) { Value = value ? 1u : 0u; }

        public static implicit operator bool(Bool32 b) => b.Value != 0;
        public static implicit operator Bool32(bool b) => new Bool32(b);
    }
}
