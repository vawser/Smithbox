# UI Framework Viability: ImGui → Retained Mode (Blazor and alternatives)

Date: 2026-11-08
Status: Analysis only, nothing changed in code.

## Summary

Swapping Smithbox to Blazor is **not viable** as a replacement for the whole UI. The blocker is the 3D viewports, not the
widgets: Blazor runs in a web view, and Smithbox draws its scenes straight onto the native swapchain under the ImGui
windows. A web view cannot sit over a Vulkan swapchain without fragile, platform-specific workarounds, and Linux/macOS
builds make it worse.

If a retained UI is wanted, **Avalonia** is the only candidate that fits (cross-platform, native, has GPU-image interop).
Even then it is a rewrite of most of `Smithbox.Program`'s UI, not a port, because the code has no view-model layer to
bind to.

Recommendation: stay on ImGui. If there are specific pain points (layout, text, accessibility, styling, testability),
deal with those directly. If a retained UI is still wanted later, run a small Avalonia + Veldrid viewport spike first
(see the end), since that is where it would succeed or fail.

## What the current UI is

Measured in `src/Smithbox.Program` (321k lines of C#, 1,187 files):

| Measure | Count |
|---|---|
| Files calling `ImGui.*` directly | 366 |
| Direct `ImGui.*` calls | ~10,400 |
| `GUI.*` helper calls (`Interface/GUI.cs`, itself immediate mode) | ~4,200 |
| `DPI.*` sizing calls | ~460 |
| `IsItemDeactivatedAfterEdit` (edit → undo action commit points) | 114 |
| `ImGuiListClipper` (virtualised lists) | 47 |
| `DockSpace` / `SetNextWindowDockID` | 41 / 77 |
| ImNodes (Havok state machine graph) | 1 file |
| ImPlot (param field decorators) | 1 file |
| ImGuizmo | a few refs |

By editor (direct `ImGui.*` calls): Map 2,366 · Param 1,901 · Havok 932 · Time Act 655 · Model 634 ·
Preferences 557 · Gparam 529 · Text 519 · Viewport 418 · Project 338 · Interface 307 · Material 287 · others < 220.

Packages: Hexa.NET.ImGui / ImGuizmo / ImNodes / ImPlot 2.2.7. Two hand-written backends:
`Renderer/Context/VulkanImGuiRenderer.cs` (885 lines) and `OpenGLImGuiRenderer.cs` (587 lines).

### How the code is shaped

- UI and logic are interleaved. A typical property row reads the value, draws the widget, and on
  `IsItemDeactivatedAfterEdit` builds the undo action, all in one method (e.g. `MapMsbPropertyView.PropEditorPropInfoRow`).
- State lives in editor fields and is read each frame. There are no view models, `INotifyPropertyChanged`, or
  observable collections. Retained UIs depend on these; ImGui does not need them.
- Property editors are reflection-driven (`PropertyInfo` loops in Map/Model/Havok/Param views). This is the one place
  that would map well onto a retained property grid.
- `GUI.cs` and `DPI.cs` are helpers, not an abstraction layer: they still call ImGui in place, so they offer no seam
  to swap the backend under.

### How the viewports fit in

From `VulkanGraphicsContext.Draw` and `Editors/Viewport/VulkanViewport.cs`:

1. Each viewport renders its scene into its own HDR target, sized to the ImGui window that holds it.
2. Post-processing, tone mapping and FXAA write **directly onto the swapchain** at that window's rect
   (`cl.SetFramebuffer(Device.SwapchainFramebuffer); cl.SetViewport(0, RenderViewport)`).
3. ImGui then draws over the whole swapchain in the same command list, and the frame is presented.
4. Mouse input is gated with `ImGui.IsWindowHovered` and positioned with `ImGui.GetWindowPos`.

So the viewport is a rectangle of the ImGui frame, not a texture inside a widget. Any new UI must either sit in
the same swapchain (an immediate-mode UI) or take the viewport as an image it composites itself.

## Option assessment

### Blazor (Hybrid: BlazorWebView in WPF/WinForms/MAUI, or Photino)

- **Viewports (blocker).** The UI is HTML in WebView2/WebKitGTK. Ways to show a 3D scene:
  - Native child window behind or beside the web view ("airspace"). The web view cannot draw over it, so no
    overlays, popups, or docking across it. It breaks the docked layout the editors use.
  - Read the frame back and stream it into the page (canvas/`<img>`/WebRTC). Adds a GPU→CPU→browser copy each frame
    and input latency. HDR output is lost. Not acceptable for the map editor.
  - Shared GPU textures into the browser. No supported path in WebView2/WebKitGTK.
- **Platforms.** BlazorWebView for WPF/WinForms is Windows-only; MAUI has no Linux. Photino covers Linux/macOS, but with
  a different web engine on each platform. Smithbox ships `-win`, `-linux` and `-osx` configs.
- **Docking / graphs / plots.** Need JS libraries (e.g. dockview, a node-graph lib, a chart lib) plus interop. Each
  is a dependency outside .NET with its own bugs.
- **Performance.** Param and MSB property grids redraw thousands of fields. Blazor diffs a render tree and sends it
  over interop; it is fine for forms but slow for typing into large grids. `<Virtualize>` helps lists, not wide rows.
- **Upside.** CSS styling, real text layout, accessibility, web-dev familiarity.

Verdict: not viable for the editor shell. Possible for **separate, viewport-free windows** (e.g. a documentation or
project browser), but that splits the UI into two stacks for little gain.

### Avalonia (native retained XAML, Windows/Linux/macOS)

- **Viewports.** Avalonia 11 can composite external GPU images (`CompositionDrawingSurface` with GPU interop,
  including Vulkan). The in-repo Veldrid fork would need to render each viewport into an exportable image
  (`VK_KHR_external_memory`) instead of onto the swapchain. Doable, but it changes the end of the render path in
  `VulkanViewport` and the OpenGL compat path would need its own route. This is the main technical risk.
- **Docking.** Dock.Avalonia exists. Layout persistence would replace `imgui.ini`-style docking.
- **Graphs / plots.** Node-editor and plot libraries exist (e.g. NodeEditor, ScottPlot/OxyPlot), but none is a
  drop-in for ImNodes/ImPlot.
- **Gizmos.** The translate/rotate gizmos are scene renderables (`Gizmos` class), so they stay in the viewport.
- **Grids.** `TreeDataGrid`/`DataGrid` virtualise rows. Large param tables need care, but this is a known pattern.
- **Cost.** All ~10k call sites plus the `GUI.*` layer become XAML + view models. The edit → undo flow
  (`IsItemDeactivatedAfterEdit`) becomes binding commit events. Realistically many months of full-time work, with
  both UIs alive in parallel during the move.

Verdict: technically viable, very expensive. Only worth it for a long-term goal ImGui cannot meet.

### WPF / WinForms / MAUI

WPF is Windows-only and its GPU interop is D3D9/D3D11, not Vulkan. MAUI has no Linux desktop. Ruled out.

### Stay on ImGui, change the architecture underneath

Most of what people want from retained UIs is the architecture, not the toolkit:

- Pull edit logic out of draw methods into small editor-model classes (value + validate + commit-to-undo). The draw
  code becomes a thin view. This is incremental and lets either UI sit on top later.
- Unify the reflection-driven property views (Map / Model / Havok / Param) behind one property-grid model.
- Address specific ImGui limits where they hurt: text shaping/fonts, theming via `GUI.ApplyBaseStyle`, DPI.

Verdict: cheapest, no risk to the viewports, and it keeps a later switch open.

## Comparison

| | Blazor Hybrid | Avalonia | Stay on ImGui (+ refactor) |
|---|---|---|---|
| 3D viewport embedding | Blocked (airspace / readback) | Possible via GPU image interop | Already works |
| Linux / macOS | Photino only, mixed engines | Yes | Yes |
| Docking | JS library | Dock.Avalonia | Built in |
| Node graph / plots | JS libraries | Third-party .NET | ImNodes / ImPlot |
| Large property grids | Slow on interop | OK with virtualisation | Fast |
| Rewrite size | Total | Total | Incremental |
| Styling / text / a11y | Best | Good | Weakest |

## If a retained UI is still wanted: suggested spike

Small and throwaway, in `Tools/` (e.g. `Tools/AvaloniaViewportSpike`):

1. Avalonia window with a Dock.Avalonia layout: two panels and one viewport panel.
2. Veldrid Vulkan device renders a test scene into an exportable image each frame; Avalonia composites it.
3. Measure: frame time, resize behaviour, input latency, popup over the viewport, Linux.
4. Add one reflection-driven property grid over a real MSB part and check typing speed with a few hundred fields.

If step 2 or 3 fails, the switch is off. If both pass, plan a staged move editor by editor, starting with a
viewport-free one (Text or Param).

## Outcome

The goal is styling and text. Both can be handled on ImGui (1.92 dynamic fonts, FreeType, text roles, richer themes).
See `UI Styling and Text Plan.md`.
