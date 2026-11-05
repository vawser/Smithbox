using System;
using Veldrid;

namespace StudioCore.Renderer;

public interface IImguiRenderer
{
    public void OnSetupDone();
    public void RecreateFontDeviceTexture();

    /// <summary>
    /// The largest width or height the font atlas texture can have on this device.
    /// </summary>
    public int MaxFontTextureSize { get; }
    public void Update(float deltaSeconds, InputSnapshot snapshot, Action updateFontAction);
}
