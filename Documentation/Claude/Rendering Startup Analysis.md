# Rendering Startup Analysis

Review of the path from `Program.Main` to the first presented frame, looking for crashes, hangs and
leaks. No code was changed.

## Startup flow

```
Program.Main (Smithbox.Vulkan/Program.cs)
 ├─ UnhandledException → CrashHandler
 ├─ Startup / LOC / Profiler setup
 └─ new Smithbox(version, Vulkan, Editor)                 Smithbox.cs:67
     ├─ SetupLogging, Setup (CFG, UI, InputManager)
     ├─ CreateGraphicsContext                             Smithbox.cs:163
     │   ├─ VulkanGraphicsContext.Initialize              VulkanGraphicsContext.cs:33
     │   │   ├─ VeldridStartup.CreateWindowAndGraphicsDevice
     │   │   │   ├─ CreateWindow (SDL, Maximized, CFG X/Y/W/H)
     │   │   │   └─ GraphicsDevice ctor                   GraphicsDevice.cs:204
     │   │   │       CreateInstance → surface → CreatePhysicalDevice → CreateLogicalDevice
     │   │   │       → VMA → per-frame data → Swapchain ctor → descriptor pools
     │   │   ├─ SceneRenderer.Initialize                  SceneRenderer.cs:92
     │   │   │   fences, samplers, geometry/instance/material/bone allocators,
     │   │   │   texture pools (32768 + 500 + 2048), default textures, debug meshes
     │   │   └─ new VulkanImGuiRenderer                   VulkanImGuiRenderer.cs:89
     │   │       font pool slot, ImGui/ImNodes/ImPlot contexts, pipeline, font atlas
     │   └─ on exception: dispose + close window, then OpenGLCompatGraphicsContext
     ├─ Orchestrator, DPI, SetupImGui, SetupFonts         Smithbox.cs:307
     ├─ ImguiRenderer.OnSetupDone (first NewFrame)
     └─ TextureManager, Soapstone
Smithbox.Run → loop: PumpEvents → Update (ImGui) → _context.Draw
     first Draw: _windowResized = true → ResizeMainWindow → swapchain recreate
```

## Findings

Ranked by how likely they are to hit users and how bad the result is.

### 1. A bad font setting crashes on every launch (high)

`Smithbox.cs:307-415`. `SetupFonts` calls `fonts.Clear()`, then adds the English font with
`MergeMode = false` and every other font with `MergeMode = true`. Each `Add*Font` silently skips a
missing file.

- If `English_Font` doesn't exist (a typo in the preferences text box, a moved file, a relative path
  that no longer resolves) but `Additional_Font_1` does (it ships by default), the first font added
  is a merge font. ImGui requires the first font in an atlas to be a non-merge font: it asserts, or
  reads `Fonts.back()` on an empty vector.
- If a chosen file exists but isn't a font ImGui can read, `AddFontFromMemoryTTF` fails and the
  atlas build fails.

The setting is saved to CFG, so this happens again on every launch. The only way out is to edit the
config by hand. The crash handler's "save CFG on initial-load crash" (`AttemptSaveOnCrash`) keeps
the bad value.

**Status: fixed.** `TryReadFontFile` checks that the file exists and has a TrueType/OpenType tag, and
logs a warning if not. When the English font can't be used, ImGui's default font is added as the
base font. The duplicate `ChineseFull` range was removed.

**Original suggestion:** if the English font is missing, add `fonts.AddFontDefault()` first (or fall back to the
default path), and only merge when `fonts.Fonts.Size > 0`. Wrap the build in a check that falls back
to the defaults and logs a warning.

### 2. Vulkan validation errors kill the process in DEBUG builds (high, DEBUG only)

`GraphicsDevice.cs:1747-1773`. `DebugCallback` is `[UnmanagedCallersOnly]` and does
`throw new VeldridException(...)` for Error severity. A managed exception can't unwind through an
`UnmanagedCallersOnly` frame back into the Vulkan loader, so the runtime fails fast: the process
ends with no crash log, `AppDomain.UnhandledException` may never run, and the OpenGL fallback
can't catch it. `gdOptions.Debug` is only set under `#if DEBUG` (`VulkanGraphicsContext.cs:51`), so
release builds are unaffected. A developer build hits this at the first validation error, including
any during startup (see 5 and 6).

