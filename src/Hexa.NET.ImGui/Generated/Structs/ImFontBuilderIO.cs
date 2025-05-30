// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HexaGen.Runtime;
using System.Numerics;

namespace Hexa.NET.ImGui
{
	/// <summary>
	/// This structure is likely to evolve as we add support for incremental atlas updates.<br/>
	/// Conceptually this could be in ImGuiPlatformIO, but we are far from ready to make this public.<br/>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct ImFontBuilderIO
	{
		/// <summary>
		/// To be documented.
		/// </summary>
		public unsafe void* FontBuilderBuild;


		/// <summary>
		/// To be documented.
		/// </summary>
		public unsafe ImFontBuilderIO(delegate*<ImFontAtlas*, bool> fontbuilderBuild = default)
		{
			FontBuilderBuild = (void*)fontbuilderBuild;
		}


	}

	/// <summary>
	/// To be documented.
	/// </summary>
	#if NET5_0_OR_GREATER
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	#endif
	public unsafe struct ImFontBuilderIOPtr : IEquatable<ImFontBuilderIOPtr>
	{
		public ImFontBuilderIOPtr(ImFontBuilderIO* handle) { Handle = handle; }

		public ImFontBuilderIO* Handle;

		public bool IsNull => Handle == null;

		public static ImFontBuilderIOPtr Null => new ImFontBuilderIOPtr(null);

		public ImFontBuilderIO this[int index] { get => Handle[index]; set => Handle[index] = value; }

		public static implicit operator ImFontBuilderIOPtr(ImFontBuilderIO* handle) => new ImFontBuilderIOPtr(handle);

		public static implicit operator ImFontBuilderIO*(ImFontBuilderIOPtr handle) => handle.Handle;

		public static bool operator ==(ImFontBuilderIOPtr left, ImFontBuilderIOPtr right) => left.Handle == right.Handle;

		public static bool operator !=(ImFontBuilderIOPtr left, ImFontBuilderIOPtr right) => left.Handle != right.Handle;

		public static bool operator ==(ImFontBuilderIOPtr left, ImFontBuilderIO* right) => left.Handle == right;

		public static bool operator !=(ImFontBuilderIOPtr left, ImFontBuilderIO* right) => left.Handle != right;

		public bool Equals(ImFontBuilderIOPtr other) => Handle == other.Handle;

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is ImFontBuilderIOPtr handle && Equals(handle);

		/// <inheritdoc/>
		public override int GetHashCode() => ((nuint)Handle).GetHashCode();

		#if NET5_0_OR_GREATER
		private string DebuggerDisplay => string.Format("ImFontBuilderIOPtr [0x{0}]", ((nuint)Handle).ToString("X"));
		#endif
		/// <summary>
		/// To be documented.
		/// </summary>
		public void* FontBuilderBuild { get => Handle->FontBuilderBuild; set => Handle->FontBuilderBuild = value; }
	}

}
