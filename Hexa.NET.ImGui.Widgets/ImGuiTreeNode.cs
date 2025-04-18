namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Extensions;
    using System;
    using System.Diagnostics;
    using System.Numerics;

    public unsafe class ImGuiTreeNode
    {
        public static bool IconTreeNode(string label, string icon, Vector4 iconColor, ImGuiTreeNodeFlags flags)
        {
            int sizeInBytes0 = System.Text.Encoding.UTF8.GetByteCount(label);
            byte* pLabel;
            if (sizeInBytes0 + 1 >= 2048)
            {
                pLabel = AllocT<byte>(sizeInBytes0 + 1);
            }
            else
            {
                byte* stackLabel = stackalloc byte[sizeInBytes0 + 1];
                pLabel = stackLabel;
            }
            System.Text.Encoding.UTF8.GetBytes(label.AsSpan(), new Span<byte>(pLabel, sizeInBytes0));
            pLabel[sizeInBytes0] = 0;

            int sizeInBytes1 = System.Text.Encoding.UTF8.GetByteCount(icon);
            byte* pIcon;
            if (sizeInBytes1 + 1 >= 2048)
            {
                pIcon = AllocT<byte>(sizeInBytes1 + 1);
            }
            else
            {
                byte* stackLabel = stackalloc byte[sizeInBytes1 + 1];
                pIcon = stackLabel;
            }
            System.Text.Encoding.UTF8.GetBytes(icon.AsSpan(), new Span<byte>(pIcon, sizeInBytes1));
            pIcon[sizeInBytes1] = 0;

            bool result = IconTreeNode(pLabel, null, pIcon, iconColor, flags);

            if (sizeInBytes0 + 1 >= 2048)
            {
                Free(pLabel);
            }
            if (sizeInBytes1 + 1 >= 2048)
            {
                Free(pIcon);
            }

            return result;
        }

        public static bool IconTreeNode(byte* label, byte* labelEnd, byte* icon, Vector4 iconColor, ImGuiTreeNodeFlags flags)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            if (window->SkipItems != 0)
                return false;

            ImGuiContextPtr g = ImGui.GetCurrentContext();
            ImGuiStylePtr style = ImGui.GetStyle();
            float fontSize = ImGui.GetFontSize();
            float lineHeight = ImGui.GetTextLineHeight();

            bool display_frame = (flags & ImGuiTreeNodeFlags.Framed) != 0;
            Vector2 padding = display_frame || (flags & ImGuiTreeNodeFlags.FramePadding) != 0 ? style.FramePadding : new Vector2(style.FramePadding.X, Math.Min(window->DC.CurrLineTextBaseOffset, style.FramePadding.Y));

            uint id = ImGui.GetID(label);
            Vector2 pos = ImGui.GetCursorScreenPos();

            if (labelEnd == null)
                labelEnd = ImGuiP.FindRenderedTextEnd(label, (byte*)null);
            Vector2 labelSize = ImGui.CalcTextSize(label, labelEnd, false);

            float text_offset_x = fontSize * 2 + (display_frame ? padding.X * 3 : padding.X * 2);   // Collapsing arrow width + Spacing
            float text_offset_y = Math.Max(padding.Y, window->DC.CurrLineTextBaseOffset);            // Latch before ItemSize changes it
            float text_width = fontSize * 2 + labelSize.X + padding.X * 2;                         // Include collapsing arrow and icon

            // We vertically grow up to current line height up the typical widget height.
            float frame_height = Math.Max(Math.Min(window->DC.CurrLineSize.Y, g.FontSize + style.FramePadding.Y * 2), labelSize.Y + padding.Y * 2);
            bool span_all_columns = (flags & ImGuiTreeNodeFlags.SpanAllColumns) != 0 && !g.CurrentTable.IsNull;
            ImRect frame_bb;
            frame_bb.Min.X = span_all_columns ? window->ParentWorkRect.Min.X : (flags & ImGuiTreeNodeFlags.SpanFullWidth) != 0 ? window->WorkRect.Min.X : pos.X;
            frame_bb.Min.Y = pos.Y;
            frame_bb.Max.X = span_all_columns ? window->ParentWorkRect.Max.X : (flags & ImGuiTreeNodeFlags.SpanLabelWidth) != 0 ? pos.X + text_width + padding.X : window->WorkRect.Max.X;
            frame_bb.Max.Y = pos.Y + frame_height;

            if (display_frame)
            {
                float outer_extend = (int)(window->WindowPadding.X * 0.5f); // Framed header expand a little outside of current limits
                frame_bb.Min.X -= outer_extend;
                frame_bb.Max.X += outer_extend;
            }

            Vector2 text_pos = new(pos.X + text_offset_x, pos.Y + text_offset_y);
            Vector2 icon_pos = new(pos.X + text_offset_x - fontSize - padding.X, pos.Y + text_offset_y);
            ImGuiP.ItemSize(new Vector2(text_width, frame_height), padding.Y);

            ImRect interact_bb = frame_bb;
            if ((flags & (ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.SpanLabelWidth | ImGuiTreeNodeFlags.SpanAllColumns)) == 0)
                interact_bb.Max.X = frame_bb.Min.X + text_width + (labelSize.X > 0.0f ? style.ItemSpacing.X * 2.0f : 0.0f);

            uint storage_id = (g.NextItemData.HasFlags & ImGuiNextItemDataFlags.HasStorageId) != 0 ? g.NextItemData.StorageId : id;
            bool is_open = ImGuiP.TreeNodeUpdateNextOpen(storage_id, flags);

            bool is_visible;
            if (span_all_columns)
            {
                // Modify ClipRect for the ItemAdd(), faster than doing a PushColumnsBackground/PushTableBackgroundChannel for every Selectable..
                float backup_clip_rect_min_x = window->ClipRect.Min.X;
                float backup_clip_rect_max_x = window->ClipRect.Max.X;
                window->ClipRect.Min.X = window->ParentWorkRect.Min.X;
                window->ClipRect.Max.X = window->ParentWorkRect.Max.X;
                is_visible = ImGuiP.ItemAdd(interact_bb, id, &interact_bb, ImGuiItemFlags.None);
                window->ClipRect.Min.X = backup_clip_rect_min_x;
                window->ClipRect.Max.X = backup_clip_rect_max_x;
            }
            else
            {
                is_visible = ImGuiP.ItemAdd(interact_bb, id, &interact_bb, ImGuiItemFlags.None);
            }
            g.LastItemData.StatusFlags |= ImGuiItemStatusFlags.HasDisplayRect;
            g.LastItemData.DisplayRect = frame_bb;

            // If a NavLeft request is happening and ImGuiTreeNodeFlags_NavLeftJumpsBackHere enabled:
            // Store data for the current depth to allow returning to this node from any child item.
            // For this purpose we essentially compare if g.NavIdIsAlive went from 0 to 1 between TreeNode() and TreePop().
            // It will become tempting to enable ImGuiTreeNodeFlags_NavLeftJumpsBackHere by default or move it to ImGuiStyle.
            bool store_tree_node_stack_data = false;
            if ((flags & ImGuiTreeNodeFlags.NoTreePushOnOpen) == 0)
            {
                if ((flags & ImGuiTreeNodeFlags.NavLeftJumpsBackHere) != 0 && is_open && !g.NavIdIsAlive)
                    if (g.NavMoveDir == ImGuiDir.Left && g.NavWindow == window && ImGuiP.NavMoveRequestButNoResultYet())
                        store_tree_node_stack_data = true;
            }

            bool is_leaf = (flags & ImGuiTreeNodeFlags.Leaf) != 0;
            if (!is_visible)
            {
                if (store_tree_node_stack_data && is_open)
                    TreeNodeStoreStackData(flags); // Call before TreePushOverrideID()
                if (is_open && (flags & ImGuiTreeNodeFlags.NoTreePushOnOpen) != 0)
                    ImGuiP.TreePushOverrideID(id);

                return is_open;
            }

            if (span_all_columns)
            {
                ImGuiP.TablePushBackgroundChannel();
                g.LastItemData.StatusFlags |= ImGuiItemStatusFlags.HasClipRect;
                g.LastItemData.ClipRect = window->ClipRect;
            }

            ImGuiButtonFlags button_flags = (ImGuiButtonFlags)ImGuiTreeNodeFlags.None;
            if ((flags & ImGuiTreeNodeFlags.AllowOverlap) != 0 || (g.LastItemData.ItemFlags & (ImGuiItemFlags)ImGuiItemFlagsPrivate.AllowOverlap) != 0)
                button_flags |= (ImGuiButtonFlags)ImGuiButtonFlagsPrivate.AllowOverlap;
            if (!is_leaf)
                button_flags |= (ImGuiButtonFlags)ImGuiButtonFlagsPrivate.PressedOnDragDropHold;

            // We allow clicking on the arrow section with keyboard modifiers held, in order to easily
            // allow browsing a tree while preserving selection with code implementing multi-selection patterns.
            // When clicking on the rest of the tree node we always disallow keyboard modifiers.
            float arrow_hit_x1 = text_pos.X - text_offset_x - style.TouchExtraPadding.X;
            float arrow_hit_x2 = text_pos.X - text_offset_x + (g.FontSize + padding.X * 2.0f) + style.TouchExtraPadding.X;
            bool is_mouse_x_over_arrow = g.IO.MousePos.X >= arrow_hit_x1 && g.IO.MousePos.X < arrow_hit_x2;

            bool is_multi_select = (g.LastItemData.ItemFlags & (ImGuiItemFlags)ImGuiItemFlagsPrivate.IsMultiSelect) != 0;
            if (is_multi_select) // We absolutely need to distinguish open vs select so _OpenOnArrow comes by default
                flags |= (flags & (ImGuiTreeNodeFlags)ImGuiTreeNodeFlagsPrivate.OpenOnMask) == 0 ? ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick : ImGuiTreeNodeFlags.OpenOnArrow;

            // Open behaviors can be altered with the _OpenOnArrow and _OnOnDoubleClick flags.
            // Some alteration have subtle effects (e.g. toggle on MouseUp vs MouseDown events) due to requirements for multi-selection and drag and drop support.
            // - Single-click on label = Toggle on MouseUp (default, when _OpenOnArrow=0)
            // - Single-click on arrow = Toggle on MouseDown (when _OpenOnArrow=0)
            // - Single-click on arrow = Toggle on MouseDown (when _OpenOnArrow=1)
            // - Double-click on label = Toggle on MouseDoubleClick (when _OpenOnDoubleClick=1)
            // - Double-click on arrow = Toggle on MouseDoubleClick (when _OpenOnDoubleClick=1 and _OpenOnArrow=0)
            // It is rather standard that arrow click react on Down rather than Up.
            // We set ImGuiButtonFlags_PressedOnClickRelease on OpenOnDoubleClick because we want the item to be active on the initial MouseDown in order for drag and drop to work.
            if (is_mouse_x_over_arrow)
                button_flags |= (ImGuiButtonFlags)ImGuiButtonFlagsPrivate.PressedOnClick;
            else if ((flags & ImGuiTreeNodeFlags.OpenOnDoubleClick) != 0)
                button_flags |= (ImGuiButtonFlags)(ImGuiButtonFlagsPrivate.PressedOnClickRelease | ImGuiButtonFlagsPrivate.PressedOnDoubleClick);
            else
                button_flags |= (ImGuiButtonFlags)ImGuiButtonFlagsPrivate.PressedOnClickRelease;

            bool selected = (flags & ImGuiTreeNodeFlags.Selected) != 0;
            bool was_selected = selected;

            // Multi-selection support (header)
            if (is_multi_select)
            {
                // Handle multi-select + alter button flags for it
                ImGuiP.MultiSelectItemHeader(id, &selected, &button_flags);
                if (is_mouse_x_over_arrow)
                    button_flags = (ImGuiButtonFlags)(((ImGuiButtonFlagsPrivate)button_flags | ImGuiButtonFlagsPrivate.PressedOnClick) & ~ImGuiButtonFlagsPrivate.PressedOnClickRelease);
            }
            else
            {
                if (window != g.HoveredWindow || !is_mouse_x_over_arrow)
                    button_flags |= (ImGuiButtonFlags)ImGuiButtonFlagsPrivate.NoKeyModsAllowed;
            }

            bool hovered, held;
            bool pressed = ImGuiP.ButtonBehavior(interact_bb, id, &hovered, &held, button_flags);
            bool toggled = false;
            if (!is_leaf)
            {
                if (pressed && g.DragDropHoldJustPressedId != id)
                {
                    if ((flags & (ImGuiTreeNodeFlags)ImGuiTreeNodeFlagsPrivate.OpenOnMask) == 0 || g.NavActivateId == id && !is_multi_select)
                        toggled = true; // Single click
                    if ((flags & ImGuiTreeNodeFlags.OpenOnArrow) != 0)
                        toggled |= is_mouse_x_over_arrow && !g.NavHighlightItemUnderNav;  // Lightweight equivalent of IsMouseHoveringRect() since ButtonBehavior() already did the job
                    if ((flags & ImGuiTreeNodeFlags.OpenOnDoubleClick) != 0 && g.IO.MouseClickedCount_0 == 2)
                        toggled = true; // Double click
                }
                else if (pressed && g.DragDropHoldJustPressedId == id)
                {
                    Trace.Assert((button_flags & (ImGuiButtonFlags)ImGuiButtonFlagsPrivate.PressedOnDragDropHold) != 0);
                    if (!is_open) // When using Drag and Drop "hold to open" we keep the node highlighted after opening, but never close it again.
                        toggled = true;
                    else
                        pressed = false; // Cancel press so it doesn't trigger selection.
                }

                if (g.NavId == id && g.NavMoveDir == ImGuiDir.Left && is_open)
                {
                    toggled = true;
                    ImGuiP.NavClearPreferredPosForAxis(ImGuiAxis.X);
                    ImGuiP.NavMoveRequestCancel();
                }
                if (g.NavId == id && g.NavMoveDir == ImGuiDir.Right && !is_open) // If there's something upcoming on the line we may want to give it the priority?
                {
                    toggled = true;
                    ImGuiP.NavClearPreferredPosForAxis(ImGuiAxis.X);
                    ImGuiP.NavMoveRequestCancel();
                }

                if (toggled)
                {
                    is_open = !is_open;
                    ImGui.GetStateStorage().SetInt(storage_id, is_open ? 1 : 0);
                    g.LastItemData.StatusFlags |= ImGuiItemStatusFlags.ToggledOpen;
                }
            }

            ImDrawListPtr draw = ImGui.GetWindowDrawList();
            ImGuiStoragePtr store = ImGui.GetStateStorage();

            // Multi-selection support (footer)
            if (is_multi_select)
            {
                bool pressed_copy = pressed && !toggled;
                ImGuiP.MultiSelectItemFooter(id, &selected, &pressed_copy);
                if (pressed)
                    ImGuiP.SetNavID(id, window->DC.NavLayerCurrent, g.CurrentFocusScopeId, interact_bb);
            }

            if (selected != was_selected)
                g.LastItemData.StatusFlags |= ImGuiItemStatusFlags.ToggledSelection;

            // Render
            {
                uint text_col = ImGui.GetColorU32(ImGuiCol.Text);
                ImGuiNavRenderCursorFlags nav_render_cursor_flags = ImGuiNavRenderCursorFlags.Compact;
                if (is_multi_select)
                    nav_render_cursor_flags |= ImGuiNavRenderCursorFlags.AlwaysDraw; // Always show the nav rectangle
                if (display_frame)
                {
                    // Framed type
                    uint bg_col = ImGui.GetColorU32(held && hovered ? ImGuiCol.HeaderActive : hovered ? ImGuiCol.HeaderHovered : ImGuiCol.Header);
                    ImGuiP.RenderFrame(frame_bb.Min, frame_bb.Max, bg_col, true, style.FrameRounding);
                    ImGuiP.RenderNavCursor(frame_bb, id, nav_render_cursor_flags);
                    if ((flags & ImGuiTreeNodeFlags.Bullet) != 0)
                        ImGuiP.RenderBullet(draw, new(text_pos.X - text_offset_x * 0.60f, text_pos.Y + g.FontSize * 0.5f), text_col);
                    else if (!is_leaf)
                        ImGuiP.RenderArrow(draw, new Vector2(text_pos.X - text_offset_x + padding.X, text_pos.Y), text_col, is_open ? (flags & (ImGuiTreeNodeFlags)ImGuiTreeNodeFlagsPrivate.UpsideDownArrow) != 0 ? ImGuiDir.Up : ImGuiDir.Down : ImGuiDir.Right, 1.0f);
                    else // Leaf without bullet, left-adjusted text
                        text_pos.X -= text_offset_x - padding.X;
                    if ((flags & (ImGuiTreeNodeFlags)ImGuiTreeNodeFlagsPrivate.ClipLabelForTrailingButton) != 0)
                        frame_bb.Max.X -= g.FontSize + style.FramePadding.X;
                    if (g.LogEnabled)
                        ImGuiP.LogSetNextTextDecoration("###", "###");
                }
                else
                {
                    // Unframed typed for tree nodes
                    if (hovered || selected)
                    {
                        uint bg_col = ImGui.GetColorU32(held && hovered ? ImGuiCol.HeaderActive : hovered ? ImGuiCol.HeaderHovered : ImGuiCol.Header);
                        ImGuiP.RenderFrame(frame_bb.Min, frame_bb.Max, bg_col, false, style.FrameRounding);
                    }
                    ImGuiP.RenderNavCursor(frame_bb, id, nav_render_cursor_flags);
                    if ((flags & ImGuiTreeNodeFlags.Bullet) != 0)
                        ImGuiP.RenderBullet(draw, new(text_pos.X - text_offset_x * 0.5f, text_pos.Y + g.FontSize * 0.5f), text_col);
                    else if (!is_leaf)
                        ImGuiP.RenderArrow(draw, new Vector2(text_pos.X - text_offset_x + padding.X, text_pos.Y + g.FontSize * 0.15f), text_col, is_open ? (flags & (ImGuiTreeNodeFlags)ImGuiTreeNodeFlagsPrivate.UpsideDownArrow) != 0 ? ImGuiDir.Up : ImGuiDir.Down : ImGuiDir.Right, 0.70f);
                    if (g.LogEnabled)
                        ImGuiP.LogSetNextTextDecoration(">", (byte*)null);
                }

                if (span_all_columns)
                    ImGuiP.TablePopBackgroundChannel();

                ImGui.PushStyleColor(ImGuiCol.Text, iconColor);
                ImGuiP.RenderTextClipped(icon_pos, new Vector2(text_pos.X, frame_bb.Max.Y), icon, (byte*)null, default, default, default);
                ImGui.PopStyleColor();

                // Label
                if (display_frame)
                {
                    ImGuiP.RenderTextClipped(text_pos, frame_bb.Max, label, labelEnd, &labelSize, default, default);
                }
                else
                {
                    ImGuiP.RenderText(text_pos, label, labelEnd, false);
                }
            }

            if (store_tree_node_stack_data && is_open)
                TreeNodeStoreStackData(flags); // Call before TreePushOverrideID()
            if (is_open && (flags & ImGuiTreeNodeFlags.NoTreePushOnOpen) == 0)
                ImGuiP.TreePushOverrideID(id); // Could use TreePush(label) but this avoid computing twice

            return is_open;
        }

        private static void TreeNodeStoreStackData(ImGuiTreeNodeFlags flags)
        {
            ImGuiContext* g = ImGui.GetCurrentContext();
            ImGuiWindow* window = g->CurrentWindow;

            ImVector<ImGuiTreeNodeStackData>* treeNodeStack = &g->TreeNodeStack;
            treeNodeStack->Resize(treeNodeStack->Size + 1);
            ImGuiTreeNodeStackData* tree_node_data = treeNodeStack->Back;
            tree_node_data->ID = g->LastItemData.ID;
            tree_node_data->TreeFlags = flags;
            tree_node_data->ItemFlags = g->LastItemData.ItemFlags;
            tree_node_data->NavRect = g->LastItemData.NavRect;
            window->DC.TreeHasStackDataDepthMask = window->DC.TreeHasStackDataDepthMask | (uint)(1 << window->DC.TreeDepth);
        }
    }
}