using System;
using Veldrid;

namespace StudioCore.Graphics;

public interface IImguiRenderer
{
    public void OnSetupDone();
    public void RecreateFontDeviceTexture();
    public void Update(float deltaSeconds, InputSnapshot snapshot, Action updateFontAction);
}
