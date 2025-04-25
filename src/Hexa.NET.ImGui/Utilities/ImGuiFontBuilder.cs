namespace Hexa.NET.ImGui.Utilities
{
    using Hexa.NET.ImGui;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a structure for storing glyph ranges for font configurations.
    /// </summary>
    public unsafe struct GlyphRanges : IDisposable
    {
        private uint* glyphs;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlyphRanges"/> struct with the specified glyphs.
        /// </summary>
        /// <param name="glyphs">An array of glyphs to be stored as ranges.</param>
        public GlyphRanges(params uint[] glyphs)
        {
            int length = glyphs.Length;
            if (glyphs[length - 1] != '\0')
            {
                length += 1;
            }
            this.glyphs = (uint*)Marshal.AllocHGlobal(sizeof(uint) * length);
            for (int i = 0; i < glyphs.Length; i++)
            {
                this.glyphs[i] = glyphs[i];
            }
            this.glyphs[length - 1] = '\0';
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlyphRanges"/> struct using a read-only span of glyphs.
        /// </summary>
        /// <param name="glyphs">A read-only span of glyphs to be stored as ranges.</param>
        public GlyphRanges(ReadOnlySpan<uint> glyphs)
        {
            int length = glyphs.Length;
            if (glyphs[length - 1] != '\0')
            {
                length += 1;
            }
            this.glyphs = (uint*)Marshal.AllocHGlobal(sizeof(uint) * length);
            for (int i = 0; i < glyphs.Length; i++)
            {
                this.glyphs[i] = glyphs[i];
            }
            this.glyphs[length - 1] = '\0';
        }

        /// <summary>
        /// Gets the glyph ranges as a pointer to a <see cref="uint"/> array.
        /// </summary>
        /// <returns>A pointer to the glyph ranges.</returns>
        public readonly uint* GetRanges()
        {
            return glyphs;
        }

        /// <summary>
        /// Disposes the memory allocated for the glyph ranges.
        /// </summary>
        public void Dispose()
        {
            if (glyphs != null)
            {
                Marshal.FreeHGlobal((nint)glyphs);
                glyphs = null;
            }
        }
    }

    /// <summary>
    /// Encapsulates a block of font data in unmanaged memory for use with ImGui or other rendering systems.
    /// </summary>
    public unsafe struct FontBlob : IDisposable
    {
        public void* Data;
        public int Length;

        public FontBlob(void* data, int length)
        {
            Data = data;
            Length = length;
        }

        public FontBlob(ReadOnlySpan<byte> bytes)
        {
            Data = (void*)Marshal.AllocHGlobal(bytes.Length);
            Length = bytes.Length;
            Span<byte> span = this;
            bytes.CopyTo(span);
        }

        public static implicit operator Span<byte>(FontBlob blob)
        {
            return new Span<byte>(blob.Data, blob.Length);
        }

        public void Dispose()
        {
            if (Data != null)
            {
                Marshal.FreeHGlobal((nint)Data);
                Data = null;
                Length = 0;
            }
        }
    }

    /// <summary>
    /// ImGuiFontBuilder is a utility for easily loading custom fonts.
    /// </summary>
    /// <remarks>
    /// ⚠️ <c>Warning</c>: If you use glyph ranges, do not dispose of this builder until after ImGui shutdown.
    /// Disposing prematurely can cause dangling pointers, as ImGui may continue to reference glyph ranges during its lifetime.
    /// </remarks>
    public unsafe class ImGuiFontBuilder : IDisposable
    {
        private ImFontAtlasPtr fontAtlas;
        private ImFontConfigPtr config;
        private ImFontPtr font;
        private readonly List<GlyphRanges> ranges = [];
        private readonly List<FontBlob> blobs = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ImGuiFontBuilder"/> class with the specified font atlas.
        /// </summary>
        /// <param name="fontAtlasPtr">Pointer to the ImGui font atlas.</param>
        public ImGuiFontBuilder(ImFontAtlasPtr fontAtlasPtr)
        {
            config = ImGui.ImFontConfig();
            config.FontDataOwnedByAtlas = false;
            fontAtlas = fontAtlasPtr;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImGuiFontBuilder"/> class with the default font atlas of <see cref="ImGui.GetIO()"/>.
        /// </summary>
        public ImGuiFontBuilder()
        {
            config = ImGui.ImFontConfig();
            config.FontDataOwnedByAtlas = false;
            fontAtlas = ImGui.GetIO().Fonts;
        }

        /// <summary>
        /// Gets the configuration settings for the font.
        /// </summary>
        public ImFontConfigPtr Config => config;

        /// <summary>
        /// Gets the constructed font after the build process is completed.
        /// </summary>
        public ImFontPtr Font => font;

        /// <summary>
        /// Configures the font by applying specified options to the configuration.
        /// </summary>
        /// <param name="action">An action that applies custom configurations.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        public ImGuiFontBuilder SetOption(Action<ImFontConfigPtr> action)
        {
            action(config);
            return this;
        }

        /// <summary>
        /// Adds the default font to the font atlas and enables merge mode for subsequent font additions.
        /// </summary>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        public ImGuiFontBuilder AddDefaultFont()
        {
            font = fontAtlas.AddFontDefault(config);
            config.MergeMode = true;
            return this;
        }

        /// <summary>
        /// Adds a font from a TTF file with specified glyph ranges provided as a read-only span of <see cref="uint"/>.
        /// </summary>
        /// <param name="path">The path to the font file.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="glyphRanges">The glyph ranges as a read-only span.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        public ImGuiFontBuilder AddFontFromFileTTF(string path, float size, ReadOnlySpan<uint> glyphRanges)
        {
            return AddFontFromFileTTF(path, size, new GlyphRanges(glyphRanges));
        }

        /// <summary>
        /// Adds a font from a file with TrueType format (TTF) and specified glyph ranges to the font atlas.
        /// </summary>
        /// <param name="path">The path to the font file.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="glyphRanges">The glyph ranges to be used.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <remarks>
        /// ⚠️ <c>Note</c>: The provided <paramref name="glyphRanges"/> become associated with this builder, transferring ownership to it.
        /// The builder takes responsibility for managing and releasing the memory for these glyph ranges,
        /// and they should not be used or modified externally after this call.
        /// </remarks>
        public ImGuiFontBuilder AddFontFromFileTTF(string path, float size, GlyphRanges glyphRanges)
        {
            ranges.Add(glyphRanges);
            return AddFontFromFileTTF(path, size, glyphRanges.GetRanges());
        }

        /// <summary>
        /// Adds a font from a TTF file with glyph ranges provided as an unsafe pointer.
        /// </summary>
        /// <param name="path">The path to the font file.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="pGlyphRanges">A pointer to the glyph ranges.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <remarks>
        /// ⚠️ <c>Important</c>: The <paramref name="pGlyphRanges"/> pointer must remain valid for the entire application or ImGui lifecycle.
        /// Disposing or altering the memory before ImGui shutdown can lead to dangling pointers and undefined behavior
        /// when ImGui accesses the font data.
        /// </remarks>
        public ImGuiFontBuilder AddFontFromFileTTF(string path, float size, uint* pGlyphRanges)
        {
            var fullpath = Path.GetFullPath(path);
            bool exists = File.Exists(fullpath);
            if (!exists)
            {
                throw new FileNotFoundException($"Font file not found: {fullpath}");
            }

            FontBlob blob = new(File.ReadAllBytes(fullpath));
            blobs.Add(blob);
            return AddFontFromMemoryTTF(blob.Data, blob.Length, size, pGlyphRanges);
        }

        /// <summary>
        /// Adds a font from a TTF file without specifying any glyph ranges.
        /// </summary>
        /// <param name="path">The path to the font file.</param>
        /// <param name="size">The desired font size.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the font file is not found.</exception>
        public ImGuiFontBuilder AddFontFromFileTTF(string path, float size)
        {
            var fullpath = Path.GetFullPath(path);
            bool exists = File.Exists(fullpath);
            if (!exists)
            {
                throw new FileNotFoundException($"Font file not found: {fullpath}");
            }
            FontBlob blob = new(File.ReadAllBytes(fullpath));
            blobs.Add(blob);
            return AddFontFromMemoryTTF(blob.Data, blob.Length, size);
        }

        /// <summary>
        /// Adds a font from an embedded resource, using the specified path and font size.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="path">The embedded resource path.</param>
        /// <param name="size">The desired font size.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the embedded resource is not found.</exception>
        public ImGuiFontBuilder AddFontFromEmbeddedResource(Assembly assembly, string path, float size)
        {
            using var stream = assembly.GetManifestResourceStream(path) ?? throw new FileNotFoundException($"Embedded resource not found: {path}");
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            FontBlob blob = new(buffer);
            blobs.Add(blob);
            return AddFontFromMemoryTTF(blob.Data, blob.Length, size);
        }

        /// <summary>
        /// Adds a font from an embedded resource with specified glyph ranges provided as a read-only span.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="path">The embedded resource path.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="glyphRanges">The glyph ranges as a read-only span.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        public ImGuiFontBuilder AddFontFromEmbeddedResource(Assembly assembly, string path, float size, ReadOnlySpan<uint> glyphRanges)
        {
            return AddFontFromEmbeddedResource(assembly, path, size, new GlyphRanges(glyphRanges));
        }

        /// <summary>
        /// Adds a font from an embedded resource with specified glyph ranges.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="path">The embedded resource path.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="glyphRanges">The glyph ranges to be used.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the embedded resource is not found.</exception>
        /// <remarks>
        /// ⚠️ <c>Note</c>: The provided <paramref name="glyphRanges"/> become associated with this builder, transferring ownership to it.
        /// The builder takes responsibility for managing and releasing the memory for these glyph ranges,
        /// and they should not be used or modified externally after this call.
        /// </remarks>
        public ImGuiFontBuilder AddFontFromEmbeddedResource(Assembly assembly, string path, float size, GlyphRanges glyphRanges)
        {
            ranges.Add(glyphRanges);
            return AddFontFromEmbeddedResource(assembly, path, size, glyphRanges.GetRanges());
        }

        /// <summary>
        /// Adds a font from an embedded resource with specified glyph ranges.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="path">The embedded resource path.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="pGlyphRanges">Pointer to the glyph ranges.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the embedded resource is not found.</exception>
        /// <remarks>
        /// ⚠️ <c>Important</c>: The <paramref name="pGlyphRanges"/> pointer must remain valid for the entire application or ImGui lifecycle.
        /// Disposing or altering the memory before ImGui shutdown can lead to dangling pointers and undefined behavior
        /// when ImGui accesses the font data.
        /// </remarks>
        public ImGuiFontBuilder AddFontFromEmbeddedResource(Assembly assembly, string path, float size, uint* pGlyphRanges)
        {
            var stream = assembly.GetManifestResourceStream(path) ?? throw new FileNotFoundException($"Embedded resource not found: {path}");
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            FontBlob blob = new(buffer);
            return AddFontFromMemoryTTF(blob.Data, blob.Length, size, pGlyphRanges);
        }

        /// <summary>
        /// Adds a font from memory using a pointer to the font data, along with the font data size and desired size.
        /// </summary>
        /// <param name="fontData">Pointer to the font data.</param>
        /// <param name="fontDataSize">The size of the font data in bytes.</param>
        /// <param name="size">The desired font size.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        public ImGuiFontBuilder AddFontFromMemoryTTF(void* fontData, int fontDataSize, float size)
        {
            // IMPORTANT: AddFontFromMemoryTTF() by default transfer ownership of the data buffer to the font atlas, which will attempt to free it on destruction.
            // This was to avoid an unnecessary copy, and is perhaps not a good API (a future version will redesign it).
            font = fontAtlas.AddFontFromMemoryTTF(fontData, fontDataSize, size, config);
            config.MergeMode = true;
            return this;
        }

        /// <summary>
        /// Adds a font from memory using a read-only span for font data and specified glyph ranges.
        /// </summary>
        /// <param name="fontData">The font data as a read-only span of bytes.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="glyphRanges">The glyph ranges as a read-only span.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        public ImGuiFontBuilder AddFontFromMemoryTTF(ReadOnlySpan<byte> fontData, float size, ReadOnlySpan<uint> glyphRanges)
        {
            return AddFontFromMemoryTTF(fontData, size, new GlyphRanges(glyphRanges));
        }

        /// <summary>
        /// Adds a font from memory using a pointer to font data, font data size, and specified glyph ranges.
        /// </summary>
        /// <param name="fontData">Pointer to the font data.</param>
        /// <param name="fontDataSize">The size of the font data in bytes.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="glyphRanges">The glyph ranges as a read-only span.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        public ImGuiFontBuilder AddFontFromMemoryTTF(byte* fontData, int fontDataSize, float size, ReadOnlySpan<uint> glyphRanges)
        {
            return AddFontFromMemoryTTF(fontData, fontDataSize, size, new GlyphRanges(glyphRanges));
        }

        /// <summary>
        /// Adds a font from memory using a read-only span for font data and specified glyph ranges.
        /// </summary>
        /// <param name="fontData">The font data as a read-only span of bytes.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="glyphRanges">The glyph ranges to be used.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <remarks>
        /// ⚠️ <c>Note</c>: The provided <paramref name="glyphRanges"/> become associated with this builder, transferring ownership to it.
        /// The builder takes responsibility for managing and releasing the memory for these glyph ranges,
        /// and they should not be used or modified externally after this call.
        /// </remarks>
        public ImGuiFontBuilder AddFontFromMemoryTTF(ReadOnlySpan<byte> fontData, float size, GlyphRanges glyphRanges)
        {
            ranges.Add(glyphRanges);
            FontBlob blob = new(fontData);
            blobs.Add(blob);
            return AddFontFromMemoryTTF(blob.Data, blob.Length, size, glyphRanges.GetRanges());
        }

        /// <summary>
        /// Adds a font from memory using a pointer to font data, font data size, and specified glyph ranges.
        /// </summary>
        /// <param name="fontData">Pointer to the font data.</param>
        /// <param name="fontDataSize">The size of the font data in bytes.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="glyphRanges">The glyph ranges to be used.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <remarks>
        /// ⚠️ <c>Note</c>: The provided <paramref name="glyphRanges"/> become associated with this builder, transferring ownership to it.
        /// The builder takes responsibility for managing and releasing the memory for these glyph ranges,
        /// and they should not be used or modified externally after this call.
        /// </remarks>
        public ImGuiFontBuilder AddFontFromMemoryTTF(byte* fontData, int fontDataSize, float size, GlyphRanges glyphRanges)
        {
            ranges.Add(glyphRanges);
            return AddFontFromMemoryTTF(fontData, fontDataSize, size, glyphRanges.GetRanges());
        }

        /// <summary>
        /// Adds a font from memory using a pointer to font data, font data size, and a pointer to glyph ranges.
        /// </summary>
        /// <param name="fontData">Pointer to the font data.</param>
        /// <param name="fontDataSize">The size of the font data in bytes.</param>
        /// <param name="size">The desired font size.</param>
        /// <param name="pGlyphRanges">Pointer to the glyph ranges.</param>
        /// <returns>The current <see cref="ImGuiFontBuilder"/> instance for method chaining.</returns>
        /// <remarks>
        /// ⚠️ <c>Important</c>: The <paramref name="pGlyphRanges"/> pointer must remain valid for the entire application or ImGui lifecycle.
        /// Disposing or altering the memory before ImGui shutdown can lead to dangling pointers and undefined behavior
        /// when ImGui accesses the font data.
        /// </remarks>
        public ImGuiFontBuilder AddFontFromMemoryTTF(void* fontData, int fontDataSize, float size, uint* pGlyphRanges)
        {
            // IMPORTANT: AddFontFromMemoryTTF() by default transfer ownership of the data buffer to the font atlas, which will attempt to free it on destruction.
            // This was to avoid an unnecessary copy, and is perhaps not a good API (a future version will redesign it).
            font = fontAtlas.AddFontFromMemoryTTF(fontData, fontDataSize, size, config, pGlyphRanges);

            return this;
        }

        /// <summary>
        /// Builds the font atlas, making the fonts ready for use.
        /// </summary>
        /// <returns>The pointer to the constructed <see cref="ImFontPtr"/>.</returns>
        public ImFontPtr Build()
        {
            fontAtlas.Build();
            return Font;
        }

        private unsafe void Destroy()
        {
            if (config.Handle == null)
            {
                return;
            }

            for (int i = 0; i < ranges.Count; i++)
            {
                ranges[i].Dispose();
            }

            for (int i = 0; i < blobs.Count; i++)
            {
                blobs[i].Dispose();
            }

            config.Destroy();
            config = default;
            fontAtlas = default;
        }

        /// <summary>
        /// Disposes the font builder, freeing resources and preventing further use.
        /// </summary>
        /// <remarks>
        /// ⚠️ <c>Warning</c>: Ensure that this method is called only after ImGui has completed its usage of any glyph ranges.
        /// Disposing prematurely may cause dangling pointers and undefined behavior, as ImGui may continue referencing the glyph ranges.
        /// </remarks>
        public void Dispose()
        {
            Destroy();
            GC.SuppressFinalize(this);
        }
    }
}