**Status: fixed.** The callback logs to the console and debug output, records
`GraphicsDevice.LastValidationError` / `ValidationErrorCount`, and returns 0. The device-lost report
gets a "Validation" section when any validation errors occurred.

**Original suggestion:** log the message (and set a flag or queue it) inside the callback, then return 0. If a hard
failure is wanted, throw from managed code after the Vulkan call returns.

### 3. Swapchain creation can crash or hang when the window has no area at startup (medium)

`Swapchain.cs:72-106`.

- `CreateSwapchain` returns `false` without creating anything when the surface extent is 0×0. That
  happens when the window is minimized, or on a locked or disconnected RDP session. The constructor
  ignores the return value and calls `AcquireNextImage` on `VkSwapchainKHR.Null`, which is invalid
  usage and usually crashes the driver.
- If that first `vkAcquireNextImageKHR` returns `ErrorOutOfDateKHR` or `SuboptimalKHR`,
  `AcquireNextImage` recreates the swapchain and returns `false`. The fence wasn't signalled
  (out-of-date), but the constructor still calls `vkWaitForFences(..., ulong.MaxValue)`, so startup
  hangs forever. `RecreateAndReacquire` (line 140) gets this right by only waiting when the acquire
  succeeded.

**Status: fixed.** A minimized window is maximized before the device is created
(`VulkanGraphicsContext.Initialize`). The swapchain constructor throws a `VeldridException` when the
surface has no area (which leads to the OpenGL fallback instead of a crash). The first acquire is
retried up to 3 times, and the constructor only waits on the fence after an image is acquired.
`VkSwapchainFramebuffer` now reallocates its image array when the image count changes. A swapchain
can't be created later on demand, because `SwapchainFramebuffer.OutputDescription` (used by the ImGui
pipeline) only exists once a swapchain does.

**Original suggestion:** in the constructor, wait on the fence only if `AcquireNextImage` returned `true`. If
`CreateSwapchain` returned `false`, leave the swapchain null and let the first resize create it.
Also have `SwapBuffersCore` skip presenting when `DeviceSwapchain` is null.

### 4. The Vulkan → OpenGL fallback can leave the Vulkan window open (medium)

`Smithbox.cs:183-195`. `vulkanContext.Dispose()` and `Window.Close()` share one `try`. If
`Dispose` throws, `Close` is skipped and the OpenGL window opens next to a dead Vulkan window.
`GraphicsDevice.Dispose` calls `WaitForIdle`, which throws through `CheckDeviceLost`, which is
likely when the failure was device-related.

The fallback also leaks partial state:
- `SceneRenderer`'s statics (`Device`, the allocators, the texture pools, `DeviceLostContext`) keep
  pointing at the disposed device. Editors are gated on `CurrentBackend`, so this is latent, but
  anything that checks `SceneRenderer.Device != null` would accept it.
- If `VulkanImGuiRenderer`'s constructor throws after `ImGui.CreateContext` (for example in
  `CreateDeviceResources` → `StaticResourceCache.GetShaders` when a shader is missing), its ImGui,
  ImNodes and ImPlot contexts and the partly created GPU resources are never freed:
  `_imGuiRenderer` was never assigned, so `Dispose` doesn't see it.

**Status: fixed.** The window is closed in a `finally` with its own `try`. `SceneRenderer.Device` is
reset to null, which is the same state as a plain OpenGL launch. `VulkanImGuiRenderer` moves its
setup into `Setup()`. If that throws, it destroys the ImGui, ImNodes and ImPlot contexts and disposes
what it created (`Dispose` is now null-safe). The other `SceneRenderer` allocators still point at the
disposed device.

**Original suggestion:** close the window in its own `try`/`finally`. Reset `SceneRenderer.Device = null` on
fallback. Have `VulkanImGuiRenderer` destroy its contexts if construction fails.

