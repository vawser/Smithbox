namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui.Widgets.Extensions;
    using System;
    using System.Numerics;
    using System.Text;

    public static unsafe class ImGuiBreadcrumb
    {
        private static readonly char[] Separators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

        private struct BreadcrumbState
        {
            public bool Breadcrumbs;
            public int Offset;
            public float Width;
            public int Length;
        }

        private static BreadcrumbState* GetState(uint id)
        {
            if (!ImGuiGC.KeepAlive<BreadcrumbState>(id, out var ptr))
            {
                ptr = ImGuiGC.Alloc<BreadcrumbState>(id);
                ZeroMemoryT(ptr); // gc does not zero memory, just manages it.
            }
            return ptr;
        }

        public static bool Breadcrumb(string strId, ref string path)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();

            if (window->SkipItems != 0)
            {
                return false;
            }

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 cursor = ImGui.GetCursorPos();
            Vector2 avail = ImGui.GetContentRegionAvail();
            Vector2 spacing = ImGui.GetStyle().FramePadding;

            float lineHeight = ImGui.GetTextLineHeight();
            float width = avail.X - 200 - spacing.X;
            bool handled = false;

            ImGuiStylePtr style = ImGui.GetStyle();

            uint id = ImGui.GetID(strId);

            var stateStorage = GetState(id);

            bool* breadcrumbs = &stateStorage->Breadcrumbs;

            bool changed = false;

            if (*breadcrumbs)
            {
                Vector2 size = new(width, 0);
                Vector2 label_size = new(0, lineHeight);
                Vector2 frame_size = ImGuiP.CalcItemSize(size, ImGui.CalcItemWidth(), label_size.Y + style.FramePadding.Y * 2.0f); // Arbitrary default of 8 lines high for multi-line
                Vector2 total_size = new(frame_size.X, frame_size.Y);

                ImRect frame_bb = new(pos, pos + frame_size);
                ImRect total_bb = new(frame_bb.Min, frame_bb.Min + total_size);

                if (!ImGuiP.ItemAdd(total_bb, id, &frame_bb, ImGuiItemFlags.None))
                {
                    ImGuiP.ItemSize(total_bb, style.FramePadding.Y);
                    return false;
                }

                ImGuiP.RenderNavCursor(frame_bb, id, ImGuiNavRenderCursorFlags.None);
                ImGuiP.RenderFrame(frame_bb.Min, frame_bb.Max, ImGui.GetColorU32(ImGuiCol.FrameBg), true, ImGui.GetStyle().FrameRounding);
                ImGui.PushClipRect(total_bb.Min, total_bb.Max, true);

                ushort separator = (byte)'>' << 0 | 0 << 8; // A very cheap string.
                float separatorWidth = ImGui.CalcTextSize((byte*)&separator).X;

                ulong dotdotdot = (byte)'.' << 0 | (byte)'.' << 8 | (byte)'.' << 16 | 0 << 24;
                float dotdotdotWidth = ImGui.CalcTextSize((byte*)&dotdotdot).X + style.FramePadding.X * 2f + style.ItemSpacing.X; // doooooooooooooooooooooooooot

                const int MaxCharacters = 1024;
                Span<byte> partBuffer = stackalloc byte[MaxCharacters]; // 1024 is enough for most path parts + id + null terminator

                ReadOnlySpan<char> part = path.AsSpan();

                float widthAvail = total_size.X;

                int offset = stateStorage->Offset;
                if (stateStorage->Width != widthAvail || stateStorage->Length != path.Length)
                {
                    stateStorage->Length = path.Length;
                    stateStorage->Width = widthAvail;

                    float currentWidth = 0;
                    int lastSafeOffset = -1;
                    offset = part.Length; // start from the end
                                          // reverse search for separators, to find turncation point.
                    while (part.Length > 0)
                    {
                        int index = part.LastIndexOfAny(Separators);

                        var partBase = part[(index + 1)..]; // skip separator
                        offset -= partBase.Length; // with the separator

                        if (partBase.IsEmpty) // fix for linux machines.
                        {
                            partBuffer[0] = (byte)'/';
                            partBuffer[1] = 0;
                        }
                        else
                        {
                            int idx = Encoding.UTF8.GetBytes(partBase, partBuffer);
                            partBuffer[idx] = 0;
                        }

                        float partWidth = ImGui.CalcTextSize(partBuffer).X + style.FramePadding.X * 2f;

                        currentWidth += partWidth;

                        if (currentWidth > widthAvail)
                        {
                            offset += partBase.Length + 1; // re add the offset, so we skip this part, since we are out of bounds.
                            if (lastSafeOffset != -1)
                            {
                                offset = lastSafeOffset;
                            }
                            break;
                        }

                        if (currentWidth > widthAvail - dotdotdotWidth && lastSafeOffset == -1)
                        {
                            lastSafeOffset = offset + partBase.Length + 1;
                        }

                        if (index > 0)
                        {
                            currentWidth += separatorWidth + style.ItemSpacing.X + style.ItemSpacing.X;
                        }

                        if (index == -1) // break out here, makes more sense that exiting prematurely, after indexing directly.
                        {
                            break;
                        }

                        offset--; // remove the last separator

                        part = part[..(index)];
                    }

                    stateStorage->Offset = offset;
                }

                if (offset < path.Length)
                {
                    part = path.AsSpan()[offset..];

                    int idxBase = offset;

                    if (offset > 0)
                    {
                        if (ImGuiButton.TransparentButton((byte*)&dotdotdot))
                        {
                            handled = true;
                        }
                        if (ImGui.IsItemHovered())
                        {
                            handled = true;
                        }
                        ImGui.SameLine();
                    }

                    int partId = 0;
                    while (part.Length > 0)
                    {
                        int index = part.IndexOfAny(Separators);
                        if (index == -1)
                        {
                            index = part.Length;
                        }

                        idxBase += index + 1;

                        var partBase = part[..index];
                        int idx = 0;
                        if (partBase.IsEmpty) // fix for linux machines.
                        {
                            partBuffer[0] = (byte)'/';
                            partBuffer[1] = 0;
                            idx = 1;
                        }
                        else
                        {
                            idx = Encoding.UTF8.GetBytes(partBase, partBuffer);
                            idx = Math.Min(idx, MaxCharacters - 4);
                            partBuffer[idx] = (byte)'#';
                            partBuffer[idx + 1] = (byte)'#';
                            partBuffer[idx + 2] = (byte)partId;
                            partBuffer[idx + 3] = 0;
                        }

                        if (ImGuiButton.TransparentButton(partBuffer))
                        {
                            handled = true;
                            if (idxBase < path.Length)
                            {
                                path = path[..idxBase];
                                changed = true;
                            }
                        }
                        if (ImGui.IsItemHovered())
                        {
                            handled = true;
                        }

                        if (index + 1 >= part.Length)
                        {
                            break;
                        }

                        part = part[(index + 1)..];

                        ImGui.SameLine();
                        ImGui.Text(">"u8);
                        ImGui.SameLine();
                        partId++;
                    }
                }
                ImGui.PopClipRect();

                ImGui.SetCursorPos(cursor);
                ImGuiP.ItemSize(total_bb, style.FramePadding.Y);
            }

            bool switched = false;
            if (!handled && ImGui.IsMouseHoveringRect(pos, pos + new Vector2(width, lineHeight)))
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.TextInput);
                if (*breadcrumbs && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    *breadcrumbs = !*breadcrumbs;
                    switched = true;
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, 0); // make it transparent to avoid flickering
                }
            }

            if (!*breadcrumbs)
            {
                ImGui.SetCursorPos(cursor);
                ImGui.PushItemWidth(width);
                changed |= ImGui.InputText($"##{strId}", ref path, 1024);
                ImGui.PopItemWidth();
                if (!ImGui.IsItemActive())
                {
                    *breadcrumbs = true;
                }

                if (switched)
                {
                    ImGui.PopStyleColor();
                }

                return changed;
            }

            return changed;
        }
    }
}