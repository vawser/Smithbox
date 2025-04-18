namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.Utilities;
    using Hexa.NET.Utilities.Text;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public unsafe class TextEditorTab
    {
        private readonly ImGuiName name;
        private bool open = true;
        private bool isFocused = false;
        private TextSource source;
        private TextHistory history;
        private readonly TextDrawData drawData = new();
        private bool needsUpdate = true;
        private JumpTarget? jumpTo;

        public TextEditorTab(string tabName, TextSource source)
        {
            name = new(tabName);
            this.source = source;
            history = new(source, 1024);
        }

        public TextEditorTab(TextSource source, string currentFile)
        {
            name = new(Path.GetFileName(currentFile));
            CurrentFile = currentFile;
            this.source = source;
            history = new(source, 1024);
        }

        public string TabName => name.Name;

        public string? CurrentFile { get; set; }

        public TextSource Source { get => source; set => source = value; }

        public TextHistory History { get => history; set => history = value; }

        public List<Breakpoint> Breakpoints => breakpoints;

        public bool IsOpen { get => open; set => open = value; }

        public bool IsFocused => isFocused;

        public ImFontPtr Font { get => font; set => font = value; }

        public SyntaxHighlight? SyntaxHighlight
        {
            get => syntaxHighlight;
            set
            {
                syntaxHighlight = value;
                needsUpdate = true;
            }
        }

        private readonly List<Breakpoint> breakpoints = [];

        private CursorState cursorState;

        private readonly List<TextSelection> selections = [];

        public void Draw(Vector2 size)
        {
            isFocused = false;
            ImGuiTabItemFlags flags = source.Changed ? ImGuiTabItemFlags.UnsavedDocument : ImGuiTabItemFlags.None;
            if (ImGui.BeginTabItem(name.UniqueName, ref open, flags))
            {
                isFocused = true;
                StdWString* text = source.Text;

                ImGui.PushFont(font);

                ImGui.BeginChild("##TextEditorChild", size, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                Vector2 avail = ImGui.GetContentRegionAvail();
                DrawEditor("##TextEditor", text, avail);
                ImGui.EndChild();

                ImGui.SameLine();
                DrawFindWindow();

                ImGui.PopFont();

                ImGui.EndTabItem();
            }
        }

        public enum TextFindFlags
        {
            None = 0,
            CaseSensitive = 1,
            WholeWorld = 2,
            Regex = 4,
            Selection = 8,
        }

        public enum FindWindowFlags
        {
            None = 0,
            Visible = 1,
            ReplaceVisible = 2
        }

        private FindWindowFlags findWindowFlags;
        private string searchText = string.Empty;
        private string replaceText = string.Empty;

        private TextFindFlags findFlags;

        private bool DrawFindWindow()
        {
            if ((findWindowFlags & FindWindowFlags.Visible) == 0)
            {
                return false;
            }

            ImGuiStylePtr style = ImGui.GetStyle();

            // Get the font size
            float fontSize = ImGui.GetFontSize();

            // Calculate the frame height
            Vector2 framePadding = style.FramePadding;
            Vector2 itemSpacing = style.ItemSpacing;

            const float padding = 40;
            const float width = 500;

            bool replaceVisible = (findWindowFlags & FindWindowFlags.ReplaceVisible) != 0;

            var cursor = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new(cursor.X - width - padding, cursor.Y));
            ImGui.PushStyleColor(ImGuiCol.ChildBg, 0xff181818);
            if (ImGui.BeginChild($"Find##{name.Id}", new Vector2(width, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY))
            {
                ImGui.PopStyleColor();

                if (IconButton(replaceVisible ? $"{MaterialIcons.ArrowDropDown}" : $"{MaterialIcons.ArrowRight}", false))
                {
                    InvertFlagInt32(ref findWindowFlags, FindWindowFlags.ReplaceVisible);
                }

                ImGui.SameLine();

                var baseCursor = ImGui.GetCursorPos();

                bool updateSearch = ImGui.InputTextEx("##SearchText", "Find...", ref searchText, 1024, new(200, 0), 0, null, null);

                ImGui.SameLine();
                if (IconButton($"{MaterialIcons.MatchCase}", (findFlags & TextFindFlags.CaseSensitive) != 0))
                {
                    InvertFlagInt32(ref findFlags, TextFindFlags.CaseSensitive);
                    updateSearch = true;
                }
                ImGui.SameLine();
                if (IconButton($"{MaterialIcons.MatchWord}", (findFlags & TextFindFlags.WholeWorld) != 0))
                {
                    InvertFlagInt32(ref findFlags, TextFindFlags.WholeWorld);
                    updateSearch = true;
                }
                ImGui.SameLine();
                if (IconButton($"{MaterialIcons.RegularExpression}", (findFlags & TextFindFlags.Regex) != 0))
                {
                    InvertFlagInt32(ref findFlags, TextFindFlags.Regex);
                    updateSearch = true;
                }

                ImGui.SameLine();
                if (IconButton($"{MaterialIcons.ArrowUpward}")) // goes upwards in the search
                {
                    findIndex--;
                    if (findIndex < 0)
                    {
                        findIndex = findResults.Count - 1;
                    }
                    JumpTo(findResults[findIndex]);
                }
                ImGui.SameLine();
                if (IconButton($"{MaterialIcons.ArrowDownward}")) // goes downwards in the search
                {
                    findIndex++;
                    if (findIndex >= findResults.Count)
                    {
                        findIndex = 0;
                    }
                    JumpTo(findResults[findIndex]);
                }
                ImGui.SameLine();
                if (IconButton($"{MaterialIcons.Subject}", (findFlags & TextFindFlags.Selection) != 0))
                {
                    InvertFlagInt32(ref findFlags, TextFindFlags.Selection);
                    updateSearch = true;
                }
                ImGui.SameLine();
                if (IconButton($"{MaterialIcons.Close}"))
                {
                    InvertFlagInt32(ref findWindowFlags, FindWindowFlags.Visible);
                    updateSearch = true;
                }

                if (replaceVisible)
                {
                    ImGui.SetCursorPosX(baseCursor.X);
                    ImGui.InputTextEx("##ReplaceText", "Replace...", ref replaceText, 1024, new(200, 0), 0, null, null);
                    ImGui.SameLine();
                    if (IconButton($"{MaterialIcons.FindReplace}")) // replace first.
                    {
                        PerformReplace(0);
                    }
                    ImGui.SameLine();
                    if (IconButton($"{MaterialIcons.SwapHoriz}")) // replace all.
                    {
                        PerformReplaceAll();
                    }
                }

                if (updateSearch)
                {
                    PerformFind();
                }
            }
            else
            {
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.SetCursorPos(cursor);

            return true;
        }

        private readonly List<SearchResult> findResults = [];
        private int findIndex = -1;

        private void PerformFind()
        {
            findResults.Clear();
            if (searchText.Length == 0)
            {
                findIndex = -1;
                return;
            }

            if ((findFlags & TextFindFlags.Regex) != 0)
            {
                RegexOptions options = RegexOptions.Multiline | RegexOptions.Compiled;
                if ((findFlags & TextFindFlags.CaseSensitive) == 0)
                {
                    options |= RegexOptions.IgnoreCase;
                }
                if ((findFlags | TextFindFlags.WholeWorld) == 0)
                {
                    options |= RegexOptions.ExplicitCapture;
                }

                int startPos = 0;
                int textLength = source.Text->Size;
                if ((findFlags & TextFindFlags.Selection) != 0)
                {
                    startPos = Math.Max(0, selection.EffectiveStart);
                    textLength = Math.Min(textLength, selection.Length);
                }

                foreach (var match in Regex.EnumerateMatches(source.Text->AsReadOnlySpan().Slice(startPos, textLength), searchText, options))
                {
                    var idx = match.Index;
                    var len = match.Length;

                    SearchResult span = SearchResult.FromIndex(source.Text, idx, len, source);
                    findResults.Add(span);
                }
            }
            else
            {
                bool caseSensitive = (findFlags & TextFindFlags.CaseSensitive) != 0;
                bool wholeWord = (findFlags & TextFindFlags.WholeWorld) != 0;
                bool selectionOnly = (findFlags & TextFindFlags.Selection) != 0;

                int textLength = source.Text->Size;
                char* pText = source.Text->CStr();

                int startPos = 0;
                if (selectionOnly)
                {
                    startPos = Math.Max(0, selection.EffectiveStart);
                    textLength = Math.Min(textLength, selection.Length);
                }

                IEqualityComparer<char> comparer = caseSensitive ? EqualityComparer<char>.Default : CaseInsensitiveComparer.Default;

                int pos = startPos;
                fixed (char* pSearchText = searchText)
                {
                    while ((pos = Find(pText, textLength, pSearchText, searchText.Length, pos, comparer)) != -1)
                    {
                        if (wholeWord && !IsWholeWordMatch(pText, pos, searchText.Length))
                        {
                            pos++;
                            continue;
                        }

                        SearchResult span = SearchResult.FromIndex(source.Text, pos, searchText.Length, source);
                        findResults.Add(span);
                        pos += searchText.Length;
                    }
                }
            }

            if (findIndex != -1)
            {
                findIndex = Math.Clamp(findIndex, 0, findResults.Count - 1);
            }
        }

        private void PerformReplaceAll()
        {
            if (findResults.Count == 0)
                return;

            PreEdit(TextEditOp.Replace);
            // Iterate in reverse order to maintain valid indices after each replacement
            for (int i = findResults.Count - 1; i >= 0; i--)
            {
                SearchResult result = findResults[i];

                // Erase the found substring
                result.String->Erase(result.Start, result.Length);

                // Insert the replacement text
                result.String->InsertRange(result.Start, replaceText);
            }
            PostEdit();
            needsUpdate = true;
        }

        private void PerformReplace(int index)
        {
            if (findResults.Count == 0)
                return;

            PreEdit(TextEditOp.Replace);

            SearchResult result = findResults[index];

            // Erase the found substring
            result.String->Erase(result.Start, result.Length);

            // Insert the replacement text
            result.String->InsertRange(result.Start, replaceText);

            PostEdit();
            needsUpdate = true;
        }

        private bool IsWholeWordMatch(char* pText, int index, int length)
        {
            bool startIsWordBoundary = index == 0 || !char.IsLetterOrDigit(pText[index - 1]);
            bool endIsWordBoundary = index + length >= source.Text->Size || !char.IsLetterOrDigit(pText[index + length]);

            return startIsWordBoundary && endIsWordBoundary;
        }

        public void ClearFind()
        {
            findResults.Clear();
            searchText = string.Empty;
            findIndex = -1;
        }

        private static bool VerticalCollapseButton(string label)
        {
            return false;
        }

        private static bool IconButton(string label, bool selected = false)
        {
            uint textColor = ImGui.GetColorU32(ImGuiCol.Text);
            uint hoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
            uint activeColor = ImGui.GetColorU32(ImGuiCol.ButtonActive);
            uint selectedColor = ImGui.GetColorU32(ImGuiCol.TabSelectedOverline);
            uint selectedBgColor = ImGui.GetColorU32(ImGuiCol.TabSelected);
            uint id = ImGui.GetID(label);

            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            ImDrawList* draw = ImGui.GetWindowDrawList();
            ImGuiStylePtr style = ImGui.GetStyle();

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 size = ImGui.CalcTextSize(label);
            Vector2 padding = style.FramePadding - new Vector2(style.FrameBorderSize * 2);
            ImRect bb = new() { Min = pos + new Vector2(padding.X, 0), Max = new(pos.X + size.X, pos.Y + size.Y) };
            ImRect bbFull = new() { Min = pos, Max = new Vector2(pos.X + size.X, pos.Y + size.Y) + padding * 2 };

            ImGuiP.ItemSize(bbFull, 0.0f);
            if (!ImGuiP.ItemAdd(bbFull, id, &bbFull, ImGuiItemFlags.None))
                return false;

            bool isHovered = false;
            bool isClicked = ImGuiP.ButtonBehavior(bbFull, id, &isHovered, null, 0);
            bool isActive = isHovered && ImGui.IsMouseDown(0);

            uint color = isActive ? activeColor : isHovered ? hoverColor : selected ? selectedBgColor : default;

            if (isActive || isHovered || selected)
            {
                draw->AddRectFilled(bbFull.Min, bbFull.Max, color, style.FrameRounding);
            }

            if (selected)
            {
                draw->AddRect(bbFull.Min, bbFull.Max, selectedColor, style.FrameRounding, 2);
            }

            bb.Min.Y += size.Y * 0.5f;

            draw->AddText(bb.Min, textColor, label);

            return isClicked;
        }

        private static void InvertFlagInt32<T>(ref T flags, T flag) where T : struct, Enum
        {
            int iFlags = Unsafe.BitCast<T, int>(flags);
            int iFlag = Unsafe.BitCast<T, int>(flag);
            iFlags ^= iFlag;
            flags = Unsafe.BitCast<int, T>(iFlags);
        }

        private void Update(float lineHeight)
        {
            ClearSelection();
            needsUpdate = false;
            source.Update(lineHeight);
            if (syntaxHighlight != null)
            {
                syntaxHighlight.Analyze(source, drawData, lineHeight);
            }
            else
            {
                SyntaxHighlightDefaults.Default.Analyze(source, drawData, lineHeight);
            }
        }

        private bool wasDragging = false;
        private bool wasFocused = false;
        private TextSelection selection = TextSelection.Invalid;

        private bool DrawEditor(string label, StdWString* text, Vector2 size)
        {
            uint id = ImGui.GetID(label);

            ImGuiContextPtr g = ImGui.GetCurrentContext();
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            ImGuiStyle* style = ImGui.GetStyle();
            ImGuiIO* io = ImGui.GetIO();
            ImDrawList* draw = ImGui.GetWindowDrawList();
            float lineHeight = ImGui.GetTextLineHeight();

            if (needsUpdate)
            {
                Update(lineHeight);
            }

            (int digits, float lineNumberWidth) = ComputeLineNumbersWidth();

            const float breakpointsWidth = 20;
            var textAreaSize = Vector2.Max(size, source.LayoutSize + new Vector2(lineNumberWidth + breakpointsWidth, size.Y - lineHeight));

            Vector2 cursor = ImGui.GetCursorScreenPos();

            ImRect bb = new() { Min = cursor, Max = cursor + textAreaSize };

            ImGuiP.ItemSize(bb, 0.0f);
            if (!ImGuiP.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            bool isHovered = ImGuiP.ItemHoverable(bb, id, g.LastItemData.ItemFlags);
            bool isFocused = ImGui.IsItemFocused();

            bool changed = false;

            Vector2 mousePos = ImGui.GetMousePos();

            float scroll = ImGui.GetScrollY();

            char* pText = text->CStr();

            if (jumpTo.HasValue)
            {
                var jump = jumpTo.Value;
                var lineIdx = jump.Line;
                var line = source.Lines[lineIdx];
                var column = jump.Column;
                float start = ImGuiWChar.CalcTextSize(line.Data, line.Data + column).X;
                float xwidth = 1;
                if (jump.Length > 0)
                {
                    xwidth = ImGuiWChar.CalcTextSize(line.Data + column, line.Data + column + jump.Length).X;
                }

                Vector2 min = cursor + new Vector2(start, lineIdx * lineHeight);
                Vector2 max = min + new Vector2(xwidth, lineHeight);
                ImGuiP.ScrollToRect(window, new ImRect() { Min = min, Max = max }, jump.Flags);
                jumpTo = null;
            }

            DrawBreakpoints(style, draw, breakpointsWidth, ref textAreaSize, ref cursor, lineHeight, mousePos, scroll);

            DrawLineNumbers(ref textAreaSize, style, draw, ref cursor, lineHeight, digits, lineNumberWidth, textAreaSize, scroll);
            cursor.X += 50;
            DrawCursorLine(draw, source, isFocused, lineHeight, cursor, textAreaSize, cursorState);
            DrawText(draw, cursor, lineHeight, textAreaSize.Y, scroll, pText);
            DrawFindResults(draw, cursor, source, lineHeight);
            DrawSelection(draw, cursor, source, lineHeight, textAreaSize.Y, scroll);

            HandleMouseInput(id, window, lineHeight, text, isHovered, isFocused, mousePos, cursor, pText);
            HandleKeyboardInput(text, ref changed, isFocused);

            if (isFocused != wasFocused)
            {
                wasFocused = isFocused;
            }

            if (changed)
            {
                source.Changed = true;
                Update(lineHeight);
            }

            return changed;
        }

        private void DrawBreakpoints(ImGuiStyle* style, ImDrawList* draw, float breakpointsWidth, ref Vector2 textAreaSize, ref Vector2 cursor, float lineHeight, Vector2 mousePos, float scroll)
        {
            var min = cursor;
            var max = cursor + new Vector2(breakpointsWidth, textAreaSize.Y);
            draw->AddRectFilled(cursor, max, 0xff2c2c2c);
            cursor.X += breakpointsWidth + style->FramePadding.X;
            textAreaSize.X -= breakpointsWidth + style->FramePadding.X;

            bool hovered = mousePos.Y >= min.Y && mousePos.X >= min.X && mousePos.Y <= max.Y && mousePos.X <= max.X;

            int hoveredLine = (int)((mousePos.Y - cursor.Y) / lineHeight);

            int start = (int)Math.Floor(scroll / lineHeight);
            int end = (int)Math.Ceiling(textAreaSize.Y / lineHeight) + start;

            List<Breakpoint> breakpoints = this.breakpoints;
            const float padding = 4;
            float radius = (lineHeight - padding) * 0.5f;

            int currentBreakpoint = breakpoints.BinarySearch(new Breakpoint(start, false), BreakpointComparer.Instance);
            if (currentBreakpoint < 0) currentBreakpoint = ~currentBreakpoint;
            Vector2 current = min;
            for (int i = start; i <= end; i++)
            {
                float lineOffset = i * lineHeight;

                var center = min + new Vector2(padding + radius, lineOffset + radius);

                var breakpoint = currentBreakpoint < breakpoints.Count ? breakpoints[currentBreakpoint] : Breakpoint.Invalid;
                if (breakpoint.Line == i)
                {
                    uint color = breakpoint.Enabled ? 0xff0000d0 : 0xffaaaaaa;
                    draw->AddCircleFilled(center, radius, color);
                    currentBreakpoint++;
                }

                // Highlight hovered line
                if (hovered && hoveredLine == i)
                {
                    draw->AddCircleFilled(center, radius, 0xffff0000);
                }
            }

            // Handle mouse click to toggle or add breakpoints
            if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                InsertBreakpoint(hoveredLine);
            }
        }

        public bool InsertBreakpoint(int line, bool removeIfPresent = true)
        {
            if (line < 0 || line >= source.LineCount) return false;
            Breakpoint breakpoint = new(line, true);
            int existingBreakpointIndex = breakpoints.BinarySearch(breakpoint, BreakpointComparer.Instance);
            if (existingBreakpointIndex >= 0)
            {
                if (!removeIfPresent) return true;
                breakpoints.RemoveAt(existingBreakpointIndex);
                return false;
            }
            else
            {
                breakpoints.Insert(~existingBreakpointIndex, breakpoint);
            }
            return true;
        }

        private (int digits, float lineNumbersWidth) ComputeLineNumbersWidth()
        {
            int digits = (int)Math.Log10(source.LineCount) + 1;
            byte* buf = stackalloc byte[digits + 1]; // no need to set last byte to \0 since stacks are naturally zeroed.
            MemsetT(buf, (byte)'0', digits);
            float lineNumbersWidth = ImGui.CalcTextSize(buf).X;
            return (digits, lineNumbersWidth);
        }

        private void DrawFindResults(ImDrawList* drawList, Vector2 cursor, TextSource source, float lineHeight)
        {
            const uint highlightColor = 0x993399FF; // ABGR
            const uint highlightColorSelected = 0x9996C9FC; // ABGR
            for (int i = 0; i < findResults.Count; i++)
            {
                SearchResult result = findResults[i];
                if (i == findIndex)
                {
                    DrawFindResult(drawList, cursor, source, lineHeight, highlightColorSelected, result);
                    continue;
                }

                DrawFindResult(drawList, cursor, source, lineHeight, highlightColor, result);
            }
        }

        private static void DrawFindResult(ImDrawList* drawList, Vector2 cursor, TextSource source, float lineHeight, uint highlightColor, SearchResult result)
        {
            var line = source.Lines[result.Line];
            float startX = ImGuiWChar.CalcTextSize(line.Data, result.Data).X;
            float endX = ImGuiWChar.CalcTextSize(line.Data, result.DataEnd).X;

            Vector2 quadOrigin = cursor + new Vector2(0, lineHeight * result.Line);

            Vector2 topLeft = quadOrigin + new Vector2(startX, 0);
            Vector2 bottomLeft = quadOrigin + new Vector2(startX, lineHeight);
            Vector2 topRight = quadOrigin + new Vector2(endX, 0);
            Vector2 bottomRight = quadOrigin + new Vector2(endX, lineHeight);

            drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, highlightColor);
        }

        private void DrawLineNumbers(ref Vector2 size, ImGuiStyle* style, ImDrawList* draw, ref Vector2 cursor, float lineHeight, int digits, float width, Vector2 textAreaSize, float scroll)
        {
            int bufferSize = digits + 1;
            byte* buf = stackalloc byte[bufferSize];
            Vector2 current = cursor;

            int start = Math.Max((int)Math.Floor(scroll / lineHeight), 0);
            int end = Math.Min((int)Math.Ceiling(textAreaSize.Y / lineHeight) + start, source.LineCount);
            current.Y += start * lineHeight;
            for (uint line = (uint)start + 1; line <= end; line++)
            {
                Utf8Formatter.Format(line, buf, bufferSize);
                draw->AddText(current, 0xff4c4c4c, buf);
                current.Y += lineHeight;
            }
            cursor.X += width + style->FramePadding.X;
            size.X -= width + style->FramePadding.X;
        }

        private void HandleMouseInput(uint id, ImGuiWindow* window, float lineHeight, StdWString* text, bool isHovered, bool isFocused, Vector2 mousePos, Vector2 origin, char* pText)
        {
            bool isMouseDown = isHovered && ImGui.IsMouseDown(ImGuiMouseButton.Left);
            bool isClick = ImGui.IsMouseClicked(ImGuiMouseButton.Left);
            bool isDoubleClick = ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);

            if (isHovered)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.TextInput);
                if (isMouseDown)
                {
                    ImGuiP.SetFocusID(id, window);
                    ImGuiP.SetActiveID(id, window);
                }
                else
                {
                    ImGuiP.ClearActiveID();
                }

                if (isClick && !ImGui.IsMouseDragging(ImGuiMouseButton.Left) && !isDoubleClick)
                {
                    ClearSelection();
                }
            }

            if (isMouseDown && isFocused)
            {
                if (!wasFocused)
                {
                    SetCursor(HitTest(source, origin, mousePos, lineHeight));
                }

                bool isDragging = ImGui.IsMouseDragging(ImGuiMouseButton.Left);
                if (isDragging)
                {
                    if (!wasDragging)
                    {
                        selection.Text = text;
                        selection.Start = selection.End = cursorState;
                        wasDragging = true;
                    }
                    else
                    {
                        SetCursor(HitTest(source, origin, mousePos, lineHeight));
                        selection.End = cursorState;
                    }
                }
                else
                {
                    wasDragging = false;
                }

                if (isDoubleClick)
                {
                    int index = HitTest(source, origin, mousePos, lineHeight);
                    int start = index;
                    while (start > 0 && char.IsLetterOrDigit(pText[start - 1]))
                    {
                        start--;
                    }
                    int end = index;
                    while (end < text->Size && char.IsLetterOrDigit(pText[end]))
                    {
                        end++;
                    }
                    SetSelection(start, end);
                    SetCursor(end);
                    return;
                }

                if (isClick)
                {
                    SetCursor(HitTest(source, origin, mousePos, lineHeight));
                    ClearSelection();
                    return;
                }
            }
        }

        public void SetCursor(int index)
        {
            StdWString text = *source.Text;
            if (index > 0 && text[index] == '\n' && text[index - 1] == '\r')
            {
                index--;
            }
            cursorState = CursorState.FromIndex(index, source);
        }

        public void OffsetCursor(int offset)
        {
            StdWString text = *source.Text;
            int index = cursorState.Index;

            int dir = offset > 0 ? 1 : -1;
            int absOffset = Math.Abs(offset);

            while (absOffset > 0 && index >= 0 && index <= text.Size)
            {
                if (dir > 0 && index >= 0 && index < text.Size - 1 && text[index] == '\r' && text[index + 1] == '\n')
                {
                    index += dir;
                }
                index += dir;

                absOffset--;
            }

            index = Math.Clamp(index, 0, text.Size);

            SetCursor(index);
        }

        public void SetSelection(int start, int end)
        {
            selection.Text = source.Text;
            selection.Start = CursorState.FromIndex(start, source);
            selection.End = CursorState.FromIndex(end, source);
        }

        public void SelectAll()
        {
            SetSelection(0, source.Text->Size);
        }

        /// <summary>
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            selection.Start = selection.End = CursorState.Invalid;
        }

        /// <summary>
        /// Erase the current selection.
        /// </summary>
        public void EraseSelection()
        {
            if (!selection.IsValid())
            {
                return;
            }

            if (selection.Length != 0)
            {
                source.Text->Erase(selection.EffectiveStart.Index, selection.Length);
                source.Update(ImGui.GetTextLineHeight());
            }

            SetCursor(selection.EffectiveStart.Index);
        }

        private void PreEdit(TextEditOp op)
        {
            // push history.
            history.UndoPush();

            if (op != TextEditOp.Replace)
            {
                // erase selection
                EraseSelection();
            }
        }

        private void PostEdit()
        {
            // clear selection
            ClearSelection();

            // update finds
            PerformFind();
        }

        private void HandleKeyboardInput(StdWString* text, ref bool changed, bool isFocused)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if (isFocused)
            {
                if (io.InputQueueCharacters.Size > 0)
                {
                    PreEdit(TextEditOp.Insert);
                    for (int i = 0; i < io.InputQueueCharacters.Size; i++)
                    {
                        char c = (char)io.InputQueueCharacters.Data[i];
                        if (!char.IsControl(c)) // Handle regular characters
                        {
                            text->Insert(cursorState, c);
                            cursorState++;
                            changed = true;
                        }
                    }
                    io.InputQueueCharacters.Resize(0); // Clear
                    PostEdit();
                }

                if (ImGui.IsKeyPressed(ImGuiKey.Backspace) && cursorState > 0) // Handle backspace
                {
                    PreEdit(TextEditOp.Erase);
                    if (!selection.IsValid() && text->Size > 0)
                    {
                        int index = cursorState - 1;
                        int length = 1;

                        char c = text->At(index);
                        if (c == '\n' || c == '\r')
                        {
                            if (c == '\n' && index - 1 >= 0 && text->At(index - 1) == '\r')
                            {
                                length++;
                                index--;
                            }

                            int newIndex = cursorState.Index - length;
                            int newLine = cursorState.Line - 1;
                            var line = source.Lines[newLine];
                            int newColumn = newIndex - line.Start;
                            cursorState = new(newIndex, newLine, newColumn);
                        }
                        else
                        {
                            cursorState--;
                        }
                        text->Erase(index, length);
                    }

                    changed = true;
                    PostEdit();
                }

                if (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)) // Handle enter
                {
                    PreEdit(TextEditOp.Insert);
                    switch (source.NewLineType)
                    {
                        case NewLineType.CRLF:
                            text->Insert(cursorState, '\r');
                            text->Insert(cursorState + 1, '\n');
                            cursorState += CursorState.NewLineCRLF;
                            break;

                        case NewLineType.LF:
                            text->Insert(cursorState, '\n');
                            cursorState += CursorState.NewLineLF;
                            break;

                        case NewLineType.CR:
                            text->Insert(cursorState, '\r');
                            cursorState += CursorState.NewLineCR;
                            break;

                        case NewLineType.Mixed:
                        default:
                            text->Insert(cursorState, '\n');
                            cursorState += CursorState.NewLineLF;
                            break;
                    }
                    cursorState.Column = 0;
                    changed = true;
                    PostEdit();
                }

                if (ImGui.IsKeyPressed(ImGuiKey.Tab))
                {
                    PreEdit(TextEditOp.Insert);
                    text->Insert(cursorState, '\t');
                    cursorState++;
                    changed = true;
                    PostEdit();
                }

                if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow) && cursorState > 0)
                {
                    MoveCursorHorizontally(-1);
                }
                if (ImGui.IsKeyPressed(ImGuiKey.RightArrow) && cursorState < text->Size)
                {
                    MoveCursorHorizontally(1);
                }

                if (ImGui.IsKeyPressed(ImGuiKey.UpArrow) && cursorState.Line > 0)
                {
                    MoveCursorVertically(-1);
                }

                if (ImGui.IsKeyPressed(ImGuiKey.DownArrow) && cursorState.Line < source.Lines.Count - 1)
                {
                    MoveCursorVertically(1);
                }

                if (io.KeyCtrl && ImGui.IsKeyDown(ImGuiKey.V))
                {
                    if (!pasted)
                    {
                        pasted = true;
                        pasteRepeatTimer = pasteInitialRepeatDelay;
                        InsertTextFromClipboard();
                        changed = true;
                    }
                    else
                    {
                        pasteRepeatTimer -= ImGui.GetIO().DeltaTime;
                        if (pasteRepeatTimer <= 0.0f)
                        {
                            pasteRepeatTimer = pasteRepeatDelay;
                            InsertTextFromClipboard();
                            changed = true;
                        }
                    }
                }
                else
                {
                    pasted = false;
                    pasteRepeatTimer = 0.0f;
                }

                if (io.KeyCtrl && ImGui.IsKeyDown(ImGuiKey.C) && selection.IsValid())
                {
                    CopySelectionToClipboard();
                }

                if (io.KeyCtrl && ImGui.IsKeyDown(ImGuiKey.X) && selection.IsValid())
                {
                    CopySelectionToClipboard();
                    PreEdit(TextEditOp.Cut);
                    PostEdit();
                    changed = true;
                }
            }
        }

        public void CopySelectionToClipboard()
        {
            var text = source.Text;
            var subString = text->SubString(selection.EffectiveStart.Index, selection.Length);
            ImGui.SetClipboardText(subString.ToString());
            subString.Release();
        }

        public void ShowFind()
        {
            InvertFlagInt32(ref findWindowFlags, FindWindowFlags.Visible);
        }

        public void JumpTo(int index, ImGuiScrollFlags scrollFlags = ImGuiScrollFlags.None)
        {
            (int line, int column) = GetLineAndColumnOfIndex(index);

            if (line == -1 || column == -1)
            {
                return;
            }

            JumpTo(index, line, column, scrollFlags);
        }

        public void JumpTo(int line, int column, ImGuiScrollFlags scrollFlags = ImGuiScrollFlags.None)
        {
            if (line < 0 || column < 0 || line >= source.Lines.Count)
            {
                return;
            }

            TextSpan lineSpan = source.Lines[line];
            if (column >= lineSpan.Length)
            {
                return;
            }

            var index = lineSpan.Start + column;

            JumpTo(index, line, column, scrollFlags);
        }

        public void JumpTo(int index, int line, int column, ImGuiScrollFlags scrollFlags = ImGuiScrollFlags.None)
        {
            jumpTo = new(index, line, column, scrollFlags);
        }

        public void JumpToWithLength(int index, int length, ImGuiScrollFlags scrollFlags = ImGuiScrollFlags.None)
        {
            (int line, int column) = GetLineAndColumnOfIndex(index);

            if (line == -1 || column == -1)
            {
                return;
            }

            JumpToWithLength(index, length, line, column, scrollFlags);
        }

        public void JumpToWithLength(int line, int column, int length, ImGuiScrollFlags scrollFlags = ImGuiScrollFlags.None)
        {
            if (line < 0 || column < 0 || line >= source.Lines.Count)
            {
                return;
            }

            TextSpan lineSpan = source.Lines[line];
            if (column >= lineSpan.Length)
            {
                return;
            }

            var index = lineSpan.Start + column;

            JumpToWithLength(index, length, line, column, scrollFlags);
        }

        public void JumpTo(SearchResult result, ImGuiScrollFlags scrollFlags = ImGuiScrollFlags.None)
        {
            JumpToWithLength(result.Start, result.Length, result.Line, result.Column, scrollFlags);
        }

        public void JumpToWithLength(int index, int length, int line, int column, ImGuiScrollFlags scrollFlags = ImGuiScrollFlags.None)
        {
            jumpTo = new(index, length, line, column, scrollFlags);
        }

        public (int line, int column) GetLineAndColumnOfIndex(int index)
        {
            int line = 0;
            int column = -1;
            for (; line < source.LineCount; line++)
            {
                var lineSpan = source.Lines[line];
                if (lineSpan.Start <= index && lineSpan.End > index)
                {
                    column = index - lineSpan.Start;
                    break;
                }
            }
            if (column == -1)
            {
                line = -1;
            }

            return (line, column);
        }

        public void Undo()
        {
            if (history.CanUndo)
            {
                history.Undo();
                needsUpdate = true;
            }
        }

        public void Redo()
        {
            if (history.CanRedo)
            {
                history.Redo();
                needsUpdate = true;
            }
        }

        private void MoveCursorHorizontally(int index)
        {
            OffsetCursor(index);
        }

        private void MoveCursorVertically(int offset)
        {
            var lineIndex = cursorState.Line + offset;
            var line = source.Lines[lineIndex];
            var column = Math.Min(line.Length, cursorState.Column);
            var index = line.Start + column;
            cursorState = new(index, lineIndex, column);
        }

        private bool pasted = false;
        private readonly float pasteRepeatDelay = 0.1f; // adjust this value to control the repeat delay
        private readonly float pasteInitialRepeatDelay = 0.5f; // adjust this value to control the repeat delay
        private float pasteRepeatTimer = 0.0f;

        public void InsertTextFromClipboard()
        {
            var text = source.Text;
            PreEdit(TextEditOp.Paste);
            var clipboardText = ImGui.GetClipboardText();
            int textSize = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(clipboardText).Length;
            text->InsertRange(cursorState, clipboardText, textSize);
            source.Update(ImGui.GetTextLineHeight());
            OffsetCursor(textSize);
            PostEdit();
        }

        private void DrawText(ImDrawList* drawList, Vector2 origin, float lineHeight, float maxHeight, float scroll, char* pText)
        {
            foreach (var span in drawData.GetVisibleSpans(scroll, lineHeight, maxHeight))
            {
                if (span.Control)
                {
                    uint c = '\\';
                    switch (*span.Data)
                    {
                        case '\r':
                            c |= 'r' << 8;
                            break;

                        case '\n':
                            c |= 'n' << 8;
                            break;

                        case '\t':
                            c |= 't' << 8;
                            break;
                    }

                    byte* p = (byte*)&c;
                    drawList->AddText(origin + span.Origin, span.Color, p, p + 2);
                }

                if (span.HasColor)
                {
                    ImGuiWChar.AddText(drawList, origin + span.Origin, span.Color, pText + span.Start, pText + span.End);
                }
                else
                {
                    ImGuiWChar.AddText(drawList, origin + span.Origin, 0xffffffff, pText + span.Start, pText + span.End);
                }
            }
        }

        private void DrawSelection(ImDrawList* drawList, Vector2 origin, TextSource source, float lineHeight, float maxHeight, float scroll)
        {
            if (!selection.IsValid())
            {
                return;
            }

            const uint selectionColor = 0x99FF9933;  // Light blue with transparency in ABGR format

            var start = selection.Start;
            var end = selection.End;

            if (start.Index > end.Index)
            {
                // swap.
                (start, end) = (end, start);
            }

            int startLine = start.Line;
            int endLine = end.Line;

            int visibleStart = Math.Max((int)MathF.Floor(scroll / lineHeight), startLine);
            int visibleEnd = Math.Min((int)MathF.Ceiling(maxHeight / lineHeight) + start, endLine);

            if (visibleStart > visibleEnd) return;

            for (int lineIndex = visibleStart; lineIndex <= visibleEnd; lineIndex++)
            {
                var line = source.Lines[lineIndex];
                Vector2 quadOrigin = origin + new Vector2(0, lineHeight * lineIndex);

                if (lineIndex == startLine)
                {
                    // Selection starts on this line
                    float startX = ImGuiWChar.CalcTextSize(line.Data, line.Data + start.Column).X;
                    float endX = lineIndex == endLine
                        ? ImGuiWChar.CalcTextSize(line.Data, line.Data + end.Column).X
                        : ImGuiWChar.CalcTextSize(line.Data, line.DataEnd).X;

                    Vector2 topLeft = quadOrigin + new Vector2(startX, 0);
                    Vector2 bottomLeft = quadOrigin + new Vector2(startX, lineHeight);
                    Vector2 topRight = quadOrigin + new Vector2(endX, 0);
                    Vector2 bottomRight = quadOrigin + new Vector2(endX, lineHeight);

                    drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, selectionColor);
                }
                else if (lineIndex == endLine)
                {
                    // Selection ends on this line
                    float endX = ImGuiWChar.CalcTextSize(line.Data, line.Data + end.Column).X;

                    Vector2 topLeft = quadOrigin;
                    Vector2 bottomLeft = quadOrigin + new Vector2(0, lineHeight);
                    Vector2 topRight = quadOrigin + new Vector2(endX, 0);
                    Vector2 bottomRight = quadOrigin + new Vector2(endX, lineHeight);

                    drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, selectionColor);
                }
                else
                {
                    // Full line selection
                    float lineWidth = ImGuiWChar.CalcTextSize(line.Data, line.DataEnd).X;

                    Vector2 topLeft = quadOrigin;
                    Vector2 bottomLeft = quadOrigin + new Vector2(0, lineHeight);
                    Vector2 topRight = quadOrigin + new Vector2(lineWidth, 0);
                    Vector2 bottomRight = quadOrigin + new Vector2(lineWidth, lineHeight);

                    drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, selectionColor);
                }
            }
        }

        private bool cursorVisible = true;
        private readonly float cursorBlinkInterval = 0.5f;
        private float cursorBlinkTimer = 0.0f;
        private SyntaxHighlight? syntaxHighlight;
        private ImFontPtr font;

        private void DrawCursorLine(ImDrawList* drawList, TextSource source, bool isFocused, float lineHeight, Vector2 origin, Vector2 avail, CursorState cursorState)
        {
            if (isFocused)
            {
                float cursorX = origin.X;
                float cursorY = origin.Y;

                if (cursorState.Line < source.Lines.Count)
                {
                    var lineSpan = source.Lines[cursorState.Line];

                    cursorX = origin.X + ImGuiWChar.CalcTextSize(lineSpan.Data, lineSpan.Data + cursorState.Column).X;
                    cursorY = origin.Y + cursorState.Line * lineHeight;
                }
                else if (source.Lines.Count > 0)
                {
                    TextSpan lastLineSpan = source.Lines[^1];
                    cursorX = origin.X + ImGuiWChar.CalcTextSize(lastLineSpan.Data, lastLineSpan.Data + lastLineSpan.Length).X;
                    cursorY = origin.Y + (source.LineCount - 1) * lineHeight;
                }

                Vector2 quadOrigin = origin + new Vector2(0, lineHeight * cursorState.Line);
                Vector2 topLeft = quadOrigin;
                Vector2 bottomLeft = quadOrigin + new Vector2(0, lineHeight);
                Vector2 topRight = quadOrigin + new Vector2(avail.X, 0);
                Vector2 bottomRight = quadOrigin + new Vector2(avail.X, lineHeight);

                drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, 0xff2c2c2c);

                cursorBlinkTimer -= ImGui.GetIO().DeltaTime;

                if (cursorBlinkTimer < 0)
                {
                    cursorBlinkTimer = cursorBlinkInterval;
                    cursorVisible = !cursorVisible;
                }

                if (cursorVisible)
                {
                    drawList->AddLine(new(cursorX, cursorY), new(cursorX, cursorY + lineHeight), 0xffffffff);
                }
            }
        }

        private static int HitTest(TextSource source, Vector2 origin, Vector2 mousePos, float lineHeight)
        {
            char* pText = source.Text->Data;
            Vector2 position = mousePos - origin;

            int lineIndex = (int)MathF.Floor(position.Y / lineHeight);

            if (lineIndex < 0 || lineIndex >= source.Lines.Count)
            {
                return source.Text->Size;
            }

            TextSpan lineSpan = source.Lines[lineIndex];

            int start = lineSpan.Start;
            int end = lineSpan.End;

            int mid;
            float midWidth, nextWidth;
            while (start < end)
            {
                mid = (start + end) / 2;
                midWidth = ImGuiWChar.CalcTextSize(pText + lineSpan.Start, pText + mid).X;
                nextWidth = ImGuiWChar.CalcTextSize(pText + lineSpan.Start, pText + mid + 1).X;

                if (midWidth <= position.X && position.X < nextWidth)
                {
                    // Decide which character is closer to the mouse position
                    if (position.X - midWidth < nextWidth - position.X)
                    {
                        return mid;
                    }
                    else
                    {
                        return mid + 1;
                    }
                }
                else if (midWidth < position.X)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid;
                }
            }

            // If we exit the loop, it means we didn't find an exact match
            // Return the closest character index, but ensure start doesn't go below lineSpan.Start
            if (start > lineSpan.Start)
            {
                float prevWidth = ImGuiWChar.CalcTextSize(pText + lineSpan.Start, pText + start - 1).X;
                if (prevWidth <= position.X)
                {
                    nextWidth = ImGuiWChar.CalcTextSize(pText + lineSpan.Start, pText + start).X;
                    if (position.X - prevWidth < nextWidth - position.X)
                    {
                        return start - 1;
                    }
                    else
                    {
                        return start;
                    }
                }
            }
            return lineSpan.Start;
        }

        public void Dispose()
        {
            source.Dispose();
            history.Dispose();
        }
    }
}