### 5. Device teardown leaves Vulkan objects alive (low-medium)

`GraphicsDevice.cs:1338-1360, 2419-2426`. `PlatformDispose` never calls `PerFrameData.Destroy`
(no `.Destroy(this)` call exists) and never disposes `_asyncTransferCommandListPool` or
`_stagingPool`. Their command pools and VMA buffers are still alive when `vmaDestroyAllocator` and
`vkDestroyDevice` run. On the fallback path (4), `SceneRenderer`'s allocations are also alive.
Release VMA skips its "allocations not freed" assert, so this is mostly a leak plus validation
errors.

**Status: fixed in Veldrid.** `PlatformDispose` now disposes the swapchain and staging pool, then
waits again so the frame resets free what those two queued for destruction. Only then does it destroy
the per-frame command pools and the async transfer pool, and finally the fences, the allocator and the
device. `SceneRenderer`'s own allocations are still never freed (it has no teardown), so VMA still
sees those on exit and on the fallback path. In DEBUG, those validation errors run into finding 2, though the messenger is destroyed
before the allocator, which limits it to errors reported earlier.

### 6. A config value can make startup buffer creation fail (low-medium)

`SceneRenderer.cs:135`: `BoneBufferAllocator` is sized `CFG.Current.Viewport_Limit_Buffer_Flver_Bone * 64`
(uint). The preference is a free `ImGui_InputUint` (`ViewportPrefs.cs:149`).
- 0 → `vkCreateBuffer` with size 0, which is invalid usage. Drivers may return an error or crash.
- Above 67,108,863 → uint overflow wraps to a small or zero size.

The error happens inside `SceneRenderer.Initialize`, so Vulkan "fails" and the app silently drops
to OpenGL, with nothing telling the user it was a preference. `Viewport_Limit_Buffer_Indirect_Draw`
is already guarded with `Math.Max`. Clamp the bone limit the same way, to roughly
`[1024, 4,194,304]`.

**Status: fixed.** The limit is clamped to `[1024, 4,194,304]` bones (64 KiB to 256 MiB) where the
buffer is created, with a warning when the configured value is out of range. The CFG value itself is
left unchanged.

### 7. GPU selection stops at the first match even if it can't present (low)

`GraphicsDevice.cs:1775-1865, 1917-1923`. `CreatePhysicalDevice` picks the first discrete GPU (then
any GPU) that has the required features, but doesn't check that a queue family can present to the
surface or that `VK_KHR_swapchain` exists. If that GPU can't present to this window (some
multi-GPU or eGPU setups), `CreateLogicalDevice` throws "Could not find graphics queue with compute
and present" and the app drops to OpenGL, even though another GPU would have worked.

**Fix:** move the present and swapchain-extension checks into the selection loop and `continue` to
the next device.

**Status: fixed.** `CanPresent` checks for `VK_KHR_swapchain` and for a graphics+compute queue family
that can present to the surface, inside the selection loop. Each skipped GPU and its reason are
listed in the "Could not find a supported GPU" exception, which reaches the log through the OpenGL
fallback warning.

### 8. Window placement isn't checked against the monitors (low)

`VulkanGraphicsContext.cs:41-48, 102-107`. X/Y come straight from CFG and are saved on every
`Moved` event. If the last monitor used is disconnected, or Windows reported the minimized position
(-32000, -32000), the window is created off-screen. `Maximized` usually rescues the first frame,
but restoring the window sends it back off-screen. Clamp to `SDL_GetDisplayBounds` (or reset to
`SDL_WINDOWPOS_CENTERED`) when the rectangle doesn't overlap any display. Width and height are
already protected from 0 by the `width == 0 || height == 0` check in `Draw`.

**Status: fixed.** `WindowPlacement.Sanitize` (Renderer/Context/WindowPlacement.cs) runs before both
contexts create their window:
- A saved size under 200 px resets to 1280×720.
- If the top 100×50 px of the window isn't on any display, the window is centred on the primary
  display's usable area and shrunk to fit it.

