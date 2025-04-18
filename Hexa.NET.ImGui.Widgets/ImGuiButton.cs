namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Extensions;
    using Hexa.NET.Utilities.Text;
    using System;
    using System.Numerics;

    public unsafe class ImGuiButton
    {
        public static bool ToggleSwitch(string label, ref bool selected)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            if (window->SkipItems != 0)
                return false;

            uint id = ImGui.GetID(label);

            float height = ImGui.GetFrameHeight();
            float width = height * 2f;
            float radius = height * 0.50f;

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 size = new(width, height);

            ImRect bb = new() { Min = pos, Max = pos + size };

            ImGuiP.ItemSize(bb, 0.0f);
            if (!ImGuiP.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            uint hoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
            uint backgroundColor = ImGui.GetColorU32(ImGuiCol.Button);
            uint activeColor = ImGui.GetColorU32(ImGuiCol.ButtonActive);
            uint selectedColor = ImGui.GetColorU32(ImGuiCol.TabSelectedOverline);
            uint selectedBgColor = ImGui.GetColorU32(ImGuiCol.TabSelected);

            ImDrawList* draw = ImGui.GetWindowDrawList();

            bool isHovered;
            bool isActive;
            bool isClicked = ImGuiP.ButtonBehavior(bb, id, &isHovered, &isActive, 0);

            float t = selected ? 1 : 0;

            if (isClicked)
            {
                selected = !selected;
                AnimationManager.AddAnimation(id, .35f, 1, AnimationType.EaseOutCubic);
            }

            float animationValue = AnimationManager.GetAnimationValue(id);
            if (animationValue != -1)
            {
                t = selected ? animationValue : (1 - animationValue);
            }

            var g = ImGui.GetCurrentContext();

            uint col_bg;
            if (isHovered)
                col_bg = ImGui.GetColorU32(ImGuiP.ImLerp(new Vector4(0.78f, 0.78f, 0.78f, 1.0f), new Vector4(0.64f, 0.83f, 0.34f, 1.0f), t));
            else
                col_bg = ImGui.GetColorU32(ImGuiP.ImLerp(new Vector4(0.85f, 0.85f, 0.85f, 1.0f), new Vector4(0.56f, 0.83f, 0.26f, 1.0f), t));

            draw->AddRectFilled(pos, new Vector2(pos.X + width, pos.Y + height), col_bg, height * 0.5f);
            draw->AddCircleFilled(new(pos.X + radius + t * (width - radius * 2.0f), pos.Y + radius), radius - 1.5f, 0xFFFFFFFF);

            return isClicked;
        }

        public static bool ToggleButton(string label, ref bool selected)
        {
            return ToggleButton(label, ref selected, default, ImGuiButtonFlags.None);
        }

        public static bool ToggleButton(string label, ref bool selected, Vector2 sizeArg)
        {
            return ToggleButton(label, ref selected, sizeArg, ImGuiButtonFlags.None);
        }

        public static bool ToggleButton(string label, ref bool selected, Vector2 sizeArg, ImGuiButtonFlags flags)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            if (window->SkipItems != 0)
                return false;

            uint id = ImGui.GetID(label);

            ImGuiStylePtr style = ImGui.GetStyle();

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 labelSize = ImGui.CalcTextSize(label, (byte*)null, true);
            if ((flags & (ImGuiButtonFlags)ImGuiButtonFlagsPrivate.AlignTextBaseLine) != 0 && style.FramePadding.Y < window->DC.CurrLineTextBaseOffset) // Try to vertically align buttons that are smaller/have no padding so that text baseline matches (bit hacky, since it shouldn't be a flag)
                pos.Y += window->DC.CurrLineTextBaseOffset - style.FramePadding.Y;
            Vector2 size = ImGuiP.CalcItemSize(sizeArg, labelSize.X + style.FramePadding.X * 2.0f, labelSize.Y + style.FramePadding.Y * 2.0f);

            ImRect bb = new() { Min = pos, Max = pos + size };

            ImGuiP.ItemSize(bb, 0.0f);
            if (!ImGuiP.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            uint hoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
            uint activeColor = ImGui.GetColorU32(ImGuiCol.ButtonActive);
            uint selectedColor = ImGui.GetColorU32(ImGuiCol.TabSelectedOverline);
            uint selectedBgColor = ImGui.GetColorU32(ImGuiCol.TabSelected);

            ImDrawList* draw = ImGui.GetWindowDrawList();

            bool isHovered;
            bool isActive;
            bool isClicked = ImGuiP.ButtonBehavior(bb, id, &isHovered, &isActive, 0);

            uint color = isActive ? activeColor : isHovered ? hoverColor : selected ? selectedBgColor : default;

            if (isActive || isHovered || selected)
            {
                draw->AddRectFilled(bb.Min, bb.Max, color, style.FrameRounding);
            }

            if (selected)
            {
                draw->AddRect(bb.Min, bb.Max, selectedColor, style.FrameRounding, 2);
            }

            ImGuiP.RenderTextClipped(bb.Min + style.FramePadding, bb.Max - style.FramePadding, label, (byte*)null, &labelSize, style.ButtonTextAlign, &bb);

            if (isClicked)
            {
                selected = !selected;
            }

            return isClicked;
        }

        public static bool TransparentButton(char label)
        {
            byte* buf = stackalloc byte[5];
            Utf8Formatter.ConvertUtf16ToUtf8(&label, 1, buf, 4);
            return TransparentButton(buf, default, ImGuiButtonFlags.None);
        }

        public static bool TransparentButton(byte* label)
        {
            return TransparentButton(label, default, ImGuiButtonFlags.None);
        }

        public static bool TransparentButton(ReadOnlySpan<byte> label)
        {
            fixed (byte* ptr = label)
            {
                return TransparentButton(ptr, default, ImGuiButtonFlags.None);
            }
        }

        public static bool TransparentButton(string label)
        {
            int sizeInBytes = System.Text.Encoding.UTF8.GetByteCount(label);
            byte* pLabel;
            if (sizeInBytes + 1 >= 2048)
            {
                pLabel = AllocT<byte>(sizeInBytes + 1);
            }
            else
            {
                byte* stackLabel = stackalloc byte[sizeInBytes + 1];
                pLabel = stackLabel;
            }
            System.Text.Encoding.UTF8.GetBytes(label.AsSpan(), new Span<byte>(pLabel, sizeInBytes));
            pLabel[sizeInBytes] = 0;
            bool result = TransparentButton(pLabel, default, ImGuiButtonFlags.None);
            if (sizeInBytes + 1 >= 2048)
            {
                Free(pLabel);
            }
            return result;
        }

        public static bool TransparentButton(byte* label, Vector2 sizeArg, ImGuiButtonFlags flags)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            if (window->SkipItems != 0)
                return false;

            uint id = ImGui.GetID(label);

            ImGuiStylePtr style = ImGui.GetStyle();

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 labelSize = ImGui.CalcTextSize(label, (byte*)null, true);
            if ((flags & (ImGuiButtonFlags)ImGuiButtonFlagsPrivate.AlignTextBaseLine) != 0 && style.FramePadding.Y < window->DC.CurrLineTextBaseOffset) // Try to vertically align buttons that are smaller/have no padding so that text baseline matches (bit hacky, since it shouldn't be a flag)
                pos.Y += window->DC.CurrLineTextBaseOffset - style.FramePadding.Y;
            Vector2 size = ImGuiP.CalcItemSize(sizeArg, labelSize.X + style.FramePadding.X * 2.0f, labelSize.Y + style.FramePadding.Y * 2.0f);

            ImRect bb = new() { Min = pos, Max = pos + size };
            ImGuiP.ItemSize(size, style.FramePadding.Y);
            if (!ImGuiP.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            uint hoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
            uint activeColor = ImGui.GetColorU32(ImGuiCol.ButtonActive);

            ImDrawList* draw = ImGui.GetWindowDrawList();

            bool hovered, held;
            bool pressed = ImGuiP.ButtonBehavior(bb, id, &hovered, &held, flags);

            ImGuiP.RenderNavCursor(bb, id, default);
            if (pressed || hovered || held)
            {
                uint col = ImGui.GetColorU32(held && hovered ? ImGuiCol.ButtonActive : hovered ? ImGuiCol.ButtonHovered : ImGuiCol.Button);
                ImGuiP.RenderFrame(bb.Min, bb.Max, col, true, style.FrameRounding);
            }

            ImGuiP.RenderTextClipped(bb.Min + style.FramePadding, bb.Max - style.FramePadding, label, (byte*)null, &labelSize, style.ButtonTextAlign, &bb);

            return pressed;
        }
    }
}