namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// A label that represents a jump target
    /// </summary>
    public sealed class Label : Instruction
    {
        /// <summary>
        /// How many instructions use this label. Used to delete labels in some optimizations
        /// </summary>
        public int UsageCount = 0;

        public readonly string LabelName;

        public Label(int id)
        {
            LabelName = $@"Label_{id}";
        }
    }
}