Both contexts no longer save the position while the window is minimized or hidden. The OpenGL
context also no longer saves a 0 size; it had no such check before.

### 9. The font atlas can get very large at high DPI (low, needs checking)

`Smithbox.cs:362-376`. Every additional font adds JP + **ChineseFull twice** + Korean + Thai +
Vietnamese + Cyrillic, with oversample 2×2. With the default Noto CJK font at a large
`Interface_Font_Size × DPI.UIScale()`, the atlas height could go over `maxImageDimension2D`
(16384 on most GPUs), and `CreateTexture` in `RecreateFontDeviceTexture` would fail. A rebuild after
a DPI change runs the same path in the middle of a frame. The duplicate `ChineseFull` line should be
removed either way. Its glyphs are deduplicated, so it costs build time, not atlas space.

To check: log `width × height` from `GetTexDataAsRGBA32` in `RecreateFontDeviceTexture` at 200%
scale with the largest font size the preferences allow.

**Status: fixed.** `SetupFonts` builds the atlas itself and checks it against
`IImguiRenderer.MaxFontTextureSize`: Vulkan's `maxImageDimension2D`, through a new
`GraphicsDevice.MaxTextureDimension2D`, or OpenGL's `GL_MAX_TEXTURE_SIZE`. Until the atlas fits, it
rebuilds in this order, logging which step was used:
1. With ImGui's default layout.
2. As wide as the GPU allows.
3. Without oversampling.
4. Without the additional fonts.

The duplicate `ChineseFull` range was removed (with fix 1), and the `ImFontConfig` is freed after
each build.

While adding `MaxTextureDimension2D` I found that `GraphicsDevice._physicalDeviceProperties` was never
assigned, so `UniformBufferMinOffsetAlignment` and `StructuredBufferMinOffsetAlignment` were always
0. Only CommandList's dynamic-offset validation reads them, where `% 0` would throw. The field is now
set when the GPU is selected.

### Minor or cosmetic

- `VeldridStartup.CreateWindow` always sets `WindowFlags.Opengl`, even for the Vulkan window. It
  works, but it loads the GL driver needlessly and can make window creation fail on machines with
  broken GL drivers, which then breaks the Vulkan path too.
- `SDL.CreateWindow` returning null isn't checked (`Sdl2Window.cs:76`), and neither is
  `SDL.GetWindowWMInfo`'s result (`VeldridStartup.cs:100`). Either leads to an unclear
  `PlatformNotSupportedException` instead of the SDL error.
- `Swapchain.CreateSwapchain` line 205: the sRGB fallback test compares `surfaceFormat.format`,
  which is always `Undefined` at that point, so it always throws when `_colorSrgb` is set. Dead code
  today (`_colorSrgb = false`).
- `AcquireNextImage` calls `SetImageIndex` before checking the result, so a failed acquire leaves
  a garbage index (`Swapchain.cs:126`).
- `VulkanImGuiRenderer.Dispose` isn't null-safe (`_vertexBuffer.Dispose()` etc.). Only reachable if
  construction failed part way.
- `SetupFonts` allocates a new `ImFontConfig` and leaks the `BuildRanges` vectors on every rebuild.
  This is small.
- `RenderQueue.KeyIndex.CompareTo(object)` compares `Key` against the boxed `KeyIndex` instead of
  its key. The generic overload is the one `List.Sort` uses, so it never runs today.

## Not bugs (checked)

- The early `return` in `VulkanGraphicsContext.Draw` on a 0-size resize skips `ImGui.Render`, but
  `BeginUpdate` renders a frame that is still open before the next `NewFrame`, so ImGui stays
  balanced.
- `PerFrameData.Reset` waits on its submission fences before freeing anything, so
  `WaitForIdleCore` calling `NextFrame` before `vkDeviceWaitIdle` is safe.
- The `CFG.Viewport_Frame_Rate` divide-by-zero is already clamped in `Run`.

## Suggested order

1 (crash loop for users) → 3 (swapchain hang/crash) → 2 (DEBUG fail-fast) → 4 + 6 (silent drop to
OpenGL) → the rest.
