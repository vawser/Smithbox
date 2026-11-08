# UI Styling and Text Plan (staying on ImGui)

Date: 2026-11-08
Follows: `UI Framework Viability.md`. The goal behind a UI switch was better styling and text, and both can be
handled on ImGui. Nothing is built yet.

## Where things are now

### Text

- **Dear ImGui 1.91.9** (Hexa.NET.ImGui 2.2.7). The font atlas is fixed: every glyph is rendered once at one size,
  when it is built (`Smithbox.SetupFonts` / `AddFonts` in `Smithbox.cs`).
- stb_truetype rasteriser, with oversampling of 3x2 or 2x2 to make up for no hinting. Text looks soft at small sizes.
- One font at one size for the whole app: `PushFont` is never called. Headers, IDs, hex values and body text all look
  the same. Hierarchy comes only from colour.
- The additional fonts bake the **full** Japanese + Chinese + Korean + Thai + Vietnamese + Cyrillic ranges, which is
  tens of thousands of glyphs. That is why `SetupFonts` needs a four-step fallback (wider atlas → no oversampling →
  drop CJK) for large font sizes or UI scales.
- Changing font size or UI scale rebuilds the whole atlas.
- Icons come from ForkAwesome (`0xF000–0xF339`), which is no longer maintained.

### Styling

- Themes set colours only: `GUI.ApplyBaseStyle` pushes 29 `UI.Current.*` colours each frame. Sizes, rounding and
  borders are hard-coded there (rounding 0, borders 1).
- Colours mostly go through the theme: `TextColored` uses `UI.Current` 136 times and a literal `new Vector4` 22 times.
  About 20 `PushStyleColor` calls also use literals.
- No semantic roles (warning, error, accent, muted, link), so editors pick colours case by case.

## What ImGui 1.92 changes

Hexa.NET.ImGui **2.2.9** (already in the local NuGet cache) ships **Dear ImGui 1.92.2**, with FreeType built in.
ImGuizmo, ImNodes and ImPlot all have matching 2.2.9 packages.

1.92 changes how fonts work:

- **Glyphs are rendered on demand.** No glyph ranges: any character in a loaded font is drawn when first used. The
  full CJK baking and the atlas-size fallback steps go away, and so does the GPU texture-size problem.
- **Any size at any time:** `PushFont(font, size)`. Headings, small captions and zoomed views need no extra atlas.
  Font size and UI scale changes apply at once, with no rebuild.
- **FreeType loader:** hinting and proper small-size rendering, with optional bold/oblique synthesis.
- `style.FontScaleMain` / `style.FontScaleDpi` replace the hand-scaled font size.

What it does **not** fix: ImGui still has no complex text shaping. There are no ligatures, Thai combining marks will
stack poorly, and there is no right-to-left text. Only a web or native UI toolkit would fix that. It matters only if
Thai text in game data has to look right.

## Plan

### Phase 1: Upgrade to Hexa 2.2.9 / ImGui 1.92.2 (required for the rest)

- Bump Hexa.NET.ImGui, ImGuizmo, ImNodes and ImPlot to 2.2.9 in `Smithbox.Program.csproj`.
- Backends: 1.92 hands textures to the renderer as `ImTextureData` with create/update/destroy requests, and the
  backend sets `ImGuiBackendFlags.RendererHasTextures`.
  - `VulkanImGuiRenderer`: each `ImTextureData` gets a `GlobalTexturePool` descriptor (as the font texture does now)
    and `WantUpdates` rects are uploaded into it. `SetTexID`/`RecreateFontDeviceTexture` go away.
  - `OpenGLImGuiRenderer`: the same with `glTexSubImage2D`.
- API fixes: `ImTextureID` → `ImTextureRef` for `ImGui.Image` (5 calls, plus about 9 other `ImTextureID` uses outside the
  renderers), `PushFont` signature, removed or renamed flags. Mostly compile-driven.
- Keep the current single font, so phase 1 only has to look the same.

Check: the app looks unchanged. CJK text in the Text editor (FMG) still shows. No atlas warnings at a large UI scale.

### Phase 2: Dynamic fonts and FreeType

- Rewrite `SetupFonts`: load the English, additional and icon fonts with no glyph ranges, and remove the fallback
  steps and `_reduceFontOversample`.
- Turn on the FreeType loader. Add a preference for hinting (none / light / full).
- Font size and UI scale through `style.FontScaleMain` / `FontScaleDpi`. Applies live.

Check: text is sharper at 12–14 px. Changing the size in Preferences applies with no restart or flicker.

### Phase 3: Text roles

- Add a small font registry, `Interface/Fonts.cs`: `Body`, `Bold`, `Heading`, `Small`, `Mono`. Bold and Heading can be
  synthesised from the body font, or set to a separate file in the preferences. Mono uses the shipped
  `RobotoMono-Light.ttf`.
- Use them in the existing helpers first, which spreads them without touching editors: `GUI.TitleHeader`,
  `SimpleHeader`, `ConditionalHeader` (Heading/Bold), `EmptyState`, `Tooltip` (Small).
- Then Mono for IDs, hex, offsets and paths in the Param, Havok and Map property views.

Check: section headers stand out without colour, and hex columns line up.

### Phase 4: Themes beyond colour

- Add to the theme files: rounding (window, frame, tab, popup, scrollbar), border sizes, padding/spacing multipliers,
  and semantic colours (`Accent`, `Muted`, `Warning`, `Error`, `Success`, `Link`).
- `ApplyBaseStyle` reads them instead of constants. Older theme files get the current values as defaults.
- Move the about 42 literal colour uses onto semantic colours.
- Ship two or three themes that use the new fields (e.g. the current flat one and a rounded softer one).

Check: switching themes changes shape as well as colour. Old theme files still load.

### Phase 5 (optional): Icon font

Replace ForkAwesome with a maintained set (e.g. Lucide or Material Symbols). With on-demand glyphs a large icon font
costs nothing until icons are drawn. It needs a mapping table from the current `Icons.cs` code points.

## Risks

- **Phase 1 backend work** is the only non-trivial part: two hand-written renderers, and partial texture updates
  in the middle of a frame. The Vulkan one already uses bindless pool slots, which makes this simpler.
- 1.92 is a large font-API change. ImNodes/ImPlot/ImGuizmo must move with it (all available at 2.2.9).
- `imgui.ini` layouts carry over (docking is unchanged).
